using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DbUp;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using VideoGameManager.Data;
using VideoGameManager.Domain;
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

        /// <summary>
        /// The type, nullability and default of named columns of a table.
        /// </summary>
        private const string ColumnShapeSql = @"
SELECT      c.name          AS ColumnName,
            t.name          AS TypeName,
            c.is_nullable   AS IsNullable,
            dc.definition   AS DefaultDefinition
FROM        sys.columns AS c
INNER JOIN  sys.types   AS t  ON t.user_type_id = c.user_type_id
LEFT JOIN   sys.default_constraints AS dc ON dc.object_id = c.default_object_id
WHERE       c.object_id = OBJECT_ID(@Table)
  AND       c.name IN @Names
ORDER BY    c.name;";

        /// <summary>
        /// One check constraint, and whether the server vouches for the rows already there.
        /// </summary>
        private const string CheckShapeSql = @"
SELECT cc.definition      AS Definition,
       cc.is_not_trusted  AS IsNotTrusted
FROM   sys.check_constraints AS cc
WHERE  cc.name = @Name
  AND  cc.parent_object_id = OBJECT_ID(@Table);";

        /// <summary>
        /// Every index on a table.
        /// </summary>
        private const string IndexNamesSql = @"
SELECT   i.name
FROM     sys.indexes AS i
WHERE    i.object_id = OBJECT_ID(@Table)
  AND    i.name IS NOT NULL
ORDER BY i.name;";

        /// <summary>
        /// Writes a game with a play state chosen by the caller, so a test can find out what the
        /// server accepts rather than what the script says it should.
        /// </summary>
        private const string InsertWithStatusSql = @"
INSERT INTO dbo.Game (Name, [Status])
VALUES (@Name, @Status);";

        /// <summary>
        /// The table as it stood before any of these scripts existed: genre and platform as free
        /// text on the row, one comment column, no journal anywhere.
        /// </summary>
        /// <remarks>
        /// This is a copy of the original shape on purpose. A test that started from the migrated
        /// schema could never show what a replay does to a catalogue that predates it, which is
        /// the one path where existing data is at risk.
        /// </remarks>
        private const string CreateFlatGameTableSql = @"
-- bypass-ok: this rebuilds the historical table so a replay can be tested against it; it is not a schema change to any real database
CREATE TABLE dbo.Game
(
    Id         INT            IDENTITY(1, 1) NOT NULL,
    Name       NVARCHAR(100)  NULL,
    Genre      NVARCHAR(50)   NULL,
    [Platform] NVARCHAR(50)   NULL,
    Score      FLOAT          NULL,
    CoverUrl   NVARCHAR(MAX)  NULL,
    Comment    NVARCHAR(MAX)  NULL,
    CONSTRAINT PK_Game PRIMARY KEY CLUSTERED (Id)
);";

        private const string InsertFlatGameSql = @"
INSERT INTO dbo.Game (Name, Genre, [Platform], Score, Comment)
VALUES (@Name, @Genre, @Platform, @Score, @Comment);";

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

            string? collation;

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

        [Fact]
        public async Task CreateAndApply_OnACleanDatabase_AddsThePlayStateAndFavouriteColumns()
        {
            string databaseName = NewDatabaseName();
            string connectionString = _fixture.ConnectionStringFor(databaseName);

            MigrationOutcome outcome = DatabaseMigrator.ForConnectionString(connectionString).CreateAndApply();
            outcome.Succeeded.Should().BeTrue(SqlServerFixture.Describe("The run failed.", outcome));

            IReadOnlyList<ColumnShape> columns;

            using (DbConnection connection = await OpenAsync(connectionString))
            {
                columns = (await connection.QueryAsync<ColumnShape>(new CommandDefinition(
                    ColumnShapeSql,
                    new { Table = "dbo.Game", Names = new[] { "Status", "IsFavourite" } }))).AsList();
            }

            ColumnShape status = columns.Single(column => column.ColumnName == "Status");
            status.TypeName.Should().Be("tinyint");
            status.IsNullable.Should().BeFalse();
            status.DefaultDefinition.Should().Be("((0))");

            ColumnShape favourite = columns.Single(column => column.ColumnName == "IsFavourite");
            favourite.TypeName.Should().Be("bit");
            favourite.IsNullable.Should().BeFalse();
            favourite.DefaultDefinition.Should().Be("((0))");
        }

        [Fact]
        public async Task CreateAndApply_OnACleanDatabase_ConstrainsThePlayStateToTheThreeKnownValues()
        {
            string databaseName = NewDatabaseName();
            string connectionString = _fixture.ConnectionStringFor(databaseName);

            MigrationOutcome outcome = DatabaseMigrator.ForConnectionString(connectionString).CreateAndApply();
            outcome.Succeeded.Should().BeTrue(SqlServerFixture.Describe("The run failed.", outcome));

            using (DbConnection connection = await OpenAsync(connectionString))
            {
                CheckShape? check = await connection.QuerySingleOrDefaultAsync<CheckShape>(new CommandDefinition(
                    CheckShapeSql, new { Table = "dbo.Game", Name = "CK_Game_Status" }));

                check.Should().NotBeNull("the play state is only a number until something limits it");
                check.IsNotTrusted.Should().BeFalse("the constraint was added WITH CHECK, so it also vouches for the rows already there");

                // The definition is one thing; what the server actually refuses is another, and it
                // is the second that protects the catalogue.
                Func<Task> writeAFourthState = () => connection.ExecuteAsync(new CommandDefinition(
                    InsertWithStatusSql, new { Name = "Out Of Range", Status = (byte)3 }));

                await writeAFourthState.Should().ThrowAsync<SqlException>();

                foreach (byte accepted in new byte[] { 0, 1, 2 })
                {
                    await connection.ExecuteAsync(new CommandDefinition(
                        InsertWithStatusSql, new { Name = "In Range", Status = accepted }));
                }
            }
        }

        [Fact]
        public async Task CreateAndApply_OnACleanDatabase_IndexesTheColumnsTheNewFiltersRead()
        {
            string databaseName = NewDatabaseName();
            string connectionString = _fixture.ConnectionStringFor(databaseName);

            MigrationOutcome outcome = DatabaseMigrator.ForConnectionString(connectionString).CreateAndApply();
            outcome.Succeeded.Should().BeTrue(SqlServerFixture.Describe("The run failed.", outcome));

            IReadOnlyList<string> indexes;

            using (DbConnection connection = await OpenAsync(connectionString))
            {
                indexes = (await connection.QueryAsync<string>(new CommandDefinition(
                    IndexNamesSql, new { Table = "dbo.Game" }))).AsList();
            }

            indexes.Should().Contain(new[] { "IX_Game_Status", "IX_Game_IsFavourite" });
        }

        [Fact]
        public async Task CreateAndApply_OnADatabaseThatPredatesTheJournal_KeepsItsRowsAndDefaultsTheNewColumns()
        {
            // This is the case that actually matters: a catalogue someone has been keeping since
            // before any of these scripts existed. It has the original flat table, it has rows,
            // and it has no journal, so the whole chain replays over it. Nothing may be lost,
            // rewritten or reordered, and the two new columns have to arrive filled in.
            string databaseName = NewDatabaseName();
            string connectionString = _fixture.ConnectionStringFor(databaseName);

            // Created without a collation of its own, so it inherits the server's, which the
            // fixture already set to the one the migrator requires.
            EnsureDatabase.For.SqlDatabase(connectionString);

            using (DbConnection connection = await OpenAsync(connectionString))
            {
                await connection.ExecuteAsync(new CommandDefinition(CreateFlatGameTableSql));

                await connection.ExecuteAsync(new CommandDefinition(InsertFlatGameSql, new[]
                {
                    new { Name = "FIFA 24", Genre = (string?)"Sports", Platform = (string?)"PS5", Score = (double?)7.4, Comment = (string?)"Much like the last one." },
                    new { Name = "Hades", Genre = (string?)"Roguelike", Platform = (string?)"PC", Score = (double?)9.6, Comment = (string?)null },
                    new { Name = "Unrated Thing", Genre = (string?)null, Platform = (string?)null, Score = (double?)null, Comment = (string?)null },
                }));
            }

            MigrationOutcome outcome = DatabaseMigrator.ForConnectionString(connectionString).CreateAndApply();

            outcome.Succeeded.Should().BeTrue(SqlServerFixture.Describe("The replay failed.", outcome));
            outcome.AppliedScripts.Select(FileNameOf).Should().Equal(SqlServerFixture.ExpectedScripts);

            IGameRepository repository = new GameRepository(SqlConnectionFactory.ForConnectionString(connectionString));
            PagedResult<Game> all = await repository.ListAsync(GameFilter.None, 1, 50);

            all.TotalCount.Should().Be(3, "the replay must not lose a row");

            Game fifa = all.Items.Single(game => game.Name == "FIFA 24");
            fifa.Genre.Should().Be("Sports");
            fifa.Platforms.Should().Equal("PS5");
            fifa.Score.Should().Be(7.4);
            fifa.LatestReview.Should().Be("Much like the last one.", "the old comment column became a review");

            // Every row, including the one that carried nothing at all, comes out in the state the
            // defaults promise rather than in one the migration invented.
            all.Items.Should().OnlyContain(game => game.Status == PlayStatus.Backlog);
            all.Items.Should().OnlyContain(game => !game.IsFavourite);

            Game bare = all.Items.Single(game => game.Name == "Unrated Thing");
            bare.Genre.Should().BeNull();
            bare.Platforms.Should().BeEmpty();
            bare.Score.Should().BeNull();
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

        /// <summary>
        /// How the server describes one column.
        /// </summary>
        private sealed class ColumnShape
        {
            public string ColumnName { get; set; } = string.Empty;

            public string TypeName { get; set; } = string.Empty;

            public bool IsNullable { get; set; }

            // A column only has a default constraint if one was declared for it, so the join that
            // reads this one is an outer join and the server really can answer with nothing here.
            public string? DefaultDefinition { get; set; }
        }

        /// <summary>
        /// How the server describes one check constraint.
        /// </summary>
        private sealed class CheckShape
        {
            public string Definition { get; set; } = string.Empty;

            public bool IsNotTrusted { get; set; }
        }
    }
}
