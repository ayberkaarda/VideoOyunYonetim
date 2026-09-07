using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VideoGameManager.Data;
using VideoGameManager.Domain;
using VideoGameManager.Services;
using VideoGameManager.Views;

namespace VideoGameManager.Presenters
{
    /// <summary>
    /// Drives <see cref="IGameListView"/>: searching, filtering, paging, sorting, deleting,
    /// launching the editor and exporting.
    /// </summary>
    /// <remarks>
    /// Rows are addressed by id throughout. Two games may share a title, so a screen that
    /// acted on the highlighted name would sooner or later act on the wrong row and report
    /// success while doing it.
    /// </remarks>
    public sealed class BrowseGamesPresenter
    {
        /// <summary>How many games one page of the list holds.</summary>
        private const int PageSize = 25;

        /// <summary>
        /// How many games one round trip reads while an export collects the whole result
        /// set. Larger than a screen page because nobody is waiting to read these rows, and
        /// still bounded so that a large catalogue is never asked for in a single statement.
        /// </summary>
        private const int ExportBatchSize = 200;

        private const string NoMatches =
            "No games match the current search and filters.";

        private const string NothingSelectedToEdit =
            "Select a game in the list first, then choose Edit.";

        private const string NothingSelectedToDelete =
            "Select a game in the list first, then choose Delete.";

        private const string DeleteFailed =
            "The game could not be deleted. It may already have been removed.";

        private const string ExportUnknownFormat =
            "That export format is not available.";

        private const string ExportFileFailed =
            "The export file could not be written. Check that the folder still exists and " +
            "that the file is not open in another application.";

        private const string ImportUnknownFormat =
            "That import format is not available.";

        private const string ImportFileFailed =
            "The import file could not be read. Check that the file still exists and that it " +
            "is not open in another application.";

        private const string ImportFileNotUnderstood =
            "That file is not in the expected format, so nothing was imported. Choose a file " +
            "this application exported.";

        /// <summary>
        /// UTF-8 without a byte order mark. The mark is not part of the exported content:
        /// a strict JSON reader rejects a document that starts with one, and every tool that
        /// reads these files treats a plain UTF-8 stream correctly. Writing the mark would
        /// make one of the two supported formats unparsable to gain nothing in the other.
        /// </summary>
        private static readonly Encoding ExportEncoding = new UTF8Encoding(false);

        /// <summary>
        /// Encoding assumed for an import file that carries no byte order mark. UTF-8, because
        /// that is what the export writes and what any other tool producing JSON writes; a file
        /// that does carry a mark is read in whatever encoding the mark announces instead.
        /// </summary>
        private static readonly Encoding ImportEncoding = new UTF8Encoding(false);

        private readonly IGameListView _view;
        private readonly IGameService _games;
        private readonly IReadOnlyList<IGameExporter> _exporters;
        private readonly IReadOnlyList<IGameImporter> _importers;
        private readonly ILogger<BrowseGamesPresenter> _logger;

        /// <summary>One-based number of the page currently on screen.</summary>
        private int _page = 1;

        /// <summary>How many pages the current result set spans. Zero when nothing matched.</summary>
        private int _pageCount;

        /// <summary>
        /// The game whose details are on screen, kept so that a confirmation prompt can name
        /// it. Cleared whenever the list is reloaded, because the highlight goes with it.
        /// </summary>
        private Game? _selected;

        /// <summary>
        /// Wires the presenter to a view.
        /// </summary>
        /// <param name="view">The screen to drive.</param>
        /// <param name="games">The catalogue.</param>
        /// <param name="exporters">Every available export format.</param>
        /// <param name="importers">Every available import format.</param>
        /// <param name="logger">Where failures are recorded in full.</param>
        /// <exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
        public BrowseGamesPresenter(
            IGameListView view,
            IGameService games,
            IEnumerable<IGameExporter> exporters,
            IEnumerable<IGameImporter> importers,
            ILogger<BrowseGamesPresenter> logger)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _games = games ?? throw new ArgumentNullException(nameof(games));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (exporters == null)
            {
                throw new ArgumentNullException(nameof(exporters));
            }

            if (importers == null)
            {
                throw new ArgumentNullException(nameof(importers));
            }

            // Copied once: the container hands over a lazy sequence, and the list is walked
            // again on every export.
            _exporters = new List<IGameExporter>(exporters);
            _importers = new List<IGameImporter>(importers);

            _view.Loaded += OnLoaded;
            _view.SelectionChanged += OnSelectionChanged;
            _view.FilterChanged += OnFilterChanged;
            _view.PreviousPageRequested += OnPreviousPageRequested;
            _view.NextPageRequested += OnNextPageRequested;
            _view.EditRequested += OnEditRequested;
            _view.DeleteRequested += OnDeleteRequested;
            _view.ExportRequested += OnExportRequested;
            _view.ImportRequested += OnImportRequested;
        }

        // ------------------------------------------------------------------
        // Event handlers
        //
        // A failed load or a failed detail fetch renders inline, in the space the list or
        // the details normally occupy, instead of a dialog: the database can drop out while
        // the user is arrowing through the list, and a dialog on every keystroke would be
        // both disruptive and, because the row stays selected, would reopen on the next one.
        // A failure of an action the user deliberately started - deleting, exporting - does
        // get a dialog, because there is nowhere else to report it and it happens once.
        // ------------------------------------------------------------------

        private async void OnLoaded(object? sender, EventArgs e)
        {
            try
            {
                PublishExportFormats();
                PublishImportFormats();
                await LoadLookupsAsync(CancellationToken.None).ConfigureAwait(true);
                await LoadPageAsync(CancellationToken.None).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loading the game list failed on {Screen}", nameof(BrowseGamesPresenter));
                _view.ShowListUnavailable(Messages.ForUser(ex));
            }
        }

        private async void OnSelectionChanged(object? sender, EventArgs e)
        {
            try
            {
                await ShowSelectedAsync(CancellationToken.None).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loading game details failed on {Screen}", nameof(BrowseGamesPresenter));
                _view.ShowDetailsUnavailable(Messages.ForUser(ex));
            }
        }

        private async void OnFilterChanged(object? sender, EventArgs e)
        {
            try
            {
                // Back to the first page. Staying on page seven of a result set that now has
                // two pages would show an empty list and read as a broken screen.
                _page = 1;
                await LoadPageAsync(CancellationToken.None).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Applying the game list filter failed on {Screen}", nameof(BrowseGamesPresenter));
                _view.ShowListUnavailable(Messages.ForUser(ex));
            }
        }

        private async void OnPreviousPageRequested(object? sender, EventArgs e)
        {
            try
            {
                if (_page <= 1)
                {
                    return;
                }

                _page--;
                await LoadPageAsync(CancellationToken.None).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Paging the game list failed on {Screen}", nameof(BrowseGamesPresenter));
                _view.ShowListUnavailable(Messages.ForUser(ex));
            }
        }

        private async void OnNextPageRequested(object? sender, EventArgs e)
        {
            try
            {
                if (_page >= _pageCount)
                {
                    return;
                }

                _page++;
                await LoadPageAsync(CancellationToken.None).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Paging the game list failed on {Screen}", nameof(BrowseGamesPresenter));
                _view.ShowListUnavailable(Messages.ForUser(ex));
            }
        }

        private async void OnEditRequested(object? sender, EventArgs e)
        {
            try
            {
                int? id = _view.SelectedGameId;
                if (id == null)
                {
                    _view.ShowInfo(NothingSelectedToEdit);
                    return;
                }

                _view.OpenEditor(id.Value);

                // The editor may have renamed, rescored or re-genred the game, any of which
                // can move it out of the current filter or to a different position in the
                // sort, so the page is read again rather than patched in place.
                await ReloadCurrentPageAsync(CancellationToken.None).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Editing a game failed on {Screen}", nameof(BrowseGamesPresenter));
                _view.ShowError(Messages.ForUser(ex));
            }
        }

        private async void OnDeleteRequested(object? sender, EventArgs e)
        {
            try
            {
                await DeleteSelectedAsync(CancellationToken.None).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Deleting a game failed on {Screen}", nameof(BrowseGamesPresenter));
                _view.ShowError(Messages.ForUser(ex));
            }
        }

        private async void OnExportRequested(object? sender, ExportRequestedEventArgs e)
        {
            try
            {
                await ExportAsync(e, CancellationToken.None).ConfigureAwait(true);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Writing the export file failed on {Screen}", nameof(BrowseGamesPresenter));
                _view.ShowError(ExportFileFailed);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Writing the export file was refused on {Screen}", nameof(BrowseGamesPresenter));
                _view.ShowError(ExportFileFailed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exporting the game list failed on {Screen}", nameof(BrowseGamesPresenter));
                _view.ShowError(Messages.ForUser(ex));
            }
        }

        private async void OnImportRequested(object? sender, ImportRequestedEventArgs e)
        {
            try
            {
                await ImportAsync(e, CancellationToken.None).ConfigureAwait(true);
            }
            catch (ImportFormatException ex)
            {
                // The file was opened and read, and turned out not to be what it claimed. The
                // catalogue is untouched, which is what the message says.
                _logger.LogError(ex, "The import file was not in the expected format on {Screen}", nameof(BrowseGamesPresenter));
                _view.ShowError(ImportFileNotUnderstood);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Reading the import file failed on {Screen}", nameof(BrowseGamesPresenter));
                _view.ShowError(ImportFileFailed);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Reading the import file was refused on {Screen}", nameof(BrowseGamesPresenter));
                _view.ShowError(ImportFileFailed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Importing games failed on {Screen}", nameof(BrowseGamesPresenter));
                _view.ShowError(Messages.ForUser(ex));
            }
        }

        // ------------------------------------------------------------------
        // Work
        // ------------------------------------------------------------------

        private void PublishExportFormats()
        {
            List<ExportFormat> formats = new List<ExportFormat>(_exporters.Count);
            foreach (IGameExporter exporter in _exporters)
            {
                formats.Add(new ExportFormat(exporter.Format, exporter.FileExtension));
            }

            _view.ExportFormats = formats;
        }

        private void PublishImportFormats()
        {
            List<ImportFormat> formats = new List<ImportFormat>(_importers.Count);
            foreach (IGameImporter importer in _importers)
            {
                formats.Add(new ImportFormat(importer.Format, importer.FileExtension));
            }

            _view.ImportFormats = formats;
        }

        private async Task LoadLookupsAsync(CancellationToken ct)
        {
            IReadOnlyList<string> genres = await _games.GetGenresAsync(ct).ConfigureAwait(true);
            IReadOnlyList<string> platforms = await _games.GetPlatformsAsync(ct).ConfigureAwait(true);

            // Both lists are handed over before the first page is read, so that filling the
            // filters cannot itself look like a filter change once the list has content.
            _view.Genres = genres;
            _view.Platforms = platforms;
        }

        private async Task LoadPageAsync(CancellationToken ct)
        {
            _view.IsBusy = true;
            try
            {
                PagedResult<Game> result = await _games
                    .SearchAsync(CurrentFilter(), _page, PageSize, _view.SortField, _view.SortDescending, ct)
                    .ConfigureAwait(true);

                _pageCount = result.PageCount;
                _selected = null;

                if (_pageCount == 0)
                {
                    // Nothing matched, so there is no page to be on. Resetting keeps the next
                    // query from asking for a page number left over from a wider filter.
                    _page = 1;
                    _view.ShowListUnavailable(NoMatches);
                }
                else
                {
                    _view.Games = result.Items;
                    _view.ShowDetails(null);
                }

                _view.ShowPage(_page, _pageCount, result.TotalCount);
            }
            finally
            {
                _view.IsBusy = false;
            }
        }

        /// <summary>
        /// Reads the current page again, stepping back one page when the page the user was
        /// on no longer exists. Deleting the only row of the last page is the ordinary way
        /// to end up there.
        /// </summary>
        private async Task ReloadCurrentPageAsync(CancellationToken ct)
        {
            await LoadPageAsync(ct).ConfigureAwait(true);

            if (_pageCount > 0 && _page > _pageCount)
            {
                _page = _pageCount;
                await LoadPageAsync(ct).ConfigureAwait(true);
            }
        }

        private async Task ShowSelectedAsync(CancellationToken ct)
        {
            int? id = _view.SelectedGameId;
            if (id == null)
            {
                _selected = null;
                _view.ShowDetails(null);
                return;
            }

            Game? game = await _games.GetAsync(id.Value, ct).ConfigureAwait(true);
            _selected = game;
            _view.ShowDetails(game);
        }

        private async Task DeleteSelectedAsync(CancellationToken ct)
        {
            int? id = _view.SelectedGameId;
            if (id == null)
            {
                _view.ShowInfo(NothingSelectedToDelete);
                return;
            }

            if (!_view.Confirm(ConfirmDeleteMessage(id.Value)))
            {
                return;
            }

            _view.IsBusy = true;
            Result outcome;
            try
            {
                outcome = await _games.DeleteAsync(id.Value, ct).ConfigureAwait(true);
            }
            finally
            {
                _view.IsBusy = false;
            }

            if (!outcome.IsSuccess)
            {
                _logger.LogWarning("Deleting game {GameId} was rejected by the service.", id.Value);
                _view.ShowError(DeleteFailed);
            }

            // The list is re-read either way: a rejected delete usually means the row is
            // already gone, and leaving it on screen would invite a second attempt.
            await ReloadCurrentPageAsync(ct).ConfigureAwait(true);
        }

        private async Task ExportAsync(ExportRequestedEventArgs request, CancellationToken ct)
        {
            IGameExporter? exporter = FindExporter(request.Format);
            if (exporter == null)
            {
                _logger.LogWarning("No exporter is registered for the format {Format}.", request.Format);
                _view.ShowError(ExportUnknownFormat);
                return;
            }

            _view.IsBusy = true;
            try
            {
                // The filter and the sort are captured once, so that a batch read halfway
                // through cannot pick up a filter the user changed while the file was being
                // written.
                IReadOnlyList<Game> rows = await ReadEveryMatchAsync(
                    CurrentFilter(), _view.SortField, _view.SortDescending, ct).ConfigureAwait(true);

                using (StreamWriter writer = new StreamWriter(request.FilePath, false, ExportEncoding))
                {
                    await exporter.WriteAsync(rows, writer, ct).ConfigureAwait(true);
                }

                _logger.LogInformation(
                    "Exported {Count} game(s) as {Format} to {FilePath}",
                    rows.Count, exporter.Format, request.FilePath);

                _view.ShowInfo(string.Format(
                    CultureInfo.CurrentCulture,
                    "Exported {0} {1} to {2}.",
                    rows.Count,
                    rows.Count == 1 ? "game" : "games",
                    request.FilePath));
            }
            finally
            {
                _view.IsBusy = false;
            }
        }

        /// <summary>
        /// Reads a file with the chosen importer and stores what it holds, then reports what
        /// happened to each entry.
        /// </summary>
        /// <remarks>
        /// Importing never overwrites and never deletes. A game whose title is already in the
        /// catalogue is left exactly as it is and counted as skipped, which is what makes
        /// importing the same file twice harmless: the second run adds nothing rather than
        /// duplicating a catalogue or writing over rows the user has since edited by hand.
        /// </remarks>
        private async Task ImportAsync(ImportRequestedEventArgs request, CancellationToken ct)
        {
            IGameImporter? importer = FindImporter(request.Format);
            if (importer == null)
            {
                _logger.LogWarning("No importer is registered for the format {Format}.", request.Format);
                _view.ShowError(ImportUnknownFormat);
                return;
            }

            _view.IsBusy = true;
            try
            {
                IReadOnlyList<Game> incoming;

                using (StreamReader reader = new StreamReader(request.FilePath, ImportEncoding, true))
                {
                    incoming = await importer.ReadAsync(reader, ct).ConfigureAwait(true);
                }

                ImportTally tally = await StoreEachAsync(incoming, ct).ConfigureAwait(true);

                _logger.LogInformation(
                    "Imported {Added} game(s) as {Format} from {FilePath}; " +
                    "{Duplicates} already present and {Invalid} rejected.",
                    tally.Added, importer.Format, request.FilePath, tally.Duplicates, tally.Invalid);

                await ShowEverythingAsync(ct).ConfigureAwait(true);

                // Reported after the list has been redrawn, so that the user reads the counts
                // with the imported games already behind the dialog rather than having to
                // dismiss it before anything on screen changes.
                _view.ShowInfo(Describe(tally));
            }
            finally
            {
                _view.IsBusy = false;
            }
        }

        /// <summary>
        /// Offers every imported game to the catalogue in turn and counts what became of it.
        /// </summary>
        /// <remarks>
        /// <para>
        /// One entry is one decision, so a file is never refused because part of it is wrong:
        /// an entry that breaks a rule is counted and passed over, and the next one is still
        /// tried. The rules themselves are not repeated here. The catalogue validates every
        /// game it is given and answers with the broken rules, so an entry that comes back
        /// rejected is counted as invalid without this method knowing what "valid" means.
        /// </para>
        /// <para>
        /// Titles are compared without regard to case and after trimming, against the titles
        /// already stored and against the ones this run has just added, so a file that lists
        /// the same game twice adds it once.
        /// </para>
        /// </remarks>
        private async Task<ImportTally> StoreEachAsync(IReadOnlyList<Game> incoming, CancellationToken ct)
        {
            HashSet<string> known = await ReadEveryNameAsync(ct).ConfigureAwait(true);

            int added = 0;
            int duplicates = 0;
            int invalid = 0;

            foreach (Game game in incoming)
            {
                ct.ThrowIfCancellationRequested();

                string name = game.Name == null ? string.Empty : game.Name.Trim();

                // An entry with no title at all is not compared: it collides with nothing, and
                // the catalogue is about to reject it for the missing title anyway.
                if (name.Length > 0 && known.Contains(name))
                {
                    duplicates++;
                    continue;
                }

                Result<int> outcome = await _games.AddAsync(game, ct).ConfigureAwait(true);

                if (!outcome.IsSuccess)
                {
                    invalid++;
                    _logger.LogWarning(
                        "An imported game was rejected: {ErrorCount} broken rule(s).",
                        outcome.Errors.Count);
                    continue;
                }

                added++;

                if (name.Length > 0)
                {
                    // Only a title that was actually stored joins the set, so that two
                    // identical entries the catalogue refuses are both counted as invalid
                    // rather than the second one being reported as a duplicate of a row that
                    // was never written.
                    known.Add(name);
                }
            }

            return new ImportTally(added, duplicates, invalid);
        }

        /// <summary>
        /// Collects the title of every game in the catalogue, compared without regard to case.
        /// </summary>
        /// <remarks>
        /// Read once, before the first insert, rather than asked per entry: an import of a
        /// hundred games would otherwise be a hundred extra round trips to answer a question
        /// this run can answer for itself as it goes.
        /// </remarks>
        private async Task<HashSet<string>> ReadEveryNameAsync(CancellationToken ct)
        {
            // No filter: a title the user cannot currently see still occupies that title, and
            // an import that only checked the games on screen would happily add a second copy
            // of one the filter was hiding.
            IReadOnlyList<Game> everything = await ReadEveryMatchAsync(
                null, GameSortField.Name, false, ct).ConfigureAwait(true);

            HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (Game game in everything)
            {
                if (!string.IsNullOrWhiteSpace(game.Name))
                {
                    names.Add(game.Name.Trim());
                }
            }

            return names;
        }

        /// <summary>
        /// Reads the lookups again and shows the first page, so that the games just imported
        /// are on screen.
        /// </summary>
        /// <remarks>
        /// The lookups are read again because an import can bring in a genre or a platform
        /// nobody had used before, and filling them puts both of those filters back on their
        /// "any" entry. That reset is wanted rather than tolerated: a narrow genre filter left
        /// in place could hide every game that was just imported and make a successful import
        /// look like one that did nothing. The page number goes back to one for the same
        /// reason, since a wider result set renumbers the pages under it.
        /// </remarks>
        private async Task ShowEverythingAsync(CancellationToken ct)
        {
            await LoadLookupsAsync(ct).ConfigureAwait(true);

            _page = 1;
            await LoadPageAsync(ct).ConfigureAwait(true);
        }

        /// <summary>
        /// Reads every game a filter selects, one batch at a time.
        /// </summary>
        /// <remarks>
        /// The export covers the whole result set rather than the page on screen. A file
        /// produced from a filter that matches 163 games but holding only the 25 that
        /// happened to be visible is a silent loss: nothing in the file says it is partial,
        /// and whoever opens it later has no way to tell. Reading it in batches keeps the
        /// database from being asked for the entire catalogue in one statement.
        /// </remarks>
        /// <param name="filter">Which games to read. <c>null</c> reads the whole catalogue.</param>
        /// <param name="sort">Column to order by.</param>
        /// <param name="descending"><c>true</c> to order from high to low.</param>
        /// <param name="ct">Cancellation token.</param>
        private async Task<IReadOnlyList<Game>> ReadEveryMatchAsync(
            GameFilter? filter, GameSortField sort, bool descending, CancellationToken ct)
        {
            List<Game> everything = new List<Game>();
            int page = 1;

            while (true)
            {
                ct.ThrowIfCancellationRequested();

                PagedResult<Game> batch = await _games
                    .SearchAsync(filter, page, ExportBatchSize, sort, descending, ct)
                    .ConfigureAwait(true);

                if (batch.Items == null || batch.Items.Count == 0)
                {
                    break;
                }

                everything.AddRange(batch.Items);

                // Both conditions are checked so that the loop ends even if a row is added or
                // removed between two batches and the totals stop agreeing.
                if (!batch.HasNextPage || everything.Count >= batch.TotalCount)
                {
                    break;
                }

                page++;
            }

            return everything;
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private GameFilter CurrentFilter()
        {
            string search = _view.SearchText;

            // false and null mean the same thing to the filter - "do not narrow" - so the
            // unchecked box is passed as null rather than as a request for the games
            // nobody marked.
            bool? onlyFavourites = _view.OnlyFavourites ? true : (bool?)null;

            return new GameFilter(
                Name: string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
                Genre: _view.SelectedGenre,
                Platform: _view.SelectedPlatform,
                Status: _view.SelectedStatus,
                OnlyFavourites: onlyFavourites);
        }

        private IGameExporter? FindExporter(string format)
        {
            foreach (IGameExporter exporter in _exporters)
            {
                if (string.Equals(exporter.Format, format, StringComparison.OrdinalIgnoreCase))
                {
                    return exporter;
                }
            }

            return null;
        }

        private IGameImporter? FindImporter(string format)
        {
            foreach (IGameImporter importer in _importers)
            {
                if (string.Equals(importer.Format, format, StringComparison.OrdinalIgnoreCase))
                {
                    return importer;
                }
            }

            return null;
        }

        /// <summary>
        /// Puts the outcome of an import into a sentence per count.
        /// </summary>
        /// <remarks>
        /// All three counts are always shown, including the zeros. A summary that mentioned
        /// only what happened would leave the user to work out whether the missing games were
        /// skipped as duplicates or refused as invalid, which is the one thing they came to
        /// this dialog to learn.
        /// </remarks>
        private static string Describe(ImportTally tally)
        {
            StringBuilder text = new StringBuilder();

            text.AppendFormat(
                CultureInfo.CurrentCulture,
                "Imported {0} {1}.",
                tally.Added,
                tally.Added == 1 ? "game" : "games");

            text.Append(Environment.NewLine);
            text.AppendFormat(
                CultureInfo.CurrentCulture,
                "Skipped {0} already in the catalogue.",
                tally.Duplicates);

            text.Append(Environment.NewLine);
            text.AppendFormat(
                CultureInfo.CurrentCulture,
                "Skipped {0} that broke a validation rule.",
                tally.Invalid);

            return text.ToString();
        }

        private string ConfirmDeleteMessage(int gameId)
        {
            // The cached game is only trusted when it is the one that is highlighted; the
            // selection can have moved on since the details were fetched.
            Game? selected = _selected;
            if (selected != null && selected.Id == gameId && !string.IsNullOrWhiteSpace(selected.Name))
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    "Delete \"{0}\" and its reviews? This cannot be undone.",
                    selected.Name);
            }

            return "Delete the selected game and its reviews? This cannot be undone.";
        }

        /// <summary>
        /// What became of the entries in one import file. The three counts add up to the
        /// number of entries the file held, which is what lets the summary be checked against
        /// the file rather than believed.
        /// </summary>
        private readonly struct ImportTally
        {
            public ImportTally(int added, int duplicates, int invalid)
            {
                Added = added;
                Duplicates = duplicates;
                Invalid = invalid;
            }

            /// <summary>How many games were stored.</summary>
            public int Added { get; }

            /// <summary>How many were passed over because that title was already stored.</summary>
            public int Duplicates { get; }

            /// <summary>How many the catalogue refused because they broke a rule.</summary>
            public int Invalid { get; }
        }
    }
}
