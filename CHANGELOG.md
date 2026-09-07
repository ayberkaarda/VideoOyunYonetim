# Changelog

All notable changes to this project are documented here.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the
project aims to follow [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-09-07

The first release. It collects the six phases that turned a single-project prototype with a
hard-coded connection string into a layered, tested and documented application.

### Phase 6 - Continuous integration and documentation

#### Added
- `.github/workflows/ci.yml`, run on every push to `main`, every pull request and on
  demand. The work is split across two runners because the solution is split across two
  target frameworks: a Linux job builds the four cross-platform projects with
  `-warnaserror` and runs the whole test suite, including the integration tests that start
  their own SQL Server container; a Windows job builds the full solution, desktop project
  included, and runs the format check. Neither runner can do the whole job alone - the
  desktop project does not build on Linux, and the SQL Server image does not run on
  Windows. Test results and the coverage report are kept as artifacts.
- `.editorconfig` describing the style the code already has, so running the formatter is a
  whitespace pass rather than a rewrite: explicit types over `var` (513 declarations
  against 6), `_camelCase` private fields, PascalCase for private `static readonly` fields,
  block-scoped namespaces, `System` usings first, braces required once a body moves to its
  own line.
- ADR 0007, recording that the nullable reference context stays off for now, with the
  measurement behind that decision - 558 diagnostics across 122 files - and the order in
  which it is planned to be turned on.

#### Changed
- The README now documents what the application actually does. It gained an architecture
  diagram, the statistics screen, a development guide covering build, test, format and
  migration commands, the rules the code is expected to keep and why each exists, and a
  contribution checklist. The roadmap no longer describes finished phases as planned, and
  the score range is written as 0-10, which is what the domain enforces.
- `end_of_line` is deliberately left out of `.editorconfig`. The repository is stored with
  LF and the working copy on Windows is CRLF; pinning either would make the format check
  fail on one of the two platforms the project is built on.

#### Removed
- `README.tr.md`. The application, the schema and the documentation were translated to
  English in phase 0, and this file was the last thing left in Turkish. It had also fallen
  out of date - it described a database and table that no longer exist, referred to ADO.NET
  which was replaced by Dapper, and all five of its screenshot links were broken.

#### Fixed
- Formatting drift in three files, found by the first `dotnet format --verify-no-changes`
  run and fixed by the first `dotnet format` run.

### Phase 5 - Search, paging, recommendation strategies, export and statistics

#### Added
- Search and filtering on the browse screen: live search by name, with filters for genre,
  platform, score range, play status and favourites. Filtering happens in SQL, not in the
  list control.
- Server-side paging and sorting through `OFFSET/FETCH`, ordered by name or by score, with
  each sort backed by an index.
- `IRecommendationStrategy` with three implementations behind it, chosen from a drop-down
  on the recommendation screen: `RandomStrategy`, `GenreWeightedStrategy`, which favours
  the genres the catalogue is scored highest in, and `BacklogFirstStrategy`, which prefers
  what has not been played yet.
- A cover image cache. Covers are downloaded asynchronously and kept on disk, so a second
  look at the same game does not hit the network; a placeholder stands in when a link is
  dead, and the failure is logged rather than shown as an exception.
- CSV and JSON export behind a single `IGameExporter`, so the view only ever sees a list of
  formats.
- A statistics screen: distribution by genre and average scores, served by
  `IStatisticsService` and drawn by a hand-written `BarChart` control. No charting library
  was added.
- Migration 0005 adding `PlayStatus` and `IsFavourite`, exposed as a filter on the browse
  screen and as fields on the add screen.

#### Fixed
- The action row on the recommendation screen was laid out in a single column that could
  not hold it. The panel is right-to-left, so the overflow was clipped from the left and
  the "Strategy:" label rendered as "egy:". The row now spans both columns. The build was
  green and every test passed; only a screenshot showed it.

### Phase 4 - Test suite

#### Added
- `VideoGameManager.Tests`, targeting `net10.0`: 396 tests covering the domain validators,
  `Result<T>`, every service, the recommendation strategies, both exporters and the
  composition root, plus integration tests that exercise the repositories and the whole
  migration chain against a real SQL Server.
- ADR 0006 recording the stack and its version pins: xUnit 2.9.3, FluentAssertions 7.2.0,
  NSubstitute 6.2.0, Testcontainers.MsSql 4.15.0 and coverlet.collector 6.0.4.

#### Changed
- FluentAssertions is pinned to the 7.x line. From 8.0 the package moved to a licence that
  charges for commercial use, and this repository is published under MIT.
- Integration tests raise their own SQL Server container and never touch the development
  database, so running them cannot damage a local catalogue.

#### Fixed
- The integration fixture waits until the container reports the collation it was started
  with, instead of connecting as soon as the server accepts logins. The image accepts
  clients before it has applied `MSSQL_COLLATION`, and a session opened in that window is
  rejected when the pool later reuses it - with a message about a failed login that says
  nothing about collation. This failed roughly one run in five; eight consecutive runs
  passed after the fix.

#### Known limitations
- The presenters have no unit tests. The test project targets `net10.0` and the desktop
  project targets `net10.0-windows`, so one cannot reference the other. Moving the
  presenters and view interfaces into a separate cross-platform project would fix this and
  is the first candidate for the next piece of work.

### Phase 3 - Database normalisation, indexes and migrations

#### Added
- A migration chain under `VideoGameManager.Data/Migrations/`, embedded in the assembly and
  applied by DbUp in name order, journalled in `dbo.SchemaVersions` (ADR 0005). The
  application applies what is pending at startup, once the database has answered.
- `VideoGameManager.Migrator`, a console entry point that applies the same scripts to a
  connection string given on the command line and creates the database - with the collation
  the application needs - when it is not there yet.
- `dbo.Genre` and `dbo.Platform` lookup tables, the `dbo.GamePlatform` link table, and
  `dbo.Review` with its own score and `CreatedAt`.
- `CHECK (Score IS NULL OR (Score >= 0 AND Score <= 10))` on both `dbo.Game` and
  `dbo.Review`, matching the range the domain already enforced.
- Indexes for the queries the application runs: `IX_Game_Name`, `IX_Game_Score`,
  `IX_Game_GenreId`, `IX_GamePlatform_PlatformId` and `IX_Review_GameId_CreatedAt`.

#### Changed
- `Game.Platform` became `Game.Platforms`. The link table is a real many-to-many, and a
  single string cannot represent one: read two platforms as `"PC, Xbox"`, write it back,
  and the catalogue gains a platform by that name. The add screen still offers one
  platform, so nothing on screen changes.
- `Game.Comment` became `Game.LatestReview`, a read-only projection of the newest row in
  `dbo.Review`. Nothing writes it back; a review is recorded through the review repository,
  which is now the only place that can. Adding a review with a score still updates the
  game's score, and adding one without a score leaves it alone, as before.
- The listing query is four fixed statements chosen by a `switch` instead of one statement
  ordering by `CASE WHEN @SortByScore = 0 THEN Name END`. The old shape was parameterised
  and safe, but it left the server no ordered index to read, so every page sorted the whole
  table and the new sort indexes would have gone unused.
- Writes that touch more than one table now run in a transaction: adding a game inserts the
  row, upserts its genre and platform names under `UPDLOCK, HOLDLOCK` so two writers cannot
  race into a unique-key violation, and writes the link rows as one unit.
- `db/schema.sql` now only creates the empty database with the required collation. Tables
  are the migration chain's business. `db/seed.sql` targets the normalised tables and no
  longer carries a `USE` statement, so it can seed a throwaway database through `-d`.
- `Microsoft.Data.SqlClient` moved to 6.1.4, the lowest version `dbup-sqlserver` 7.2.0
  resolves to; pinning the older one failed the build on a package downgrade.
- Deleting a game now takes its link rows and its reviews with it, through the foreign keys.

#### Fixed
- `dbo.Game.Name` is `NOT NULL`. A nameless row could not be shown or searched for.
- `GetGenresAsync` and `GetPlatformsAsync` read the lookup tables rather than collecting
  `DISTINCT` values off every game row.

### Phase 2 - Configuration and error handling

#### Added
- File logging with Serilog, behind `Microsoft.Extensions.Logging`: a daily rolling file
  under `%LOCALAPPDATA%\VideoGameManager\logs`, seven files retained, configurable through
  the `Serilog` section of `appsettings.json`. Only the desktop project references Serilog
  (ADR 0004).
- Handlers for the three ways a failure can escape a Windows Forms application: an
  exception reaching the message loop, one reaching the application domain, and a
  background task nobody awaited. Each is logged in full and reported to the user as a
  sentence, never as exception text.
- A startup connection check. When the database does not answer, the application opens a
  dialog naming the server and database it tried - never the password - and offers Retry,
  Continue anyway or Quit instead of failing to start.
- `IDatabaseProbe` and `IDatabaseHealthService`: a `SELECT 1` with a five second connect
  timeout, reported as a status object rather than thrown, so a caller can show a banner
  without catching a provider exception.

#### Changed
- The browse and recommendation screens report an unreachable database inline, in the
  space the list and the details normally occupy. A dialog on every selection change was
  disruptive, and the row stayed selected, so it reopened on the next keystroke.
- Services log a completed write at information level and a rejected input at warning
  level. An exception is logged where it is swallowed - in a presenter or a global handler
  - and nowhere else, so one failure produces one entry.

#### Fixed
- Cover image downloads no longer fail silently. A failed download is logged and the
  placeholder is shown; the recommendation screen never observed the result of its own
  load at all.
- A validation error naming a field the screen has no control for is logged and reported
  instead of being pinned to whichever control happened to be first. The review screen was
  pointing the score rule at its game selector.

### Phase 1 - Layered architecture

#### Added
- `docs/architecture.md`, the contract for the rewrite: project layout, entity shapes,
  repository and service signatures, the MVP view contract and the DI composition root.
  Every signature is fixed before implementation so the layers can be built against each
  other without guessing.
- Architecture decision records for the three choices the brief asked to be justified:
  Dapper over EF Core (ADR 0001), .NET 10 over .NET 8 (ADR 0002) and MVP over MVVM
  (ADR 0003).
- A themed WinForms control library under `VideoGameManager/UI`: a `Theme` token set and
  thirteen owner-drawn controls (`FlatButton`, `FlatCardButton`, `RoundedPanel`,
  `InputFrame`, `RatingBadge`, `TitleBar`, `ChromelessForm`, `CoverImageBox`,
  `GameListBox`, `SearchBox`, `LayoutGrid`). No third-party dependency.
- `UI/DesignGallery.cs` and a `--gallery` command-line argument, so the control library can
  be reviewed without a database.

#### Fixed
- Every colour pair in the new theme is measured against WCAG 2.1. The four main-menu
  buttons the application shipped with fail AA with white text at 2.78, 2.90, 1.63 and
  1.48 to 1; their replacements sit between 5.69 and 6.54.
- `ChromelessForm` restores the window behaviour the borderless forms had lost: Alt+F4,
  Escape, taskbar minimise and restore, and Aero Snap dragging.

### Phase 0 - Repository hygiene and English-only codebase

#### Added
- `.gitignore` covering Visual Studio caches, build output, database backups and secrets.
- `db/schema.sql` and `db/seed.sql`, both idempotent. The schema and fifteen rows of
  sample data were recovered from the data pages of the SQL Server backup that used to be
  committed to this repository, so the backup is no longer needed to set the project up.
- `db/docker-compose.yml` and `db/.env.example`, so a local SQL Server 2022 instance
  starts with one command.
- `LICENSE` (MIT) and a rewritten `README.md` with in-app screenshots.

#### Changed
- Migrated the project to the SDK-style format targeting .NET 10, from .NET Framework
  4.7.2 in the legacy csproj format.
- Replaced `System.Data.SqlClient`, which is absent from the .NET 10 BCL and no longer
  maintained, with `Microsoft.Data.SqlClient`. That provider defaults to `Encrypt=True`,
  so local connection strings now carry `TrustServerCertificate=True`.
- Translated the application to English: the solution, project, namespace and assembly are
  now `VideoGameManager`, along with every form, control, model, event handler, message and
  SQL statement. The database is `VideoGameManager` and the table is `dbo.Game` with
  English column names.
- Pinned `ApplicationHighDpiMode` to `DpiUnaware` to preserve the pixel-aligned layout of
  the fixed-size forms, which matched the .NET Framework 4.7.2 default.

#### Fixed
- Collated the database `Latin1_General_100_CI_AI` instead of `Turkish_CI_AS`. Under a
  Turkish collation `I` and `i` are different letters, so `Name LIKE '%fifa%'` did not
  match `FIFA 24`: search returned nothing and reported no error. Accent-insensitive
  matching comes along with the change, so `pokemon` now matches `Pokemon`.
- Pinned the UI culture to `en-US`. On a Turkish Windows a score of 9.5 rendered as "9,5",
  because `double.ToString()` follows the machine's regional settings.
- Gave the detail captions a fixed width with right alignment. They had been aligned by
  hand with `AutoSize`, so the longer English labels overlapped the value labels and
  "Your Review:" was clipped to "Your".
- Switched the cover `PictureBox` from `StretchImage` to `Zoom`, so cover art keeps its
  aspect ratio, and sorted the game list by name.
- Declared the review column as `NVARCHAR(MAX)` instead of the original `TEXT`, a
  deprecated non-Unicode type that mangled non-ASCII characters.

#### Removed
- Untracked 32 files that should never have been versioned: Visual Studio caches under
  `.vs/`, compiled binaries under `bin/` and `obj/`, and the 4.7 MB database backup. They
  remain on disk.
