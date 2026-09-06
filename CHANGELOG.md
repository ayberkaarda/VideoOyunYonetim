# Changelog

All notable changes to this project are documented here.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the
project aims to follow [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Phase 1 - Layered architecture (in progress)

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

## Phase 0 - Repository hygiene and English-only codebase

### Added
- `.gitignore` covering Visual Studio caches, build output, database backups and secrets.
- `db/schema.sql` and `db/seed.sql`, both idempotent. The schema and fifteen rows of
  sample data were recovered from the data pages of the SQL Server backup that used to be
  committed to this repository, so the backup is no longer needed to set the project up.
- `db/docker-compose.yml` and `db/.env.example`, so a local SQL Server 2022 instance
  starts with one command.
- `LICENSE` (MIT) and a rewritten `README.md` with in-app screenshots.

### Changed
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

### Fixed
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

### Removed
- Untracked 32 files that should never have been versioned: Visual Studio caches under
  `.vs/`, compiled binaries under `bin/` and `obj/`, and the 4.7 MB database backup. They
  remain on disk.
