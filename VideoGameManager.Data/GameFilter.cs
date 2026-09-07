using VideoGameManager.Domain;

namespace VideoGameManager.Data
{
    /// <summary>
    /// Narrows a game listing. Every member is optional; a member left at <c>null</c> is not
    /// applied at all.
    /// </summary>
    /// <param name="Name">
    /// Substring of the title to look for. Matching is case and accent insensitive because of
    /// the database collation, so <c>fifa</c> finds <c>FIFA 24</c>.
    /// </param>
    /// <param name="Genre">Exact genre to keep, as picked from the genre list.</param>
    /// <param name="Platform">Exact platform to keep, as picked from the platform list.</param>
    /// <param name="MinScore">Lowest score to keep, inclusive.</param>
    /// <param name="MaxScore">Highest score to keep, inclusive.</param>
    /// <param name="Status">Play state to keep, or <c>null</c> to keep every state.</param>
    /// <param name="OnlyFavourites">
    /// <c>true</c> keeps only the games marked as favourites. <c>null</c> and <c>false</c> both
    /// keep everything: the flag says which games to single out, and there is no screen that asks
    /// for the games nobody marked, so <c>false</c> is read as "do not narrow" rather than as
    /// "only the ones that are not favourites".
    /// </param>
    public sealed record GameFilter(
        string? Name = null,
        string? Genre = null,
        string? Platform = null,
        double? MinScore = null,
        double? MaxScore = null,
        PlayStatus? Status = null,
        bool? OnlyFavourites = null)
    {
        /// <summary>
        /// A filter that keeps everything.
        /// </summary>
        public static GameFilter None { get; } = new GameFilter();
    }
}
