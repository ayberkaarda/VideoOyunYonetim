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
    /// Dapper implementation of <see cref="IReviewRepository"/>.
    /// </summary>
    /// <remarks>
    /// The current schema has no review table. A game carries a single review in
    /// <c>dbo.Game.Comment</c>, which is what the application has always written, so a review
    /// is read from and written to that column and the game identity doubles as the review
    /// identity. There is no column for the moment a review was written, so
    /// <see cref="Review.CreatedAt"/> is left at its default on the way out and ignored on the
    /// way in. When reviews move into a table of their own this class changes and the interface
    /// does not.
    /// <para>
    /// The row is addressed by identity. The statement this replaced matched on the title, so
    /// two games sharing a name meant the review landed on the wrong row and the application
    /// still reported success.
    /// </para>
    /// </remarks>
    public sealed class ReviewRepository : IReviewRepository
    {
        private const string SelectSql = @"
SELECT Id, Score, Comment
FROM   dbo.Game
WHERE  Id = @GameId;";

        /// <summary>
        /// Writes the review text and, when the review carries a score, the score as well.
        /// A review without a score leaves the existing rating untouched, which is how the
        /// review screen has always behaved.
        /// </summary>
        private const string UpsertSql = @"
UPDATE dbo.Game
SET    Comment = @Body,
       Score   = COALESCE(@Score, Score)
WHERE  Id = @GameId;";

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

                StoredReview stored = await connection
                    .QuerySingleOrDefaultAsync<StoredReview>(
                        new CommandDefinition(SelectSql, new { GameId = gameId }, cancellationToken: ct))
                    .ConfigureAwait(false);

                if (stored == null || string.IsNullOrWhiteSpace(stored.Comment))
                {
                    return new Review[0];
                }

                return new[]
                {
                    new Review
                    {
                        Id = stored.Id,
                        GameId = stored.Id,
                        Score = stored.Score,
                        Body = stored.Comment,
                    },
                };
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
            };

            using (DbConnection connection = _connections.Create())
            {
                await connection.OpenAsync(ct).ConfigureAwait(false);

                int affected = await connection
                    .ExecuteAsync(new CommandDefinition(UpsertSql, parameters, cancellationToken: ct))
                    .ConfigureAwait(false);

                return affected > 0 ? review.GameId : 0;
            }
        }

        /// <summary>
        /// Shape of the row that holds a review in the current schema.
        /// </summary>
        private sealed class StoredReview
        {
            public int Id { get; set; }

            public double? Score { get; set; }

            public string Comment { get; set; }
        }
    }
}
