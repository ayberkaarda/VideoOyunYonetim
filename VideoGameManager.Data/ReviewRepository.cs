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
    /// Dapper implementation of <see cref="IReviewRepository"/> over the <c>dbo.Review</c> table.
    /// </summary>
    /// <remarks>
    /// Reviews are rows of their own, so a game keeps every review ever written for it and each
    /// one carries its own identity, its own score and the moment it was written.
    /// <para>
    /// Writing a review also refreshes the rating on the game when the review carries a score,
    /// which is the behaviour the review screen has always had. The two writes are one
    /// transaction: a stored review whose score never reached the game would leave the catalogue
    /// showing a rating nothing supports.
    /// </para>
    /// <para>
    /// Rows are addressed by identity. The statement this replaced matched a game on its title, so
    /// two games sharing a name meant the review landed on the wrong row and the application still
    /// reported success.
    /// </para>
    /// </remarks>
    public sealed class ReviewRepository : IReviewRepository
    {
        /// <summary>
        /// Every review written for one game, newest first. Two reviews written inside the same
        /// millisecond are separated by their identity, so the order never wobbles between runs.
        /// </summary>
        private const string SelectSql = @"
SELECT   Id, GameId, Score, Body, CreatedAt
FROM     dbo.Review
WHERE    GameId = @GameId
ORDER BY CreatedAt DESC, Id DESC;";

        /// <summary>
        /// Whether the game a review points at still exists. Checked inside the transaction so
        /// that a review for a game deleted meanwhile is reported as a missing game rather than
        /// surfacing as a foreign key violation the user cannot read.
        /// </summary>
        private const string GameExistsSql = @"
SELECT COUNT(1)
FROM   dbo.Game
WHERE  Id = @GameId;";

        private const string InsertSql = @"
INSERT INTO dbo.Review (GameId, Score, Body, CreatedAt)
OUTPUT INSERTED.Id
VALUES (@GameId, @Score, @Body, @CreatedAt);";

        /// <summary>
        /// Copies the score of the new review onto the game. A review without a score leaves the
        /// existing rating untouched, which is what the review screen has always done, so the
        /// statement matches nothing at all when no score was given.
        /// </summary>
        private const string SyncScoreSql = @"
UPDATE dbo.Game
SET    Score = @Score
WHERE  Id = @GameId AND @Score IS NOT NULL;";

        private readonly IDbConnectionFactory _connections;

        /// <summary>
        /// Creates the repository.
        /// </summary>
        /// <param name="connections">Factory that hands out connections.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connections"/> is <c>null</c>.</exception>
        public ReviewRepository(IDbConnectionFactory connections)
        {
            _connections = connections ?? throw new ArgumentNullException(nameof(connections));
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Review>> GetForGameAsync(int gameId, CancellationToken ct = default)
        {
            using (DbConnection connection = _connections.Create())
            {
                await connection.OpenAsync(ct).ConfigureAwait(false);

                return (await connection
                    .QueryAsync<Review>(
                        new CommandDefinition(SelectSql, new { GameId = gameId }, cancellationToken: ct))
                    .ConfigureAwait(false)).AsList();
            }
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException"><paramref name="review"/> is <c>null</c>.</exception>
        public async Task<int> AddAsync(Review review, CancellationToken ct = default)
        {
            if (review == null)
            {
                throw new ArgumentNullException(nameof(review));
            }

            var parameters = new
            {
                GameId = review.GameId,
                Body = review.Body,
                Score = review.Score,

                // The moment comes from the caller rather than from a column default, so the value
                // that was validated is the value that is stored.
                CreatedAt = review.CreatedAt,
            };

            using (DbConnection connection = _connections.Create())
            {
                await connection.OpenAsync(ct).ConfigureAwait(false);

                using (DbTransaction transaction = await connection.BeginTransactionAsync(ct).ConfigureAwait(false))
                {
                    int games = await connection
                        .ExecuteScalarAsync<int>(
                            new CommandDefinition(GameExistsSql, new { GameId = review.GameId }, transaction, cancellationToken: ct))
                        .ConfigureAwait(false);

                    if (games == 0)
                    {
                        // Nothing to attach the review to. Leaving the transaction uncommitted
                        // undoes nothing here, and zero is the answer the contract asks for.
                        return 0;
                    }

                    int id = await connection
                        .ExecuteScalarAsync<int>(
                            new CommandDefinition(InsertSql, parameters, transaction, cancellationToken: ct))
                        .ConfigureAwait(false);

                    await connection
                        .ExecuteAsync(
                            new CommandDefinition(SyncScoreSql, parameters, transaction, cancellationToken: ct))
                        .ConfigureAwait(false);

                    await transaction.CommitAsync(ct).ConfigureAwait(false);

                    return id;
                }
            }
        }
    }
}
