using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VideoGameManager.Domain;

namespace VideoGameManager.Services
{
    /// <summary>
    /// Suggests a game, using one of the registered strategies.
    /// </summary>
    public interface IRecommendationService
    {
        /// <summary>
        /// Names of the strategies that can be asked for, in registration order. The first is
        /// the default.
        /// </summary>
        IReadOnlyList<string> AvailableStrategies { get; }

        /// <summary>
        /// Suggests a game.
        /// </summary>
        /// <param name="strategyName">
        /// Name of the strategy to use, or <c>null</c> for the default one.
        /// </param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The suggested game, or <c>null</c> when nothing qualifies.</returns>
        /// <exception cref="DataAccessException">The database call failed.</exception>
        Task<Game> RecommendAsync(string strategyName = null, CancellationToken ct = default);
    }
}
