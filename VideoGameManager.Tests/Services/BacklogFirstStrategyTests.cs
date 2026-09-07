using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using VideoGameManager.Data;
using VideoGameManager.Domain;
using VideoGameManager.Services;
using VideoGameManager.Tests.TestDoubles;
using Xunit;

namespace VideoGameManager.Tests.Services
{
    public class BacklogFirstStrategyTests
    {
        private readonly IGameRepository _games = Substitute.For<IGameRepository>();
        private readonly List<GameFilter> _filters = new List<GameFilter>();

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            Action act = () => new BacklogFirstStrategy(null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("games");
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(10.1)]
        [InlineData(double.NaN)]
        public void Constructor_ThresholdOutsideTheScoreRange_ThrowsArgumentOutOfRangeException(double threshold)
        {
            Action act = () => new BacklogFirstStrategy(_games, threshold);

            act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("minimumScore");
        }

        [Fact]
        public void Constructor_NoThresholdGiven_UsesTheBottomOfTheScoreRange()
        {
            BacklogFirstStrategy strategy = new BacklogFirstStrategy(_games);

            strategy.MinimumScore.Should().Be(ScoreRange.Min);
        }

        [Fact]
        public void Name_IsTheNameTheStrategyIsSelectedBy()
        {
            BacklogFirstStrategy strategy = new BacklogFirstStrategy(_games);

            strategy.Name.Should().Be("BacklogFirst");
            strategy.Name.Should().Be(BacklogFirstStrategy.StrategyName);
        }

        [Fact]
        public void Name_CarriesNoSurroundingSpace()
        {
            // The service that routes by name trims what it is given but not what a strategy
            // reports, so a padded name would register and then never be selectable.
            BacklogFirstStrategy.StrategyName.Should().Be(BacklogFirstStrategy.StrategyName.Trim());
        }

        [Fact]
        public async Task PickAsync_BacklogHoldsAnEligibleGame_SuggestsIt()
        {
            GiveCatalogue(
                new Game { Name = "Hollow Knight", Score = 9.0, Status = PlayStatus.Backlog },
                new Game { Name = "Celeste", Score = 9.4, Status = PlayStatus.Finished });
            BacklogFirstStrategy strategy = new BacklogFirstStrategy(_games);

            Game recommendation = await strategy.PickAsync();

            recommendation.Name.Should().Be("Hollow Knight");
        }

        [Fact]
        public async Task PickAsync_FilterCarriesBothTheBacklogStatusAndTheThreshold()
        {
            GiveCatalogue(new Game { Name = "Hollow Knight", Score = 9.0, Status = PlayStatus.Backlog });
            BacklogFirstStrategy strategy = new BacklogFirstStrategy(_games, 8.5);

            await strategy.PickAsync();

            GameFilter filter = _filters.Should().ContainSingle().Subject;
            filter.Should().Be(new GameFilter(MinScore: 8.5, Status: PlayStatus.Backlog));
        }

        [Fact]
        public async Task PickAsync_ThresholdAtTheBottomOfTheRange_LeavesTheScoreOutOfTheFilter()
        {
            GiveCatalogue(new Game { Name = "Hollow Knight", Score = 9.0, Status = PlayStatus.Backlog });
            BacklogFirstStrategy strategy = new BacklogFirstStrategy(_games, ScoreRange.Min);

            await strategy.PickAsync();

            GameFilter filter = _filters.Should().ContainSingle().Subject;
            filter.MinScore.Should().BeNull();
            filter.Should().Be(new GameFilter(Status: PlayStatus.Backlog));
        }

        [Fact]
        public async Task PickAsync_NoThresholdSet_CanSuggestABacklogGameNobodyHasRated()
        {
            // The point of the strategy: a game that has not been played is the one least likely
            // to carry a score, so a threshold that quietly required one would empty the backlog.
            GiveCatalogue(
                new Game { Name = "Bought In A Sale", Score = null, Status = PlayStatus.Backlog },
                new Game { Name = "Celeste", Score = 9.4, Status = PlayStatus.Finished });
            BacklogFirstStrategy strategy = new BacklogFirstStrategy(_games);

            Game recommendation = await strategy.PickAsync();

            recommendation.Should().NotBeNull();
            recommendation.Name.Should().Be("Bought In A Sale");
            _filters.Should().ContainSingle();
        }

        [Theory]
        [InlineData(PlayStatus.Playing)]
        [InlineData(PlayStatus.Finished)]
        public async Task PickAsync_NothingIsWaitingInTheBacklog_FallsBackToAnUnfilteredPick(PlayStatus status)
        {
            GiveCatalogue(new Game { Name = "Celeste", Score = 9.4, Status = status });
            BacklogFirstStrategy strategy = new BacklogFirstStrategy(_games, 7.0);

            Game recommendation = await strategy.PickAsync();

            recommendation.Name.Should().Be("Celeste");
            _filters.Should().HaveCount(2);
            _filters[0].Status.Should().Be(PlayStatus.Backlog);
            _filters[1].Should().Be(new GameFilter(MinScore: 7.0));
        }

        [Fact]
        public async Task PickAsync_BacklogHoldsOnlyGamesBelowTheThreshold_FallsBackToAnUnfilteredPick()
        {
            GiveCatalogue(
                new Game { Name = "A Dusty Purchase", Score = 4.0, Status = PlayStatus.Backlog },
                new Game { Name = "Celeste", Score = 9.4, Status = PlayStatus.Finished });
            BacklogFirstStrategy strategy = new BacklogFirstStrategy(_games, 8.0);

            Game recommendation = await strategy.PickAsync();

            recommendation.Name.Should().Be("Celeste");
            _filters.Should().HaveCount(2);
        }

        [Fact]
        public async Task PickAsync_EmptyCatalogue_SuggestsNothing()
        {
            GiveCatalogue();
            BacklogFirstStrategy strategy = new BacklogFirstStrategy(_games);

            Game recommendation = await strategy.PickAsync();

            recommendation.Should().BeNull();
        }

        [Fact]
        public async Task PickAsync_PassesTheTokenOn()
        {
            CancellationToken ct = new CancellationTokenSource().Token;
            GiveCatalogue(new Game { Name = "Hollow Knight", Score = 9.0, Status = PlayStatus.Backlog });
            BacklogFirstStrategy strategy = new BacklogFirstStrategy(_games);

            await strategy.PickAsync(ct);

            await _games.Received(1).GetRandomAsync(Arg.Any<GameFilter>(), ct);
        }

        [Fact]
        public async Task PickAsync_RepositoryThrowsProviderFailure_ThrowsDataAccessException()
        {
            SqlException provider = SqlExceptionFactory.Create();
            _games.GetRandomAsync(Arg.Any<GameFilter>(), Arg.Any<CancellationToken>()).Throws(provider);
            BacklogFirstStrategy strategy = new BacklogFirstStrategy(_games);

            Func<Task> act = () => strategy.PickAsync();

            (await act.Should().ThrowAsync<DataAccessException>())
                .And.InnerException.Should().BeSameAs(provider);
        }

        [Fact]
        public async Task PickAsync_RepositoryThrowsSomethingElse_LetsItTravelUnchanged()
        {
            InvalidOperationException unrelated = new InvalidOperationException("not a provider failure");
            _games.GetRandomAsync(Arg.Any<GameFilter>(), Arg.Any<CancellationToken>()).Throws(unrelated);
            BacklogFirstStrategy strategy = new BacklogFirstStrategy(_games);

            Func<Task> act = () => strategy.PickAsync();

            (await act.Should().ThrowAsync<InvalidOperationException>()).And.Should().BeSameAs(unrelated);
        }

        /// <summary>
        /// Makes the repository behave like the real query: it hands back a game drawn from those
        /// that carry the status it was asked for, if any, and that reach the threshold. Every
        /// filter it is handed is recorded so a test can assert what actually reached it.
        /// </summary>
        private void GiveCatalogue(params Game[] catalogue)
        {
            Random draw = new Random(20260907);

            _games.GetRandomAsync(Arg.Any<GameFilter>(), Arg.Any<CancellationToken>()).Returns(call =>
            {
                GameFilter filter = call.Arg<GameFilter>();
                _filters.Add(filter);

                // Mirrors the optional predicates the real query uses: with no threshold every
                // game is a candidate, and with one the comparison itself removes the games
                // that carry no score.
                List<Game> qualifying = catalogue
                    .Where(game => !filter.Status.HasValue || game.Status == filter.Status.Value)
                    .Where(game => !filter.MinScore.HasValue
                        || (game.Score.HasValue && game.Score.Value >= filter.MinScore.Value))
                    .ToList();

                return Task.FromResult(qualifying.Count == 0
                    ? null
                    : qualifying[draw.Next(qualifying.Count)]);
            });
        }
    }
}
