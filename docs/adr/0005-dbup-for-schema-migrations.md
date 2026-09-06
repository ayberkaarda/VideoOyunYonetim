# ADR 0005 - DbUp for schema migrations

Status: accepted

## Context

Until now the database was created by running `db/schema.sql` by hand. That script is
idempotent, so re-running it is safe, but it can only ever describe the *current* shape of
the schema. It cannot express a change: normalising `Genre` and `Platform` into lookup
tables means copying the existing text values into new rows and then dropping the old
columns, and a script that both creates a fresh database and rewrites an existing one ends
up as a pile of `IF COL_LENGTH(...) IS NULL` branches that nobody can read or test.

There is also a live database to protect. The development container holds the fifteen
seeded rows plus whatever the user has added since, and its volume is the only copy - the
backup file that these rows were recovered from is no longer tracked. A schema change has
to move that data, not recreate it.

Three shapes were considered: hand-ordered scripts with a version table written for this
project, DbUp, and the migration tooling that ships with an ORM.

## Decision

Use DbUp (`dbup-sqlserver`), with the scripts embedded in `VideoGameManager.Data` under
`Migrations/` and a journal table `dbo.SchemaVersions`.

- Scripts are plain `.sql` files named `NNNN_description.sql`, marked `EmbeddedResource`,
  and applied in name order. They are the only place the schema is allowed to change.
- `db/schema.sql` shrinks to one job: create the database with the right collation. It no
  longer creates tables.
- The application applies pending scripts at startup, before the first window opens.
- `VideoGameManager.Migrator` is a small console project that applies the same scripts to
  a connection string given on the command line, so a clean database can be built and the
  chain re-run without launching the desktop application.

## Rationale

DbUp is roughly the amount of code this project would otherwise have written itself -
a journal table, an ordered script list, one transaction per script - but already tested
against SQL Server and already handling the parts that are easy to get wrong: `GO` batch
splitting, and creating the journal table on first run. Writing that by hand is a day of
work whose failure mode is a half-applied schema.

It stays inside the constraint that the data access library is Dapper. DbUp brings no
object mapping, no query generation and no model: it executes SQL text and records which
files it has executed. `Migrations/0002_normalise_genre_and_platform.sql` is the same SQL
that would have been typed into a query window, which matters because the interesting part
of this phase is the data movement, not the tool.

The ORM route was rejected with the data-access library in ADR 0001, and its migration
tooling would have brought the rest of the ORM with it.

`EnsureDatabase.For.SqlDatabase(..., collation: ...)` means the migrator can create the
database as well as fill it, which is what makes an integration test able to stand up a
throwaway database from nothing. The collation is not cosmetic here: under a Turkish
collation `I` and `i` are different letters, so a search for `fifa` silently returns no
rows for a title stored as `FIFA 24`. The value is stated in both places that can create
the database, and `db/schema.sql` refuses to continue if it finds an existing database
with a different one.

## Consequences

- The schema has a version. `dbo.SchemaVersions` lists which scripts ran and when, and a
  database that is behind is brought forward the next time the application starts.
- A migration script is immutable once it has been applied anywhere. Correcting a mistake
  means adding a script, not editing one, because DbUp identifies a script by name and
  will not notice that the contents changed.
- Every script must survive being run against a database that already has what it
  creates. DbUp normally guarantees a script runs once, but the journal does not exist
  yet in the database that is already in use, so the first run there replays the whole
  chain from the beginning. `0001` therefore recreates today's single-table schema only
  when it is absent.
- Startup now touches the database twice: once to migrate, once for the health check that
  decides whether to open the main window. A machine with no database reachable still
  gets the connection dialog rather than a migration stack trace.
- `Microsoft.Data.SqlClient` moves from 6.1.1 to 6.1.4, which is what `dbup-sqlserver`
  7.2.0 depends on. Pinning the older version would make the build fail on a package
  downgrade rather than quietly resolve it.
- The scripts are embedded in the data-access assembly rather than copied next to the
  executable, so a deployed application cannot be pointed at an edited copy of a
  migration, and the test project gets the same scripts by referencing the project.
