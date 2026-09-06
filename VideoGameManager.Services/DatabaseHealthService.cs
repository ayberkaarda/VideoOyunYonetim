using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using VideoGameManager.Data;

namespace VideoGameManager.Services
{
    /// <summary>
    /// Default <see cref="IDatabaseHealthService"/>: runs the probe and turns a provider
    /// failure into a status a caller can show without catching anything.
    /// </summary>
    public sealed class DatabaseHealthService : IDatabaseHealthService
    {
        private const string UnreachableMessage =
            "The database could not be reached. Confirm it is running and try again.";

        private const string MisconfiguredMessage =
            "The database connection is not set up correctly. Check the application configuration.";

        private readonly IDatabaseProbe _probe;
        private readonly ILogger<DatabaseHealthService> _logger;

        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="probe">Probe used to reach the database.</param>
        /// <param name="logger">Logger the failure is recorded on.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="probe"/> or <paramref name="logger"/> is <c>null</c>.
        /// </exception>
        public DatabaseHealthService(IDatabaseProbe probe, ILogger<DatabaseHealthService> logger)
        {
            _probe = probe ?? throw new ArgumentNullException(nameof(probe));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<DatabaseStatus> CheckAsync(CancellationToken ct = default)
        {
            try
            {
                await _probe.PingAsync(ct).ConfigureAwait(false);
                return DatabaseStatus.Reachable();
            }
            catch (OperationCanceledException)
            {
                // The caller gave up on the wait; the database was never actually found
                // unreachable, so this is not a health verdict and must not be reported as one.
                throw;
            }
            catch (SqlException ex)
            {
                // The server refused the connection or the network attempt timed out. This is
                // caught here, and only here: the exception stops at this service instead of
                // reaching a caller that would otherwise have to know about the SQL provider.
                _logger.LogWarning(ex, "Database health check failed: the server could not be reached.");
                return DatabaseStatus.Unreachable(UnreachableMessage, ex);
            }
            catch (InvalidOperationException ex)
            {
                // Thrown when the connection string is missing or malformed rather than when
                // the server is down, for example when the configured connection string entry
                // has not been set up on this machine. Treated as unreachable rather than
                // rethrown so a missing setup file does not crash a caller that only wanted to
                // show a status banner.
                _logger.LogError(ex, "Database health check failed: the connection is not configured correctly.");
                return DatabaseStatus.Unreachable(MisconfiguredMessage, ex);
            }
        }
    }
}
