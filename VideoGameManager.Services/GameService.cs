using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VideoGameManager.Data;
using VideoGameManager.Domain;

namespace VideoGameManager.Services
{
    /// <summary>
    /// Default <see cref="IGameService"/>: validates, then delegates to the repository.
    /// </summary>
    public sealed class GameService : IGameService
    {
        private const string ReadFailed = "The game catalogue could not be read.";
        private const string WriteFailed = "The change could not be saved to the game catalogue.";

        private readonly IGameRepository _games;

        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="games">Repository the service delegates to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="games"/> is <c>null</c>.</exception>
        public GameService(IGameRepository games)
        {
            _games = games ?? throw new ArgumentNullException(nameof(games));
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="page"/> or <paramref name="pageSize"/> is below one. The caller
        /// controls both, so a wrong value is a defect rather than user input.
        /// </exception>
        public Task<PagedResult<Game>> SearchAsync(GameFilter filter, int page, int pageSize,
            GameSortField sort = GameSortField.Name, bool descending = false,
            CancellationToken ct = default)
        {
            if (page < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(page), page, "Page numbering starts at one.");
            }

            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "A page holds at least one row.");
            }

            return DatabaseCall.RunAsync(
                () => _games.ListAsync(filter, page, pageSize, sort, descending, ct),
                ReadFailed);
        }

        /// <inheritdoc />
        public Task<Game> GetAsync(int id, CancellationToken ct = default) =>
            DatabaseCall.RunAsync(() => _games.GetAsync(id, ct), ReadFailed);

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException"><paramref name="game"/> is <c>null</c>.</exception>
        public async Task<Result<int>> AddAsync(Game game, CancellationToken ct = default)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            Game candidate = Normalise(game);
            ValidationResult validation = GameValidator.Validate(candidate);

            if (!validation.IsValid)
            {
                return Result<int>.Invalid(validation);
            }

            int id = await DatabaseCall
                .RunAsync(() => _games.AddAsync(candidate, ct), WriteFailed)
                .ConfigureAwait(false);

            return Result<int>.Success(id);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException"><paramref name="game"/> is <c>null</c>.</exception>
        public async Task<Result> UpdateAsync(Game game, CancellationToken ct = default)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            if (game.Id <= 0)
            {
                return Result.Invalid(new ValidationError(nameof(Game.Id), "The game to update was not identified."));
            }

            Game candidate = Normalise(game);
            ValidationResult validation = GameValidator.Validate(candidate);

            if (!validation.IsValid)
            {
                return Result.Invalid(validation);
            }

            bool updated = await DatabaseCall
                .RunAsync(() => _games.UpdateAsync(candidate, ct), WriteFailed)
                .ConfigureAwait(false);

            return updated
                ? Result.Success()
                : Result.Invalid(new ValidationError(nameof(Game.Id), "That game no longer exists."));
        }

        /// <inheritdoc />
        public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
        {
            if (id <= 0)
            {
                return Result.Invalid(new ValidationError(nameof(Game.Id), "The game to delete was not identified."));
            }

            bool deleted = await DatabaseCall
                .RunAsync(() => _games.DeleteAsync(id, ct), WriteFailed)
                .ConfigureAwait(false);

            return deleted
                ? Result.Success()
                : Result.Invalid(new ValidationError(nameof(Game.Id), "That game no longer exists."));
        }

        /// <inheritdoc />
        public Task<IReadOnlyList<string>> GetGenresAsync(CancellationToken ct = default) =>
            DatabaseCall.RunAsync(() => _games.GetGenresAsync(ct), ReadFailed);

        /// <inheritdoc />
        public Task<IReadOnlyList<string>> GetPlatformsAsync(CancellationToken ct = default) =>
            DatabaseCall.RunAsync(() => _games.GetPlatformsAsync(ct), ReadFailed);

        /// <summary>
        /// Copies a game with its text trimmed and blank text turned into nothing at all, so
        /// that a stray space typed into a form does not become part of the stored title and an
        /// untouched cover box does not store an empty string.
        /// </summary>
        private static Game Normalise(Game game) => new Game
        {
            Id = game.Id,
            Name = Trim(game.Name),
            Genre = Trim(game.Genre),
            Platform = Trim(game.Platform),
            Score = game.Score,
            CoverUrl = Trim(game.CoverUrl),
            Comment = Trim(game.Comment),
        };

        private static string Trim(string value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
