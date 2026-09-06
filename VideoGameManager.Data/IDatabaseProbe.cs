using System.Threading;
using System.Threading.Tasks;

namespace VideoGameManager.Data
{
    /// <summary>
    /// Opens a connection and asks the database a trivial question.
    /// </summary>
    /// <remarks>
    /// The probe never catches: a failure to reach the server is reported by letting the
    /// provider exception travel, exactly like every repository call does. The caller decides
    /// what a failure means.
    /// </remarks>
    public interface IDatabaseProbe
    {
        /// <summary>
        /// Opens a connection and confirms the server answers.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A task that completes when the server has answered.</returns>
        Task PingAsync(CancellationToken ct = default);
    }
}
