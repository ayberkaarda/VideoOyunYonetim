using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VideoGameManager.Data;
using VideoGameManager.Domain;
using VideoGameManager.UI.Controls;
using VideoGameManager.Views;

namespace VideoGameManager
{
    /// <summary>
    /// Passive view for the "browse games" screen. It renders what it is given, reports which
    /// row is highlighted and how the list is filtered, and raises an event for every action
    /// the user starts; the queries and the decisions live behind the presenter.
    /// </summary>
    public partial class BrowseGamesForm : VideoGameManager.UI.Controls.ChromelessForm, IGameListView
    {
        private const string AnyGenre = "Any genre";
        private const string AnyPlatform = "Any platform";

        private const string NoExportFormats =
            "No export format is available.";

        private readonly Presenters.BrowseGamesPresenter? _presenter;
        private readonly IServiceProvider? _provider;

        private IReadOnlyList<ExportFormat> _exportFormats = new ExportFormat[0];

        /// <summary>
        /// Set while the filter controls are being filled from code. Every filter control
        /// raises a change event when its contents are replaced, and without this the act of
        /// listing the genres would look exactly like a user picking one and would send the
        /// screen back to page one in the middle of its first load.
        /// </summary>
        private bool _fillingFilters;

        /// <summary>Parameterless constructor for the Visual Studio designer only.</summary>
        public BrowseGamesForm()
        {
            InitializeComponent();

            FillFixedFilters();
        }

        /// <summary>The constructor the container uses. It wires the presenter to this view.</summary>
        /// <param name="provider">
        /// The container, used to resolve the editing screen. The list screen opens it the same
        /// way the main menu opens any other screen: in a scope of its own, so the editor
        /// arrives with its own presenter and its own database connection.
        /// </param>
        /// <param name="games">The catalogue.</param>
        /// <param name="exporters">Every registered export format.</param>
        /// <param name="covers">
        /// The shared cover cache. It is handed in rather than created here so that every
        /// screen draws from the same one and artwork survives a window closing.
        /// </param>
        /// <param name="presenterLogger">Logger for the presenter.</param>
        public BrowseGamesForm(
            IServiceProvider provider,
            Services.IGameService games,
            IEnumerable<Services.IGameExporter> exporters,
            ICoverImageProvider covers,
            ILogger<Presenters.BrowseGamesPresenter> presenterLogger)
            : this()
        {
            _provider = provider;
            picCover.Provider = covers;
            _presenter = new Presenters.BrowseGamesPresenter(this, games, exporters, presenterLogger);
        }

        /// <inheritdoc />
        public event EventHandler? Loaded;

        /// <inheritdoc />
        public event EventHandler? SelectionChanged;

        /// <inheritdoc />
        public event EventHandler? FilterChanged;

        /// <inheritdoc />
        public event EventHandler? PreviousPageRequested;

        /// <inheritdoc />
        public event EventHandler? NextPageRequested;

        /// <inheritdoc />
        public event EventHandler? EditRequested;

        /// <inheritdoc />
        public event EventHandler? DeleteRequested;

        /// <inheritdoc />
        public event EventHandler<ExportRequestedEventArgs>? ExportRequested;

        IReadOnlyList<Game> Views.IGameListView.Games
        {
            set
            {
                lblListStatus.Visible = false;
                lstGames.Visible = true;

                lstGames.Items.Clear();
                foreach (Game game in value)
                {
                    lstGames.Items.Add(new GameRow(game));
                }
            }
        }

        IReadOnlyList<string> Views.IGameListView.Genres
        {
            set { FillLookup(cmbGenre, AnyGenre, value); }
        }

        IReadOnlyList<string> Views.IGameListView.Platforms
        {
            set { FillLookup(cmbPlatform, AnyPlatform, value); }
        }

        IReadOnlyList<ExportFormat> Views.IGameListView.ExportFormats
        {
            set { _exportFormats = value ?? new ExportFormat[0]; }
        }

        /// <inheritdoc />
        public int? SelectedGameId => lstGames.SelectedItem is GameRow row ? row.Id : (int?)null;

        /// <inheritdoc />
        public string SearchText => searchGames.Text;

        /// <inheritdoc />
        public string? SelectedGenre => SelectedLookup(cmbGenre);

        /// <inheritdoc />
        public string? SelectedPlatform => SelectedLookup(cmbPlatform);

        /// <inheritdoc />
        public PlayStatus? SelectedStatus =>
            cmbStatus.SelectedItem is StatusOption option ? option.Value : null;

        /// <inheritdoc />
        public bool OnlyFavourites => chkFavourites.Checked;

        /// <inheritdoc />
        public GameSortField SortField =>
            cmbSort.SelectedItem is SortOption option ? option.Field : GameSortField.Name;

        /// <inheritdoc />
        public bool SortDescending =>
            cmbSort.SelectedItem is SortOption option && option.Descending;

        bool Views.IView.IsBusy
        {
            set => Cursor = value ? Cursors.WaitCursor : Cursors.Default;
        }

        /// <inheritdoc />
        public void ShowDetails(Game? game)
        {
            if (game is null)
            {
                lblName.Text = string.Empty;
                lblGenre.Text = string.Empty;
                lblPlatform.Text = string.Empty;
                badgeScore.Score = null;
                lblStatus.Text = string.Empty;
                lblFavourite.Text = string.Empty;
                lblComment.Text = string.Empty;
                _ = picCover.LoadCoverAsync(null);
                return;
            }

            lblName.Text = game.Name;
            lblGenre.Text = game.Genre;
            lblPlatform.Text = JoinPlatforms(game.Platforms);
            badgeScore.Score = game.Score;
            lblStatus.Text = DescribeStatus(game.Status);
            lblFavourite.Text = game.IsFavourite ? "Yes" : "No";
            lblComment.Text = game.LatestReview;

            // LoadCoverAsync cancels a load still in flight for a previous selection, so
            // arrowing through the list quickly can never leave a stale cover on screen.
            // It drives IsLoading itself and swallows provider failures.
            _ = picCover.LoadCoverAsync(game.CoverUrl);
        }

        /// <inheritdoc />
        public void ShowPage(int page, int pageCount, int totalCount)
        {
            btnPreviousPage.Enabled = page > 1;
            btnNextPage.Enabled = page < pageCount;

            if (totalCount <= 0)
            {
                lblPage.Text = "No games";
                return;
            }

            lblPage.Text = string.Format(
                CultureInfo.CurrentCulture,
                "Page {0} of {1} - {2} {3}",
                page,
                pageCount,
                totalCount,
                totalCount == 1 ? "game" : "games");
        }

        /// <inheritdoc />
        public void ShowListUnavailable(string message)
        {
            lstGames.Items.Clear();
            lstGames.Visible = false;
            lblListStatus.Text = message;
            lblListStatus.Visible = true;

            ShowDetails(null);
        }

        /// <inheritdoc />
        public void ShowDetailsUnavailable(string message)
        {
            lblName.Text = string.Empty;
            lblGenre.Text = string.Empty;
            lblPlatform.Text = string.Empty;
            badgeScore.Score = null;
            lblStatus.Text = string.Empty;
            lblFavourite.Text = string.Empty;
            lblComment.Text = message;
            _ = picCover.LoadCoverAsync(null);
        }

        /// <inheritdoc />
        public void OpenEditor(int gameId)
        {
            if (_provider is null)
            {
                return;
            }

            // The same shape the main menu uses: a scope per dialog, so the editing screen
            // gets its own presenter and its own scoped services and is disposed with the
            // dialog rather than living as long as this window.
            using (IServiceScope scope = _provider.CreateScope())
            using (AddGameForm editor = scope.ServiceProvider.GetRequiredService<AddGameForm>())
            {
                editor.LoadForEditing(gameId);
                editor.ShowDialog(this);
            }
        }

        /// <inheritdoc />
        public void ShowError(string message)
        {
            MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <inheritdoc />
        public void ShowInfo(string message)
        {
            MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <inheritdoc />
        public bool Confirm(string message)
        {
            return MessageBox.Show(this, message, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                   == DialogResult.Yes;
        }

        // ------------------------------------------------------------------
        // Event handlers. Each one raises a view event and decides nothing.
        // ------------------------------------------------------------------

        private void btnClose_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void BrowseGamesForm_Load(object? sender, EventArgs e)
        {
            Loaded?.Invoke(this, EventArgs.Empty);
        }

        private void lstGames_SelectedIndexChanged(object? sender, EventArgs e)
        {
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void filterControl_Changed(object? sender, EventArgs e)
        {
            if (_fillingFilters)
            {
                return;
            }

            FilterChanged?.Invoke(this, EventArgs.Empty);
        }

        private void btnPreviousPage_Click(object? sender, EventArgs e)
        {
            PreviousPageRequested?.Invoke(this, EventArgs.Empty);
        }

        private void btnNextPage_Click(object? sender, EventArgs e)
        {
            NextPageRequested?.Invoke(this, EventArgs.Empty);
        }

        private void btnEdit_Click(object? sender, EventArgs e)
        {
            EditRequested?.Invoke(this, EventArgs.Empty);
        }

        private void btnDelete_Click(object? sender, EventArgs e)
        {
            DeleteRequested?.Invoke(this, EventArgs.Empty);
        }

        private void btnExport_Click(object? sender, EventArgs e)
        {
            if (_exportFormats.Count == 0)
            {
                ShowInfo(NoExportFormats);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "Export games";
                dialog.Filter = BuildExportFilter(_exportFormats);
                dialog.FilterIndex = 1;
                dialog.FileName = "games";

                // The extension comes from whichever filter entry is chosen, so a user who
                // switches format in the dialog does not end up with a JSON file named .csv.
                dialog.AddExtension = true;
                dialog.OverwritePrompt = true;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                // FilterIndex counts from one, and the dialog can hand back an index outside
                // the list if the filter string was rejected, so it is clamped before use.
                int index = dialog.FilterIndex - 1;
                if (index < 0 || index >= _exportFormats.Count)
                {
                    index = 0;
                }

                ExportRequested?.Invoke(
                    this,
                    new ExportRequestedEventArgs(_exportFormats[index].Name, dialog.FileName));
            }
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        /// <summary>
        /// Fills the two filters whose entries never come from the database.
        /// </summary>
        private void FillFixedFilters()
        {
            _fillingFilters = true;
            try
            {
                cmbStatus.Items.Clear();
                cmbStatus.Items.Add(new StatusOption("Any play state", null));
                cmbStatus.Items.Add(new StatusOption("Backlog", PlayStatus.Backlog));
                cmbStatus.Items.Add(new StatusOption("Playing", PlayStatus.Playing));
                cmbStatus.Items.Add(new StatusOption("Finished", PlayStatus.Finished));
                cmbStatus.SelectedIndex = 0;

                cmbSort.Items.Clear();
                cmbSort.Items.Add(new SortOption("Name (A-Z)", GameSortField.Name, false));
                cmbSort.Items.Add(new SortOption("Name (Z-A)", GameSortField.Name, true));
                cmbSort.Items.Add(new SortOption("Score (high to low)", GameSortField.Score, true));
                cmbSort.Items.Add(new SortOption("Score (low to high)", GameSortField.Score, false));
                cmbSort.SelectedIndex = 0;

                cmbGenre.Items.Clear();
                cmbGenre.Items.Add(AnyGenre);
                cmbGenre.SelectedIndex = 0;

                cmbPlatform.Items.Clear();
                cmbPlatform.Items.Add(AnyPlatform);
                cmbPlatform.SelectedIndex = 0;
            }
            finally
            {
                _fillingFilters = false;
            }
        }

        /// <summary>
        /// Replaces the entries of a lookup filter, keeping its "any" entry first and
        /// selected. The selection is deliberately reset rather than preserved: the previous
        /// choice may no longer exist in the new list, and silently keeping a filter the user
        /// can no longer see would be worse than starting from a known state.
        /// </summary>
        private void FillLookup(ComboBox combo, string anyEntry, IReadOnlyList<string> values)
        {
            _fillingFilters = true;
            try
            {
                combo.Items.Clear();
                combo.Items.Add(anyEntry);

                if (values != null)
                {
                    foreach (string value in values)
                    {
                        combo.Items.Add(value);
                    }
                }

                combo.SelectedIndex = 0;
            }
            finally
            {
                _fillingFilters = false;
            }
        }

        /// <summary>
        /// The chosen entry of a lookup filter, or <c>null</c> when the "any" entry at the
        /// top is selected.
        /// </summary>
        private static string? SelectedLookup(ComboBox combo)
        {
            return combo.SelectedIndex <= 0 ? null : combo.SelectedItem as string;
        }

        /// <summary>
        /// Builds a save dialog filter string out of the formats the presenter published.
        /// </summary>
        private static string BuildExportFilter(IReadOnlyList<ExportFormat> formats)
        {
            StringBuilder filter = new StringBuilder();

            for (int i = 0; i < formats.Count; i++)
            {
                if (i > 0)
                {
                    filter.Append('|');
                }

                ExportFormat format = formats[i];
                filter.Append(format.Name)
                      .Append(" file (*")
                      .Append(format.FileExtension)
                      .Append(")|*")
                      .Append(format.FileExtension);
            }

            return filter.ToString();
        }

        /// <summary>
        /// The wording shown for a play state. Written out rather than taken from the enum
        /// name so that renaming a member cannot silently change what the screen says.
        /// </summary>
        private static string DescribeStatus(PlayStatus status)
        {
            switch (status)
            {
                case PlayStatus.Playing:
                    return "Playing";
                case PlayStatus.Finished:
                    return "Finished";
                default:
                    return "Backlog";
            }
        }

        /// <summary>
        /// Joins a game's platforms for display in one label. A game can carry more than
        /// one platform; today's catalogue has exactly one per game, so the joined text
        /// looks the same as before. A missing or empty list renders as an empty string
        /// rather than a null-reference failure.
        /// </summary>
        private static string JoinPlatforms(IReadOnlyList<string> platforms)
        {
            return platforms is null || platforms.Count == 0
                ? string.Empty
                : string.Join(", ", platforms);
        }

        /// <summary>One entry of the play state filter.</summary>
        private sealed class StatusOption
        {
            private readonly string _label;

            public StatusOption(string label, PlayStatus? value)
            {
                _label = label;
                Value = value;
            }

            /// <summary>The state to filter by, or <c>null</c> for every state.</summary>
            public PlayStatus? Value { get; }

            public override string ToString() => _label;
        }

        /// <summary>One entry of the sort filter: a column and a direction together.</summary>
        private sealed class SortOption
        {
            private readonly string _label;

            public SortOption(string label, GameSortField field, bool descending)
            {
                _label = label;
                Field = field;
                Descending = descending;
            }

            /// <summary>Column to order by.</summary>
            public GameSortField Field { get; }

            /// <summary><c>true</c> to order from high to low.</summary>
            public bool Descending { get; }

            public override string ToString() => _label;
        }

        /// <summary>Adapts a <see cref="Game"/> to the two-line row the list draws.</summary>
        private sealed class GameRow : IGameListItem
        {
            private readonly Game _game;

            public GameRow(Game game) => _game = game;

            public int Id => _game.Id;

            public string PrimaryText => _game.Name;

            public string SecondaryText => string.Join(
                "  -  ",
                Parts(JoinPlatforms(_game.Platforms), _game.Genre));

            public double? Score => _game.Score;

            public override string ToString() => _game.Name;

            private static IEnumerable<string> Parts(params string?[] values)
            {
                foreach (string? value in values)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        yield return value;
                    }
                }
            }
        }
    }
}
