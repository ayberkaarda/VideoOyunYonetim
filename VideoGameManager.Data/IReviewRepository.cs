using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VideoGameManager.Domain;

namespace VideoGameManager.Data
{
    /// <summary>
    /// Reads and writes the reviews attached to a game.
    /// </summary>
    /// <remarks>
    /// Implementations do not catch database failures; the service layer wraps them.
    /// </remarks>
    public interface IReviewRepository
    {
        /// <summary>
        /// Reads the reviews written for one game, newest first.
        /// </summary>
        /// <param name="gameId">Identity of the game.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The reviews, empty when the game has none or does not exist.</returns>
        Task<IReadOnlyList<Review>> GetForGameAsync(int gameId, CancellationToken ct = default);

        /// <summary>
        /// Stores a review.
        /// </summary>
        /// <param name="review">Review to store, carrying the identity of the game it belongs to.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// The identity of the stored review, or zero when the game it refers to does not exist.
        /// </returns>
        Task<int> AddAsync(Review review, CancellationToken ct = default);
    }
}
