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
    /// Dapper implementation of <see cref="IGameRepository"/> over the <c>dbo.Game</c> table.
    /// </summary>
    /// <remarks>
    /// Every statement is a constant with bound parameters, column lists are written out, and a
    /// row is always addressed by its identity. Addressing a row by title corrupts the wrong
    /// row without any error as soon as two games share a name.
    /// </remarks>
    public sealed class GameRepository : IGameRepository
    {
        /// <summary>
        /// One page of the listing. Every filter clause is skipped when its parameter is null,
        /// so a single fixed statement serves every combination of filters and nothing is ever
        /// assembled from strings.
        /// <para>
        /// The ordering is chosen by bound parameters rather than by pasting a column name into
        /// the statement. Each CASE yields NULL for the arms that are not selected, and the
        /// identity is the final tie-break so that paging is stable across requests.
        /// </para>
        /// <para>
        /// The predicate is repeated verbatim in <see cref="CountSql"/>. The two must stay in
        /// step; they are written out twice because building them from a shared fragment would
        /// mean assembling SQL at run time.
        /// </para>
        /// </summary>
        private const string ListSql = @"
SELECT Id, Name, Genre, [Platform], Score, CoverUrl, Comment
FROM   dbo.Game
WHERE  (@NamePattern IS NULL OR Name LIKE @NamePattern ESCAPE '\')
  AND  (@Genre       IS NULL OR Genre = @Genre)
  AND  (@Platform    IS NULL OR [Platform] = @Platform)
  AND  (@MinScore    IS NULL OR Score >= @MinScore)
  AND  (@MaxScore    IS NULL OR Score <= @MaxScore)
ORDER BY
    CASE WHEN @SortByScore = 0 AND @Descending = 0 THEN Name  END ASC,
    CASE WHEN @SortByScore = 0 AND @Descending = 1 THEN Name  END DESC,
    CASE WHEN @SortByScore = 1 AND @Descending = 0 THEN Score END ASC,
    CASE WHEN @SortByScore = 1 AND @Descending = 1 THEN Score END DESC,
    Id ASC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

        /// <summary>
        /// Total number of matching rows, filtered exactly as <see cref="ListSql"/> is.
        /// </summary>
        private const string CountSql = @"
SELECT COUNT(*)
FROM   dbo.Game
WHERE  (@NamePattern IS NULL OR Name LIKE @NamePattern ESCAPE '\')
  AND  (@Genre       IS NULL OR Genre = @Genre)
  AND  (@Platform    IS NULL OR [Platform] = @Platform)
  AND  (@MinScore    IS NULL OR Score >= @MinScore)
  AND  (@MaxScore    IS NULL OR Score <= @MaxScore);";

        private const string GetSql = @"
SELECT Id, Name, Genre, [Platform], Score, CoverUrl, Comment
FROM   dbo.Game
WHERE  Id = @Id;";

        private const string InsertSql = @"
INSERT INTO dbo.Game (Name, Genre, [Platform], Score, CoverUrl, Comment)
OUTPUT INSERTED.Id
VALUES (@Name, @Genre, @Platform, @Score, @CoverUrl, @Comment);";

        private const string UpdateSql = @"
UPDATE dbo.Game
SET    Name       = @Name,
       Genre      = @Genre,
       [Platform] = @Platform,
       Score      = @Score,
       CoverUrl   = @CoverUrl,
       Comment    = @Comment
WHERE  Id = @Id;";

        private const string DeleteSql = @"
DELETE FROM dbo.Game
WHERE  Id = @Id;";

        private const string GenresSql = @"
SELECT DISTINCT Genre
FROM   dbo.Game
WHERE  Genre IS NOT NULL AND LEN(Genre) > 0
ORDER BY Genre;";

        private const string PlatformsSql = @"
SELECT DISTINCT [Platform]
FROM   dbo.Game
WHERE  [Platform] IS NOT NULL AND LEN([Platform]) > 0
ORDER BY [Platform];";

        /// <summary>
        /// Reproduces the sampling the application has always used: let the server order the
        /// whole table by a fresh identifier and keep the first row.
        /// </summary>
        private const string RandomSql = @"
SELECT TOP 1 Id, Name, Genre, [Platform], Score, CoverUrl, Comment
FROM   dbo.Game
WHERE  (@MinScore IS NULL OR Score >= @MinScore)
ORDER BY NEWID();";

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

            GameFilter effective = filter ?? GameFilter.None;

            string namePattern = ToContainsPattern(effective.Name);
            string genre = Normalise(effective.Genre);
            string platform = Normalise(effective.Platform);

            var countParameters = new
            {
                NamePattern = namePattern,
                Genre = genre,
                Platform = platform,
                MinScore = effective.MinScore,
                MaxScore = effective.MaxScore,
            };

            var pageParameters = new
            {
                NamePattern = namePattern,
                Genre = genre,
                Platform = platform,
                MinScore = effective.MinScore,
                MaxScore = effective.MaxScore,
                SortByScore = sort == GameSortField.Score ? 1 : 0,
                Descending = descending ? 1 : 0,
                Offset = (page - 1) * pageSize,
                PageSize = pageSize,
            };

            using (DbConnection connection = _connections.Create())
            {
                await connection.OpenAsync(ct).ConfigureAwait(false);

                List<Game> items = (await connection
                    .QueryAsync<Game>(new CommandDefinition(ListSql, pageParameters, cancellationToken: ct))
                    .ConfigureAwait(false)).AsList();

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

                return await connection
                    .QuerySingleOrDefaultAsync<Game>(new CommandDefinition(GetSql, new { Id = id }, cancellationToken: ct))
                    .ConfigureAwait(false);
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

            var parameters = new
            {
                Name = game.Name,
                Genre = game.Genre,
                Platform = game.Platform,
                Score = game.Score,
                CoverUrl = game.CoverUrl,
                Comment = game.Comment,
            };

            using (DbConnection connection = _connections.Create())
            {
                await connection.OpenAsync(ct).ConfigureAwait(false);

                return await connection
                    .ExecuteScalarAsync<int>(new CommandDefinition(InsertSql, parameters, cancellationToken: ct))
                    .ConfigureAwait(false);
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

            var parameters = new
            {
                Id = game.Id,
                Name = game.Name,
                Genre = game.Genre,
                Platform = game.Platform,
                Score = game.Score,
                CoverUrl = game.CoverUrl,
                Comment = game.Comment,
            };

            using (DbConnection connection = _connections.Create())
            {
                await connection.OpenAsync(ct).ConfigureAwait(false);

                int affected = await connection
                    .ExecuteAsync(new CommandDefinition(UpdateSql, parameters, cancellationToken: ct))
                    .ConfigureAwait(false);

                return affected > 0;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            using (DbConnection connection = _connections.Create())
            {
                await connection.OpenAsync(ct).ConfigureAwait(false);

                int affected = await connection
                    .ExecuteAsync(new CommandDefinition(DeleteSql, new { Id = id }, cancellationToken: ct))
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
        public async Task<Game> GetRandomAsync(double minScore, CancellationToken ct = default)
        {
            using (DbConnection connection = _connections.Create())
            {
                await connection.OpenAsync(ct).ConfigureAwait(false);

                return await connection
                    .QueryFirstOrDefaultAsync<Game>(
                        new CommandDefinition(RandomSql, new { MinScore = minScore }, cancellationToken: ct))
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
    }
}
