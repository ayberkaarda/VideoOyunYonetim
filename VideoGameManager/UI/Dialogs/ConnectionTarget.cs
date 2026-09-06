using System;
using System.Data.Common;

namespace VideoGameManager.UI.Dialogs
{
    /// <summary>
    /// The server and the database a connection string points at, and nothing else.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Only the two keys that name the target are copied out. The credentials sitting in
    /// the same string are never read, so no screen and no log line built from this type
    /// can end up carrying a password.
    /// </para>
    /// <para>
    /// The generic builder is used rather than the SQL Server specific one because the
    /// presentation layer has no reference to a database driver, and naming a server does
    /// not need one: a connection string is a semicolon separated list of key/value pairs
    /// and the builder reads it as such, with keys compared case insensitively.
    /// </para>
    /// </remarks>
    internal sealed class ConnectionTarget
    {
        private const string UnknownValue = "(not configured)";

        // Both spellings of each key are accepted: the same connection string can be
        // written with either, and which one a machine happens to use must not decide
        // whether the dialog can say where it was looking.
        private static readonly string[] ServerKeys =
        {
            "Data Source", "Server", "Address", "Addr", "Network Address"
        };

        private static readonly string[] DatabaseKeys =
        {
            "Initial Catalog", "Database"
        };

        private readonly string _server;
        private readonly string _database;

        private ConnectionTarget(string server, string database)
        {
            _server = server;
            _database = database;
        }

        /// <summary>Gets the server the application tried to reach.</summary>
        public string Server
        {
            get { return _server; }
        }

        /// <summary>Gets the database the application tried to open.</summary>
        public string Database
        {
            get { return _database; }
        }

        /// <summary>
        /// Reads the target out of a connection string.
        /// </summary>
        /// <param name="connectionString">The connection string, which may be null or malformed.</param>
        /// <returns>The target; unknown parts read as a placeholder rather than throwing.</returns>
        public static ConnectionTarget FromConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return new ConnectionTarget(UnknownValue, UnknownValue);
            }

            try
            {
                DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
                builder.ConnectionString = connectionString;

                return new ConnectionTarget(Find(builder, ServerKeys), Find(builder, DatabaseKeys));
            }
            catch (ArgumentException)
            {
                // A connection string that cannot be parsed is a separate failure, and
                // the health check reports it. Here it only means the dialog cannot name
                // the target, which is not worth stopping the dialog over.
                return new ConnectionTarget(UnknownValue, UnknownValue);
            }
        }

        private static string Find(DbConnectionStringBuilder builder, string[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                object value;
                if (!builder.TryGetValue(keys[i], out value) || value == null)
                {
                    continue;
                }

                string text = value.ToString().Trim();
                if (text.Length > 0)
                {
                    return text;
                }
            }

            return UnknownValue;
        }
    }
}
