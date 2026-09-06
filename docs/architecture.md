# Architecture

This document is the contract for the layered rewrite (Phase 1). Every interface
signature here is fixed before implementation starts, so that the layers can be built
against each other without guessing.

## Projects and dependency direction

```
VideoGameManager.WinForms   net10.0-windows   Forms, Views, Presenters, composition root
        |
        v
VideoGameManager.Services   net10.0           Business rules, recommendation strategies
        |
        v
VideoGameManager.Data       net10.0           Dapper repositories, connection factory, migrations
        |
        v
VideoGameManager.Domain     net10.0           Entities, enums, validation. No dependencies.
```

`VideoGameManager.Tests` (net10.0) references all four.

`VideoGameManager.Migrator` (net10.0) is a console entry point that references Data alone.
It exists so that a database can be created and brought up to date without starting the
desktop application - from a build agent, or against a throwaway database in a test.

The arrow is one-way and enforced by project references plus the `layer-guard` hook. Only
the WinForms project targets Windows, so Domain, Data and Services can run their tests on
a Linux CI agent.

## Domain

No package references. No `System.Windows.Forms`, no `Microsoft.Data.SqlClient`.

```csharp
public sealed class Game
{
    public int Id { get; init; }
    public string Name { get; set; }
    public string Genre { get; set; }
    public IReadOnlyList<string> Platforms { get; set; }
    public double? Score { get; set; }
    public string CoverUrl { get; set; }
    public string LatestReview { get; init; }
}

public sealed class Review
{
    public int Id { get; init; }
    public int GameId { get; init; }
    public double? Score { get; set; }
    public string Body { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
}
```

`Genre` and `Platform` were strings in Phase 1 and became lookup tables in Phase 3. The
repository interface did not change, because it already exposed them by name rather than
by key - but the entity did, in two places:

- `Platforms` replaced `Platform`. The bridge table `dbo.GamePlatform` is a genuine
  many-to-many, and a single string cannot represent it. Mirroring a set as one value is
  the kind of mismatch that corrupts silently: read two platforms as `"PC, Xbox"`, write
  it back, and the catalogue gains a platform by that name. The list is never `null`; a
  game with no platform row reads as an empty list. The add screen still offers one
  platform, so every row written today has exactly one.
- `LatestReview` replaced `Comment`. It is a read-only projection of the newest row in
  `dbo.Review` (`ORDER BY CreatedAt DESC, Id DESC`), filled by an `OUTER APPLY` in the
  repository's `SELECT` and written by nothing. The only way to record a review is
  `IReviewRepository`.

`Game.Score` is the game's own rating and stays on `dbo.Game`. It is not derived from
review scores: a game can be scored without ever being reviewed, which is what the add
screen does, and sorting, the score filter and the recommendation threshold all read it.
Adding a review with a score still updates the game's score, as the application has always
done; adding one without a score leaves it alone.

### Validation

Validation rules live here and nowhere else. The UI renders the result, it does not
re-implement the rules.

```csharp
public readonly record struct ValidationError(string Field, string Message);

public sealed class ValidationResult
{
    public static ValidationResult Ok { get; }
    public bool IsValid { get; }
    public IReadOnlyList<ValidationError> Errors { get; }
}

public static class GameValidator
{
    // Name required, <= 100 chars. Genre and Platform required.
    // Score null or within [ScoreRange.Min, ScoreRange.Max].
    // CoverUrl null/empty or an absolute http/https URI.
    public static ValidationResult Validate(Game game);
}

public static class ScoreRange
{
    public const double Min = 0.0;
    public const double Max = 10.0;
}
```

`ReviewValidator` lives here too, for the same reason: `IReviewService.AddAsync` takes a
score and a body that have to be checked, and putting that rule in Services would split
validation across two layers.

`ValidationError.Field` carries the domain property name (`nameof(Game.Name)`); the
presenter maps it to a control for `ErrorProvider`.

> The score range is 0-10. Today's combo box offers whole numbers 1-10 while the seed data
> holds 9.5 and 8.6, so the UI cannot express the data it already contains. Phase 1 widens
> the input to accept one decimal place.

## Data

References Domain, `Dapper`, `Microsoft.Data.SqlClient`, `dbup-sqlserver` and
`Microsoft.Extensions.Configuration.Abstractions`.

```csharp
public interface IDbConnectionFactory
{
    DbConnection Create();
}

public sealed record GameFilter(
    string Name = null,
    string Genre = null,
    string Platform = null,
    double? MinScore = null,
    double? MaxScore = null);

public enum GameSortField { Name, Score }

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

public interface IGameRepository
{
    Task<PagedResult<Game>> ListAsync(GameFilter filter, int page, int pageSize,
        GameSortField sort = GameSortField.Name, bool descending = false,
        CancellationToken ct = default);

    Task<Game> GetAsync(int id, CancellationToken ct = default);
    Task<int> AddAsync(Game game, CancellationToken ct = default);      // returns new Id
    Task<bool> UpdateAsync(Game game, CancellationToken ct = default);  // false if Id missing
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);     // false if Id missing
    Task<IReadOnlyList<string>> GetGenresAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetPlatformsAsync(CancellationToken ct = default);
    Task<Game> GetRandomAsync(double minScore, CancellationToken ct = default);
}

public interface IReviewRepository
{
    Task<IReadOnlyList<Review>> GetForGameAsync(int gameId, CancellationToken ct = default);
    Task<int> AddAsync(Review review, CancellationToken ct = default);
}
```

### Schema

Phase 3 normalised the single table into five, and every change to them goes through a
migration script (see below).

```
dbo.Genre         Id, Name (unique)
dbo.Platform      Id, Name (unique)
dbo.Game          Id, Name, GenreId -> dbo.Genre, Score, CoverUrl
dbo.GamePlatform  GameId -> dbo.Game, PlatformId -> dbo.Platform     (composite key)
dbo.Review        Id, GameId -> dbo.Game, Score, Body, CreatedAt
dbo.SchemaVersions                                            (migration journal)
```

Both `Score` columns carry `CHECK (Score IS NULL OR (Score >= 0 AND Score <= 10))`, which
is the same range `ScoreRange` enforces in the domain. The rule lives in the domain and
the constraint is the backstop for anything that reaches the database another way.

`GamePlatform.GameId` and `Review.GameId` cascade on delete, so deleting a game stays the
single statement it was.

Indexes: `IX_Game_Name`, `IX_Game_Score (Score DESC, Id)`, `IX_Game_GenreId`,
`IX_GamePlatform_PlatformId (PlatformId, GameId)` and
`IX_Review_GameId_CreatedAt (GameId, CreatedAt DESC, Id DESC)`.

For the sort indexes to be usable the listing query is written as four fixed statements -
name ascending, name descending, score ascending, score descending - chosen by a `switch`.
The single statement it replaced ordered by `CASE WHEN @SortByScore = 0 THEN Name END`,
which is parameterised and safe but leaves the server no ordered index to read from, so
every page cost a sort of the whole table.

Search is `LIKE '%term%'`, which cannot seek. `IX_Game_Name` still helps: the scan reads a
narrow index instead of the clustered table, and the same index serves name ordering.

### Migrations

`VideoGameManager.Data/Migrations/*.sql`, embedded in the assembly and applied in name
order by DbUp, journalled in `dbo.SchemaVersions`. The application applies pending scripts
at startup once the database has answered; `VideoGameManager.Migrator` applies the same
scripts from the command line, and creates the database when it is missing. See
[ADR 0005](adr/0005-dbup-for-schema-migrations.md).

Every script is written to be safe to re-run. DbUp normally runs a script once, but a
database that was in use before the journal existed replays the whole chain on its first
migration, so `0001` recreates the original single-table schema only when it is absent.

The connection string is read from configuration under the key
`ConnectionStrings:VideoGameManager`. The WinForms `appsettings.json` must use the same
key; this is the integration point between the two.

Rules:

- Every method is async and takes a `CancellationToken`.
- SQL is a `const string` with a Dapper parameter object. String concatenation or
  interpolation to build SQL is forbidden (`sql-guard` blocks it).
- Column lists are explicit; no `SELECT *`. Schema is qualified (`dbo.Game`).
- `Platform` is a table now, not a column, and is written schema-qualified as
  `dbo.Platform`. The rule that a bare `Platform` must be bracketed still stands for any
  statement that names it unqualified.
- Rows are addressed by `Id`, never by `Name`. Two games may share a name; the original
  `UPDATE dbo.Game SET Comment = @c WHERE Name = @n` wrote to the wrong row and reported
  success. Genre and platform names are the exception - they are unique by constraint, and
  the lookup upsert matches on them under `UPDLOCK, HOLDLOCK` so that two writers adding
  the same new name cannot race into a unique-key violation.
- A write that touches more than one table runs in a transaction: adding a game inserts
  the game, upserts its genre and platform names, and writes the bridge rows as one unit.
- Repositories do not catch. `SqlException` surfaces to the service layer, which wraps it.

## Services

References Domain and Data.

```csharp
public sealed class DataAccessException : Exception { }   // wraps SqlException

public interface IGameService
{
    Task<PagedResult<Game>> SearchAsync(GameFilter filter, int page, int pageSize,
        GameSortField sort = GameSortField.Name, bool descending = false,
        CancellationToken ct = default);

    Task<Game> GetAsync(int id, CancellationToken ct = default);
    Task<Result<int>> AddAsync(Game game, CancellationToken ct = default);
    Task<Result> UpdateAsync(Game game, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetGenresAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetPlatformsAsync(CancellationToken ct = default);
}

public interface IReviewService
{
    Task<IReadOnlyList<Review>> GetForGameAsync(int gameId, CancellationToken ct = default);
    Task<Result> AddAsync(int gameId, double? score, string body, CancellationToken ct = default);
}

public interface IRecommendationStrategy
{
    string Name { get; }
    Task<Game> PickAsync(CancellationToken ct = default);
}

public interface IRecommendationService
{
    IReadOnlyList<string> AvailableStrategies { get; }
    Task<Game> RecommendAsync(string strategyName = null, CancellationToken ct = default);
}
```

`Result` / `Result<T>` carry either success (with a value) or a `ValidationResult`. The
service validates before touching the repository, so an invalid `Game` never reaches SQL.

### Recommendation

`RandomStrategy` is the Phase 1 implementation and reproduces today's behaviour:
`SELECT TOP 1 ... ORDER BY NEWID()`, filtered by a configurable `MinimumScore`.

`MinimumScore` defaults to **0.0**, which is what the app does today for every row that has
a score. The original README claimed "random high-scoring game" while the query had no
threshold at all; making the threshold configurable settles that mismatch without silently
changing behaviour. Phase 5 adds `GenreWeightedStrategy` behind the same interface.

> One latent difference: `Score >= 0` excludes rows whose score is `NULL`, while today's
> unfiltered query would have offered them. All fifteen seeded rows have a score, so
> nothing changes yet - but `Score` is nullable in the domain model, so a game added
> without one would never be recommended. Revisit when Phase 5 introduces a wishlist or
> backlog state, where an unscored game is the normal case.

## WinForms (MVP)

References Services and Domain. It must not reference Data.

Each form splits into three files:

```
Views/IGameListView.cs      no System.Windows.Forms types
Presenters/GameListPresenter.cs
Forms/BrowseGamesForm.cs    partial class : ChromelessForm, IGameListView
```

The view exposes properties and events only:

```csharp
public interface IGameListView
{
    IReadOnlyList<Game> Games { set; }
    Game SelectedGame { get; set; }
    bool IsBusy { set; }

    event EventHandler Loaded;
    event EventHandler SelectionChanged;
    event EventHandler DeleteRequested;
    event EventHandler EditRequested;

    void ShowError(string message);
    void ShowFieldError(string field, string message);
    void ShowInfo(string message);
    bool Confirm(string message);
}
```

Rules:

- The presenter never references `Form`, `Control` or `MessageBox`. User feedback goes
  through the view's `ShowError` / `ShowInfo` / `Confirm`.
- The form's event handlers only raise view events; they contain no logic and no SQL.
- No `async void` outside event handlers, and every `async void` handler has a try/catch
  that routes failures to `ShowError`.
- `*.Designer.cs` keeps the control names; only the namespace changes.

### Composition root

`Program.cs` builds the container and resolves the main form:

```csharp
var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);
services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
services.AddScoped<IGameRepository, GameRepository>();
services.AddScoped<IReviewRepository, ReviewRepository>();
services.AddScoped<IGameService, GameService>();
services.AddScoped<IReviewService, ReviewService>();
services.AddScoped<IRecommendationStrategy, RandomStrategy>();
services.AddScoped<IRecommendationService, RecommendationService>();
services.AddTransient<MainForm>();
services.AddTransient<BrowseGamesForm>();
// ... one registration per form and presenter
Application.Run(provider.GetRequiredService<MainForm>());
```

Forms are resolved from the container, never constructed with `new` from another form.

## What happens to today's code

| Today | Becomes |
|---|---|
| `DatabaseHelper` (static, hard-coded connection string) | `SqlConnectionFactory` + repositories |
| SQL inside form code-behind | `VideoGameManager.Data` repositories |
| `MessageBox.Show(ex.Message)` | logged by the service, `IView.ShowError` with a readable message |
| `Game.cs` | `VideoGameManager.Domain/Game.cs` |
| `GameStore.cs`, `Player.cs` | deleted; nothing references them |
| `pictureBox.Load(url)` on the UI thread | `ICoverImageProvider` with async load and a placeholder |

## Build order

Domain → Data → Services → WinForms. Each layer builds and its tests pass before the next
one starts. The UI control library under `VideoGameManager/UI/` is independent of this
chain and can be built in parallel.
