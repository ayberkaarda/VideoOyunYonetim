using System.Threading;
using System.Threading.Tasks;
using VideoGameManager.Domain;

namespace VideoGameManager.Services
{
    /// <summary>
    /// One way of choosing a game to suggest.
    /// </summary>
    /// <remarks>
    /// Strategies are interchangeable behind this interface, so a new one is added without any
    /// change to the screen that shows the suggestion.
    /// </remarks>
    public interface IRecommendationStrategy
    {
        /// <summary>
        /// Name the user picks this strategy by. Unique among the registered strategies.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Chooses a game to suggest.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The chosen game, or <c>null</c> when nothing qualifies.</returns>
        /// <exception cref="DataAccessException">The database call failed.</exception>
        Task<Game> PickAsync(CancellationToken ct = default);
    }
}
