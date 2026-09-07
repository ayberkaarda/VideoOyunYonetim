using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<GameService> _logger;

        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="games">Repository the service delegates to.</param>
        /// <param name="logger">Logger completed mutations and rejected writes are recorded on.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="games"/> or <paramref name="logger"/> is <c>null</c>.
        /// </exception>
        public GameService(IGameRepository games, ILogger<GameService> logger)
        {
            _games = games ?? throw new ArgumentNullException(nameof(games));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                _logger.LogWarning(
                    "Game add rejected: {ErrorCount} validation error(s) on {GameName}.",
                    validation.Errors.Count, candidate.Name);
                return Result<int>.Invalid(validation);
            }

            int id = await DatabaseCall
                .RunAsync(() => _games.AddAsync(candidate, ct), WriteFailed)
                .ConfigureAwait(false);

            _logger.LogInformation("Game added: {GameId} {GameName}", id, candidate.Name);

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
                _logger.LogWarning("Game update rejected: no identity was supplied.");
                return Result.Invalid(new ValidationError(nameof(Game.Id), "The game to update was not identified."));
            }

            Game candidate = Normalise(game);
            ValidationResult validation = GameValidator.Validate(candidate);

            if (!validation.IsValid)
            {
                _logger.LogWarning(
                    "Game update rejected: {ErrorCount} validation error(s) on {GameId}.",
                    validation.Errors.Count, candidate.Id);
                return Result.Invalid(validation);
            }

            bool updated = await DatabaseCall
                .RunAsync(() => _games.UpdateAsync(candidate, ct), WriteFailed)
                .ConfigureAwait(false);

            if (updated)
            {
                _logger.LogInformation("Game updated: {GameId} {GameName}", candidate.Id, candidate.Name);
                return Result.Success();
            }

            _logger.LogWarning("Game update rejected: {GameId} no longer exists.", candidate.Id);
            return Result.Invalid(new ValidationError(nameof(Game.Id), "That game no longer exists."));
        }

        /// <inheritdoc />
        public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Game delete rejected: no identity was supplied.");
                return Result.Invalid(new ValidationError(nameof(Game.Id), "The game to delete was not identified."));
            }

            bool deleted = await DatabaseCall
                .RunAsync(() => _games.DeleteAsync(id, ct), WriteFailed)
                .ConfigureAwait(false);

            if (deleted)
            {
                _logger.LogInformation("Game deleted: {GameId}", id);
                return Result.Success();
            }

            _logger.LogWarning("Game delete rejected: {GameId} no longer exists.", id);
            return Result.Invalid(new ValidationError(nameof(Game.Id), "That game no longer exists."));
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
        /// <remarks>
        /// <para>
        /// The text of the newest review is not carried over. It is a projection the repository
        /// fills in when it reads a game; a review is written through the review service, so
        /// letting it travel back down a write path here would be a second, silent way to change
        /// it.
        /// </para>
        /// <para>
        /// Every other property is copied by name, which means this list has to grow whenever the
        /// entity does. A property left out here is not merely ignored: the copy is what reaches
        /// the repository, so the missing value is written as the default of its type on every add
        /// and every update. Nothing complains -- the build passes and the caller is told the save
        /// succeeded -- while the play state or the favourite flag the user just set is quietly
        /// dropped. Adding a property to the entity therefore means adding a line here and a test
        /// that fails if the line goes away again.
        /// </para>
        /// </remarks>
        private static Game Normalise(Game game) => new Game
        {
            Id = game.Id,
            Name = Trim(game.Name),
            Genre = Trim(game.Genre),
            Platforms = TrimPlatforms(game.Platforms),
            Score = game.Score,
            CoverUrl = Trim(game.CoverUrl),
            Status = game.Status,
            IsFavourite = game.IsFavourite,
        };

        /// <summary>
        /// Copies a platform list with each name trimmed, blank entries dropped and repeats
        /// removed without regard to case, keeping the order the caller gave them in.
        /// </summary>
        /// <remarks>
        /// A list that was never supplied stays missing rather than becoming an empty one, so the
        /// validator sees the difference between "the caller sent nothing" and "the caller sent an
        /// empty list". Both are rejected with the same message.
        /// </remarks>
        private static IReadOnlyList<string> TrimPlatforms(IReadOnlyList<string> platforms)
        {
            if (platforms == null)
            {
                return null;
            }

            List<string> kept = new List<string>(platforms.Count);
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string platform in platforms)
            {
                string trimmed = Trim(platform);

                if (trimmed != null && seen.Add(trimmed))
                {
                    kept.Add(trimmed);
                }
            }

            return kept;
        }

        private static string Trim(string value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
