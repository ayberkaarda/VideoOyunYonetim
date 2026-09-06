using System.Threading;
using System.Threading.Tasks;
using VideoGameManager.Data;

namespace VideoGameManager.Services
{
    /// <summary>
    /// Brings the configured database up to the schema the application expects.
    /// </summary>
    /// <remarks>
    /// Called once at start-up, before anything reads the catalogue, so the application
    /// never queries a table that a pending script was about to add.
    /// </remarks>
    public interface IDatabaseMigrationService
    {
        /// <summary>
        /// Applies every migration the database has not recorded yet.
        /// </summary>
        /// <param name="ct">Cancels the wait for the run to finish.</param>
        /// <returns>
        /// What the run did. A failure is reported in the result rather than thrown, so a
        /// caller can decide whether to carry on with a database it could not upgrade.
        /// </returns>
        Task<MigrationOutcome> ApplyPendingAsync(CancellationToken ct = default);
    }
}
