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
    public string Platform { get; set; }
    public double? Score { get; set; }
    public string CoverUrl { get; set; }
    public string Comment { get; set; }
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

`Genre` and `Platform` stay strings in Phase 1. They become lookup tables in Phase 3; the
repository interface does not change when they do, because it already exposes them as
strings on `Game`.

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

`ValidationError.Field` carries the domain property name (`nameof(Game.Name)`); the
presenter maps it to a control for `ErrorProvider`.

> The score range is 0-10. Today's combo box offers whole numbers 1-10 while the seed data
> holds 9.5 and 8.6, so the UI cannot express the data it already contains. Phase 1 widens
> the input to accept one decimal place.

## Data

References Domain, `Dapper`, `Microsoft.Data.SqlClient`, `Microsoft.Extensions.Configuration.Abstractions`.

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

Rules:

- Every method is async and takes a `CancellationToken`.
- SQL is a `const string` with a Dapper parameter object. String concatenation or
  interpolation to build SQL is forbidden (`sql-guard` blocks it).
- Column lists are explicit; no `SELECT *`. Schema is qualified (`dbo.Game`).
- `Platform` is always bracketed as `[Platform]`.
- Rows are addressed by `Id`, never by `Name`. Two games may share a name; today's
  `UPDATE dbo.Game SET Comment = @c WHERE Name = @n` writes to the wrong row and reports
  success.
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

`MinimumScore` defaults to **0.0**, which is exactly what the app does today. The original
README claimed "random high-scoring game" while the query had no threshold at all; making
the threshold configurable settles that mismatch without silently changing behaviour.
Phase 5 adds `GenreWeightedStrategy` behind the same interface.

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
