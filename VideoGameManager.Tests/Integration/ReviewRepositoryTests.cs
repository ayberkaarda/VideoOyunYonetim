using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FluentAssertions;
using VideoGameManager.Data;
using VideoGameManager.Domain;
using Xunit;

namespace VideoGameManager.Tests.Integration
{
    /// <summary>
    /// Exercises <see cref="ReviewRepository"/> against a real SQL Server.
    /// </summary>
    /// <remarks>
    /// Writing a review touches two tables inside one transaction and its ordering depends on a
    /// column type that keeps a UTC offset, so both are checked here rather than against a
    /// substituted connection.
    /// </remarks>
    [Collection(DatabaseCollection.Name)]
    public sealed class ReviewRepositoryTests : IAsyncLifetime
    {
        private const string CountReviewsSql = @"
SELECT COUNT(*)
FROM   dbo.Review
WHERE  GameId = @GameId;";

        private const string CountAllReviewsSql = @"
SELECT COUNT(*)
FROM   dbo.Review
WHERE  Id > 0;";

        private const string ScoreOfGameSql = @"
SELECT Score
FROM   dbo.Game
WHERE  Id = @Id;";

        private readonly SqlServerFixture _fixture;

        private readonly IGameRepository _games;

        private readonly IReviewRepository _reviews;

        /// <summary>
        /// Creates the test class against the shared server.
        /// </summary>
        /// <param name="fixture">The running SQL Server.</param>
        public ReviewRepositoryTests(SqlServerFixture fixture)
        {
            _fixture = fixture;
            _games = new GameRepository(fixture.Connections);
            _reviews = new ReviewRepository(fixture.Connections);
        }

        /// <inheritdoc />
        public Task InitializeAsync() => _fixture.ResetAsync();

        /// <inheritdoc />
        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task AddAsync_StoresTheReviewAndReturnsItsId()
        {
            int gameId = await AddGameAsync("Outer Wilds", 9.0);

            int reviewId = await _reviews.AddAsync(NewReview(gameId, 8.5, "Best played blind."));

            reviewId.Should().BeGreaterThan(0);

            IReadOnlyList<Review> stored = await _reviews.GetForGameAsync(gameId);

            stored.Should().ContainSingle();
            stored[0].Id.Should().Be(reviewId);
            stored[0].GameId.Should().Be(gameId);
            stored[0].Score.Should().Be(8.5);
            stored[0].Body.Should().Be("Best played blind.");
        }

        [Fact]
        public async Task AddAsync_KeepsTheMomentTheCallerSuppliedRatherThanTheColumnDefault()
        {
            // The column carries a default of "now", so a repository that let it fill itself in
            // would look correct until a caller needed to record when the review was actually
            // written. A moment well in the past, with a non-zero offset, tells the two apart.
            int gameId = await AddGameAsync("Outer Wilds", 9.0);
            DateTimeOffset written = new DateTimeOffset(2024, 3, 4, 5, 6, 7, 123, TimeSpan.FromHours(3));

            await _reviews.AddAsync(NewReview(gameId, 8.5, "Written a while ago.", written));

            IReadOnlyList<Review> stored = await _reviews.GetForGameAsync(gameId);

            stored.Should().ContainSingle();
            stored[0].CreatedAt.Should().Be(written);
            stored[0].CreatedAt.Offset.Should().Be(TimeSpan.FromHours(3), "the writer's offset is kept, not flattened to UTC");
        }

        [Fact]
        public async Task AddAsync_WithAScore_UpdatesTheScoreOfTheGame()
        {
            int gameId = await AddGameAsync("Outer Wilds", 5.0);

            await _reviews.AddAsync(NewReview(gameId, 9.6, "Far better than I first thought."));

            double? score = await ScalarAsync<double?>(ScoreOfGameSql, new { Id = gameId });

            score.Should().Be(9.6);
        }

        [Fact]
        public async Task AddAsync_WithoutAScore_LeavesTheScoreOfTheGameAlone()
        {
            int gameId = await AddGameAsync("Outer Wilds", 5.0);

            await _reviews.AddAsync(NewReview(gameId, null, "No number from me."));

            double? score = await ScalarAsync<double?>(ScoreOfGameSql, new { Id = gameId });

            score.Should().Be(5.0, "a review that carries no score says nothing about the rating");
        }

        [Fact]
        public async Task AddAsync_WithoutAScore_LeavesAnUnratedGameUnrated()
        {
            int gameId = await AddGameAsync("Outer Wilds", null);

            await _reviews.AddAsync(NewReview(gameId, null, "Still thinking about it."));

            double? score = await ScalarAsync<double?>(ScoreOfGameSql, new { Id = gameId });

            score.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_ForAGameThatDoesNotExist_ReturnsZeroAndWritesNothing()
        {
            int reviewId = await _reviews.AddAsync(NewReview(987654, 7.0, "About nothing at all."));

            reviewId.Should().Be(0);

            int total = await ScalarAsync<int>(CountAllReviewsSql, null);

            total.Should().Be(0, "there is no game to attach the review to");
        }

        [Fact]
        public async Task GetForGameAsync_ReturnsTheReviewsNewestFirst()
        {
            int gameId = await AddGameAsync("Outer Wilds", 9.0);
            DateTimeOffset baseMoment = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);

            await _reviews.AddAsync(NewReview(gameId, 6.0, "Oldest", baseMoment));
            await _reviews.AddAsync(NewReview(gameId, 7.0, "Newest", baseMoment.AddDays(2)));
            await _reviews.AddAsync(NewReview(gameId, 8.0, "Middle", baseMoment.AddDays(1)));

            IReadOnlyList<Review> stored = await _reviews.GetForGameAsync(gameId);

            stored.Select(review => review.Body).Should().Equal(new[] { "Newest", "Middle", "Oldest" });
        }

        [Fact]
        public async Task GetForGameAsync_ForReviewsWrittenAtTheSameMoment_PutsTheLatestIdentityFirst()
        {
            // Two reviews saved inside the same millisecond would otherwise come back in an order
            // the server is free to change between runs.
            int gameId = await AddGameAsync("Outer Wilds", 9.0);
            DateTimeOffset moment = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);

            int first = await _reviews.AddAsync(NewReview(gameId, 6.0, "First", moment));
            int second = await _reviews.AddAsync(NewReview(gameId, 7.0, "Second", moment));

            IReadOnlyList<Review> stored = await _reviews.GetForGameAsync(gameId);

            stored.Select(review => review.Id).Should().Equal(new[] { second, first });
        }

        [Fact]
        public async Task GetForGameAsync_ForAGameWithNoReviews_ReturnsAnEmptyList()
        {
            int gameId = await AddGameAsync("Outer Wilds", 9.0);

            IReadOnlyList<Review> stored = await _reviews.GetForGameAsync(gameId);

            stored.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public async Task GetForGameAsync_ForAGameThatDoesNotExist_ReturnsAnEmptyList()
        {
            IReadOnlyList<Review> stored = await _reviews.GetForGameAsync(987654);

            stored.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public async Task GetForGameAsync_ReturnsOnlyTheReviewsOfThatGame()
        {
            int reviewed = await AddGameAsync("Outer Wilds", 9.0);
            int other = await AddGameAsync("Subnautica", 8.8);

            await _reviews.AddAsync(NewReview(reviewed, 9.0, "Mine."));
            await _reviews.AddAsync(NewReview(other, 8.0, "Someone else's."));

            IReadOnlyList<Review> stored = await _reviews.GetForGameAsync(reviewed);

            stored.Should().ContainSingle();
            stored[0].Body.Should().Be("Mine.");
            (await ScalarAsync<int>(CountReviewsSql, new { GameId = other })).Should().Be(1);
        }

        [Fact]
        public async Task AddAsync_ThenReadingTheGame_ShowsTheNewestReviewAsLatestReview()
        {
            int gameId = await AddGameAsync("Outer Wilds", 9.0);
            DateTimeOffset baseMoment = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);

            await _reviews.AddAsync(NewReview(gameId, 7.0, "The first thing I said.", baseMoment));

            Game afterFirst = await _games.GetAsync(gameId);
            afterFirst.LatestReview.Should().Be("The first thing I said.");

            await _reviews.AddAsync(NewReview(gameId, 8.0, "What I say now.", baseMoment.AddDays(1)));

            Game afterSecond = await _games.GetAsync(gameId);
            afterSecond.LatestReview.Should().Be("What I say now.", "the projection shows the newest review, not the first");
        }

        [Fact]
        public async Task GetAsync_ForAGameWithNoReviews_LeavesLatestReviewNull()
        {
            int gameId = await AddGameAsync("Outer Wilds", 9.0);

            Game stored = await _games.GetAsync(gameId);

            stored.LatestReview.Should().BeNull();
        }

        /// <summary>
        /// Stores a game the reviews can hang off and returns its identity.
        /// </summary>
        private Task<int> AddGameAsync(string name, double? score) =>
            _games.AddAsync(new Game
            {
                Name = name,
                Genre = "Adventure",
                Platforms = new[] { "PC" },
                Score = score,
            });

        /// <summary>
        /// Builds a review. The moment defaults to a fixed one so that a test which does not care
        /// about ordering does not depend on the clock.
        /// </summary>
        private static Review NewReview(int gameId, double? score, string body, DateTimeOffset? createdAt = null) =>
            new Review
            {
                GameId = gameId,
                Score = score,
                Body = body,
                CreatedAt = createdAt ?? new DateTimeOffset(2024, 6, 1, 10, 30, 0, TimeSpan.Zero),
            };

        private async Task<T> ScalarAsync<T>(string sql, object parameters)
        {
            using (DbConnection connection = await _fixture.OpenConnectionAsync().ConfigureAwait(false))
            {
                return await connection
                    .ExecuteScalarAsync<T>(new CommandDefinition(sql, parameters))
                    .ConfigureAwait(false);
            }
        }
    }
}
