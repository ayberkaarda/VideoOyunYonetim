using System;
using System.Collections.Generic;
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
    /// Exercises <see cref="ReviewGamePresenter"/> against a substituted view, catalogue and
    /// review service.
    /// </summary>
    public class ReviewGamePresenterTests
    {
        private readonly IReviewGameView _view = Substitute.For<IReviewGameView>();
        private readonly IGameService _games = Substitute.For<IGameService>();
        private readonly IReviewService _reviews = Substitute.For<IReviewService>();
        private readonly ReviewGamePresenter _presenter;

        public ReviewGamePresenterTests()
        {
            _presenter = new ReviewGamePresenter(
                _view, _games, _reviews, NullLogger<ReviewGamePresenter>.Instance);
        }

        [Fact]
        public void Constructor_NullView_ThrowsArgumentNullException()
        {
            Action act = () => new ReviewGamePresenter(
                null, _games, _reviews, NullLogger<ReviewGamePresenter>.Instance);

            act.Should().Throw<ArgumentNullException>().WithParameterName("view");
        }

        [Fact]
        public void Constructor_NullGameService_ThrowsArgumentNullException()
        {
            Action act = () => new ReviewGamePresenter(
                _view, null, _reviews, NullLogger<ReviewGamePresenter>.Instance);

            act.Should().Throw<ArgumentNullException>().WithParameterName("games");
        }

        [Fact]
        public void Constructor_NullReviewService_ThrowsArgumentNullException()
        {
            Action act = () => new ReviewGamePresenter(
                _view, _games, null, NullLogger<ReviewGamePresenter>.Instance);

            act.Should().Throw<ArgumentNullException>().WithParameterName("reviews");
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            Action act = () => new ReviewGamePresenter(_view, _games, _reviews, null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public void Loaded_FillsThePickerWithEveryGameAndItsIdentity()
        {
            List<Game> games = new List<Game>
            {
                new Game { Id = 1, Name = "Celeste" },
                new Game { Id = 2, Name = "Hollow Knight" },
            };
            _games.SearchAsync(
                    Arg.Any<GameFilter>(), Arg.Any<int>(), Arg.Any<int>(),
                    Arg.Any<GameSortField>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new PagedResult<Game>(games, games.Count, 1, 1000)));

            RaiseLoaded();

            _view.Received(1).SetGames(Arg.Is<IReadOnlyList<GameListItem>>(items =>
                items.Count == 2 &&
                items[0].Id == 1 && items[0].Name == "Celeste" &&
                items[1].Id == 2 && items[1].Name == "Hollow Knight"));
        }

        [Fact]
        public void Loaded_ServiceThrowsDataAccessException_ShowsAnUnderstandableMessage()
        {
            _games.SearchAsync(
                    Arg.Any<GameFilter>(), Arg.Any<int>(), Arg.Any<int>(),
                    Arg.Any<GameSortField>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Throws(new DataAccessException("boom"));

            RaiseLoaded();

            _view.Received(1).ShowError(Messages.DatabaseUnreachable);
        }

        [Fact]
        public void SaveRequested_NoGameChosen_ShowsAnErrorAndDoesNotCallTheService()
        {
            _view.SelectedGameId.Returns((int?)null);

            RaiseSave();

            _view.Received(1).ShowError(Arg.Is<string>(m => m.Contains("select a game")));
            _reviews.DidNotReceiveWithAnyArgs().AddAsync(default, default, default);
        }

        [Fact]
        public void SaveRequested_RejectedByValidation_ShowsFieldErrors()
        {
            _view.SelectedGameId.Returns(4);
            _view.ReviewText.Returns(string.Empty);
            _reviews.AddAsync(4, null, string.Empty, Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result.Invalid(new ValidationError(nameof(Review.Body), "Review text is required."))));

            RaiseSave();

            _view.Received(1).ShowFieldError(nameof(Review.Body), "Review text is required.");
            _view.DidNotReceive().ClearReviewText();
        }

        [Fact]
        public void SaveRequested_ServiceAccepts_ClearsTheTextAndShowsInfo()
        {
            _view.SelectedGameId.Returns(4);
            _view.ReviewText.Returns("Great game.");
            _reviews.AddAsync(4, null, "Great game.", Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result.Success()));

            RaiseSave();

            _view.Received(1).ClearReviewText();
            _view.Received(1).ShowInfo(Arg.Any<string>());
        }

        [Fact]
        public void SaveRequested_ServiceThrowsDataAccessException_ShowsAnUnderstandableMessage()
        {
            _view.SelectedGameId.Returns(4);
            _reviews.AddAsync(Arg.Any<int>(), Arg.Any<double?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Throws(new DataAccessException("boom"));

            RaiseSave();

            _view.Received(1).ShowError(Messages.DatabaseUnreachable);
        }

        private void RaiseLoaded() => _view.Loaded += Raise.Event();

        private void RaiseSave() => _view.SaveRequested += Raise.Event();
    }
}
