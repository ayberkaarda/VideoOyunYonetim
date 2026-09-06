using System;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace VideoGameManager.Data
{
    /// <summary>
    /// Creates SQL Server connections from a configured connection string.
    /// </summary>
    /// <remarks>
    /// The connection string is never compiled in. It is read from configuration, whose real
    /// values live in files that are not under version control.
    /// </remarks>
    public sealed class SqlConnectionFactory : IDbConnectionFactory
    {
        /// <summary>
        /// Key looked up under the <c>ConnectionStrings</c> section.
        /// </summary>
        public const string ConnectionStringName = "VideoGameManager";

        private readonly string _connectionString;

        /// <summary>
        /// Creates the factory from application configuration.
        /// </summary>
        /// <param name="configuration">Configuration holding the connection string.</param>
        /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">
        /// The <c>ConnectionStrings:VideoGameManager</c> entry is missing or empty.
        /// </exception>
        public SqlConnectionFactory(IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            string value = configuration.GetConnectionString(ConnectionStringName);

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    "Connection string 'ConnectionStrings:" + ConnectionStringName +
                    "' is missing. Set it in the environment-specific settings file.");
            }

            _connectionString = value;
        }

        private SqlConnectionFactory(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("A connection string is required.", nameof(connectionString));
            }

            _connectionString = connectionString;
        }

        /// <summary>
        /// Creates a factory for a connection string the caller supplies directly, which is what
        /// an integration test does when it targets a throwaway database.
        /// </summary>
        /// <param name="connectionString">Connection string to use.</param>
        /// <returns>A factory bound to that connection string.</returns>
        /// <exception cref="ArgumentException"><paramref name="connectionString"/> is empty.</exception>
        public static SqlConnectionFactory ForConnectionString(string connectionString) =>
            new SqlConnectionFactory(connectionString);

        /// <inheritdoc />
        public DbConnection Create() => new SqlConnection(_connectionString);
    }
}
