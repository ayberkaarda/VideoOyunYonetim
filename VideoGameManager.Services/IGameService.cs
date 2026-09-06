using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VideoGameManager.Data;
using VideoGameManager.Domain;

namespace VideoGameManager.Services
{
    /// <summary>
    /// The catalogue, seen from the presentation layer.
    /// </summary>
    /// <remarks>
    /// Rules are checked here before anything reaches the database, so an invalid game never
    /// becomes a SQL statement. A rejected input comes back as a <see cref="Result"/>; a broken
    /// database throws <see cref="DataAccessException"/>.
    /// </remarks>
    public interface IGameService
    {
        /// <summary>
        /// Reads one page of games that match a filter.
        /// </summary>
        /// <param name="filter">Which games to keep. <c>null</c> keeps everything.</param>
        /// <param name="page">One-based page number.</param>
        /// <param name="pageSize">How many rows the page holds.</param>
        /// <param name="sort">Column to order by.</param>
        /// <param name="descending"><c>true</c> to order from high to low.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The page and the total number of matching rows.</returns>
        /// <exception cref="DataAccessException">The database call failed.</exception>
        Task<PagedResult<Game>> SearchAsync(GameFilter filter, int page, int pageSize,
            GameSortField sort = GameSortField.Name, bool descending = false,
            CancellationToken ct = default);

        /// <summary>
        /// Reads a single game.
        /// </summary>
        /// <param name="id">Identity of the game.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The game, or <c>null</c> when no game has that identity.</returns>
        /// <exception cref="DataAccessException">The database call failed.</exception>
        Task<Game> GetAsync(int id, CancellationToken ct = default);

        /// <summary>
        /// Validates a game and stores it.
        /// </summary>
        /// <param name="game">Game to store. Its identity is ignored.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// The identity the database assigned, or the broken rules that stopped the write.
        /// </returns>
        /// <exception cref="DataAccessException">The database call failed.</exception>
        Task<Result<int>> AddAsync(Game game, CancellationToken ct = default);

        /// <summary>
        /// Validates a game and overwrites the row with the same identity.
        /// </summary>
        /// <param name="game">Game to write, carrying the identity of the row to change.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// Success, or the broken rules that stopped the write. A game that no longer exists is
        /// reported as an error on <see cref="Game.Id"/>.
        /// </returns>
        /// <exception cref="DataAccessException">The database call failed.</exception>
        Task<Result> UpdateAsync(Game game, CancellationToken ct = default);

        /// <summary>
        /// Removes a game.
        /// </summary>
        /// <param name="id">Identity of the game to remove.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// Success, or an error on <see cref="Game.Id"/> when no game has that identity.
        /// </returns>
        /// <exception cref="DataAccessException">The database call failed.</exception>
        Task<Result> DeleteAsync(int id, CancellationToken ct = default);

        /// <summary>
        /// Lists the distinct genres already in use, ordered alphabetically.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The genres found in the catalogue.</returns>
        /// <exception cref="DataAccessException">The database call failed.</exception>
        Task<IReadOnlyList<string>> GetGenresAsync(CancellationToken ct = default);

        /// <summary>
        /// Lists the distinct platforms already in use, ordered alphabetically.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The platforms found in the catalogue.</returns>
        /// <exception cref="DataAccessException">The database call failed.</exception>
        Task<IReadOnlyList<string>> GetPlatformsAsync(CancellationToken ct = default);
    }
}
