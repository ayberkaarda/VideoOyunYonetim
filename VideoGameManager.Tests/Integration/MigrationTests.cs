using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FluentAssertions;
using VideoGameManager.Data;
using Xunit;

namespace VideoGameManager.Tests.Integration
{
    /// <summary>
    /// Exercises the migration chain against a database nobody has touched before.
    /// </summary>
    /// <remarks>
    /// Every test here creates a database of its own on the shared container instead of using the
    /// one the fixture already migrated, because what is under test is what happens on the way
    /// from nothing to the finished schema. The databases are left behind on purpose: they
    /// disappear with the container, and dropping them would put a statement in the suite that
    /// could destroy a real catalogue if it were ever pointed somewhere else.
    /// </remarks>
    [Collection(DatabaseCollection.Name)]
    public sealed class MigrationTests
    {
        /// <summary>
        /// Reads the collation a database was created with. A database is asked about by name
        /// because the collation is a property of the database, not of the session.
        /// </summary>
        private const string CollationSql = @"
SELECT collation_name
FROM   sys.databases
WHERE  name = @Name;";

        /// <summary>
        /// Every table in the database, schema-qualified.
        /// </summary>
        private const string TablesSql = @"
SELECT     s.name + '.' + t.name
FROM       sys.tables  AS t
INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
ORDER BY   s.name, t.name;";

        /// <summary>
        /// The scripts the journal records as applied.
        /// </summary>
        private const string JournalSql = @"
SELECT   ScriptName
FROM     dbo.SchemaVersions
ORDER BY ScriptName;";

        private readonly SqlServerFixture _fixture;

        /// <summary>
        /// Creates the test class. xunit hands in the shared fixture.
        /// </summary>
        /// <param name="fixture">The running SQL Server.</param>
        public MigrationTests(SqlServerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void CreateAndApply_OnACleanDatabase_AppliesEveryScriptInOrder()
        {
            string databaseName = NewDatabaseName();
            IDatabaseMigrator migrator = DatabaseMigrator.ForConnectionString(_fixture.ConnectionStringFor(databaseName));

            MigrationOutcome outcome = migrator.CreateAndApply();

            outcome.Succeeded.Should().BeTrue(SqlServerFixture.Describe("The first run failed.", outcome));
            outcome.Failure.Should().BeNull();

            // The runner reports embedded resource names, which carry the assembly namespace in
            // front of the file name, so the file names are compared rather than the whole string.
            outcome.AppliedScripts
                .Select(FileNameOf)
                .Should().Equal(SqlServerFixture.ExpectedScripts);
        }

        [Fact]
        public async Task CreateAndApply_RunTwice_AppliesNothingTheSecondTime()
        {
            // This is what makes the chain safe to run at every application start: a database that
            // is already up to date must come out of a migration run untouched.
            string databaseName = NewDatabaseName();
            string connectionString = _fixture.ConnectionStringFor(databaseName);
            IDatabaseMigrator migrator = DatabaseMigrator.ForConnectionString(connectionString);

            MigrationOutcome first = migrator.CreateAndApply();
            first.Succeeded.Should().BeTrue(SqlServerFixture.Describe("The first run failed.", first));

            MigrationOutcome second = migrator.CreateAndApply();

            second.Succeeded.Should().BeTrue(SqlServerFixture.Describe("The second run failed.", second));
            second.AppliedScripts.Should().BeEmpty("a database that is already up to date has nothing left to apply");

            IReadOnlyList<string> journalled = await ReadStringsAsync(connectionString, JournalSql);

            journalled
                .Select(FileNameOf)
                .Should().Equal(SqlServerFixture.ExpectedScripts, "the journal records each script once, not twice");
        }

        [Fact]
        public async Task CreateAndApply_OnACleanDatabase_UsesTheRequiredCollation()
        {
            // A Turkish collation would make a search for "fifa" miss the row "FIFA 24" and raise
            // no error while doing it, so the collation is asserted rather than assumed.
            string databaseName = NewDatabaseName();
            string connectionString = _fixture.ConnectionStringFor(databaseName);

            MigrationOutcome outcome = DatabaseMigrator.ForConnectionString(connectionString).CreateAndApply();
            outcome.Succeeded.Should().BeTrue(SqlServerFixture.Describe("The run failed.", outcome));

            string collation;

            using (DbConnection connection = await OpenAsync(connectionString))
            {
                collation = await connection
                    .ExecuteScalarAsync<string>(new CommandDefinition(CollationSql, new { Name = databaseName }));
            }

            collation.Should().Be(DatabaseMigrator.RequiredCollation);
        }

        [Fact]
        public async Task CreateAndApply_OnACleanDatabase_CreatesTheExpectedTables()
        {
            string databaseName = NewDatabaseName();
            string connectionString = _fixture.ConnectionStringFor(databaseName);

            MigrationOutcome outcome = DatabaseMigrator.ForConnectionString(connectionString).CreateAndApply();
            outcome.Succeeded.Should().BeTrue(SqlServerFixture.Describe("The run failed.", outcome));

            IReadOnlyList<string> tables = await ReadStringsAsync(connectionString, TablesSql);

            tables.Should().Contain(new[]
            {
                "dbo.Game",
                "dbo.GamePlatform",
                "dbo.Genre",
                "dbo.Platform",
                "dbo.Review",
                "dbo.SchemaVersions",
            });
        }

        [Fact]
        public async Task CreateAndApply_OnACleanDatabase_LeavesTheFlatColumnsBehind()
        {
            // The baseline script creates Genre, Platform and Comment as columns of dbo.Game and
            // the scripts that follow move them into tables of their own. A clean install runs the
            // whole chain, so those columns must not survive it: code that still reads them would
            // work on a freshly created database and fail on a migrated one, or the other way
            // round, depending on which column was left.
            string databaseName = NewDatabaseName();
            string connectionString = _fixture.ConnectionStringFor(databaseName);

            MigrationOutcome outcome = DatabaseMigrator.ForConnectionString(connectionString).CreateAndApply();
            outcome.Succeeded.Should().BeTrue(SqlServerFixture.Describe("The run failed.", outcome));

            const string columnsSql = @"
SELECT c.name
FROM   sys.columns AS c
WHERE  c.object_id = OBJECT_ID(@Table)
ORDER BY c.name;";

            IReadOnlyList<string> columns;

            using (DbConnection connection = await OpenAsync(connectionString))
            {
                columns = (await connection
                    .QueryAsync<string>(new CommandDefinition(columnsSql, new { Table = "dbo.Game" }))).AsList();
            }

            columns.Should().Contain(new[] { "Id", "Name", "GenreId", "Score", "CoverUrl" });
            columns.Should().NotContain(new[] { "Genre", "Platform", "Comment" });
        }

        /// <summary>
        /// A database name no other run can be using.
        /// </summary>
        private static string NewDatabaseName() => "vgm_migration_" + Guid.NewGuid().ToString("N");

        /// <summary>
        /// Strips the namespace an embedded resource name carries, leaving the script file name.
        /// </summary>
        private static string FileNameOf(string resourceName)
        {
            const string marker = ".Migrations.";

            int start = resourceName.IndexOf(marker, StringComparison.Ordinal);

            return start < 0 ? resourceName : resourceName.Substring(start + marker.Length);
        }

        private static async Task<DbConnection> OpenAsync(string connectionString)
        {
            DbConnection connection = SqlConnectionFactory.ForConnectionString(connectionString).Create();

            await connection.OpenAsync();

            return connection;
        }

        private static async Task<IReadOnlyList<string>> ReadStringsAsync(string connectionString, string sql)
        {
            using (DbConnection connection = await OpenAsync(connectionString))
            {
                return (await connection
                    .QueryAsync<string>(new CommandDefinition(sql))).AsList();
            }
        }
    }
}
