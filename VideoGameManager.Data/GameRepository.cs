using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using VideoGameManager.Domain;

namespace VideoGameManager.Data
{
    /// <summary>
    /// Dapper implementation of <see cref="IGameRepository"/>.
    /// </summary>
    /// <remarks>
    /// A game is spread over four tables: the row itself, the genre it points at, the bridge that
    /// attaches it to platforms, and the reviews written for it. This class is the only place that
    /// knows about that; everything above it keeps working with a game that carries a genre name,
    /// a list of platform names and the text of its newest review.
    /// <para>
    /// Every statement is a constant with bound parameters, column lists are written out, and a
    /// row is always addressed by its identity. Addressing a row by title corrupts the wrong row
    /// without any error as soon as two games share a name.
    /// </para>
    /// </remarks>
    public sealed class GameRepository : IGameRepository
    {
        /// <summary>
        /// The columns every read of a game returns.
        /// </summary>
        /// <remarks>
        /// The genre arrives as a name through a left join, so a game whose genre has not been set
        /// still comes back rather than disappearing from the listing. The newest review is picked
        /// per row by an OUTER APPLY: newest is defined as the latest moment, and because two
        /// reviews can be written inside the same millisecond the identity breaks the tie, which
        /// keeps the answer stable between two runs of the same query.
        /// </remarks>
        private const string SelectSql = @"
SELECT      g.Id, g.Name, ge.Name AS Genre, g.Score, g.CoverUrl, g.[Status], g.IsFavourite,
            lr.Body AS LatestReview
FROM        dbo.Game AS g
LEFT JOIN   dbo.Genre AS ge ON ge.Id = g.GenreId
OUTER APPLY (SELECT TOP 1 r.Body
             FROM   dbo.Review AS r
             WHERE  r.GameId = g.Id
             ORDER BY r.CreatedAt DESC, r.Id DESC) AS lr";

        /// <summary>
        /// The listing predicate. Every clause is skipped when its parameter is null, so one fixed
        /// statement serves every combination of filters and nothing is ever assembled from
        /// strings.
        /// </summary>
        /// <remarks>
        /// The listing, the count beside it and the random pick all build on this one constant
        /// rather than on copies of it. A copied predicate drifts the moment one of them gains a
        /// clause: the count would then disagree with the rows the pages hold, and a suggestion
        /// would be drawn from a set the user is not looking at. Neither failure announces itself.
        /// </remarks>
        private const string ListWhereSql = @"
WHERE  (@NamePattern    IS NULL OR g.Name LIKE @NamePattern ESCAPE '\')
  AND  (@Genre          IS NULL OR ge.Name = @Genre)
  AND  (@PlatformName   IS NULL OR EXISTS (
            SELECT 1
            FROM   dbo.GamePlatform AS gp
            INNER JOIN dbo.Platform AS p ON p.Id = gp.PlatformId
            WHERE  gp.GameId = g.Id AND p.Name = @PlatformName))
  AND  (@MinScore       IS NULL OR g.Score >= @MinScore)
  AND  (@MaxScore       IS NULL OR g.Score <= @MaxScore)
  AND  (@Status         IS NULL OR g.[Status] = @Status)
  AND  (@OnlyFavourites IS NULL OR g.IsFavourite = 1)";

        private const string OrderByNameAscSql = @"
ORDER BY g.Name ASC, g.Id ASC";

        private const string OrderByNameDescSql = @"
ORDER BY g.Name DESC, g.Id ASC";

        private const string OrderByScoreAscSql = @"
ORDER BY g.Score ASC, g.Id ASC";

        private const string OrderByScoreDescSql = @"
ORDER BY g.Score DESC, g.Id ASC";

        private const string PageSql = @"
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

        // The four listing statements below are built by joining constants at compile time: the
        // result is a literal baked into the assembly, exactly as if it had been typed out four
        // times, and no value the caller supplies can reach it. Writing one statement instead,
        // with the ordering chosen by CASE expressions over bound parameters, produced an ORDER BY
        // the server cannot satisfy from an index, so every listing paid for a full sort.
        private const string ListByNameAscSql = SelectSql + ListWhereSql + OrderByNameAscSql + PageSql;

        private const string ListByNameDescSql = SelectSql + ListWhereSql + OrderByNameDescSql + PageSql;

        private const string ListByScoreAscSql = SelectSql + ListWhereSql + OrderByScoreAscSql + PageSql;

        private const string ListByScoreDescSql = SelectSql + ListWhereSql + OrderByScoreDescSql + PageSql;

        /// <summary>
        /// Row source for the count: the same two tables the listing reads, without the newest
        /// review, because counting rows does not need it.
        /// </summary>
        private const string CountFromSql = @"
SELECT      COUNT(*)
FROM        dbo.Game AS g
LEFT JOIN   dbo.Genre AS ge ON ge.Id = g.GenreId";

        /// <summary>
        /// Total number of matching rows. It is built from the listing's own predicate rather
        /// than from a copy of it, so the reported total cannot drift away from the rows the
        /// pages actually contain.
        /// </summary>
        private const string CountSql = CountFromSql + ListWhereSql + ";";

        private const string GetWhereSql = @"
WHERE  g.Id = @Id;";

        private const string GetSql = SelectSql + GetWhereSql;

        /// <summary>
        /// Reproduces the sampling the application has always used: let the server order the
        /// candidates by a fresh identifier and keep the first row. The single row is asked for
        /// with OFFSET/FETCH rather than TOP so that the shared select body can be reused
        /// unchanged; both express the same one row.
        /// </summary>
        private const string RandomOrderSql = @"
ORDER BY NEWID()
OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY;";

        /// <summary>
        /// The random pick, over the listing's own row source and the listing's own predicate.
        /// </summary>
        private const string RandomSql = SelectSql + ListWhereSql + RandomOrderSql;

        /// <summary>
        /// Every genre on offer.
        /// </summary>
        /// <remarks>
        /// This reads the lookup table, so it also lists a genre that no game currently uses.
        /// Before genres had a table of their own the same call returned the distinct values found
        /// on the games themselves; today the two answers are identical, because the lookup rows
        /// were created from those values.
        /// </remarks>
        private const string GenresSql = @"
SELECT   Name
FROM     dbo.Genre
ORDER BY Name;";

        /// <summary>
        /// Every platform on offer, read from the lookup table for the same reason as the genres.
        /// </summary>
        private const string PlatformsSql = @"
SELECT   Name
FROM     dbo.Platform
ORDER BY Name;";

        /// <summary>
        /// The platforms attached to a set of games, in one round trip. Dapper expands the
        /// identity list into one bound parameter per entry, so the list never becomes text.
        /// </summary>
        private const string PlatformsForGamesSql = @"
SELECT     gp.GameId, p.Name
FROM       dbo.GamePlatform AS gp
INNER JOIN dbo.Platform AS p ON p.Id = gp.PlatformId
WHERE      gp.GameId IN @GameIds
ORDER BY   gp.GameId, p.Name;";

        /// <summary>
        /// Finds the genre with this name, creating it when it is new, and returns its identity.
        /// </summary>
        /// <remarks>
        /// The lock hints hold the range the lookup examined until the surrounding transaction
        /// ends. Without them two writers adding the same new genre at the same moment would both
        /// find nothing, both insert, and the second would fail against the unique constraint on
        /// the name.
        /// </remarks>
        private const string EnsureGenreSql = @"
DECLARE @GenreId INT;
SELECT @GenreId = Id FROM dbo.Genre WITH (UPDLOCK, HOLDLOCK) WHERE Name = @Name;
IF @GenreId IS NULL
BEGIN
    INSERT INTO dbo.Genre (Name) VALUES (@Name);
    SET @GenreId = CAST(SCOPE_IDENTITY() AS INT);
END
SELECT @GenreId;";

        /// <summary>
        /// The same lookup-or-create for a platform, with the same locking for the same reason.
        /// </summary>
        private const string EnsurePlatformSql = @"
DECLARE @PlatformId INT;
SELECT @PlatformId = Id FROM dbo.Platform WITH (UPDLOCK, HOLDLOCK) WHERE Name = @Name;
IF @PlatformId IS NULL
BEGIN
    INSERT INTO dbo.Platform (Name) VALUES (@Name);
    SET @PlatformId = CAST(SCOPE_IDENTITY() AS INT);
END
SELECT @PlatformId;";

        private const string InsertGameSql = @"
INSERT INTO dbo.Game (Name, GenreId, Score, CoverUrl, [Status], IsFavourite)
OUTPUT INSERTED.Id
VALUES (@Name, @GenreId, @Score, @CoverUrl, @Status, @IsFavourite);";

        private const string UpdateGameSql = @"
UPDATE dbo.Game
SET    Name        = @Name,
       GenreId     = @GenreId,
       Score       = @Score,
       CoverUrl    = @CoverUrl,
       [Status]    = @Status,
       IsFavourite = @IsFavourite
WHERE  Id = @Id;";

        /// <summary>
        /// Counts the scored reviews per genre and averages them.
        /// </summary>
        /// <remarks>
        /// Reviews without a score are excluded before the grouping rather than averaged as
        /// zero, which would drag every genre towards the bottom of the scale for no reason other
        /// than someone having written prose without a number. The joins are inner ones on
        /// purpose: a review of a game that belongs to no genre has no genre to be attributed to,
        /// and a genre with no scored review has nothing to average, so neither produces a row.
        /// </remarks>
        private const string GenreAffinitiesSql = @"
SELECT      ge.Name        AS Genre,
            COUNT(*)       AS ScoredReviewCount,
            AVG(r.Score)   AS AverageReviewScore
FROM        dbo.Review AS r
INNER JOIN  dbo.Game   AS g  ON g.Id  = r.GameId
INNER JOIN  dbo.Genre  AS ge ON ge.Id = g.GenreId
WHERE       r.Score IS NOT NULL
GROUP BY    ge.Name
ORDER BY    ge.Name;";

        /// <summary>
        /// The catalogue totals, as one row.
        /// </summary>
        /// <remarks>
        /// The average is of the games' own scores. Unscored games are left out of it by the
        /// aggregate itself, and an empty catalogue yields no average at all rather than a zero
        /// that would read as "everything here is terrible".
        /// </remarks>
        private const string StatisticsTotalsSql = @"
SELECT (SELECT COUNT(*)   FROM dbo.Game)   AS TotalGames,
       (SELECT COUNT(*)   FROM dbo.Review) AS ReviewCount,
       (SELECT AVG(Score) FROM dbo.Game)   AS AverageScore;";

        /// <summary>
        /// The per-genre breakdown of the catalogue.
        /// </summary>
        /// <remarks>
        /// The join to the lookup is a left one, so games that belong to no genre are gathered
        /// into a single row whose name is null instead of disappearing. Dropping them would make
        /// the breakdown add up to less than the total beside it, and nothing on the screen would
        /// say why. Largest group first is what a reader wants; the name settles ties so that two
        /// runs over the same data give the same order.
        /// </remarks>
        private const string StatisticsByGenreSql = @"
SELECT      ge.Name      AS Genre,
            COUNT(*)     AS GameCount,
            AVG(g.Score) AS AverageScore
FROM        dbo.Game  AS g
LEFT JOIN   dbo.Genre AS ge ON ge.Id = g.GenreId
GROUP BY    ge.Name
ORDER BY    COUNT(*) DESC, ge.Name;";

        private const string StatisticsSql = StatisticsTotalsSql + StatisticsByGenreSql;

        /// <summary>
        /// Attaches a game to a platform. The existence check keeps the statement repeatable, so a
        /// list that happens to mention the same platform twice cannot break the write against the
        /// primary key of the bridge table.
        /// </summary>
        private const string LinkPlatformSql = @"
INSERT INTO dbo.GamePlatform (GameId, PlatformId)
SELECT @GameId, @PlatformId
WHERE  NOT EXISTS (SELECT 1
                   FROM   dbo.GamePlatform
                   WHERE  GameId = @GameId AND PlatformId = @PlatformId);";

        private const string ClearPlatformsSql = @"
DELETE FROM dbo.GamePlatform
WHERE  GameId = @GameId;";

        /// <summary>
        /// Removes a game. Its bridge rows and its reviews go with it through the cascade on their
        /// foreign keys, so nothing is left pointing at an identity that no longer exists.
        /// </summary>
        private const string DeleteGameSql = @"
DELETE FROM dbo.Game
WHERE  Id = @Id;";

        private readonly IDbConnectionFactory _connections;

        /// <summary>
        /// Creates the repository.
        /// </summary>
        /// <param name="connections">Factory that hands out connections.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connections"/> is <c>null</c>.</exception>
        public GameRepository(IDbConnectionFactory connections)
        {
            _connections = connections ?? throw new ArgumentNullException(nameof(connections));
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="page"/> or <paramref name="pageSize"/> is below one.
        /// </exception>
        public async Task<PagedResult<Game>> ListAsync(GameFilter filter, int page, int pageSize,
            GameSortField sort = GameSortField.Name, bool descending = false,
            CancellationToken ct = default)
        {
            if (page < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(page), page, "Page numbering starts at one.");
            }

            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "A page holds at least one row.");
            }

            FilterArguments countParameters = ToArguments(filter);
            PageArguments pageParameters = ToArguments(filter, page, pageSize);

            using (DbConnection connection = _connections.Create())
            {
                await connection.OpenAsync(ct).ConfigureAwait(false);

                List<Game> items = (await connection
                    .QueryAsync<Game>(new CommandDefinition(OrderedListSql(sort, descending), pageParameters, cancellationToken: ct))
                    .ConfigureAwait(false)).AsList();

                await AttachPlatformsAsync(connection, items, ct).ConfigureAwait(false);

                int total = await connection
                    .ExecuteScalarAsync<int>(new CommandDefinition(CountSql, countParameters, cancellationToken: ct))
                    .ConfigureAwait(false);

                return new PagedResult<Game>(items, total, page, pageSize);
            }
        }

        /// <inheritdoc />
        public async Task<Game> GetAsync(int id, CancellationToken ct = default)
        {
            using (DbConnection connection = _connections.Create())
            {
                await connection.OpenAsync(ct).ConfigureAwait(false);

                Game game = await connection
                    .QuerySingleOrDefaultAsync<Game>(new CommandDefinition(GetSql, new { Id = id }, cancellationToken: ct))
                    .ConfigureAwait(false);

                await AttachPlatformsAsync(connection, ToList(game), ct).ConfigureAwait(false);

                return game;
            }
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException"><paramref name="game"/> is <c>null</c>.</exception>
        public async Task<int> AddAsync(Game game, CancellationToken ct = default)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            using (DbConnection connection = _connections.Create())
            {
                await connection.OpenAsync(ct).ConfigureAwait(false);

                // The row, its genre and its platform links are one change. Committing only part
                // of it would leave a game the listing shows on no platform at all.
                using (DbTransaction transaction = await connection.BeginTransactionAsync(ct).ConfigureAwait(false))
                {
                    int? genreId = await EnsureGenreAsync(connection, transaction, game.Genre, ct).ConfigureAwait(false);

                    var parameters = new
                    {
                        Name = game.Name,
                        GenreId = genreId,
                        Score = game.Score,
                        CoverUrl = game.CoverUrl,
                        Status = (byte)game.Status,
                        IsFavourite = game.IsFavourite,
                    };

                    int id = await connection
                        .ExecuteScalarAsync<int>(
                            new CommandDefinition(InsertGameSql, parameters, transaction, cancellationToken: ct))
                        .ConfigureAwait(false);

                    await LinkPlatformsAsync(connection, transaction, id, game.Platforms, ct).ConfigureAwait(false);

                    await transaction.CommitAsync(ct).ConfigureAwait(false);

                    return id;
                }
            }
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException"><paramref name="game"/> is <c>null</c>.</exception>
        public async Task<bool> UpdateAsync(Game game, CancellationToken ct = default)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            using (DbConnection connection = _connections.Create())
            {
                await connection.OpenAsync(ct).ConfigureAwait(false);

                using (DbTransaction transaction = await connection.BeginTransactionAsync(ct).ConfigureAwait(false))
                {
                    int? genreId = await EnsureGenreAsync(connection, transaction, game.Genre, ct).ConfigureAwait(false);

                    var parameters = new
                    {
                        Id = game.Id,
                        Name = game.Name,
                        GenreId = genreId,
                        Score = game.Score,
                        CoverUrl = game.CoverUrl,
                        Status = (byte)game.Status,
                        IsFavourite = game.IsFavourite,
                    };

                    int affected = await connection
                        .ExecuteAsync(
                            new CommandDefinition(UpdateGameSql, parameters, transaction, cancellationToken: ct))
                        .ConfigureAwait(false);

                    if (affected == 0)
                    {
                        // No row carries that identity. Leaving the transaction uncommitted rolls
                        // back the genre this call may have created for a game that does not exist.
                        return false;
                    }

                    // The platform list is replaced rather than merged: the caller sends the whole
                    // list, so a platform it no longer mentions has been taken away.
                    await connection
                        .ExecuteAsync(
                            new CommandDefinition(ClearPlatformsSql, new { GameId = game.Id }, transaction, cancellationToken: ct))
                        .ConfigureAwait(false);

                    await LinkPlatformsAsync(connection, transaction, game.Id, game.Platforms, ct).ConfigureAwait(false);

                    await transaction.CommitAsync(ct).ConfigureAwait(false);

                    return true;
                }
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            using (DbConnection connection = _connections.Create())
            {
                await connection.OpenAsync(ct).ConfigureAwait(false);

                int affected = await connection
                    .ExecuteAsync(new CommandDefinition(DeleteGameSql, new { Id = id }, cancellationToken: ct))
                    .ConfigureAwait(false);

                return affected > 0;
            }
        }

        /// <inheritdoc />
        public Task<IReadOnlyList<string>> GetGenresAsync(CancellationToken ct = default) =>
            ReadStringsAsync(GenresSql, ct);

        /// <inheritdoc />
        public Task<IReadOnlyList<string>> GetPlatformsAsync(CancellationToken ct = default) =>
            ReadStringsAsync(PlatformsSql, ct);

        /// <inheritdoc />
        public async Task<Game> GetRandomAsync(GameFilter filter, CancellationToken ct = default)
        {
            using (DbConnection connection = _connections.Create())
            {
                await connection.OpenAsync(ct).ConfigureAwait(false);

                Game game = await connection
                    .QueryFirstOrDefaultAsync<Game>(
                        new CommandDefinition(RandomSql, ToArguments(filter), cancellationToken: ct))
                    .ConfigureAwait(false);

                await AttachPlatformsAsync(connection, ToList(game), ct).ConfigureAwait(false);

                return game;
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<GenreReviewSummary>> GetGenreAffinitiesAsync(CancellationToken ct = default)
        {
            using (DbConnection connection = _connections.Create())
            {
                await connection.OpenAsync(ct).ConfigureAwait(false);

                return (await connection
                    .QueryAsync<GenreReviewSummary>(new CommandDefinition(GenreAffinitiesSql, cancellationToken: ct))
                    .ConfigureAwait(false)).AsList();
            }
        }

        /// <inheritdoc />
        public async Task<CatalogueStatistics> GetStatisticsAsync(CancellationToken ct = default)
        {
            using (DbConnection connection = _connections.Create())
            {
                await connection.OpenAsync(ct).ConfigureAwait(false);

                // Both questions travel in one command, so the totals and the breakdown are read
                // from the same catalogue rather than from two moments a write could fall between.
                using (SqlMapper.GridReader grid = await connection
                    .QueryMultipleAsync(new CommandDefinition(StatisticsSql, cancellationToken: ct))
                    .ConfigureAwait(false))
                {
                    CatalogueTotals totals = await grid.ReadSingleAsync<CatalogueTotals>().ConfigureAwait(false);

                    List<GenreDistribution> byGenre =
                        (await grid.ReadAsync<GenreDistribution>().ConfigureAwait(false)).AsList();

                    return new CatalogueStatistics(
                        totals.TotalGames, totals.ReviewCount, totals.AverageScore, byGenre);
                }
            }
        }

        /// <summary>
        /// Picks the statement that orders the listing the way the caller asked for.
        /// </summary>
        /// <remarks>
        /// The choice is between four constants rather than a column name pasted into one
        /// statement, so the ordering can never carry anything the caller supplied. The identity
        /// is the last tie-break in all four, which stops a row from drifting between pages when
        /// several rows share a title or a score.
        /// </remarks>
        private static string OrderedListSql(GameSortField sort, bool descending)
        {
            switch (sort)
            {
                case GameSortField.Score:
                    return descending ? ListByScoreDescSql : ListByScoreAscSql;
                default:
                    return descending ? ListByNameDescSql : ListByNameAscSql;
            }
        }

        /// <summary>
        /// Fills in <see cref="Game.Platforms"/> for games that have just been read.
        /// </summary>
        /// <remarks>
        /// The platforms are not a column, so they arrive in a second query over the same open
        /// connection and are matched up here. A game with no bridge rows is given an empty list
        /// rather than being left at <c>null</c>, which is what the entity promises its callers.
        /// </remarks>
        private static async Task AttachPlatformsAsync(DbConnection connection, IReadOnlyList<Game> games,
            CancellationToken ct)
        {
            if (games.Count == 0)
            {
                // No identities to ask about; the query would have an empty list to expand.
                return;
            }

            int[] ids = new int[games.Count];

            for (int i = 0; i < games.Count; i++)
            {
                ids[i] = games[i].Id;
            }

            IEnumerable<PlatformLink> links = await connection
                .QueryAsync<PlatformLink>(
                    new CommandDefinition(PlatformsForGamesSql, new { GameIds = ids }, cancellationToken: ct))
                .ConfigureAwait(false);

            Dictionary<int, List<string>> byGame = new Dictionary<int, List<string>>();

            foreach (PlatformLink link in links)
            {
                if (!byGame.TryGetValue(link.GameId, out List<string> names))
                {
                    names = new List<string>();
                    byGame.Add(link.GameId, names);
                }

                names.Add(link.Name);
            }

            foreach (Game game in games)
            {
                game.Platforms = byGame.TryGetValue(game.Id, out List<string> names)
                    ? names
                    : (IReadOnlyList<string>)Array.Empty<string>();
            }
        }

        /// <summary>
        /// Looks the genre up by name, creating it when it is new.
        /// </summary>
        /// <returns>
        /// The identity of the genre, or <c>null</c> when no genre was given. Validation requires
        /// one, but the repository still stores a game without one rather than failing, because
        /// the column accepts no genre at all.
        /// </returns>
        private static async Task<int?> EnsureGenreAsync(DbConnection connection, DbTransaction transaction,
            string genre, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(genre))
            {
                return null;
            }

            return await connection
                .ExecuteScalarAsync<int>(
                    new CommandDefinition(EnsureGenreSql, new { Name = genre.Trim() }, transaction, cancellationToken: ct))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Attaches a game to every platform in the list, creating the ones that are new.
        /// </summary>
        private static async Task LinkPlatformsAsync(DbConnection connection, DbTransaction transaction, int gameId,
            IReadOnlyList<string> platforms, CancellationToken ct)
        {
            if (platforms == null)
            {
                return;
            }

            foreach (string platform in platforms)
            {
                if (string.IsNullOrWhiteSpace(platform))
                {
                    continue;
                }

                int platformId = await connection
                    .ExecuteScalarAsync<int>(
                        new CommandDefinition(EnsurePlatformSql, new { Name = platform.Trim() }, transaction, cancellationToken: ct))
                    .ConfigureAwait(false);

                await connection
                    .ExecuteAsync(
                        new CommandDefinition(LinkPlatformSql, new { GameId = gameId, PlatformId = platformId }, transaction, cancellationToken: ct))
                    .ConfigureAwait(false);
            }
        }

        private async Task<IReadOnlyList<string>> ReadStringsAsync(string sql, CancellationToken ct)
        {
            using (DbConnection connection = _connections.Create())
            {
                await connection.OpenAsync(ct).ConfigureAwait(false);

                return (await connection
                    .QueryAsync<string>(new CommandDefinition(sql, cancellationToken: ct))
                    .ConfigureAwait(false)).AsList();
            }
        }

        /// <summary>
        /// Wraps a single game so that the one stitching routine serves the single-row reads too.
        /// A missing game becomes an empty list and nothing further is asked of the database.
        /// </summary>
        private static IReadOnlyList<Game> ToList(Game game) =>
            game == null ? Array.Empty<Game>() : new[] { game };

        /// <summary>
        /// Trims a filter value and turns an empty one into <c>null</c>, so that an untouched
        /// search box switches its clause off instead of looking for the empty string.
        /// </summary>
        private static string Normalise(string value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        /// <summary>
        /// Builds a contains-pattern for LIKE. Wildcards typed by the user are escaped so that
        /// a title containing a percent sign is searched for literally rather than matching
        /// everything.
        /// </summary>
        private static string ToContainsPattern(string value)
        {
            string trimmed = Normalise(value);

            if (trimmed == null)
            {
                return null;
            }

            string escaped = trimmed
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_")
                .Replace("[", "\\[");

            return string.Concat("%", escaped, "%");
        }

        /// <summary>
        /// Turns a filter into the bound values the shared predicate expects.
        /// </summary>
        private static FilterArguments ToArguments(GameFilter filter) =>
            Fill(new FilterArguments(), filter);

        /// <summary>
        /// The same values, plus the window one page of the listing needs.
        /// </summary>
        private static PageArguments ToArguments(GameFilter filter, int page, int pageSize)
        {
            PageArguments arguments = Fill(new PageArguments(), filter);

            arguments.Offset = (page - 1) * pageSize;
            arguments.PageSize = pageSize;

            return arguments;
        }

        /// <summary>
        /// Copies a filter onto the argument object every statement that uses the shared
        /// predicate binds.
        /// </summary>
        /// <remarks>
        /// The values are typed rather than gathered into an untyped bag, so that a clause
        /// comparing a number really does receive a number: an argument the provider has to guess
        /// the type of arrives as text and makes the server convert on every row.
        /// </remarks>
        private static T Fill<T>(T arguments, GameFilter filter)
            where T : FilterArguments
        {
            GameFilter effective = filter ?? GameFilter.None;

            arguments.NamePattern = ToContainsPattern(effective.Name);
            arguments.Genre = Normalise(effective.Genre);
            arguments.PlatformName = Normalise(effective.Platform);
            arguments.MinScore = effective.MinScore;
            arguments.MaxScore = effective.MaxScore;
            arguments.Status = effective.Status.HasValue ? (byte)effective.Status.Value : (byte?)null;

            // Asking for favourites narrows the listing; not asking for them does not widen it to
            // the games nobody marked. Both "no opinion" and "false" therefore switch the clause
            // off, which is what a checkbox nobody ticked has to mean.
            arguments.OnlyFavourites = effective.OnlyFavourites == true ? (bool?)true : null;

            return arguments;
        }

        /// <summary>
        /// The values bound to the shared listing predicate.
        /// </summary>
        private class FilterArguments
        {
            public string NamePattern { get; set; }

            public string Genre { get; set; }

            public string PlatformName { get; set; }

            public double? MinScore { get; set; }

            public double? MaxScore { get; set; }

            public byte? Status { get; set; }

            public bool? OnlyFavourites { get; set; }
        }

        /// <summary>
        /// The listing predicate plus the page window. Inheriting keeps the two statements bound
        /// to one set of filter values instead of two lists that have to be kept in step.
        /// </summary>
        private sealed class PageArguments : FilterArguments
        {
            public int Offset { get; set; }

            public int PageSize { get; set; }
        }

        /// <summary>
        /// The single row of catalogue totals, before the breakdown is attached to it.
        /// </summary>
        private sealed class CatalogueTotals
        {
            public int TotalGames { get; set; }

            public int ReviewCount { get; set; }

            public double? AverageScore { get; set; }
        }

        /// <summary>
        /// One row of the bridge query: which game is attached to which platform name.
        /// </summary>
        private sealed class PlatformLink
        {
            public int GameId { get; set; }

            public string Name { get; set; }
        }
    }
}
