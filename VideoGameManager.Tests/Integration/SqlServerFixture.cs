using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;
using VideoGameManager.Data;
using Xunit;

namespace VideoGameManager.Tests.Integration
{
    /// <summary>
    /// Raises one SQL Server for the whole integration run and points a freshly migrated,
    /// throwaway database at it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The container is private to the test run. Nothing here ever reaches the database used
    /// for development: the server is started on a port the host assigns, its name is chosen
    /// by the container runtime, and the catalogue this fixture works in carries a generated
    /// name, so two runs on the same machine cannot collide either.
    /// </para>
    /// <para>
    /// One container serves every test class. Starting one per class costs minutes of wall
    /// clock and buys nothing, because the classes are kept apart by the reset below instead.
    /// </para>
    /// <para>
    /// The schema is never created here. It is built by running the same migration chain the
    /// application runs at startup, so the tests execute against the schema that ships rather
    /// than against a copy of it that can drift.
    /// </para>
    /// </remarks>
    public sealed class SqlServerFixture : IAsyncLifetime
    {
        /// <summary>
        /// Image the container runs. Pinned to the same tag the development database and the
        /// build agent use, so a test never passes against a different server than the one the
        /// application meets.
        /// </summary>
        public const string SqlServerImage = "mcr.microsoft.com/mssql/server:2022-latest";

        /// <summary>
        /// Empties every catalogue table, in the order the foreign keys allow.
        /// </summary>
        /// <remarks>
        /// Each statement carries a predicate rather than being an unqualified delete: an
        /// identity is always positive, so the predicate keeps every row in scope while still
        /// stating out loud which rows are meant. The lookup tables are cleared too, otherwise a
        /// genre left behind by one test would make the next one's "this name is new" assertion
        /// meaningless.
        /// </remarks>
        private const string ResetSql = @"
DELETE FROM dbo.Review       WHERE Id > 0;
DELETE FROM dbo.GamePlatform WHERE GameId > 0;
DELETE FROM dbo.Game         WHERE Id > 0;
DELETE FROM dbo.Genre        WHERE Id > 0;
DELETE FROM dbo.[Platform]   WHERE Id > 0;";

        /// <summary>
        /// The collation the server itself is running under, which is not the same question as
        /// the collation of any one database.
        /// </summary>
        private const string ServerCollationSql = "SELECT CAST(SERVERPROPERTY('Collation') AS NVARCHAR(128));";

        /// <summary>How long the server is given to finish applying its collation.</summary>
        private static readonly TimeSpan CollationTimeout = TimeSpan.FromMinutes(2);

        /// <summary>How long to wait between two attempts to read the server collation.</summary>
        private static readonly TimeSpan CollationPollInterval = TimeSpan.FromMilliseconds(500);

        private readonly MsSqlContainer _container;

        private string _connectionString;

        private SqlConnectionFactory _connections;

        /// <summary>
        /// Creates the fixture. The container is described here and started by xunit.
        /// </summary>
        public SqlServerFixture()
        {
            // The image is named to the builder rather than left to its default, so the container,
            // the database used for development and the build agent all run the same server.
            _container = new MsSqlBuilder(SqlServerImage)

                // The server collation decides how every column created after it compares text.
                // Under a Turkish collation "I" and "i" are different letters, so a search for
                // "fifa" would silently miss the row "FIFA 24" and raise no error at all. The
                // migrator applies the same collation to the database it creates; setting it on
                // the server as well keeps the two from disagreeing.
                .WithEnvironment("MSSQL_COLLATION", DatabaseMigrator.RequiredCollation)
                .Build();
        }

        /// <summary>
        /// Connection string of the migrated, throwaway database the tests work in.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                if (_connectionString == null)
                {
                    throw new InvalidOperationException("The fixture has not been initialised yet.");
                }

                return _connectionString;
            }
        }

        /// <summary>
        /// Connection factory the repositories under test are built with.
        /// </summary>
        public IDbConnectionFactory Connections
        {
            get
            {
                if (_connections == null)
                {
                    throw new InvalidOperationException("The fixture has not been initialised yet.");
                }

                return _connections;
            }
        }

        /// <summary>
        /// Starts the container and migrates a database of its own onto it.
        /// </summary>
        /// <exception cref="InvalidOperationException">The migration chain did not succeed.</exception>
        public async Task InitializeAsync()
        {
            await _container.StartAsync().ConfigureAwait(false);
            await WaitUntilTheServerCollationIsAppliedAsync().ConfigureAwait(false);

            string databaseName = "vgm_test_" + Guid.NewGuid().ToString("N");

            _connectionString = ConnectionStringFor(databaseName);
            _connections = SqlConnectionFactory.ForConnectionString(_connectionString);

            MigrationOutcome outcome = DatabaseMigrator.ForConnectionString(_connectionString).CreateAndApply();

            if (!outcome.Succeeded)
            {
                // A fixture that carried on here would hand every test an empty database and
                // produce a wall of failures that say nothing about the real cause.
                throw new InvalidOperationException(Describe("Could not migrate the test database.", outcome));
            }
        }

        /// <summary>
        /// Stops and removes the container.
        /// </summary>
        public Task DisposeAsync() => _container.DisposeAsync().AsTask();

        /// <summary>
        /// Builds a connection string for another database on the same container, which is how a
        /// test that needs a database nobody has migrated yet gets one.
        /// </summary>
        /// <param name="databaseName">Name of the database to point at.</param>
        /// <returns>A connection string for that database.</returns>
        public string ConnectionStringFor(string databaseName)
        {
            var builder = new SqlConnectionStringBuilder(_container.GetConnectionString())
            {
                InitialCatalog = databaseName,

                // The container presents a certificate it signed itself. Without this the
                // connection fails during the handshake, and the error text talks about the
                // connection rather than the certificate.
                TrustServerCertificate = true,
            };

            return builder.ConnectionString;
        }

        /// <summary>
        /// Opens a connection to the migrated test database, for a test that wants to check the
        /// rows a repository wrote without going back through that repository.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>An open connection the caller owns and must dispose.</returns>
        public async Task<DbConnection> OpenConnectionAsync(CancellationToken ct = default)
        {
            DbConnection connection = Connections.Create();

            await connection.OpenAsync(ct).ConfigureAwait(false);

            return connection;
        }

        /// <summary>
        /// Removes every catalogue row, so each test starts from an empty database.
        /// </summary>
        /// <remarks>
        /// The alternative -- letting rows pile up and having every test assert only on rows it
        /// created under a unique name -- rules out the assertions that matter most here: a total
        /// count, a page boundary, and a sorted lookup list are all statements about the whole
        /// table. Emptying the tables between tests keeps those honest, and it is cheap because
        /// the tests in this collection run one after another against a database nobody else uses.
        /// </remarks>
        /// <param name="ct">Cancellation token.</param>
        public async Task ResetAsync(CancellationToken ct = default)
        {
            using (DbConnection connection = await OpenConnectionAsync(ct).ConfigureAwait(false))
            {
                await connection
                    .ExecuteAsync(new CommandDefinition(ResetSql, cancellationToken: ct))
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Blocks until the server reports the collation it was configured with.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The image accepts client connections before it has applied that collation: it starts on
        /// the built-in default, announces itself ready, and changes the server default collation
        /// a few seconds later. The server log records the two moments plainly -- "SQL Server is
        /// now ready for client connections", and then "Attempting to change default collation to
        /// Latin1_General_100_CI_AI".
        /// </para>
        /// <para>
        /// A connection opened inside that window belongs to a session created under the old
        /// collation. Closing it hands it to the connection pool, and the next caller that takes
        /// it out gets a login failure the server explains as "Current collation did not match the
        /// database's collation during connection reset". What reaches the client instead is a
        /// message about resetting the connection and a session in the kill state, which names
        /// neither the collation nor the moment it changed, so the failure looks random and lands
        /// on whichever call happened to draw that connection.
        /// </para>
        /// <para>
        /// The wait itself connects with pooling switched off, so that a connection opened before
        /// the change cannot be the one handed out after it.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The server never reported the required collation.
        /// </exception>
        private async Task WaitUntilTheServerCollationIsAppliedAsync()
        {
            var builder = new SqlConnectionStringBuilder(_container.GetConnectionString())
            {
                InitialCatalog = "master",
                TrustServerCertificate = true,
                Pooling = false,
                ConnectTimeout = 5,
            };

            string connectionString = builder.ConnectionString;
            DateTime deadline = DateTime.UtcNow.Add(CollationTimeout);
            string collation = null;

            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync().ConfigureAwait(false);

                        collation = await connection
                            .ExecuteScalarAsync<string>(new CommandDefinition(ServerCollationSql))
                            .ConfigureAwait(false);
                    }

                    if (string.Equals(collation, DatabaseMigrator.RequiredCollation, StringComparison.Ordinal))
                    {
                        return;
                    }
                }
                catch (SqlException)
                {
                    // The server is still starting, or it is in the middle of the change and is
                    // refusing logins while it restarts. Both are answered by asking again.
                    collation = null;
                }

                await Task.Delay(CollationPollInterval).ConfigureAwait(false);
            }

            throw new InvalidOperationException(
                "The test server did not report the collation " + DatabaseMigrator.RequiredCollation +
                " within " + CollationTimeout + ". The last answer was " + (collation ?? "no answer at all") + ".");
        }

        /// <summary>
        /// Renders a failed migration run as text, keeping the reason and the log the runner
        /// produced so that the failure names its own cause.
        /// </summary>
        /// <param name="headline">What was being attempted.</param>
        /// <param name="outcome">The run that failed.</param>
        /// <returns>A message describing the failure.</returns>
        public static string Describe(string headline, MigrationOutcome outcome)
        {
            StringBuilder message = new StringBuilder(headline);

            if (outcome != null)
            {
                if (outcome.Failure != null)
                {
                    message.Append(Environment.NewLine).Append("Failure: ").Append(outcome.Failure);
                }

                if (outcome.Log != null)
                {
                    message.Append(Environment.NewLine).Append("Log:");

                    foreach (string line in outcome.Log)
                    {
                        message.Append(Environment.NewLine).Append("  ").Append(line);
                    }
                }
            }

            return message.ToString();
        }

        /// <summary>
        /// Names of the migration scripts the chain ships, in the order they run.
        /// </summary>
        /// <remarks>
        /// Kept here rather than in one test because both the "clean database" and the "already
        /// migrated" cases talk about the same chain.
        /// </remarks>
        public static IReadOnlyList<string> ExpectedScripts { get; } = new[]
        {
            "0001_create_game_table.sql",
            "0002_normalise_genre_and_platform.sql",
            "0003_add_review_table.sql",
            "0004_add_indexes.sql",
        };
    }
}
