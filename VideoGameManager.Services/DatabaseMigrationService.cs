using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VideoGameManager.Data;

namespace VideoGameManager.Services
{
    /// <summary>
    /// Default <see cref="IDatabaseMigrationService"/>: runs the migrator off the calling
    /// thread and records what it did.
    /// </summary>
    public sealed class DatabaseMigrationService : IDatabaseMigrationService
    {
        private readonly IDatabaseMigrator _migrator;
        private readonly ILogger<DatabaseMigrationService> _logger;

        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="migrator">Migrator the service delegates to.</param>
        /// <param name="logger">Logger the applied scripts and any failure are recorded on.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="migrator"/> or <paramref name="logger"/> is <c>null</c>.
        /// </exception>
        public DatabaseMigrationService(IDatabaseMigrator migrator, ILogger<DatabaseMigrationService> logger)
        {
            _migrator = migrator ?? throw new ArgumentNullException(nameof(migrator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<MigrationOutcome> ApplyPendingAsync(CancellationToken ct = default)
        {
            // The migration runner is synchronous all the way down, so there is no
            // asynchronous call to await. It is moved to the thread pool rather than
            // dressed up as asynchronous, which is what keeps a UI thread responsive
            // while a long script runs.
            MigrationOutcome outcome = await Task
                .Run(() => _migrator.Apply(), ct)
                .ConfigureAwait(false);

            if (!outcome.Succeeded)
            {
                // The exception is written to the log, where the stack trace and the SQL
                // state are useful. The caller shows its own wording; the raw provider
                // message is not something a user can act on.
                _logger.LogError(
                    outcome.Failure,
                    "Database migration failed. {ScriptCount} script(s) had been applied before the failure.",
                    outcome.AppliedScripts.Count);

                return outcome;
            }

            if (outcome.AppliedScripts.Count == 0)
            {
                _logger.LogInformation("Database schema is up to date; no migration was pending.");
                return outcome;
            }

            foreach (string script in outcome.AppliedScripts)
            {
                _logger.LogInformation("Database migration applied: {ScriptName}.", script);
            }

            _logger.LogInformation(
                "Database schema updated: {ScriptCount} migration(s) applied.",
                outcome.AppliedScripts.Count);

            return outcome;
        }
    }
}
