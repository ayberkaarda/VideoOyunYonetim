using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using VideoGameManager.Domain;
using VideoGameManager.Presenters;
using VideoGameManager.Services;
using VideoGameManager.Views;
using Xunit;

namespace VideoGameManager.Tests.Presenters
{
    /// <summary>
    /// Exercises <see cref="AddGamePresenter"/> against a substituted view and service, the
    /// same way a real WinForms form and the real <see cref="GameService"/> would drive it.
    /// </summary>
    public class AddGamePresenterTests
    {
        private readonly IAddGameView _view = Substitute.For<IAddGameView>();
        private readonly IGameService _games = Substitute.For<IGameService>();
        private readonly AddGamePresenter _presenter;

        public AddGamePresenterTests()
        {
            SetValidInput();
            _presenter = new AddGamePresenter(_view, _games, NullLogger<AddGamePresenter>.Instance);
        }

        [Fact]
        public void Constructor_NullView_ThrowsArgumentNullException()
        {
            Action act = () => new AddGamePresenter(null, _games, NullLogger<AddGamePresenter>.Instance);

            act.Should().Throw<ArgumentNullException>().WithParameterName("view");
        }

        [Fact]
        public void Constructor_NullGameService_ThrowsArgumentNullException()
        {
            Action act = () => new AddGamePresenter(_view, null, NullLogger<AddGamePresenter>.Instance);

            act.Should().Throw<ArgumentNullException>().WithParameterName("games");
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            Action act = () => new AddGamePresenter(_view, _games, null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public void Save_ScorePlaceholderSelected_SendsNullScoreToTheService()
        {
            _view.ScoreText.Returns((string)null);
            _games.AddAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result<int>.Success(1)));

            RaiseSave();

            _games.Received(1).AddAsync(Arg.Is<Game>(g => g.Score == null), Arg.Any<CancellationToken>());
        }

        [Fact]
        public void Save_ScoreTextIsNotANumber_ShowsFieldErrorAndDoesNotCallTheService()
        {
            _view.ScoreText.Returns("abc");

            RaiseSave();

            _view.Received(1).ShowFieldError(nameof(Game.Score), Arg.Any<string>());
            _games.DidNotReceiveWithAnyArgs().AddAsync(default, default);
        }

        [Fact]
        public void Save_AddRejectedByValidation_ShowsFieldErrorsAndDoesNotResetInput()
        {
            _games.AddAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result<int>.Invalid(new ValidationError(nameof(Game.Name), "Name is required."))));

            RaiseSave();

            _view.Received(1).ShowFieldError(nameof(Game.Name), "Name is required.");
            _view.DidNotReceive().ResetInput();
            _view.DidNotReceive().ShowInfo(Arg.Any<string>());
        }

        [Fact]
        public void Save_AddSucceeds_ResetsInputAndShowsInfo()
        {
            _games.AddAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result<int>.Success(7)));

            RaiseSave();

            _view.Received(1).ResetInput();
            _view.Received(1).ShowInfo(Arg.Any<string>());
            _view.DidNotReceive().CloseAfterSave();
        }

        [Fact]
        public void Save_IsBusy_IsSetTrueThenFalse()
        {
            List<bool> busyStates = new List<bool>();
            _view.When(v => v.IsBusy = Arg.Any<bool>()).Do(call => busyStates.Add(call.Arg<bool>()));
            _games.AddAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result<int>.Success(1)));

            RaiseSave();

            busyStates.Should().Equal(true, false);
        }

        [Fact]
        public void Save_ServiceThrowsDataAccessException_ShowsAnUnderstandableMessage()
        {
            _games.AddAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>()).Throws(new DataAccessException("boom"));

            RaiseSave();

            _view.Received(1).ShowError(Messages.DatabaseUnreachable);
        }

        [Fact]
        public void EditRequested_GameFound_ShowsGameAndSwitchesToEditingMode()
        {
            Game game = SampleGame(5);
            _games.GetAsync(5, Arg.Any<CancellationToken>()).Returns(Task.FromResult(game));

            RaiseEdit(5);

            _view.Received(1).ShowEditing(true);
            _view.Received(1).ShowGame(game);
        }

        [Fact]
        public void EditRequested_GameNoLongerExists_ShowsLoadFailedMessage()
        {
            _games.GetAsync(9, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Game>(null));

            RaiseEdit(9);

            _view.Received(1).ShowLoadFailed(Arg.Is<string>(m => m.Contains("could not be found")));
        }

        [Fact]
        public void EditRequested_ServiceThrowsDataAccessException_ShowsAnUnderstandableMessage()
        {
            _games.GetAsync(9, Arg.Any<CancellationToken>()).Throws(new DataAccessException("boom"));

            RaiseEdit(9);

            _view.Received(1).ShowLoadFailed(Messages.DatabaseUnreachable);
        }

        [Fact]
        public void Save_AfterSuccessfulEdit_UpdatesAndClosesTheDialog()
        {
            Game loaded = SampleGame(12);
            _games.GetAsync(12, Arg.Any<CancellationToken>()).Returns(Task.FromResult(loaded));
            RaiseEdit(12);

            _games.UpdateAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result.Success()));

            RaiseSave();

            _games.Received(1).UpdateAsync(Arg.Is<Game>(g => g.Id == 12), Arg.Any<CancellationToken>());
            _view.Received(1).CloseAfterSave();
            _view.DidNotReceive().ResetInput();
        }

        [Fact]
        public void Save_UpdateRejectedByValidation_ShowsFieldErrorsAndDoesNotClose()
        {
            Game loaded = SampleGame(12);
            _games.GetAsync(12, Arg.Any<CancellationToken>()).Returns(Task.FromResult(loaded));
            RaiseEdit(12);

            _games.UpdateAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result.Invalid(new ValidationError(nameof(Game.Genre), "Genre is required."))));

            RaiseSave();

            _view.Received(1).ShowFieldError(nameof(Game.Genre), "Genre is required.");
            _view.DidNotReceive().CloseAfterSave();
        }

        private void RaiseSave() => _view.SaveRequested += Raise.Event();

        private void RaiseEdit(int gameId) => _view.EditRequested += Raise.EventWith(new GameEditRequestedEventArgs(gameId));

        private void SetValidInput()
        {
            _view.GameName.Returns("Hollow Knight");
            _view.Genre.Returns("Metroidvania");
            _view.Platforms.Returns(new List<string> { "PC" });
            _view.ScoreText.Returns("9.4");
            _view.CoverUrl.Returns((string)null);
            _view.Status.Returns(PlayStatus.Backlog);
            _view.IsFavourite.Returns(false);
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
    }
}
