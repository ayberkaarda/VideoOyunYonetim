using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using DbUp;
using DbUp.Engine;
using DbUp.Engine.Output;

namespace VideoGameManager.Data
{
    /// <summary>
    /// Applies the SQL scripts embedded under <c>Migrations/</c> to a SQL Server database.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every script that has not been recorded in <c>dbo.SchemaVersions</c> is run once, in
    /// file-name order, each inside its own transaction. A script that fails half way is
    /// rolled back whole, so the database is never left between two schemas.
    /// </para>
    /// <para>
    /// The work is synchronous because the underlying runner is. It is not wrapped in a
    /// fake asynchronous shell here; a caller that must not block a UI thread moves the
    /// call off it, which is what the service layer does.
    /// </para>
    /// </remarks>
    public sealed class DatabaseMigrator : IDatabaseMigrator
    {
        /// <summary>
        /// Collation a VideoGameManager database is created with.
        /// </summary>
        /// <remarks>
        /// Deliberately not a Turkish collation. Under one, <c>I</c> and <c>i</c> are
        /// different letters, so <c>Name LIKE '%fifa%'</c> never matches the row
        /// <c>FIFA 24</c>: the search returns nothing and raises no error, which is the
        /// hardest kind of fault to notice. Accent insensitivity comes with this choice as
        /// a bonus, so <c>pokemon</c> also matches <c>Pokemon</c>.
        /// </remarks>
        public const string RequiredCollation = "Latin1_General_100_CI_AI";

        /// <summary>Schema the journal table lives in.</summary>
        private const string JournalSchema = "dbo";

        /// <summary>Table recording which scripts have already run.</summary>
        private const string JournalTable = "SchemaVersions";

        /// <summary>
        /// Marks the embedded resources that are migration scripts. Resource names are
        /// namespace-shaped, so the folder appears as a dotted segment.
        /// </summary>
        private const string MigrationResourceMarker = ".Migrations.";

        private readonly string _connectionString;

        /// <summary>
        /// Creates the migrator for the database the application itself talks to.
        /// </summary>
        /// <param name="connections">Factory the connection string is taken from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connections"/> is <c>null</c>.</exception>
        public DatabaseMigrator(IDbConnectionFactory connections)
        {
            if (connections == null)
            {
                throw new ArgumentNullException(nameof(connections));
            }

            // The runner wants a connection string rather than a connection: it opens and
            // closes several of its own. Taking it from an unopened connection keeps the
            // credentials in the factory, which is the only place that knows them.
            using (DbConnection template = connections.Create())
            {
                _connectionString = template.ConnectionString;
            }
        }

        private DatabaseMigrator(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("A connection string is required.", nameof(connectionString));
            }

            _connectionString = connectionString;
        }

        /// <summary>
        /// Creates a migrator for a connection string the caller supplies directly, which is
        /// what the command-line runner and an integration test do.
        /// </summary>
        /// <param name="connectionString">Connection string to use.</param>
        /// <returns>A migrator bound to that connection string.</returns>
        /// <exception cref="ArgumentException"><paramref name="connectionString"/> is empty.</exception>
        public static DatabaseMigrator ForConnectionString(string connectionString) =>
            new DatabaseMigrator(connectionString);

        /// <inheritdoc />
        public MigrationOutcome Apply() => Run(createDatabase: false);

        /// <inheritdoc />
        public MigrationOutcome CreateAndApply() => Run(createDatabase: true);

        private MigrationOutcome Run(bool createDatabase)
        {
            CollectingLog log = new CollectingLog();

            try
            {
                if (createDatabase)
                {
                    // Creates the database only when it is absent, and only then applies the
                    // collation; an existing database keeps whatever it was created with.
                    EnsureDatabase.For.SqlDatabase(_connectionString, log, collation: RequiredCollation);
                }

                UpgradeEngine engine = DeployChanges.To
                    .SqlDatabase(_connectionString)
                    .WithScriptsEmbeddedInAssembly(
                        typeof(DatabaseMigrator).Assembly,
                        name => name.IndexOf(MigrationResourceMarker, StringComparison.Ordinal) >= 0)
                    .JournalToSqlTable(JournalSchema, JournalTable)
                    .WithTransactionPerScript()
                    .LogTo(log)
                    .Build();

                DatabaseUpgradeResult result = engine.PerformUpgrade();

                IReadOnlyList<string> applied = result.Scripts == null
                    ? (IReadOnlyList<string>)Array.Empty<string>()
                    : result.Scripts.Select(script => script.Name).ToArray();

                return new MigrationOutcome(result.Successful, applied, log.Lines, result.Error);
            }
            catch (Exception ex)
            {
                // The engine reports a failing script through its result, but it can also fail
                // before the first script runs: an unreachable server, a rejected connection
                // string, a journal table it cannot create. Those arrive as exceptions. They
                // are turned into the same outcome so the caller has one shape to handle, and
                // the exception itself is carried along rather than swallowed.
                log.LogError(ex, "Migration run failed before it could finish.");
                return new MigrationOutcome(false, Array.Empty<string>(), log.Lines, ex);
            }
        }

        /// <summary>
        /// Keeps the runner's output in a list instead of writing it anywhere.
        /// </summary>
        /// <remarks>
        /// The data layer takes no logging dependency: it hands the lines back and lets the
        /// service layer decide where they belong.
        /// </remarks>
        private sealed class CollectingLog : IUpgradeLog
        {
            private readonly List<string> _lines = new List<string>();

            /// <summary>Everything logged so far, oldest first.</summary>
            internal IReadOnlyList<string> Lines => _lines;

            public void LogTrace(string format, params object[] args) => Add("TRACE", format, args);

            public void LogDebug(string format, params object[] args) => Add("DEBUG", format, args);

            public void LogInformation(string format, params object[] args) => Add("INFO", format, args);

            public void LogWarning(string format, params object[] args) => Add("WARN", format, args);

            public void LogError(string format, params object[] args) => Add("ERROR", format, args);

            public void LogError(Exception ex, string format, params object[] args)
            {
                Add("ERROR", format, args);

                if (ex != null)
                {
                    _lines.Add("ERROR: " + ex.Message);
                }
            }

            private void Add(string level, string format, object[] args)
            {
                _lines.Add(level + ": " + Render(format, args));
            }

            private static string Render(string format, object[] args)
            {
                if (format == null)
                {
                    return string.Empty;
                }

                if (args == null || args.Length == 0)
                {
                    return format;
                }

                try
                {
                    // The runner's own messages are the only format strings that reach this
                    // point, but a malformed one must not bring down a migration that has
                    // otherwise succeeded, so the unformatted text is kept instead.
                    return string.Format(CultureInfo.InvariantCulture, format, args);
                }
                catch (FormatException)
                {
                    return format;
                }
            }
        }
    }
}
