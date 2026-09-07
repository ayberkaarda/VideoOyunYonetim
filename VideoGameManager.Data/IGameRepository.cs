using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VideoGameManager.Domain;

namespace VideoGameManager.Data
{
    /// <summary>
    /// Reads and writes games.
    /// </summary>
    /// <remarks>
    /// Implementations do not catch database failures. A provider exception travels up to the
    /// service layer, which is the layer that decides how to present it.
    /// </remarks>
    public interface IGameRepository
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
        Task<PagedResult<Game>> ListAsync(GameFilter filter, int page, int pageSize,
            GameSortField sort = GameSortField.Name, bool descending = false,
            CancellationToken ct = default);

        /// <summary>
        /// Reads a single game.
        /// </summary>
        /// <param name="id">Identity of the game.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The game, or <c>null</c> when no row has that identity.</returns>
        Task<Game> GetAsync(int id, CancellationToken ct = default);

        /// <summary>
        /// Stores a new game.
        /// </summary>
        /// <param name="game">Game to store. Its <see cref="Game.Id"/> is ignored.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The identity the database assigned.</returns>
        Task<int> AddAsync(Game game, CancellationToken ct = default);

        /// <summary>
        /// Overwrites an existing game, addressed by <see cref="Game.Id"/>.
        /// </summary>
        /// <param name="game">Game to write, carrying the identity of the row to change.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns><c>false</c> when no row carries that identity.</returns>
        Task<bool> UpdateAsync(Game game, CancellationToken ct = default);

        /// <summary>
        /// Removes a game.
        /// </summary>
        /// <param name="id">Identity of the game to remove.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns><c>false</c> when no row carries that identity.</returns>
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);

        /// <summary>
        /// Lists the distinct genres already in use, ordered alphabetically.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The genres found in the catalogue.</returns>
        Task<IReadOnlyList<string>> GetGenresAsync(CancellationToken ct = default);

        /// <summary>
        /// Lists the distinct platforms already in use, ordered alphabetically.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The platforms found in the catalogue.</returns>
        Task<IReadOnlyList<string>> GetPlatformsAsync(CancellationToken ct = default);

        /// <summary>
        /// Picks one game at random from those a filter keeps.
        /// </summary>
        /// <remarks>
        /// The filter means exactly what it means in <see cref="ListAsync"/> -- every field of it
        /// is honoured, and the candidates are the same rows that listing would show. A suggestion
        /// drawn from a different set of rows than the one the user is looking at would be a
        /// second, quietly diverging definition of the same question.
        /// </remarks>
        /// <param name="filter">Which games may be picked. <c>null</c> allows every game.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A random matching game, or <c>null</c> when nothing matches.</returns>
        Task<Game> GetRandomAsync(GameFilter filter, CancellationToken ct = default);

        /// <summary>
        /// Reports, per genre, how many scored reviews the catalogue holds and what they average.
        /// </summary>
        /// <remarks>
        /// Raw facts, not a preference. Only reviews that carry a score are counted, and each is
        /// attributed to the genre of the game it was written about; a genre nobody has scored is
        /// absent from the answer rather than present with a count of zero. What weight those
        /// numbers deserve is a rule, and rules live above this layer.
        /// </remarks>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>One row per genre that has at least one scored review, ordered by name.</returns>
        Task<IReadOnlyList<GenreReviewSummary>> GetGenreAffinitiesAsync(CancellationToken ct = default);

        /// <summary>
        /// Reads the totals and the per-genre breakdown of the whole catalogue in one go.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The figures describing the catalogue as it stands.</returns>
        Task<CatalogueStatistics> GetStatisticsAsync(CancellationToken ct = default);
    }
}
