# Changelog

All notable changes to this project are documented here.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the
project aims to follow [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2026-09-08

A round of work on top of the first release. The presentation layer moved into a project of
its own so it could be tested, the nullable reference context was turned on everywhere, the
cover art pipeline stopped re-downloading full-size images, and the catalogue can now be
read back in from a file as well as written out.

### Added
- Import the catalogue from a JSON file, on the browse screen beside Export.
  `JsonGameImporter` reads back exactly what `JsonGameExporter` writes, so an export and an
  import are a round trip - proven by a test that writes with one and reads with the other.
  The stored `Id` is deliberately not read: an imported game is a new row, and a number from
  another database either collides or means something else.

  Importing is not destructive. A game whose title is already stored is skipped, compared
  without regard to case, so importing the same file twice cannot duplicate or overwrite the
  catalogue. A file that is not JSON, or not an array of games, imports nothing at all; a
  single entry that breaks a domain rule is skipped while the rest still arrive. Every run
  reports the three counts it produced - stored, already present, refused by the validator -
  and the counts always add up to the number of entries in the file.
- `VideoGameManager.Presentation`, a `net10.0` project holding the view interfaces and the
  presenters, which used to sit inside the desktop project. They mentioned no WinForms type
  even then, but a `net10.0` test project cannot reference a `net10.0-windows` one, so the
  layer that holds the screen logic was the only one in the solution with no tests at all.
  The namespaces did not move, so not one form file changed (ADR 0008).
- Unit tests for all five presenters, driven through their `IView` with substituted views
  and services: 91% line coverage on a project that had none. Plus a unit test for
  `SqlConnectionFactory`'s connection-string validation and an integration test for
  `SqlDatabaseProbe` against both a reachable and an unreachable server, which lifted the
  data layer from 87% to 92%. The suite is 471 tests.
- `Directory.Build.props`, holding the settings every project repeated - the language
  version, implicit usings, and the nullable context.
- `global.json`, pinning the SDK feature band with `rollForward: latestFeature`.
- `README.tr.md`, a Turkish translation of the README. The English one remains the README
  the repository opens with; the two are updated together.
- ADR 0010, recording why `README.md` stays the primary document and `README.tr.md` a
  translation kept beside it, rather than the other way around, which is what an earlier
  phase of the brief had called for.
- `VideoGameManager.Tests.Desktop`, a second test project targeting `net10.0-windows`, for
  code that cannot be reached from a portable one. It holds nine tests covering the cover
  art pipeline - scaling, cache keys, negative caching and the cache bound - and the
  Windows CI job runs it. Everything else stays in `VideoGameManager.Tests` and still runs
  on Linux (ADR 0009).
- A `User-Agent` on the cover art requests. Wikimedia answers 403 without one, which is
  what several "dead" cover links actually were.

### Changed
- The nullable reference context is on for the whole solution, turned on one layer at a
  time in the direction the dependencies run: entities, repositories, services, presenters,
  then the forms and the tests. The setting lives in `Directory.Build.props` and no project
  overrides it, so a new project inherits it rather than having to remember it (ADR 0007).

  Absence is annotated where absence is a real answer - `Game.Genre`, `Game.CoverUrl`,
  `Game.LatestReview`, the text fields of `GameFilter`, every repository and service method
  that already returned `null` to mean "no such game", `IGameListView.ShowDetails` when
  nothing is selected and `IRecommendationView.ShowGame` when no recommendation was found.
  Fields where absence is not a real answer start empty instead: `Game.Name`,
  `Game.Platforms`, `Review.Body`.

  No null-forgiving operator, no `required` modifier and no in-file pragma appears in
  production code. In the tests `null!` does appear, and deliberately: a test asserting that
  a guard rejects `null` is asserting what happens when a caller ignores the annotation, so
  the annotation is overridden rather than the test weakened.
- `SearchBox.Text` carries `[AllowNull]`. `Control.Text` accepts `null`, and the override
  was quietly stricter than the member it replaced.
- Nine `[InlineData(null)]` theory parameters are `string?`. The mismatch was invisible
  while the nullable context was off and became a build error under `-warnaserror` once it
  was on.
- The screen name recorded in log scopes is the presenter rather than the form, which no
  longer exists in that assembly. Log files written before and after this change use
  different names for the same screen.
- The CI workflow runs `actions/checkout@v5`, `actions/setup-dotnet@v5` and
  `actions/upload-artifact@v6` - the oldest versions of each that run on Node 24, now that
  GitHub is retiring Node 20.

### Performance
- A cached cover is stored in the format its content calls for: JPEG at quality 90 unless
  the picture has transparent pixels, in which case PNG is kept. The decision is made by
  inspecting the pixels rather than trusting the declared pixel format - cover art published
  as PNG routinely carries a fully opaque alpha channel, and the catalogue's largest cover is
  exactly such a file, so the cheap check would have kept it lossless for no benefit. The
  three largest covers went from 1,887,210 bytes on disk to 236,646, and their decode time
  roughly halved. The scan costs under a millisecond, once per download.
- Cache files left under the previous naming scheme are deleted once per run, limited to the
  cache folder's own top level and to that exact extension, with every failure swallowed.
- Cover art is scaled to twice the size it is drawn at before it reaches the disk cache,
  and the target size is mixed into the cache key so an entry cannot outlive the size it
  was stored for. Decoding the largest cover in the sample data went from 71 ms to 4 ms,
  because what is decoded is now a 480-pixel image rather than a 2600-pixel one.
- The recommendation screen goes through the same cached path as the browse screen. Its
  cover slot was a plain `PictureBox` calling `LoadAsync`, so every recommendation
  downloaded the full-size artwork again: measured at 1024 ms for the largest cover, now
  paid once and then 5 ms from disk or nothing at all from memory.
- `ICoverImageProvider` is a singleton in the composition root rather than something each
  form constructs for itself. The in-memory cache used to be discarded every time a window
  closed, and `BrowseGamesForm` built two providers and threw the first away.
- An address that fails is remembered for the rest of the run, so a dead cover link costs
  one timeout rather than one per selection. The request timeout is 6 seconds, down from
  10.
- The in-memory cache holds 32 covers and disposes what it evicts. It had no bound, which
  did not matter while it died with the window and would have as a singleton.

### Fixed
- ADR 0007 reported roughly twice as many nullable diagnostics as there were. MSBuild prints
  every diagnostic twice, once where it occurs and once in the project summary, so counting
  the lines a build emits doubles the real figure. The record now carries unique counts and
  says how to count them.
- Documentation that named tooling which is not part of this repository, in two ADRs and the
  architecture document. The reasons now stand on their own.
- The Celeste and Stardew Valley seed rows point at the direct file URLs. Their thumbnail
  paths answer 400 whatever size is asked for, while the file paths answer 200. The other
  two broken links are left alone rather than guessed at: Hollow Knight's original is a
  `.webp`, which `System.Drawing` cannot decode, and the FIFA 24 link is genuinely gone.
  Existing databases keep the old values - the seed script only inserts rows that are
  missing, it does not update rows that are there.
- The FIFA 24 and Hollow Knight seed rows, the two links the fix above left alone for want
  of anything better. FIFA 24's answered 404 and Hollow Knight's answered 400. Both now
  point at Steam's CDN, which answers with the image itself. As before, an existing
  database keeps whatever it already has - the seed script only inserts rows that are
  missing.
- Two unreachable null checks in the exporters, left over from before `Game.Platforms` was
  guaranteed never to be null.
- `VideoGameManager.Tests.Desktop`'s coverage report never showed the assembly it exists to
  measure. Coverlet's Mono.Cecil-based instrumenter cannot resolve `System.Windows.Forms`
  while rewriting `VideoGameManager.dll` - it sees reference assemblies only, not the
  runtime pack - and dropped that assembly from the report instead of failing the build, so
  the run stayed green while covering nothing in the one project the report was for.
  `PreserveCompilationContext` on the test project fixes the resolution; `VideoGameManager`
  now appears in the report, with `CachedCoverImageProvider` at 81.0% line coverage.

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
  measurement behind that decision and the order in which it is planned to be turned on.
  (The figure that record originally carried was double the real one; see the Unreleased
  section above.)

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

[1.1.0]: https://github.com/ayberkaarda/VideoOyunYonetim/releases/tag/v1.1.0
[1.0.0]: https://github.com/ayberkaarda/VideoOyunYonetim/releases/tag/v1.0.0
