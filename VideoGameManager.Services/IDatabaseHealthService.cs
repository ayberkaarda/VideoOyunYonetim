using System.Threading;
using System.Threading.Tasks;

namespace VideoGameManager.Services
{
    /// <summary>
    /// Checks whether the database can be reached, without throwing when it cannot.
    /// </summary>
    public interface IDatabaseHealthService
    {
        /// <summary>
        /// Checks whether the database can be reached right now.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The outcome of the check.</returns>
        Task<DatabaseStatus> CheckAsync(CancellationToken ct = default);
    }
}
