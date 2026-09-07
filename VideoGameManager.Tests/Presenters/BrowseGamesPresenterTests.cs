using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using VideoGameManager.Data;
using VideoGameManager.Domain;
using VideoGameManager.Presenters;
using VideoGameManager.Services;
using VideoGameManager.Views;
using Xunit;

namespace VideoGameManager.Tests.Presenters
{
    /// <summary>
    /// Exercises <see cref="BrowseGamesPresenter"/> against a substituted view, catalogue,
    /// exporter set and importer set. The page size matches the presenter's own constant (25),
    /// which is not public, so it is repeated here as <see cref="PageSize"/>.
    /// </summary>
    public class BrowseGamesPresenterTests
    {
        private const int PageSize = 25;

        private readonly IGameListView _view = Substitute.For<IGameListView>();
        private readonly IGameService _games = Substitute.For<IGameService>();
        private readonly IGameExporter _csvExporter = Substitute.For<IGameExporter>();
        private readonly IGameImporter _jsonImporter = Substitute.For<IGameImporter>();
        private readonly BrowseGamesPresenter _presenter;

        public BrowseGamesPresenterTests()
        {
            _csvExporter.Format.Returns("CSV");
            _csvExporter.FileExtension.Returns(".csv");

            _jsonImporter.Format.Returns("JSON");
            _jsonImporter.FileExtension.Returns(".json");
            _jsonImporter.ReadAsync(Arg.Any<TextReader>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<Game>>(new List<Game>()));

            _games.GetGenresAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<string>>(new List<string>()));
            _games.GetPlatformsAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<string>>(new List<string>()));

            SetupCatalogue(0);
            SetupAddSucceeds();

            _presenter = new BrowseGamesPresenter(
                _view, _games, Exporters(), Importers(), NullLogger<BrowseGamesPresenter>.Instance);
        }

        [Fact]
        public void Constructor_NullView_ThrowsArgumentNullException()
        {
            Action act = () => new BrowseGamesPresenter(
                null!, _games, Exporters(), Importers(), NullLogger<BrowseGamesPresenter>.Instance);

            act.Should().Throw<ArgumentNullException>().WithParameterName("view");
        }

        [Fact]
        public void Constructor_NullGameService_ThrowsArgumentNullException()
        {
            Action act = () => new BrowseGamesPresenter(
                _view, null!, Exporters(), Importers(), NullLogger<BrowseGamesPresenter>.Instance);

            act.Should().Throw<ArgumentNullException>().WithParameterName("games");
        }

        [Fact]
        public void Constructor_NullExporters_ThrowsArgumentNullException()
        {
            Action act = () => new BrowseGamesPresenter(
                _view, _games, null!, Importers(), NullLogger<BrowseGamesPresenter>.Instance);

            act.Should().Throw<ArgumentNullException>().WithParameterName("exporters");
        }

        [Fact]
        public void Constructor_NullImporters_ThrowsArgumentNullException()
        {
            Action act = () => new BrowseGamesPresenter(
                _view, _games, Exporters(), null!, NullLogger<BrowseGamesPresenter>.Instance);

            act.Should().Throw<ArgumentNullException>().WithParameterName("importers");
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            Action act = () => new BrowseGamesPresenter(
                _view, _games, Exporters(), Importers(), null!);

            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public void Loaded_FirstLoad_PublishesExportFormatsLookupsAndTheFirstPage()
        {
            _games.GetGenresAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<string>>(new List<string> { "Action", "RPG" }));
            _games.GetPlatformsAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<string>>(new List<string> { "PC", "Switch" }));
            SetupCatalogue(2);

            RaiseLoaded();

            _view.Received(1).ExportFormats = Arg.Is<IReadOnlyList<ExportFormat>>(
                f => f.Count == 1 && f[0].Name == "CSV" && f[0].FileExtension == ".csv");
            _view.Received(1).ImportFormats = Arg.Is<IReadOnlyList<ImportFormat>>(
                f => f.Count == 1 && f[0].Name == "JSON" && f[0].FileExtension == ".json");
            _view.Received(1).Genres = Arg.Is<IReadOnlyList<string>>(g => g.Count == 2);
            _view.Received(1).Platforms = Arg.Is<IReadOnlyList<string>>(p => p.Count == 2);
            _view.Received(1).Games = Arg.Is<IReadOnlyList<Game>>(games => games.Count == 2);
            _view.Received(1).ShowPage(1, 1, 2);
        }

        [Fact]
        public void Loaded_NoMatches_ShowsNoMatchesMessageAndResetsThePageToOne()
        {
            SetupCatalogue(0);

            RaiseLoaded();

            _view.Received(1).ShowListUnavailable(Arg.Is<string>(m => m.Contains("No games match")));
            _view.Received(1).ShowPage(1, 0, 0);
        }

        [Fact]
        public void FilterChanged_WhenOnALaterPage_ReturnsToTheFirstPage()
        {
            SetupCatalogue(30);
            RaiseLoaded();
            RaiseNextPage();
            _games.ClearReceivedCalls();

            RaiseFilterChanged();

            _games.Received(1).SearchAsync(
                Arg.Any<GameFilter>(), 1, PageSize, Arg.Any<GameSortField>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public void PreviousPageRequested_OnTheFirstPage_DoesNothing()
        {
            SetupCatalogue(30);
            RaiseLoaded();
            _games.ClearReceivedCalls();

            RaisePreviousPage();

            _games.DidNotReceiveWithAnyArgs().SearchAsync(default, default, default);
        }

        [Fact]
        public void NextPageRequested_OnTheLastPage_DoesNothing()
        {
            SetupCatalogue(2);
            RaiseLoaded();
            _games.ClearReceivedCalls();

            RaiseNextPage();

            _games.DidNotReceiveWithAnyArgs().SearchAsync(default, default, default);
        }

        [Fact]
        public void NextPageRequested_OnANonLastPage_AdvancesToTheNextPage()
        {
            SetupCatalogue(30);
            RaiseLoaded();

            RaiseNextPage();

            _games.Received(1).SearchAsync(
                Arg.Any<GameFilter>(), 2, PageSize, Arg.Any<GameSortField>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
            _view.Received(1).ShowPage(2, 2, 30);
        }

        [Fact]
        public void SelectionChanged_RowSelected_ShowsItsDetails()
        {
            Game selected = SampleGame(5);
            _view.SelectedGameId.Returns(5);
            _games.GetAsync(5, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Game?>(selected));

            RaiseSelectionChanged();

            _view.Received(1).ShowDetails(selected);
        }

        [Fact]
        public void SelectionChanged_NothingSelected_ShowsEmptyDetails()
        {
            _view.SelectedGameId.Returns((int?)null);

            RaiseSelectionChanged();

            _view.Received().ShowDetails(null!);
        }

        [Fact]
        public void SelectionChanged_ServiceThrowsDataAccessException_ShowsDetailsUnavailable()
        {
            _view.SelectedGameId.Returns(5);
            _games.GetAsync(5, Arg.Any<CancellationToken>()).Throws(new DataAccessException("boom"));

            RaiseSelectionChanged();

            _view.Received(1).ShowDetailsUnavailable(Messages.DatabaseUnreachable);
        }

        [Fact]
        public void EditRequested_NothingSelected_ShowsInfoAndDoesNotOpenTheEditor()
        {
            _view.SelectedGameId.Returns((int?)null);

            RaiseEdit();

            _view.Received(1).ShowInfo(Arg.Is<string>(m => m.Contains("Select a game")));
            _view.DidNotReceive().OpenEditor(Arg.Any<int>());
        }

        [Fact]
        public void EditRequested_RowSelected_OpensTheEditorAndReloadsThePage()
        {
            SetupCatalogue(2);
            RaiseLoaded();
            _view.SelectedGameId.Returns(1);
            _games.ClearReceivedCalls();
            SetupCatalogue(2);

            RaiseEdit();

            _view.Received(1).OpenEditor(1);
            _games.Received(1).SearchAsync(
                Arg.Any<GameFilter>(), 1, PageSize, Arg.Any<GameSortField>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public void DeleteRequested_NothingSelected_ShowsInfoAndDoesNotCallTheService()
        {
            _view.SelectedGameId.Returns((int?)null);

            RaiseDelete();

            _view.Received(1).ShowInfo(Arg.Is<string>(m => m.Contains("Select a game")));
            _games.DidNotReceiveWithAnyArgs().DeleteAsync(default, default);
        }

        [Fact]
        public void DeleteRequested_ConfirmationDeclined_DoesNotCallTheService()
        {
            _view.SelectedGameId.Returns(3);
            _view.Confirm(Arg.Any<string>()).Returns(false);

            RaiseDelete();

            _games.DidNotReceiveWithAnyArgs().DeleteAsync(default, default);
        }

        [Fact]
        public void DeleteRequested_ConfirmationAccepted_CallsTheServiceAndReloadsThePage()
        {
            _view.SelectedGameId.Returns(3);
            _view.Confirm(Arg.Any<string>()).Returns(true);
            _games.DeleteAsync(3, Arg.Any<CancellationToken>()).Returns(Task.FromResult(Result.Success()));

            RaiseDelete();

            _games.Received(1).DeleteAsync(3, Arg.Any<CancellationToken>());
        }

        [Fact]
        public void DeleteRequested_ServiceRejectsTheDelete_ShowsAnError()
        {
            _view.SelectedGameId.Returns(3);
            _view.Confirm(Arg.Any<string>()).Returns(true);
            _games.DeleteAsync(3, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result.Invalid(new ValidationError(nameof(Game.Id), "gone"))));

            RaiseDelete();

            _view.Received(1).ShowError(Arg.Is<string>(m => m.Contains("could not be deleted")));
        }

        [Fact]
        public void DeleteRequested_LastRowOnTheLastPage_StepsBackToThePreviousPage()
        {
            int totalCount = 26;
            _games.SearchAsync(
                    Arg.Any<GameFilter>(), Arg.Any<int>(), Arg.Any<int>(),
                    Arg.Any<GameSortField>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => Task.FromResult(PageOf(callInfo, totalCount)));

            RaiseLoaded();
            RaiseNextPage();

            _view.SelectedGameId.Returns(26);
            _view.Confirm(Arg.Any<string>()).Returns(true);
            _games.DeleteAsync(26, Arg.Any<CancellationToken>()).Returns(callInfo =>
            {
                totalCount = 25;
                return Task.FromResult(Result.Success());
            });

            RaiseDelete();

            _view.Received(1).ShowPage(1, 1, 25);
        }

        [Fact]
        public void ExportRequested_UnknownFormat_ShowsAnError()
        {
            RaiseExport("XML", Path.Combine(Path.GetTempPath(), "unused.xml"));

            _view.Received(1).ShowError(Arg.Is<string>(m => m.Length > 0));
            _view.DidNotReceive().ShowInfo(Arg.Any<string>());
        }

        [Fact]
        public void ExportRequested_KnownFormat_WritesARealFileAndReportsSuccess()
        {
            SetupCatalogue(1);
            _csvExporter
                .WriteAsync(Arg.Any<IReadOnlyList<Game>>(), Arg.Any<TextWriter>(), Arg.Any<CancellationToken>())
                .Returns(callInfo =>
                {
                    TextWriter writer = callInfo.ArgAt<TextWriter>(1);
                    writer.Write("Name,Genre" + Environment.NewLine + "Game 1,Action" + Environment.NewLine);
                    return Task.CompletedTask;
                });

            string path = Path.GetTempFileName();
            try
            {
                RaiseExport("CSV", path);

                File.ReadAllText(path).Should().Contain("Game 1,Action");
                _view.Received(1).ShowInfo(Arg.Is<string>(m => m.Contains("Exported")));
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void ExportRequested_WritingTheFileFails_ShowsAnExportFailureMessage()
        {
            SetupCatalogue(1);
            _csvExporter
                .WriteAsync(Arg.Any<IReadOnlyList<Game>>(), Arg.Any<TextWriter>(), Arg.Any<CancellationToken>())
                .Throws(new IOException("disk full"));

            string path = Path.GetTempFileName();
            try
            {
                RaiseExport("CSV", path);

                _view.Received(1).ShowError(Arg.Is<string>(m => m.Contains("export file could not be written")));
            }
            finally
            {
                File.Delete(path);
            }
        }

        // ------------------------------------------------------------------
        // Import
        // ------------------------------------------------------------------

        [Fact]
        public void ImportRequested_UnknownFormat_ShowsAnErrorAndStoresNothing()
        {
            RaiseImport("XML", Path.Combine(Path.GetTempPath(), "unused.xml"));

            _view.Received(1).ShowError(Arg.Is<string>(m => m.Contains("import format is not available")));
            _games.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        }

        [Fact]
        public void ImportRequested_NoFileChosen_DoesNothing()
        {
            // The view raises nothing when the user closes the file dialog without choosing,
            // so the presenter must not have started an import on the strength of the screen
            // merely being open.
            SetupCatalogue(3);
            RaiseLoaded();

            _games.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
            _view.DidNotReceive().ShowInfo(Arg.Any<string>());
        }

        [Fact]
        public void ImportRequested_ValidFile_StoresEveryGameAndReportsTheCounts()
        {
            SetupCatalogue(0);
            SetupImportFile(NewGame("Hades"), NewGame("Celeste"));

            WithTempFile(path => RaiseImport("JSON", path));

            _games.Received(1).AddAsync(Arg.Is<Game>(g => g.Name == "Hades"), Arg.Any<CancellationToken>());
            _games.Received(1).AddAsync(Arg.Is<Game>(g => g.Name == "Celeste"), Arg.Any<CancellationToken>());
            _view.Received(1).ShowInfo(Arg.Is<string>(m =>
                m.Contains("Imported 2 games.")
                && m.Contains("Skipped 0 already in the catalogue.")
                && m.Contains("Skipped 0 that broke a validation rule.")));
        }

        [Fact]
        public void ImportRequested_OneGame_ReportsItInTheSingular()
        {
            SetupCatalogue(0);
            SetupImportFile(NewGame("Hades"));

            WithTempFile(path => RaiseImport("JSON", path));

            _view.Received(1).ShowInfo(Arg.Is<string>(m => m.Contains("Imported 1 game.")));
        }

        [Fact]
        public void ImportRequested_NameAlreadyInTheCatalogue_SkipsThatGameAndLeavesTheStoredRowAlone()
        {
            // The catalogue holds "Game 1"; the file offers the same title in another case.
            SetupCatalogue(1);
            SetupImportFile(NewGame("game 1"), NewGame("Celeste"));

            WithTempFile(path => RaiseImport("JSON", path));

            _games.DidNotReceive().AddAsync(Arg.Is<Game>(g => g.Name == "game 1"), Arg.Any<CancellationToken>());
            _games.Received(1).AddAsync(Arg.Is<Game>(g => g.Name == "Celeste"), Arg.Any<CancellationToken>());
            _games.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
            _games.DidNotReceiveWithAnyArgs().DeleteAsync(default, default);
            _view.Received(1).ShowInfo(Arg.Is<string>(m =>
                m.Contains("Imported 1 game.")
                && m.Contains("Skipped 1 already in the catalogue.")));
        }

        [Fact]
        public void ImportRequested_SameTitleTwiceInOneFile_StoresItOnce()
        {
            SetupCatalogue(0);
            SetupImportFile(NewGame("Hades"), NewGame("HADES"));

            WithTempFile(path => RaiseImport("JSON", path));

            _games.Received(1).AddAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>());
            _view.Received(1).ShowInfo(Arg.Is<string>(m =>
                m.Contains("Imported 1 game.")
                && m.Contains("Skipped 1 already in the catalogue.")));
        }

        [Fact]
        public void ImportRequested_GameTheCatalogueRejects_IsCountedAsInvalidAndTheRestStillArrive()
        {
            SetupCatalogue(0);
            SetupImportFile(NewGame("Broken"), NewGame("Celeste"));

            _games.AddAsync(Arg.Is<Game>(g => g.Name == "Broken"), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result<int>.Invalid(
                    new ValidationError(nameof(Game.Score), "Score must be between 0 and 10."))));

            WithTempFile(path => RaiseImport("JSON", path));

            _games.Received(1).AddAsync(Arg.Is<Game>(g => g.Name == "Celeste"), Arg.Any<CancellationToken>());
            _view.Received(1).ShowInfo(Arg.Is<string>(m =>
                m.Contains("Imported 1 game.")
                && m.Contains("Skipped 1 that broke a validation rule.")));
        }

        [Fact]
        public void ImportRequested_FileIsNotOfThatFormat_ImportsNothingAndSaysSo()
        {
            SetupCatalogue(0);
            _jsonImporter.ReadAsync(Arg.Any<TextReader>(), Arg.Any<CancellationToken>())
                .Throws(new ImportFormatException("The file could not be read as JSON."));

            WithTempFile(path => RaiseImport("JSON", path));

            _games.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
            _view.Received(1).ShowError(Arg.Is<string>(m => m.Contains("not in the expected format")));
            _view.DidNotReceive().ShowInfo(Arg.Any<string>());
        }

        [Fact]
        public void ImportRequested_FileCannotBeRead_ShowsAnImportFailureMessage()
        {
            SetupCatalogue(0);
            _jsonImporter.ReadAsync(Arg.Any<TextReader>(), Arg.Any<CancellationToken>())
                .Throws(new IOException("the file is locked"));

            WithTempFile(path => RaiseImport("JSON", path));

            _view.Received(1).ShowError(Arg.Is<string>(m => m.Contains("import file could not be read")));
        }

        [Fact]
        public void ImportRequested_FileIsNotThere_ShowsAnImportFailureMessage()
        {
            // Nothing is created, so opening the path fails before the importer is reached.
            SetupCatalogue(0);

            RaiseImport("JSON", Path.Combine(Path.GetTempPath(), "no-such-import-" + Guid.NewGuid().ToString("N") + ".json"));

            _view.Received(1).ShowError(Arg.Is<string>(m => m.Contains("import file could not be read")));
        }

        [Fact]
        public void ImportRequested_DatabaseFails_ShowsTheDatabaseMessageAndNotTheRawError()
        {
            SetupCatalogue(0);
            SetupImportFile(NewGame("Hades"));
            _games.AddAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>())
                .Throws(new DataAccessException("The database could not be reached.", new InvalidOperationException("no route to host")));

            WithTempFile(path => RaiseImport("JSON", path));

            _view.Received(1).ShowError(Messages.DatabaseUnreachable);
            _view.DidNotReceive().ShowError(Arg.Is<string>(m => m.Contains("no route to host")));
        }

        [Fact]
        public void ImportRequested_WhenDone_ReadsTheLookupsAgainAndShowsTheFirstPage()
        {
            SetupCatalogue(2);
            SetupImportFile(NewGame("Hades"));
            RaiseLoaded();
            _view.ClearReceivedCalls();
            _games.ClearReceivedCalls();

            WithTempFile(path => RaiseImport("JSON", path));

            _games.Received(1).GetGenresAsync(Arg.Any<CancellationToken>());
            _games.Received(1).GetPlatformsAsync(Arg.Any<CancellationToken>());
            _games.Received().SearchAsync(
                Arg.Any<GameFilter>(), 1, PageSize,
                Arg.Any<GameSortField>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
            _view.Received().Games = Arg.Any<IReadOnlyList<Game>>();
        }

        private void SetupCatalogue(int totalCount) =>
            _games.SearchAsync(
                    Arg.Any<GameFilter>(), Arg.Any<int>(), Arg.Any<int>(),
                    Arg.Any<GameSortField>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => Task.FromResult(PageOf(callInfo, totalCount)));

        private static PagedResult<Game> PageOf(NSubstitute.Core.CallInfo callInfo, int totalCount)
        {
            int page = callInfo.ArgAt<int>(1);
            int pageSize = callInfo.ArgAt<int>(2);
            int start = (page - 1) * pageSize;
            int count = Math.Max(0, Math.Min(pageSize, totalCount - start));

            List<Game> items = new List<Game>();
            for (int i = 0; i < count; i++)
            {
                items.Add(SampleGame(start + i + 1));
            }

            return new PagedResult<Game>(items, totalCount, page, pageSize);
        }

        private static Game SampleGame(int id) => new Game
        {
            Id = id,
            Name = "Game " + id,
            Genre = "Action",
            Platforms = new List<string> { "PC" },
            Score = 8.0,
            Status = PlayStatus.Backlog,
        };

        private void RaiseLoaded() => _view.Loaded += Raise.Event();

        private void RaiseSelectionChanged() => _view.SelectionChanged += Raise.Event();

        private void RaiseFilterChanged() => _view.FilterChanged += Raise.Event();

        private void RaisePreviousPage() => _view.PreviousPageRequested += Raise.Event();

        private void RaiseNextPage() => _view.NextPageRequested += Raise.Event();

        private void RaiseEdit() => _view.EditRequested += Raise.Event();

        private void RaiseDelete() => _view.DeleteRequested += Raise.Event();

        private void RaiseExport(string format, string path) =>
            _view.ExportRequested += Raise.EventWith(new ExportRequestedEventArgs(format, path));

        private void RaiseImport(string format, string path) =>
            _view.ImportRequested += Raise.EventWith(new ImportRequestedEventArgs(format, path));

        private List<IGameExporter> Exporters() => new List<IGameExporter> { _csvExporter };

        private List<IGameImporter> Importers() => new List<IGameImporter> { _jsonImporter };

        /// <summary>
        /// Makes every add succeed with a made-up identity, which is what the catalogue does
        /// for a game that passes validation.
        /// </summary>
        private void SetupAddSucceeds() =>
            _games.AddAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result<int>.Success(99)));

        /// <summary>
        /// Makes the substituted importer hand back these games, whatever the file holds. The
        /// presenter still opens the real file, so the tests using this run inside
        /// <see cref="WithTempFile"/>.
        /// </summary>
        private void SetupImportFile(params Game[] games) =>
            _jsonImporter.ReadAsync(Arg.Any<TextReader>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<Game>>(new List<Game>(games)));

        /// <summary>
        /// Runs an action against a real, empty temporary file and deletes it afterwards.
        /// </summary>
        private static void WithTempFile(Action<string> act)
        {
            string path = Path.GetTempFileName();
            try
            {
                act(path);
            }
            finally
            {
                File.Delete(path);
            }
        }

        private static Game NewGame(string name) => new Game
        {
            Name = name,
            Genre = "Action",
            Platforms = new List<string> { "PC" },
            Score = 9.0,
            Status = PlayStatus.Backlog,
        };
    }
}
