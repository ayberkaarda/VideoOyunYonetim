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
    /// Exercises <see cref="RecommendationPresenter"/> against a substituted view and
    /// recommendation service.
    /// </summary>
    public class RecommendationPresenterTests
    {
        private readonly IRecommendationView _view = Substitute.For<IRecommendationView>();
        private readonly IRecommendationService _recommendations = Substitute.For<IRecommendationService>();
        private readonly RecommendationPresenter _presenter;

        public RecommendationPresenterTests()
        {
            _presenter = new RecommendationPresenter(
                _view, _recommendations, NullLogger<RecommendationPresenter>.Instance);
        }

        [Fact]
        public void Constructor_NullView_ThrowsArgumentNullException()
        {
            Action act = () => new RecommendationPresenter(
                null!, _recommendations, NullLogger<RecommendationPresenter>.Instance);

            act.Should().Throw<ArgumentNullException>().WithParameterName("view");
        }

        [Fact]
        public void Constructor_NullRecommendationService_ThrowsArgumentNullException()
        {
            Action act = () => new RecommendationPresenter(_view, null!, NullLogger<RecommendationPresenter>.Instance);

            act.Should().Throw<ArgumentNullException>().WithParameterName("recommendations");
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            Action act = () => new RecommendationPresenter(_view, _recommendations, null!);

            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public void Loaded_FillsThePickerFromTheService()
        {
            IReadOnlyList<string> strategies = new List<string> { "Random", "GenreWeighted" };
            _recommendations.AvailableStrategies.Returns(strategies);

            RaiseLoaded();

            _view.Received(1).Strategies = strategies;
        }

        [Fact]
        public void RecommendationRequested_ServiceFindsAGame_ShowsIt()
        {
            Game recommended = SampleGame(3);
            _view.SelectedStrategy.Returns("Random");
            _recommendations.RecommendAsync("Random", Arg.Any<CancellationToken>()).Returns(Task.FromResult<Game?>(recommended));

            RaiseRecommend();

            _view.Received(1).ShowGame(recommended);
        }

        [Fact]
        public void RecommendationRequested_NothingQualifies_ShowsEmptyPickAndAnInfoMessage()
        {
            _recommendations.RecommendAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Game?>(null));

            RaiseRecommend();

            _view.Received(1).ShowGame(null!);
            _view.Received(1).ShowInfo(Arg.Any<string>());
        }

        [Fact]
        public void RecommendationRequested_IsBusy_IsSetTrueThenFalse()
        {
            List<bool> busyStates = new List<bool>();
            _view.When(v => v.IsBusy = Arg.Any<bool>()).Do(call => busyStates.Add(call.Arg<bool>()));
            _recommendations.RecommendAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Game?>(SampleGame(1)));

            RaiseRecommend();

            busyStates.Should().Equal(true, false);
        }

        [Fact]
        public void RecommendationRequested_ServiceThrowsDataAccessException_ShowsAnUnderstandableMessage()
        {
            _recommendations.RecommendAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Throws(new DataAccessException("boom"));

            RaiseRecommend();

            _view.Received(1).ShowLoadError(Messages.DatabaseUnreachable);
        }

        [Fact]
        public void RecommendationRequested_ServiceThrowsSomethingUnexpected_ShowsTheGenericMessage()
        {
            _recommendations.RecommendAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Throws(new InvalidOperationException("boom"));

            RaiseRecommend();

            _view.Received(1).ShowLoadError(Messages.Unexpected);
        }

        private void RaiseLoaded() => _view.Loaded += Raise.Event();

        private void RaiseRecommend() => _view.RecommendationRequested += Raise.Event();

        private static Game SampleGame(int id) => new Game
        {
            Id = id,
            Name = "Game " + id,
            Genre = "Action",
            Platforms = new List<string> { "PC" },
            Score = 8.0,
        };
    }
}
