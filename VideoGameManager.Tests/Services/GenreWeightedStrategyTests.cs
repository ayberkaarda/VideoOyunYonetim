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
    public class GenreWeightedStrategyTests
    {
        /// <summary>
        /// Fixed so that the draw is repeatable: a failure here is a real one and not a run of
        /// bad luck that disappears on the next build.
        /// </summary>
        private const int Seed = 20260907;

        private readonly IGameRepository _games = Substitute.For<IGameRepository>();
        private readonly List<GameFilter> _filters = new List<GameFilter>();

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            Action act = () => new GenreWeightedStrategy(null!);

            act.Should().Throw<ArgumentNullException>().WithParameterName("games");
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(10.1)]
        [InlineData(double.NaN)]
        public void Constructor_ThresholdOutsideTheScoreRange_ThrowsArgumentOutOfRangeException(double threshold)
        {
            Action act = () => new GenreWeightedStrategy(_games, threshold);

            act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("minimumScore");
        }

        [Fact]
        public void Constructor_NoThresholdGiven_UsesTheBottomOfTheScoreRange()
        {
            GenreWeightedStrategy strategy = new GenreWeightedStrategy(_games);

            strategy.MinimumScore.Should().Be(ScoreRange.Min);
        }

        [Fact]
        public void Name_IsTheNameTheStrategyIsSelectedBy()
        {
            GenreWeightedStrategy strategy = new GenreWeightedStrategy(_games);

            strategy.Name.Should().Be("GenreWeighted");
            strategy.Name.Should().Be(GenreWeightedStrategy.StrategyName);
        }

        [Fact]
        public void Name_CarriesNoSurroundingSpace()
        {
            // The service that routes by name trims what it is given but not what a strategy
            // reports, so a padded name would register and then never be selectable.
            GenreWeightedStrategy.StrategyName.Should().Be(GenreWeightedStrategy.StrategyName.Trim());
        }

        [Fact]
        public void Neutral_IsTheMidpointOfTheScoreRange()
        {
            GenreWeightedStrategy.Neutral.Should().Be((ScoreRange.Min + ScoreRange.Max) / 2.0);
            GenreWeightedStrategy.Neutral.Should().Be(5.0);
        }

        [Fact]
        public async Task PickAsync_NoGeneratorGiven_StillSuggestsFromTheFavouredGenre()
        {
            GiveAffinities(Affinity("RPG", 4, 9.0));
            GiveCatalogue(new Game { Name = "Disco Elysium", Genre = "RPG", Score = 9.5 });
            GenreWeightedStrategy strategy = new GenreWeightedStrategy(_games);

            Game? recommendation = await strategy.PickAsync();

            recommendation!.Genre.Should().Be("RPG");
        }

        [Fact]
        public async Task PickAsync_GenreRatedAboveTheMidpoint_IsReachable()
        {
            GiveAffinities(Affinity("RPG", 2, 8.0), Affinity("Sports", 5, 3.0));
            GiveCatalogue(
                new Game { Name = "Disco Elysium", Genre = "RPG", Score = 9.5 },
                new Game { Name = "FIFA 24", Genre = "Sports", Score = 6.0 });
            GenreWeightedStrategy strategy = Build();

            await Draw(strategy, 50);

            ChosenGenres().Should().Contain("RPG");
        }

        [Theory]
        [InlineData(5.0)]
        [InlineData(3.0)]
        [InlineData(0.0)]
        public async Task PickAsync_GenreRatedAtOrBelowTheMidpoint_IsNeverChosen(double average)
        {
            GiveAffinities(Affinity("RPG", 2, 8.0), Affinity("Sports", 5, average));
            GiveCatalogue(
                new Game { Name = "Disco Elysium", Genre = "RPG", Score = 9.5 },
                new Game { Name = "FIFA 24", Genre = "Sports", Score = 6.0 });
            GenreWeightedStrategy strategy = Build();

            await Draw(strategy, 200);

            ChosenGenres().Should().OnlyContain(genre => genre == "RPG");
        }

        [Fact]
        public async Task PickAsync_GenreWithNoScoredReviews_IsNeverChosen()
        {
            // A high average over zero reviews is an average of nothing; it must carry no pull.
            GiveAffinities(Affinity("RPG", 2, 8.0), Affinity("Sports", 0, 10.0));
            GiveCatalogue(
                new Game { Name = "Disco Elysium", Genre = "RPG", Score = 9.5 },
                new Game { Name = "FIFA 24", Genre = "Sports", Score = 6.0 });
            GenreWeightedStrategy strategy = Build();

            await Draw(strategy, 200);

            ChosenGenres().Should().OnlyContain(genre => genre == "RPG");
        }

        [Fact]
        public async Task PickAsync_SameAverageButMoreScoredReviews_IsChosenMoreOften()
        {
            // Weights are 3 x (8 - 5) = 9 against 1 x (8 - 5) = 3, so three draws in four should
            // land on the genre the user has reviewed more often. Asserted as a share with a
            // tolerance rather than as a fixed sequence, which would pin the test to one
            // runtime's generator.
            GiveAffinities(Affinity("RPG", 3, 8.0), Affinity("Puzzle", 1, 8.0));
            GiveCatalogue(
                new Game { Name = "Disco Elysium", Genre = "RPG", Score = 9.5 },
                new Game { Name = "Baba Is You", Genre = "Puzzle", Score = 9.0 });
            GenreWeightedStrategy strategy = Build();

            const int draws = 2000;
            await Draw(strategy, draws);

            List<string?> chosen = ChosenGenres();
            chosen.Should().HaveCount(draws);

            double rpgShare = chosen.Count(genre => genre == "RPG") / (double)draws;
            rpgShare.Should().BeApproximately(0.75, 0.05);
        }

        [Fact]
        public async Task PickAsync_ChosenGenre_FilterCarriesBothTheGenreAndTheThreshold()
        {
            GiveAffinities(Affinity("RPG", 4, 9.0));
            GiveCatalogue(new Game { Name = "Disco Elysium", Genre = "RPG", Score = 9.5 });
            GenreWeightedStrategy strategy = Build(7.5);

            await strategy.PickAsync();

            GameFilter filter = _filters.Should().ContainSingle().Subject;
            filter.Genre.Should().Be("RPG");
            filter.MinScore.Should().Be(7.5);
        }

        [Fact]
        public async Task PickAsync_ThresholdAtTheBottomOfTheRange_LeavesTheScoreOutOfTheFilter()
        {
            GiveAffinities(Affinity("RPG", 4, 9.0));
            GiveCatalogue(new Game { Name = "Disco Elysium", Genre = "RPG", Score = 9.5 });
            GenreWeightedStrategy strategy = Build(ScoreRange.Min);

            await strategy.PickAsync();

            // A threshold at the bottom of the range is not a threshold. Sending it as one would
            // narrow the genre to the games that carry a score at all.
            GameFilter filter = _filters.Should().ContainSingle().Subject;
            filter.Genre.Should().Be("RPG");
            filter.MinScore.Should().BeNull();
        }

        [Fact]
        public async Task PickAsync_NoThresholdSet_CanSuggestAGameNobodyHasRated()
        {
            GiveAffinities(Affinity("RPG", 4, 9.0));
            GiveCatalogue(new Game { Name = "An Unrated RPG", Genre = "RPG", Score = null });
            GenreWeightedStrategy strategy = Build();

            Game? recommendation = await strategy.PickAsync();

            recommendation.Should().NotBeNull();
            recommendation.Name.Should().Be("An Unrated RPG");
        }

        [Fact]
        public async Task PickAsync_NoAffinitiesAtAll_FallsBackToAnUnfilteredPick()
        {
            GiveAffinities();
            GiveCatalogue(new Game { Name = "Celeste", Genre = "Platformer", Score = 9.0 });
            GenreWeightedStrategy strategy = Build(6.0);

            Game? recommendation = await strategy.PickAsync();

            recommendation!.Name.Should().Be("Celeste");
            AssertUnfilteredPick(_filters.Should().ContainSingle().Subject, 6.0);
        }

        [Fact]
        public async Task PickAsync_AffinitiesUnavailable_FallsBackToAnUnfilteredPick()
        {
            // Defensive: the repository contract promises a list, but a null must not take down
            // the recommendation screen.
            _games.GetGenreAffinitiesAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<GenreReviewSummary>>(null!));
            GiveCatalogue(new Game { Name = "Celeste", Genre = "Platformer", Score = 9.0 });
            GenreWeightedStrategy strategy = Build();

            Game? recommendation = await strategy.PickAsync();

            recommendation!.Name.Should().Be("Celeste");
            AssertUnfilteredPick(_filters.Should().ContainSingle().Subject, null);
        }

        [Fact]
        public async Task PickAsync_EveryWeightIsZero_FallsBackToAnUnfilteredPick()
        {
            GiveAffinities(Affinity("Sports", 5, 4.0), Affinity("Racing", 2, 5.0));
            GiveCatalogue(new Game { Name = "Celeste", Genre = "Platformer", Score = 9.0 });
            GenreWeightedStrategy strategy = Build();

            Game? recommendation = await strategy.PickAsync();

            recommendation!.Name.Should().Be("Celeste");
            AssertUnfilteredPick(_filters.Should().ContainSingle().Subject, null);
        }

        [Fact]
        public async Task PickAsync_ChosenGenreYieldsNoGame_FallsBackToAnUnfilteredPick()
        {
            // The user reviewed an RPG well, but every RPG in the catalogue now sits below the
            // threshold, so the favoured genre has nothing to offer.
            GiveAffinities(Affinity("RPG", 4, 9.0));
            GiveCatalogue(
                new Game { Name = "A Tired RPG", Genre = "RPG", Score = 4.0 },
                new Game { Name = "Celeste", Genre = "Platformer", Score = 9.0 });
            GenreWeightedStrategy strategy = Build(8.0);

            Game? recommendation = await strategy.PickAsync();

            recommendation!.Name.Should().Be("Celeste");
            _filters.Should().HaveCount(2);
            _filters[0].Genre.Should().Be("RPG");
            AssertUnfilteredPick(_filters[1], 8.0);
        }

        [Fact]
        public async Task PickAsync_NothingQualifiesAnywhere_SuggestsNothing()
        {
            GiveAffinities(Affinity("RPG", 4, 9.0));
            GiveCatalogue();
            GenreWeightedStrategy strategy = Build();

            Game? recommendation = await strategy.PickAsync();

            recommendation.Should().BeNull();
        }

        [Fact]
        public async Task PickAsync_PassesTheTokenOn()
        {
            CancellationToken ct = new CancellationTokenSource().Token;
            GiveAffinities(Affinity("RPG", 4, 9.0));
            GiveCatalogue(new Game { Name = "Disco Elysium", Genre = "RPG", Score = 9.5 });
            GenreWeightedStrategy strategy = Build();

            await strategy.PickAsync(ct);

            await _games.Received(1).GetGenreAffinitiesAsync(ct);
            await _games.Received(1).GetRandomAsync(Arg.Any<GameFilter>(), ct);
        }

        [Fact]
        public async Task PickAsync_AffinityQueryThrowsProviderFailure_ThrowsDataAccessException()
        {
            SqlException provider = SqlExceptionFactory.Create();
            _games.GetGenreAffinitiesAsync(Arg.Any<CancellationToken>()).Throws(provider);
            GenreWeightedStrategy strategy = Build();

            Func<Task> act = () => strategy.PickAsync();

            (await act.Should().ThrowAsync<DataAccessException>())
                .And.InnerException.Should().BeSameAs(provider);
        }

        [Fact]
        public async Task PickAsync_RandomPickThrowsProviderFailure_ThrowsDataAccessException()
        {
            SqlException provider = SqlExceptionFactory.Create();
            GiveAffinities(Affinity("RPG", 4, 9.0));
            _games.GetRandomAsync(Arg.Any<GameFilter>(), Arg.Any<CancellationToken>()).Throws(provider);
            GenreWeightedStrategy strategy = Build();

            Func<Task> act = () => strategy.PickAsync();

            (await act.Should().ThrowAsync<DataAccessException>())
                .And.InnerException.Should().BeSameAs(provider);
        }

        [Fact]
        public async Task PickAsync_RepositoryThrowsSomethingElse_LetsItTravelUnchanged()
        {
            InvalidOperationException unrelated = new InvalidOperationException("not a provider failure");
            _games.GetGenreAffinitiesAsync(Arg.Any<CancellationToken>()).Throws(unrelated);
            GenreWeightedStrategy strategy = Build();

            Func<Task> act = () => strategy.PickAsync();

            (await act.Should().ThrowAsync<InvalidOperationException>()).And.Should().BeSameAs(unrelated);
        }

        private GenreWeightedStrategy Build(double minimumScore = ScoreRange.Min) =>
            new GenreWeightedStrategy(_games, minimumScore, new Random(Seed));

        private static GenreReviewSummary Affinity(string genre, int scoredReviewCount, double averageReviewScore) =>
            new GenreReviewSummary(genre, scoredReviewCount, averageReviewScore);

        private static async Task Draw(GenreWeightedStrategy strategy, int draws)
        {
            for (int draw = 0; draw < draws; draw++)
            {
                await strategy.PickAsync();
            }
        }

        /// <summary>
        /// The genre of every filter the strategy handed the repository.
        /// </summary>
        private List<string?> ChosenGenres() => _filters.Select(filter => filter.Genre).ToList();

        /// <summary>
        /// A fallback pick narrows nothing but the score: no genre, and no other member touched.
        /// </summary>
        private static void AssertUnfilteredPick(GameFilter filter, double? threshold)
        {
            filter.Should().Be(new GameFilter(MinScore: threshold));
            filter.Genre.Should().BeNull();
        }

        private void GiveAffinities(params GenreReviewSummary[] affinities)
        {
            _games.GetGenreAffinitiesAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IReadOnlyList<GenreReviewSummary>>(affinities));
        }

        /// <summary>
        /// Makes the repository behave like the real query: it hands back a game drawn from those
        /// that match the genre it was asked for, if any, and that reach the threshold. Every
        /// filter it is handed is recorded so a test can assert what actually reached it.
        /// </summary>
        private void GiveCatalogue(params Game[] catalogue)
        {
            Random draw = new Random(Seed);

            _games.GetRandomAsync(Arg.Any<GameFilter>(), Arg.Any<CancellationToken>()).Returns(call =>
            {
                GameFilter filter = call.Arg<GameFilter>();
                _filters.Add(filter);

                // Mirrors the optional predicates the real query uses: with no threshold every
                // game is a candidate, and with one the comparison itself removes the games
                // that carry no score.
                List<Game> qualifying = catalogue
                    .Where(game => filter.Genre == null || game.Genre == filter.Genre)
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
