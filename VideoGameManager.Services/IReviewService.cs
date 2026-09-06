using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VideoGameManager.Domain;

namespace VideoGameManager.Services
{
    /// <summary>
    /// Reviews, seen from the presentation layer.
    /// </summary>
    public interface IReviewService
    {
        /// <summary>
        /// Reads the reviews written for one game.
        /// </summary>
        /// <param name="gameId">Identity of the game.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The reviews, empty when the game has none or does not exist.</returns>
        /// <exception cref="DataAccessException">The database call failed.</exception>
        Task<IReadOnlyList<Review>> GetForGameAsync(int gameId, CancellationToken ct = default);

        /// <summary>
        /// Validates a review and stores it against a game.
        /// </summary>
        /// <param name="gameId">Identity of the game being reviewed.</param>
        /// <param name="score">
        /// Rating to record, or <c>null</c> to leave the game's current rating alone.
        /// </param>
        /// <param name="body">Text of the review. Required.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// Success, or the broken rules that stopped the write. A game that does not exist is
        /// reported as an error on <see cref="Review.GameId"/>.
        /// </returns>
        /// <exception cref="DataAccessException">The database call failed.</exception>
        Task<Result> AddAsync(int gameId, double? score, string body, CancellationToken ct = default);
    }
}
