using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace VideoGameManager.Data
{
    /// <summary>
    /// Dapper implementation of <see cref="IDatabaseProbe"/> over the configured connection.
    /// </summary>
    public sealed class SqlDatabaseProbe : IDatabaseProbe
    {
        /// <summary>
        /// Seconds a connection attempt gets before giving up. The client default is fifteen
        /// seconds, which would keep a startup screen waiting far too long when the server
        /// cannot be reached at all.
        /// </summary>
        private const int ConnectTimeoutSeconds = 5;

        private const string ProbeSql = "SELECT 1;";

        private readonly IDbConnectionFactory _connections;

        /// <summary>
        /// Creates the probe.
        /// </summary>
        /// <param name="connections">Factory that hands out connections.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connections"/> is <c>null</c>.</exception>
        public SqlDatabaseProbe(IDbConnectionFactory connections)
        {
            _connections = connections ?? throw new ArgumentNullException(nameof(connections));
        }

        /// <inheritdoc />
        public async Task PingAsync(CancellationToken ct = default)
        {
            using (DbConnection connection = _connections.Create())
            {
                ApplyShortConnectTimeout(connection);

                await connection.OpenAsync(ct).ConfigureAwait(false);

                await connection
                    .ExecuteScalarAsync<int>(new CommandDefinition(ProbeSql, cancellationToken: ct))
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Shortens the connection timeout before the connection is opened. Changing it once
        /// the connection is open has no effect, so this must run first.
        /// </summary>
        private static void ApplyShortConnectTimeout(DbConnection connection)
        {
            var builder = new SqlConnectionStringBuilder(connection.ConnectionString)
            {
                ConnectTimeout = ConnectTimeoutSeconds,
            };

            connection.ConnectionString = builder.ConnectionString;
        }
    }
}
