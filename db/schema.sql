/*
 * Video Game Manager - database creation
 * --------------------------------------
 * This script only creates the empty database. It is idempotent: it can be run
 * more than once.
 *
 * Tables, columns, constraints and indexes are NOT defined here. They are
 * created by the migration scripts that ship inside the data layer, and running
 * the migrator is the only supported way to change the schema. A hand-made
 * ALTER leaves a database that no clean install can be rebuilt to match.
 *
 * Usage:
 *
 *   1. Create the database:
 *        sqlcmd -S localhost,1433 -U sa -P <password> -C -i db/schema.sql
 *
 *   2. Create the tables (this step also creates the database if you skipped
 *      step 1, so step 1 is only useful when you want it created by hand):
 *        dotnet run --project VideoGameManager.Migrator -- \
 *          "Server=localhost,1433;Database=VideoGameManager;User Id=sa;Password=<password>;TrustServerCertificate=True"
 *
 *   3. Load the example catalogue:
 *        sqlcmd -S localhost,1433 -U sa -P <password> -C -d VideoGameManager -f 65001 -i db/seed.sql
 *
 * The original schema was reverse-engineered from the SQL Server backup that
 * used to live in this repository. Three deviations from that backup were
 * deliberate and still hold:
 *
 *   1. Review text is NVARCHAR(MAX). The backup declared the equivalent column
 *      as `TEXT`, a deprecated non-Unicode type that mangled non-ASCII
 *      characters. The text now lives in its own Review table.
 *
 *   2. The collation is Latin1_General_100_CI_AI, not Turkish_CI_AS. Under a
 *      Turkish collation `I` and `i` are different letters, so
 *      `Name LIKE '%fifa%'` does NOT match 'FIFA 24' -- search silently
 *      returns nothing, with no error. Accent insensitivity is a bonus:
 *      'pokemon' matches 'Pokemon'.
 *
 *   3. Identifiers are English. The backup used Turkish ones.
 */

IF DB_ID(N'VideoGameManager') IS NULL
BEGIN
    PRINT N'Creating database VideoGameManager...';
    EXEC (N'CREATE DATABASE [VideoGameManager] COLLATE Latin1_General_100_CI_AI;');
END
ELSE IF CONVERT(NVARCHAR(128), DATABASEPROPERTYEX(N'VideoGameManager', 'Collation')) <> N'Latin1_General_100_CI_AI'
    RAISERROR(N'VideoGameManager already exists with a different collation. Drop it (docker compose -f db/docker-compose.yml down -v) and run this script again.', 16, 1);
ELSE
    PRINT N'Database VideoGameManager already exists.';
GO

PRINT N'Database ready. Run the migrator next to create the tables.';
GO
