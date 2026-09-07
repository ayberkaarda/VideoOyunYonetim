using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using VideoGameManager.Data;
using VideoGameManager.Presenters;
using VideoGameManager.Services;
using VideoGameManager.Views;
using Xunit;

namespace VideoGameManager.Tests.Presenters
{
    /// <summary>
    /// Exercises <see cref="StatisticsPresenter"/> against a substituted view and statistics
    /// service.
    /// </summary>
    public class StatisticsPresenterTests
    {
        private readonly IStatisticsView _view = Substitute.For<IStatisticsView>();
        private readonly IStatisticsService _statistics = Substitute.For<IStatisticsService>();
        private readonly StatisticsPresenter _presenter;

        public StatisticsPresenterTests()
        {
            _presenter = new StatisticsPresenter(_view, _statistics, NullLogger<StatisticsPresenter>.Instance);
        }

        [Fact]
        public void Constructor_NullView_ThrowsArgumentNullException()
        {
            Action act = () => new StatisticsPresenter(null!, _statistics, NullLogger<StatisticsPresenter>.Instance);

            act.Should().Throw<ArgumentNullException>().WithParameterName("view");
        }

        [Fact]
        public void Constructor_NullStatisticsService_ThrowsArgumentNullException()
        {
            Action act = () => new StatisticsPresenter(_view, null!, NullLogger<StatisticsPresenter>.Instance);

            act.Should().Throw<ArgumentNullException>().WithParameterName("statistics");
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            Action act = () => new StatisticsPresenter(_view, _statistics, null!);

            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public void Loaded_ServiceAnswers_ShowsTheFigures()
        {
            CatalogueStatistics statistics = new CatalogueStatistics(
                10, 4, 8.2, new List<GenreDistribution> { new GenreDistribution("Action", 3, 7.5) });
            _statistics.GetAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(statistics));

            RaiseLoaded();

            _view.Received(1).ShowStatistics(statistics);
        }

        [Fact]
        public void Loaded_ServiceReturnsNull_ShowsUnavailable()
        {
            _statistics.GetAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<CatalogueStatistics>(null!));

            RaiseLoaded();

            _view.Received(1).ShowUnavailable(Arg.Any<string>());
            _view.DidNotReceiveWithAnyArgs().ShowStatistics(default!);
        }

        [Fact]
        public void RefreshRequested_BehavesLikeLoaded_AsksTheServiceAgain()
        {
            CatalogueStatistics statistics = new CatalogueStatistics(0, 0, null, new List<GenreDistribution>());
            _statistics.GetAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(statistics));

            RaiseRefresh();

            _view.Received(1).ShowStatistics(statistics);
        }

        [Fact]
        public void Loaded_IsBusy_IsSetTrueThenFalse()
        {
            List<bool> busyStates = new List<bool>();
            _view.When(v => v.IsBusy = Arg.Any<bool>()).Do(call => busyStates.Add(call.Arg<bool>()));
            _statistics.GetAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new CatalogueStatistics(0, 0, null, new List<GenreDistribution>())));

            RaiseLoaded();

            busyStates.Should().Equal(true, false);
        }

        [Fact]
        public void Loaded_ServiceThrowsDataAccessException_ShowsAnUnderstandableMessage()
        {
            _statistics.GetAsync(Arg.Any<CancellationToken>()).Throws(new DataAccessException("boom"));

            RaiseLoaded();

            _view.Received(1).ShowUnavailable(Messages.DatabaseUnreachable);
        }

        [Fact]
        public void Loaded_ServiceThrowsSomethingUnexpected_ShowsTheGenericMessage()
        {
            _statistics.GetAsync(Arg.Any<CancellationToken>()).Throws(new InvalidOperationException("boom"));

            RaiseLoaded();

            _view.Received(1).ShowUnavailable(Messages.Unexpected);
        }

        private void RaiseLoaded() => _view.Loaded += Raise.Event();

        private void RaiseRefresh() => _view.RefreshRequested += Raise.Event();
    }
}
