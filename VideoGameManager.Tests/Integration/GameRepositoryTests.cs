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
    /// Exercises <see cref="GameRepository"/> against a real SQL Server.
    /// </summary>
    /// <remarks>
    /// These tests are the only place the SQL itself is checked. A repository unit test with a
    /// substituted connection would prove that the C# calls Dapper, not that the statements the
    /// server receives mean what they are meant to mean: the cascade on a delete, the collation
    /// behind a case-insensitive search and the transaction around a failed update all live in
    /// the database and nowhere else.
    /// <para>
    /// Every test starts from an empty catalogue, so counts and orderings can be asserted over
    /// the whole table rather than over rows carefully picked out by name.
    /// </para>
    /// </remarks>
    [Collection(DatabaseCollection.Name)]
    public sealed class GameRepositoryTests : IAsyncLifetime
    {
        private const string CountGenresSql = @"
SELECT COUNT(*)
FROM   dbo.Genre
WHERE  Name = @Name;";

        private const string CountPlatformsSql = @"
SELECT COUNT(*)
FROM   dbo.[Platform]
WHERE  Name = @Name;";

        private const string GenreIdOfGameSql = @"
SELECT GenreId
FROM   dbo.Game
WHERE  Id = @Id;";

        private const string CountPlatformLinksSql = @"
SELECT COUNT(*)
FROM   dbo.GamePlatform
WHERE  GameId = @GameId;";

        private const string CountReviewsSql = @"
SELECT COUNT(*)
FROM   dbo.Review
WHERE  GameId = @GameId;";

        private const string CountGamesSql = @"
SELECT COUNT(*)
FROM   dbo.Game
WHERE  Id = @Id;";

        private const string InsertReviewSql = @"
INSERT INTO dbo.Review (GameId, Score, Body, CreatedAt)
VALUES (@GameId, @Score, @Body, @CreatedAt);";

        private readonly SqlServerFixture _fixture;

        private readonly IGameRepository _repository;

        /// <summary>
        /// Creates the test class against the shared server.
        /// </summary>
        /// <param name="fixture">The running SQL Server.</param>
        public GameRepositoryTests(SqlServerFixture fixture)
        {
            _fixture = fixture;
            _repository = new GameRepository(fixture.Connections);
        }

        /// <inheritdoc />
        public Task InitializeAsync() => _fixture.ResetAsync();

        /// <inheritdoc />
        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task AddAsync_ForANewGame_StoresItAndReturnsItsId()
        {
            int id = await _repository.AddAsync(NewGame("Hollow Knight", "Metroidvania", 9.2, "PC", "Switch"));

            id.Should().BeGreaterThan(0);

            Game stored = await _repository.GetAsync(id);

            stored.Should().NotBeNull();
            stored.Id.Should().Be(id);
            stored.Name.Should().Be("Hollow Knight");
            stored.Genre.Should().Be("Metroidvania");
            stored.Score.Should().Be(9.2);
            stored.CoverUrl.Should().Be("https://example.invalid/hollow-knight.png");
            stored.Platforms.Should().BeEquivalentTo(new[] { "PC", "Switch" });
        }

        [Fact]
        public async Task AddAsync_WhenTheGenreIsNew_CreatesTheLookupRow()
        {
            await _repository.AddAsync(NewGame("Disco Elysium", "RPG", 9.5, "PC"));

            (await CountAsync(CountGenresSql, new { Name = "RPG" })).Should().Be(1);
        }

        [Fact]
        public async Task AddAsync_WhenTheGenreAlreadyExists_ReusesTheLookupRow()
        {
            int first = await _repository.AddAsync(NewGame("Disco Elysium", "RPG", 9.5, "PC"));
            int second = await _repository.AddAsync(NewGame("Baldur's Gate 3", "RPG", 9.8, "PC"));

            (await CountAsync(CountGenresSql, new { Name = "RPG" }))
                .Should().Be(1, "a second game in a genre must not create a second lookup row");

            int firstGenreId = await ScalarAsync<int>(GenreIdOfGameSql, new { Id = first });
            int secondGenreId = await ScalarAsync<int>(GenreIdOfGameSql, new { Id = second });

            secondGenreId.Should().Be(firstGenreId);
        }

        [Fact]
        public async Task AddAsync_LinksEveryPlatformCreatingTheOnesThatAreNew()
        {
            int id = await _repository.AddAsync(NewGame("Elden Ring", "Action", 9.6, "PC", "PS5", "Xbox"));

            (await CountAsync(CountPlatformLinksSql, new { GameId = id })).Should().Be(3);
            (await CountAsync(CountPlatformsSql, new { Name = "PS5" })).Should().Be(1);

            // A platform that is already on file is reused rather than duplicated.
            await _repository.AddAsync(NewGame("Sekiro", "Action", 9.1, "PC", "PS5"));

            (await CountAsync(CountPlatformsSql, new { Name = "PS5" })).Should().Be(1);
        }

        [Fact]
        public async Task GetAsync_ForAnUnknownId_ReturnsNull()
        {
            Game found = await _repository.GetAsync(987654);

            found.Should().BeNull();
        }

        [Fact]
        public async Task GetAsync_ForAGameOnNoPlatform_ReturnsAnEmptyListNotNull()
        {
            int id = await _repository.AddAsync(new Game
            {
                Name = "Unreleased",
                Genre = "Adventure",
                Platforms = Array.Empty<string>(),
                Score = null,
            });

            Game stored = await _repository.GetAsync(id);

            stored.Platforms.Should().NotBeNull().And.BeEmpty();
            stored.Score.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_ReplacesThePlatformListRatherThanMergingIt()
        {
            int id = await _repository.AddAsync(NewGame("Celeste", "Platformer", 9.0, "PC", "Switch", "PS4"));

            bool updated = await _repository.UpdateAsync(new Game
            {
                Id = id,
                Name = "Celeste (Remastered)",
                Genre = "Precision Platformer",
                Platforms = new[] { "PC" },
                Score = 9.4,
                CoverUrl = "https://example.invalid/celeste.png",
            });

            updated.Should().BeTrue();

            Game stored = await _repository.GetAsync(id);

            stored.Name.Should().Be("Celeste (Remastered)");
            stored.Genre.Should().Be("Precision Platformer");
            stored.Score.Should().Be(9.4);
            // The list is replaced, not merged: Switch and PS4 were dropped by the caller and so
            // are gone from the bridge table.
            stored.Platforms.Should().Equal(new[] { "PC" });
            (await CountAsync(CountPlatformLinksSql, new { GameId = id })).Should().Be(1);
        }

        [Fact]
        public async Task UpdateAsync_ForAnUnknownId_ReturnsFalseAndLeavesNoGenreBehind()
        {
            // The genre is looked up or created before the row is addressed, so an update that
            // matches nothing must roll that lookup row back with the rest of the transaction.
            bool updated = await _repository.UpdateAsync(new Game
            {
                Id = 987654,
                Name = "Ghost",
                Genre = "Genre That Should Not Survive",
                Platforms = new[] { "PC" },
                Score = 5.0,
            });

            updated.Should().BeFalse();

            (await CountAsync(CountGenresSql, new { Name = "Genre That Should Not Survive" }))
                .Should().Be(0, "the transaction was never committed");
        }

        [Fact]
        public async Task DeleteAsync_RemovesTheGameAndEverythingThatPointedAtIt()
        {
            int id = await _repository.AddAsync(NewGame("Portal 2", "Puzzle", 9.7, "PC", "PS3"));
            await AddReviewRowAsync(id, 9.0, "Still the best.");

            (await CountAsync(CountPlatformLinksSql, new { GameId = id })).Should().Be(2);
            (await CountAsync(CountReviewsSql, new { GameId = id })).Should().Be(1);

            bool deleted = await _repository.DeleteAsync(id);

            deleted.Should().BeTrue();
            (await CountAsync(CountGamesSql, new { Id = id })).Should().Be(0);

            // Both foreign keys are declared ON DELETE CASCADE, so the link rows and the reviews
            // go with the game instead of blocking the delete.
            (await CountAsync(CountPlatformLinksSql, new { GameId = id })).Should().Be(0);
            (await CountAsync(CountReviewsSql, new { GameId = id })).Should().Be(0);

            // The platform lookup rows are not cascaded: they belong to the catalogue, not to
            // the game that happened to use them.
            (await CountAsync(CountPlatformsSql, new { Name = "PS3" })).Should().Be(1);
        }

        [Fact]
        public async Task DeleteAsync_ForAnUnknownId_ReturnsFalse()
        {
            bool deleted = await _repository.DeleteAsync(987654);

            deleted.Should().BeFalse();
        }

        [Fact]
        public async Task ListAsync_PagesTheResultAndReportsTheWholeTotal()
        {
            await AddManyAsync();

            PagedResult<Game> page = await _repository.ListAsync(GameFilter.None, 2, 2);

            page.TotalCount.Should().Be(5);
            page.Page.Should().Be(2);
            page.PageSize.Should().Be(2);
            page.PageCount.Should().Be(3);
            page.HasNextPage.Should().BeTrue();
            page.Items.Select(game => game.Name).Should().Equal("Control", "Dishonored");
        }

        [Fact]
        public async Task ListAsync_ForAPageBeyondTheEnd_ReturnsNoRowsButTheRealTotal()
        {
            await AddManyAsync();

            PagedResult<Game> page = await _repository.ListAsync(GameFilter.None, 9, 2);

            page.Items.Should().BeEmpty();
            page.TotalCount.Should().Be(5);
            page.HasNextPage.Should().BeFalse();
        }

        [Theory]
        [InlineData(false, new[] { "Anno 1800", "Bastion", "Control", "Dishonored", "Everspace" })]
        [InlineData(true, new[] { "Everspace", "Dishonored", "Control", "Bastion", "Anno 1800" })]
        public async Task ListAsync_OrdersByName(bool descending, string[] expected)
        {
            await AddManyAsync();

            PagedResult<Game> page = await _repository.ListAsync(GameFilter.None, 1, 10, GameSortField.Name, descending);

            page.Items.Select(game => game.Name).Should().Equal(expected);
        }

        [Theory]
        [InlineData(false, new[] { "Everspace", "Dishonored", "Control", "Bastion", "Anno 1800" })]
        [InlineData(true, new[] { "Anno 1800", "Bastion", "Control", "Dishonored", "Everspace" })]
        public async Task ListAsync_OrdersByScore(bool descending, string[] expected)
        {
            await AddManyAsync();

            PagedResult<Game> page = await _repository.ListAsync(GameFilter.None, 1, 10, GameSortField.Score, descending);

            page.Items.Select(game => game.Name).Should().Equal(expected);
        }

        [Theory]
        [InlineData("fifa")]
        [InlineData("FIFA")]
        [InlineData("FiFa")]
        [InlineData("ifa 2")]
        public async Task ListAsync_FiltersByNameWhateverTheCase(string term)
        {
            // The database collation is case insensitive on purpose. Under a Turkish collation
            // "I" and "i" are different letters, so "fifa" would find nothing here and no error
            // would be raised to explain why.
            await _repository.AddAsync(NewGame("FIFA 24", "Sports", 7.4, "PS5"));
            await _repository.AddAsync(NewGame("Forza Horizon 5", "Racing", 9.0, "Xbox"));

            PagedResult<Game> page = await _repository.ListAsync(new GameFilter(Name: term), 1, 10);

            page.TotalCount.Should().Be(1);
            page.Items.Single().Name.Should().Be("FIFA 24");
        }

        [Fact]
        public async Task ListAsync_FiltersByGenre()
        {
            await AddManyAsync();

            PagedResult<Game> page = await _repository.ListAsync(new GameFilter(Genre: "Strategy"), 1, 10);

            page.TotalCount.Should().Be(1);
            page.Items.Single().Name.Should().Be("Anno 1800");
        }

        [Fact]
        public async Task ListAsync_FiltersByPlatform()
        {
            await _repository.AddAsync(NewGame("Halo Infinite", "Shooter", 8.5, "Xbox", "PC"));
            await _repository.AddAsync(NewGame("Bloodborne", "Action", 9.3, "PS4"));

            PagedResult<Game> page = await _repository.ListAsync(new GameFilter(Platform: "Xbox"), 1, 10);

            page.TotalCount.Should().Be(1);
            page.Items.Single().Name.Should().Be("Halo Infinite");
        }

        [Fact]
        public async Task ListAsync_FiltersByScoreRange()
        {
            await AddManyAsync();

            PagedResult<Game> page = await _repository.ListAsync(new GameFilter(MinScore: 7.0, MaxScore: 8.5), 1, 10);

            // Both bounds are inclusive: Bastion sits exactly on the upper one.
            page.TotalCount.Should().Be(2);
            page.Items.Select(game => game.Name).Should().Equal(new[] { "Bastion", "Control" });
        }

        [Fact]
        public async Task ListAsync_TreatsAPercentSignInTheTermAsALetter()
        {
            // Without escaping, a term of "%" would be a wildcard and match the whole catalogue.
            await _repository.AddAsync(NewGame("100% Orange Juice", "Party", 6.5, "PC"));
            await _repository.AddAsync(NewGame("Stardew Valley", "Simulation", 9.1, "PC"));

            PagedResult<Game> page = await _repository.ListAsync(new GameFilter(Name: "%"), 1, 10);

            page.TotalCount.Should().Be(1);
            page.Items.Single().Name.Should().Be("100% Orange Juice");
        }

        [Fact]
        public async Task ListAsync_TreatsAnUnderscoreInTheTermAsALetter()
        {
            // An unescaped underscore matches any single character, so it would find both rows.
            await _repository.AddAsync(NewGame("Half_Life", "Shooter", 9.4, "PC"));
            await _repository.AddAsync(NewGame("HalfXLife", "Shooter", 4.0, "PC"));

            PagedResult<Game> page = await _repository.ListAsync(new GameFilter(Name: "Half_Life"), 1, 10);

            page.TotalCount.Should().Be(1);
            page.Items.Single().Name.Should().Be("Half_Life");
        }

        [Fact]
        public async Task ListAsync_TreatsABracketInTheTermAsALetter()
        {
            await _repository.AddAsync(NewGame("Rez [Infinite]", "Rhythm", 8.8, "PC"));
            await _repository.AddAsync(NewGame("Rez Classic", "Rhythm", 8.0, "PC"));

            PagedResult<Game> page = await _repository.ListAsync(new GameFilter(Name: "[Infinite]"), 1, 10);

            page.TotalCount.Should().Be(1);
            page.Items.Single().Name.Should().Be("Rez [Infinite]");
        }

        [Fact]
        public async Task ListAsync_WithABlankFilterValue_IgnoresThatClause()
        {
            await AddManyAsync();

            PagedResult<Game> page = await _repository.ListAsync(new GameFilter(Name: "   ", Genre: "  "), 1, 10);

            page.TotalCount.Should().Be(5);
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(-1, 10)]
        public async Task ListAsync_WithAPageBelowOne_Throws(int page, int pageSize)
        {
            Func<Task> act = () => _repository.ListAsync(GameFilter.None, page, pageSize);

            await act.Should().ThrowAsync<ArgumentOutOfRangeException>().WithParameterName("page");
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, -5)]
        public async Task ListAsync_WithAPageSizeBelowOne_Throws(int page, int pageSize)
        {
            Func<Task> act = () => _repository.ListAsync(GameFilter.None, page, pageSize);

            await act.Should().ThrowAsync<ArgumentOutOfRangeException>().WithParameterName("pageSize");
        }

        [Fact]
        public async Task GetRandomAsync_NeverReturnsAGameBelowTheThreshold()
        {
            await _repository.AddAsync(NewGame("High One", "Action", 9.5, "PC"));
            await _repository.AddAsync(NewGame("High Two", "Action", 8.0, "PC"));
            await _repository.AddAsync(NewGame("Exactly At", "Action", 8.0, "PC"));
            await _repository.AddAsync(NewGame("Low One", "Action", 4.0, "PC"));
            await _repository.AddAsync(NewGame("Low Two", "Action", 1.5, "PC"));
            await _repository.AddAsync(NewGame("Unrated", "Action", null, "PC"));

            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            // The pick is random, so one call proves nothing. Repeating it makes the assertion
            // about the query rather than about a lucky draw.
            for (int attempt = 0; attempt < 30; attempt++)
            {
                Game picked = await _repository.GetRandomAsync(new GameFilter(MinScore: 8.0));

                picked.Should().NotBeNull();
                picked.Score.Should().NotBeNull();
                picked.Score.Value.Should().BeGreaterThanOrEqualTo(8.0);

                seen.Add(picked.Name);
            }

            seen.Should().BeSubsetOf(new[] { "High One", "High Two", "Exactly At" });
        }

        [Fact]
        public async Task GetRandomAsync_WhenNothingReachesTheThreshold_ReturnsNull()
        {
            await _repository.AddAsync(NewGame("Low One", "Action", 4.0, "PC"));
            await _repository.AddAsync(NewGame("Unrated", "Action", null, "PC"));

            Game picked = await _repository.GetRandomAsync(new GameFilter(MinScore: 9.9));

            picked.Should().BeNull();
        }

        [Fact]
        public async Task GetRandomAsync_ReturnsTheGameWithItsPlatforms()
        {
            await _repository.AddAsync(NewGame("Only Choice", "Action", 9.0, "PC", "Switch"));

            Game picked = await _repository.GetRandomAsync(new GameFilter(MinScore: 1.0));

            picked.Name.Should().Be("Only Choice");
            picked.Platforms.Should().BeEquivalentTo(new[] { "PC", "Switch" });
        }

        [Fact]
        public async Task GetRandomAsync_WithNoFilterAtAll_MayPickAnUnratedGame()
        {
            // The predicate is the listing's, so a null bound switches its clause off entirely
            // rather than quietly excluding the rows that have no score.
            await _repository.AddAsync(NewGame("Unrated", "Action", null, "PC"));

            Game picked = await _repository.GetRandomAsync(GameFilter.None);

            picked.Should().NotBeNull();
            picked.Name.Should().Be("Unrated");
        }

        [Fact]
        public async Task GetRandomAsync_HonoursStatusGenreAndScoreTogether()
        {
            // Each of the three decoys breaks exactly one clause, so a filter that dropped any one
            // of them would let a decoy through and this would fail rather than pass by luck.
            await AddAsync(NewGame("The Only Match", "RPG", 9.0, "PC"), PlayStatus.Playing, false);
            await AddAsync(NewGame("Wrong State", "RPG", 9.0, "PC"), PlayStatus.Backlog, false);
            await AddAsync(NewGame("Wrong Genre", "Racing", 9.0, "PC"), PlayStatus.Playing, false);
            await AddAsync(NewGame("Too Low", "RPG", 4.0, "PC"), PlayStatus.Playing, false);

            GameFilter filter = new GameFilter(Genre: "RPG", MinScore: 8.0, Status: PlayStatus.Playing);

            for (int attempt = 0; attempt < 20; attempt++)
            {
                Game picked = await _repository.GetRandomAsync(filter);

                picked.Should().NotBeNull();
                picked.Name.Should().Be("The Only Match");
            }
        }

        [Fact]
        public async Task GetRandomAsync_WhenTheFilterMatchesNothing_ReturnsNull()
        {
            await AddAsync(NewGame("Backlogged", "RPG", 9.0, "PC"), PlayStatus.Backlog, false);

            Game picked = await _repository.GetRandomAsync(new GameFilter(Status: PlayStatus.Finished));

            picked.Should().BeNull();
        }

        [Fact]
        public async Task GetRandomAsync_AskedForFavouritesOnly_PicksOnlyFromThem()
        {
            await AddAsync(NewGame("Loved", "RPG", 5.0, "PC"), PlayStatus.Backlog, true);
            await AddAsync(NewGame("Merely Owned", "RPG", 9.9, "PC"), PlayStatus.Backlog, false);

            for (int attempt = 0; attempt < 20; attempt++)
            {
                Game picked = await _repository.GetRandomAsync(new GameFilter(OnlyFavourites: true));

                picked.Name.Should().Be("Loved");
            }
        }

        [Fact]
        public async Task AddAsync_ThenGetAsync_CarriesThePlayStateAndTheFavouriteFlagBothWays()
        {
            int id = await AddAsync(NewGame("Hades", "Roguelike", 9.6, "PC"), PlayStatus.Playing, true);

            Game stored = await _repository.GetAsync(id);

            stored.Status.Should().Be(PlayStatus.Playing);
            stored.IsFavourite.Should().BeTrue();

            stored.Status = PlayStatus.Finished;
            stored.IsFavourite = false;

            (await _repository.UpdateAsync(stored)).Should().BeTrue();

            Game reread = await _repository.GetAsync(id);

            reread.Status.Should().Be(PlayStatus.Finished);
            reread.IsFavourite.Should().BeFalse();
        }

        [Fact]
        public async Task AddAsync_WithoutSayingAnythingAboutTheState_StoresBacklogAndNotAFavourite()
        {
            int id = await _repository.AddAsync(NewGame("Fresh Arrival", "Action", 7.0, "PC"));

            Game stored = await _repository.GetAsync(id);

            stored.Status.Should().Be(PlayStatus.Backlog);
            stored.IsFavourite.Should().BeFalse();
        }

        [Theory]
        [InlineData(PlayStatus.Backlog, "Waiting")]
        [InlineData(PlayStatus.Playing, "Underway")]
        [InlineData(PlayStatus.Finished, "Done")]
        public async Task ListAsync_FiltersByPlayState(PlayStatus status, string expected)
        {
            await AddAsync(NewGame("Waiting", "Action", 7.0, "PC"), PlayStatus.Backlog, false);
            await AddAsync(NewGame("Underway", "Action", 7.0, "PC"), PlayStatus.Playing, false);
            await AddAsync(NewGame("Done", "Action", 7.0, "PC"), PlayStatus.Finished, false);

            PagedResult<Game> page = await _repository.ListAsync(new GameFilter(Status: status), 1, 10);

            page.TotalCount.Should().Be(1);
            page.Items.Single().Name.Should().Be(expected);
        }

        [Fact]
        public async Task ListAsync_AskedForFavouritesOnly_KeepsOnlyTheMarkedGames()
        {
            await AddAsync(NewGame("Loved", "Action", 7.0, "PC"), PlayStatus.Backlog, true);
            await AddAsync(NewGame("Also Loved", "Action", 7.0, "PC"), PlayStatus.Finished, true);
            await AddAsync(NewGame("Merely Owned", "Action", 7.0, "PC"), PlayStatus.Backlog, false);

            PagedResult<Game> page = await _repository.ListAsync(new GameFilter(OnlyFavourites: true), 1, 10);

            page.TotalCount.Should().Be(2);
            page.Items.Select(game => game.Name).Should().Equal("Also Loved", "Loved");
        }

        [Theory]
        [InlineData(null)]
        [InlineData(false)]
        public async Task ListAsync_NotAskedForFavourites_KeepsEverything(bool? onlyFavourites)
        {
            // Not asking for favourites is not the same as asking for the games nobody marked;
            // there is no screen that wants those, so both readings of "no" keep the whole list.
            await AddAsync(NewGame("Loved", "Action", 7.0, "PC"), PlayStatus.Backlog, true);
            await AddAsync(NewGame("Merely Owned", "Action", 7.0, "PC"), PlayStatus.Backlog, false);

            PagedResult<Game> page = await _repository.ListAsync(
                new GameFilter(OnlyFavourites: onlyFavourites), 1, 10);

            page.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task ListAsync_CombiningStateAndFavourite_AppliesBoth()
        {
            await AddAsync(NewGame("Wanted", "Action", 7.0, "PC"), PlayStatus.Playing, true);
            await AddAsync(NewGame("Right State Only", "Action", 7.0, "PC"), PlayStatus.Playing, false);
            await AddAsync(NewGame("Favourite Only", "Action", 7.0, "PC"), PlayStatus.Backlog, true);

            PagedResult<Game> page = await _repository.ListAsync(
                new GameFilter(Status: PlayStatus.Playing, OnlyFavourites: true), 1, 10);

            page.TotalCount.Should().Be(1);
            page.Items.Single().Name.Should().Be("Wanted");
        }

        [Fact]
        public async Task GetGenreAffinitiesAsync_CountsAndAveragesTheScoredReviewsOfEachGenre()
        {
            int firstRpg = await _repository.AddAsync(NewGame("Disco Elysium", "RPG", 9.5, "PC"));
            int secondRpg = await _repository.AddAsync(NewGame("Baldur's Gate 3", "RPG", 9.8, "PC"));
            int racer = await _repository.AddAsync(NewGame("Forza Horizon 5", "Racing", 9.0, "Xbox"));

            await AddReviewRowAsync(firstRpg, 8.0, "Long, and worth it.");
            await AddReviewRowAsync(secondRpg, 9.0, "Longer still.");
            await AddReviewRowAsync(racer, 6.0, "Pretty, shallow.");

            IReadOnlyList<GenreReviewSummary> affinities = await _repository.GetGenreAffinitiesAsync();

            affinities.Select(row => row.Genre).Should().Equal("Racing", "RPG");

            GenreReviewSummary rpg = affinities.Single(row => row.Genre == "RPG");
            rpg.ScoredReviewCount.Should().Be(2);
            rpg.AverageReviewScore.Should().Be(8.5);

            GenreReviewSummary racing = affinities.Single(row => row.Genre == "Racing");
            racing.ScoredReviewCount.Should().Be(1);
            racing.AverageReviewScore.Should().Be(6.0);
        }

        [Fact]
        public async Task GetGenreAffinitiesAsync_IgnoresReviewsThatCarryNoScore()
        {
            // A review without a score is prose about a game, not an opinion that can be averaged.
            // Counting it as a zero would drag its genre to the bottom of the scale for no reason.
            int id = await _repository.AddAsync(NewGame("Disco Elysium", "RPG", 9.5, "PC"));

            await AddReviewRowAsync(id, 8.0, "Long, and worth it.");
            await AddReviewRowAsync(id, null, "No number for this one.");

            GenreReviewSummary rpg = (await _repository.GetGenreAffinitiesAsync()).Single();

            rpg.ScoredReviewCount.Should().Be(1);
            rpg.AverageReviewScore.Should().Be(8.0);
        }

        [Fact]
        public async Task GetGenreAffinitiesAsync_LeavesOutGenresNobodyHasScored()
        {
            int reviewed = await _repository.AddAsync(NewGame("Disco Elysium", "RPG", 9.5, "PC"));
            await _repository.AddAsync(NewGame("Forza Horizon 5", "Racing", 9.0, "Xbox"));

            await AddReviewRowAsync(reviewed, 8.0, "Long, and worth it.");

            IReadOnlyList<GenreReviewSummary> affinities = await _repository.GetGenreAffinitiesAsync();

            affinities.Should().ContainSingle().Which.Genre.Should().Be("RPG");
        }

        [Fact]
        public async Task GetGenreAffinitiesAsync_OnAnEmptyCatalogue_ReturnsNothing()
        {
            IReadOnlyList<GenreReviewSummary> affinities = await _repository.GetGenreAffinitiesAsync();

            affinities.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public async Task GetStatisticsAsync_ReportsTheTotalsAndTheBreakdown()
        {
            int firstRpg = await _repository.AddAsync(NewGame("Disco Elysium", "RPG", 9.5, "PC"));
            await _repository.AddAsync(NewGame("Baldur's Gate 3", "RPG", 9.9, "PC"));
            await _repository.AddAsync(NewGame("Forza Horizon 5", "Racing", 9.0, "Xbox"));

            await AddReviewRowAsync(firstRpg, 8.0, "Long, and worth it.");
            await AddReviewRowAsync(firstRpg, null, "Still thinking about it.");

            CatalogueStatistics statistics = await _repository.GetStatisticsAsync();

            statistics.TotalGames.Should().Be(3);
            statistics.ReviewCount.Should().Be(2, "a review without a score is still a review");
            statistics.AverageScore.Should().BeApproximately((9.5 + 9.9 + 9.0) / 3.0, 0.000001);

            // Largest group first, then by name, so two runs over the same data agree.
            statistics.ByGenre.Select(row => row.Genre).Should().Equal("RPG", "Racing");

            GenreDistribution rpg = statistics.ByGenre.Single(row => row.Genre == "RPG");
            rpg.GameCount.Should().Be(2);
            rpg.AverageScore.Should().BeApproximately(9.7, 0.000001);
        }

        [Fact]
        public async Task GetStatisticsAsync_GamesWithNoGenre_GetARowOfTheirOwn()
        {
            // They are gathered under a null name rather than dropped, so the counts in the
            // breakdown still add up to the total printed beside them.
            await _repository.AddAsync(NewGame("Disco Elysium", "RPG", 9.5, "PC"));
            await _repository.AddAsync(new Game
            {
                Name = "Uncategorised",
                Genre = null,
                Platforms = new[] { "PC" },
                Score = 5.0,
            });

            CatalogueStatistics statistics = await _repository.GetStatisticsAsync();

            statistics.TotalGames.Should().Be(2);
            statistics.ByGenre.Sum(row => row.GameCount).Should().Be(statistics.TotalGames);

            GenreDistribution none = statistics.ByGenre.Single(row => row.Genre == null);
            none.GameCount.Should().Be(1);
            none.AverageScore.Should().Be(5.0);
        }

        [Fact]
        public async Task GetStatisticsAsync_WhenNoGameIsScored_ReportsNoAverageRatherThanZero()
        {
            await _repository.AddAsync(NewGame("Unrated One", "Action", null, "PC"));
            await _repository.AddAsync(NewGame("Unrated Two", "Action", null, "PC"));

            CatalogueStatistics statistics = await _repository.GetStatisticsAsync();

            statistics.TotalGames.Should().Be(2);
            statistics.AverageScore.Should().BeNull();
            statistics.ByGenre.Single().AverageScore.Should().BeNull();
        }

        [Fact]
        public async Task GetStatisticsAsync_OnAnEmptyCatalogue_ReportsZerosAndNoBreakdown()
        {
            CatalogueStatistics statistics = await _repository.GetStatisticsAsync();

            statistics.TotalGames.Should().Be(0);
            statistics.ReviewCount.Should().Be(0);
            statistics.AverageScore.Should().BeNull();
            statistics.ByGenre.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public async Task GetGenresAsync_ReturnsTheNamesSorted()
        {
            await _repository.AddAsync(NewGame("Anno 1800", "Strategy", 8.7, "PC"));
            await _repository.AddAsync(NewGame("Bastion", "Action", 8.6, "PC"));
            await _repository.AddAsync(NewGame("Control", "Adventure", 8.4, "PC"));

            IReadOnlyList<string> genres = await _repository.GetGenresAsync();

            genres.Should().Equal("Action", "Adventure", "Strategy");
        }

        [Fact]
        public async Task GetPlatformsAsync_ReturnsTheNamesSorted()
        {
            await _repository.AddAsync(NewGame("Anno 1800", "Strategy", 8.7, "PC", "Xbox"));
            await _repository.AddAsync(NewGame("Bastion", "Action", 8.6, "Switch", "PC"));

            IReadOnlyList<string> platforms = await _repository.GetPlatformsAsync();

            platforms.Should().Equal("PC", "Switch", "Xbox");
        }

        /// <summary>
        /// Builds a game whose fields are all set, so a test can change just the one it cares
        /// about.
        /// </summary>
        private static Game NewGame(string name, string genre, double? score, params string[] platforms) =>
            new Game
            {
                Name = name,
                Genre = genre,
                Platforms = platforms,
                Score = score,
                CoverUrl = "https://example.invalid/" + name.ToLowerInvariant().Replace(' ', '-') + ".png",
            };

        /// <summary>
        /// Stores a game in a chosen play state, which is what a test that filters on the state
        /// needs and what the plain factory deliberately leaves at its default.
        /// </summary>
        private Task<int> AddAsync(Game game, PlayStatus status, bool isFavourite)
        {
            game.Status = status;
            game.IsFavourite = isFavourite;

            return _repository.AddAsync(game);
        }

        /// <summary>
        /// Fills the catalogue with five games whose names and scores run in opposite directions,
        /// so that an ordering test cannot pass by accident on either column.
        /// </summary>
        private async Task AddManyAsync()
        {
            await _repository.AddAsync(NewGame("Anno 1800", "Strategy", 9.5, "PC"));
            await _repository.AddAsync(NewGame("Bastion", "Action", 8.5, "PC"));
            await _repository.AddAsync(NewGame("Control", "Action", 7.5, "PC"));
            await _repository.AddAsync(NewGame("Dishonored", "Action", 6.9, "PC"));
            await _repository.AddAsync(NewGame("Everspace", "Action", 6.0, "PC"));
        }

        /// <summary>
        /// Writes a review row directly, for a test that needs one to exist without making a
        /// statement about the review repository.
        /// </summary>
        private async Task AddReviewRowAsync(int gameId, double? score, string body)
        {
            using (DbConnection connection = await _fixture.OpenConnectionAsync().ConfigureAwait(false))
            {
                await connection
                    .ExecuteAsync(new CommandDefinition(InsertReviewSql, new
                    {
                        GameId = gameId,
                        Score = score,
                        Body = body,
                        CreatedAt = DateTimeOffset.UtcNow,
                    }))
                    .ConfigureAwait(false);
            }
        }

        private Task<int> CountAsync(string sql, object parameters) => ScalarAsync<int>(sql, parameters);

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
