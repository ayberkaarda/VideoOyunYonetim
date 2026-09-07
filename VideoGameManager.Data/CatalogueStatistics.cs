using System.Collections.Generic;

namespace VideoGameManager.Data
{
    /// <summary>
    /// A single reading of the whole catalogue.
    /// </summary>
    /// <remarks>
    /// Every figure here comes from one round trip, so the totals and the breakdown describe the
    /// same catalogue at the same moment. Asking for them one at a time would let a write land
    /// between two of the questions and produce a summary whose parts disagree.
    /// </remarks>
    /// <param name="TotalGames">How many games the catalogue holds.</param>
    /// <param name="ReviewCount">How many reviews have been written, across every game.</param>
    /// <param name="AverageScore">
    /// Mean of the games' own scores, or <c>null</c> when no game is scored. This is the rating on
    /// the game itself, not an average of what the reviews said.
    /// </param>
    /// <param name="ByGenre">
    /// One row per genre, largest first and then by name. The rows cover every game, including
    /// the ones that belong to no genre.
    /// </param>
    public sealed record CatalogueStatistics(
        int TotalGames,
        int ReviewCount,
        double? AverageScore,
        IReadOnlyList<GenreDistribution> ByGenre);
}
