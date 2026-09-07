using System.Threading;
using System.Threading.Tasks;
using VideoGameManager.Data;

namespace VideoGameManager.Services
{
    /// <summary>
    /// Aggregate figures over the whole catalogue, seen from the presentation layer.
    /// </summary>
    /// <remarks>
    /// The aggregation itself is one query in the repository; pulling every game into memory to
    /// count it would not survive a real database. This service exists to apply the same
    /// provider-failure translation every other service applies, not to compute anything.
    /// </remarks>
    public interface IStatisticsService
    {
        /// <summary>
        /// Reads the current catalogue statistics.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Totals and the per-genre breakdown, computed by the database.</returns>
        /// <exception cref="DataAccessException">The database call failed.</exception>
        Task<CatalogueStatistics> GetAsync(CancellationToken ct = default);
    }
}
