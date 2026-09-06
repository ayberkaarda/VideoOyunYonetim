<div align="center">

# 🎮 Video Game Manager

**A Windows desktop app for cataloguing, rating and discovering video games.**

[![Platform](https://img.shields.io/badge/platform-Windows-0078D6)](#)
[![Language](https://img.shields.io/badge/C%23-.NET%2010-512BD4)](#)
[![Database](https://img.shields.io/badge/database-SQL%20Server-CC2927)](#)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

<img src="screenshots/main_menu.png" alt="Main menu" width="620">

</div>

---

## Overview

Video Game Manager is a Windows Forms application backed by SQL Server. You add games to a
personal catalogue with a genre, a platform, a score and a cover image, browse the
catalogue with full details, leave a written review on any game, and get a random pick
when you cannot decide what to play.

## Features

| | |
|---|---|
| ➕ **Add a game** | Name, genre, platform, score (1–10) and a cover image URL |
| 📃 **Browse & inspect** | Pick a game from the list and see every field plus its cover art |
| 🗣️ **Review** | Attach a written review to any game in the catalogue |
| 🎲 **Recommend** | Draw a random game from the catalogue with its cover |
| 🪟 **Custom chrome** | Borderless forms with hand-rolled minimise/close buttons |

## Screens

<table>
<tr>
<td width="50%">

**Add a game** — `AddGameForm`

<img src="screenshots/add_game.png" alt="Add game screen" width="100%">

</td>
<td width="50%">

**Browse games** — `BrowseGamesForm`

<img src="screenshots/browse_games.png" alt="Browse games screen" width="100%">

</td>
</tr>
<tr>
<td width="50%">

**Recommendation** — `RecommendationForm`

<img src="screenshots/recommendation.png" alt="Recommendation screen" width="100%">

</td>
<td width="50%">

**Review a game** — `ReviewGameForm`

<img src="screenshots/review_game.png" alt="Review screen" width="100%">

</td>
</tr>
</table>

## Tech stack

- **C# / Windows Forms** on **.NET 10** (SDK-style project)
- **SQL Server** for storage — the repository ships a Docker Compose file
- **ADO.NET** via [`Microsoft.Data.SqlClient`](https://github.com/dotnet/SqlClient) with parameterised commands

## Getting started

### Prerequisites

- Windows 10/11
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) — `winget install Microsoft.DotNet.SDK.10`
- Docker Desktop (recommended) **or** any SQL Server instance you already have
- `sqlcmd` — `winget install Microsoft.Sqlcmd`

Visual Studio is optional; the solution builds and runs entirely from the CLI.

### 1. Start SQL Server

```powershell
# pick a password and put it in db/.env (git-ignored)
Copy-Item db/.env.example db/.env    # then edit the password

docker compose -f db/docker-compose.yml up -d
docker compose -f db/docker-compose.yml ps    # wait for "healthy"
```

Already running SQL Server (Express, Developer, LocalDB)? Skip this step and use your own
server name in the commands below.

### 2. Create the database

The database is created from version-controlled SQL scripts. Both are idempotent, so
re-running them is safe.

```powershell
$sa = (Get-Content db/.env | Select-String 'MSSQL_SA_PASSWORD=(.*)').Matches.Groups[1].Value

# schema — creates the VideoGameManager database and the Game table
sqlcmd -S localhost,1433 -U sa -P $sa -C -i db/schema.sql

# sample data — 15 games, only inserts rows that are missing
sqlcmd -S localhost,1433 -U sa -P $sa -C -i db/seed.sql

# verify
sqlcmd -S localhost,1433 -U sa -P $sa -C -d VideoGameManager -Q "SELECT COUNT(*) FROM dbo.Game"   # 15
```

Prefer a GUI? Open both files in SQL Server Management Studio or Azure Data Studio and
execute them in that order.

### 3. Point the app at your server

Open [`VideoGameManager/DatabaseHelper.cs`](VideoGameManager/DatabaseHelper.cs) and set the
connection string:

```csharp
private static string connectionString =
    "Server=localhost,1433;Database=VideoGameManager;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;";
```

> The connection string is currently hard-coded. Moving it to `appsettings.json` is
> tracked as Phase 2 of the [roadmap](#roadmap).

### 4. Build and run

```powershell
# from the repository root
dotnet build VideoGameManager.sln
dotnet run --project VideoGameManager
```

Or open `VideoGameManager.sln` in Visual Studio and press <kbd>F5</kbd>.

To review the shared control library on its own - every control in every state, no database
needed:

```powershell
dotnet run --project VideoGameManager -- --gallery
```

## Repository layout

```
.
├── db/
│   ├── docker-compose.yml       # local SQL Server 2022 container
│   ├── .env.example             # template for the SA password
│   ├── schema.sql               # database + table definition (idempotent)
│   └── seed.sql                 # 15 sample games (idempotent)
├── screenshots/                 # images used by this README
├── VideoGameManager/
│   ├── Program.cs               # entry point
│   ├── MainForm.cs              # main menu
│   ├── AddGameForm.cs           # add a game
│   ├── BrowseGamesForm.cs       # browse the catalogue
│   ├── RecommendationForm.cs    # random recommendation
│   ├── ReviewGameForm.cs        # write a review
│   ├── DatabaseHelper.cs        # ADO.NET helper
│   ├── Game.cs                  # game model
│   ├── GameStore.cs             # in-memory store (unused; removed in Phase 1)
│   └── Player.cs                # player model (unused; removed in Phase 1)
└── VideoGameManager.sln
```

### Database schema

`dbo.Game`, collated `Latin1_General_100_CI_AI` so that `LIKE '%fifa%'` matches `FIFA 24`
and `pokemon` matches `Pokémon`.

| Column | Type | Notes |
|---|---|---|
| `Id` | `INT IDENTITY` | Primary key |
| `Name` | `NVARCHAR(100)` | Game name |
| `Genre` | `NVARCHAR(50)` | Action, RPG, Strategy, … |
| `Platform` | `NVARCHAR(50)` | PC / PlayStation / PS5 / Xbox / Switch |
| `Score` | `FLOAT` | Score, 1–10 |
| `CoverUrl` | `NVARCHAR(MAX)` | Cover image URL |
| `Comment` | `NVARCHAR(MAX)` | User review |

## Roadmap

This project is being lifted from a student-grade prototype to a maintainable application
in numbered phases:

| Phase | Scope | Status |
|---|---|---|
| 0 | Repository hygiene — `.gitignore`, SQL scripts instead of a `.bak`, SDK-style .NET 10 project, English-only codebase | ✅ done |
| 1 | Layered architecture — Domain / Data / Services / WinForms, MVP, dependency injection | ⏳ planned |
| 2 | Configuration & error handling — `appsettings.json`, Serilog, validation, `async/await` | ⏳ planned |
| 3 | Database — normalisation, indexes, migrations | ⏳ planned |
| 4 | Tests — xUnit, FluentAssertions, NSubstitute | ⏳ planned |
| 5 | Features — search, paging, smarter recommendations, image cache, export, statistics | ⏳ planned |
| 6 | CI & documentation — GitHub Actions, `.editorconfig`, `CHANGELOG.md` | ⏳ planned |

## Contributing

Issues and pull requests are welcome. Please keep each phase in its own commit and make
sure the solution still builds before opening a PR.

## Author

**Ayberk Arda** — [@ayberkaarda](https://github.com/ayberkaarda)

## License

Released under the [MIT License](LICENSE).
