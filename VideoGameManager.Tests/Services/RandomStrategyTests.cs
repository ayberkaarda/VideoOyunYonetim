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
    public class RandomStrategyTests
    {
        private readonly IGameRepository _games = Substitute.For<IGameRepository>();

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            Action act = () => new RandomStrategy(null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("games");
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(10.1)]
        [InlineData(double.NaN)]
        public void Constructor_ThresholdOutsideTheScoreRange_ThrowsArgumentOutOfRangeException(double threshold)
        {
            Action act = () => new RandomStrategy(_games, threshold);

            act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("minimumScore");
        }

        [Fact]
        public void Constructor_NoThresholdGiven_UsesTheBottomOfTheScoreRange()
        {
            RandomStrategy strategy = new RandomStrategy(_games);

            strategy.MinimumScore.Should().Be(ScoreRange.Min);
        }

        [Fact]
        public void Name_IsTheNameTheStrategyIsSelectedBy()
        {
            RandomStrategy strategy = new RandomStrategy(_games);

            strategy.Name.Should().Be("Random");
            strategy.Name.Should().Be(RandomStrategy.StrategyName);
        }

        [Fact]
        public async Task PickAsync_AsksTheRepositoryForExactlyTheConfiguredThreshold()
        {
            RandomStrategy strategy = new RandomStrategy(_games, 8.5);
            _games.GetRandomAsync(Arg.Any<double>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<Game>(null));

            await strategy.PickAsync();

            await _games.Received(1).GetRandomAsync(8.5, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task PickAsync_PassesTheTokenOn()
        {
            CancellationToken ct = new CancellationTokenSource().Token;
            RandomStrategy strategy = new RandomStrategy(_games);
            _games.GetRandomAsync(Arg.Any<double>(), ct).Returns(Task.FromResult<Game>(null));

            await strategy.PickAsync(ct);

            await _games.Received(1).GetRandomAsync(ScoreRange.Min, ct);
        }

        [Fact]
        public async Task PickAsync_CatalogueHoldsOnlyGamesBelowTheThreshold_SuggestsNothing()
        {
            GiveCatalogue(
                new Game { Name = "Barely Playable", Score = 3.0 },
                new Game { Name = "Passable", Score = 5.0 },
                new Game { Name = "Unrated", Score = null });
            RandomStrategy strategy = new RandomStrategy(_games, 8.0);

            Game recommendation = await strategy.PickAsync();

            recommendation.Should().BeNull();
        }

        [Fact]
        public async Task PickAsync_CatalogueHoldsBothSidesOfTheThreshold_NeverSuggestsOneBelowIt()
        {
            GiveCatalogue(
                new Game { Name = "Barely Playable", Score = 3.0 },
                new Game { Name = "Passable", Score = 5.0 },
                new Game { Name = "Excellent", Score = 9.0 });
            RandomStrategy strategy = new RandomStrategy(_games, 8.0);

            // Repeated because the pick is meant to be random: a single call could pass by luck
            // even if the threshold were being ignored.
            for (int attempt = 0; attempt < 20; attempt++)
            {
                Game recommendation = await strategy.PickAsync();

                recommendation.Should().NotBeNull();
                recommendation.Score.Should().BeGreaterThanOrEqualTo(8.0);
            }
        }

        [Fact]
        public async Task PickAsync_GameExactlyOnTheThreshold_Qualifies()
        {
            GiveCatalogue(new Game { Name = "Right On The Line", Score = 8.0 });
            RandomStrategy strategy = new RandomStrategy(_games, 8.0);

            Game recommendation = await strategy.PickAsync();

            recommendation.Should().NotBeNull();
            recommendation.Name.Should().Be("Right On The Line");
        }

        [Fact]
        public async Task PickAsync_EmptyCatalogue_SuggestsNothing()
        {
            GiveCatalogue();
            RandomStrategy strategy = new RandomStrategy(_games);

            Game recommendation = await strategy.PickAsync();

            recommendation.Should().BeNull();
        }

        [Fact]
        public async Task PickAsync_RepositoryThrowsProviderFailure_ThrowsDataAccessException()
        {
            SqlException provider = SqlExceptionFactory.Create();
            _games.GetRandomAsync(Arg.Any<double>(), Arg.Any<CancellationToken>()).Throws(provider);
            RandomStrategy strategy = new RandomStrategy(_games);

            Func<Task> act = () => strategy.PickAsync();

            (await act.Should().ThrowAsync<DataAccessException>())
                .And.InnerException.Should().BeSameAs(provider);
        }

        [Fact]
        public async Task PickAsync_RepositoryThrowsSomethingElse_LetsItTravelUnchanged()
        {
            InvalidOperationException unrelated = new InvalidOperationException("not a provider failure");
            _games.GetRandomAsync(Arg.Any<double>(), Arg.Any<CancellationToken>()).Throws(unrelated);
            RandomStrategy strategy = new RandomStrategy(_games);

            Func<Task> act = () => strategy.PickAsync();

            (await act.Should().ThrowAsync<InvalidOperationException>()).And.Should().BeSameAs(unrelated);
        }

        /// <summary>
        /// Makes the repository behave like the real query: it hands back a game drawn at random
        /// from those that reach the threshold it was asked for, and nothing when none do.
        /// </summary>
        private void GiveCatalogue(params Game[] catalogue)
        {
            Random draw = new Random(20260906);

            _games.GetRandomAsync(Arg.Any<double>(), Arg.Any<CancellationToken>()).Returns(call =>
            {
                double threshold = call.Arg<double>();
                List<Game> qualifying = catalogue
                    .Where(game => game.Score.HasValue && game.Score.Value >= threshold)
                    .ToList();

                return Task.FromResult(qualifying.Count == 0
                    ? null
                    : qualifying[draw.Next(qualifying.Count)]);
            });
        }
    }
}
