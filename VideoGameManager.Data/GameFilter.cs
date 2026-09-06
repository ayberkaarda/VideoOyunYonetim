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
    public sealed record GameFilter(
        string Name = null,
        string Genre = null,
        string Platform = null,
        double? MinScore = null,
        double? MaxScore = null)
    {
        /// <summary>
        /// A filter that keeps everything.
        /// </summary>
        public static GameFilter None { get; } = new GameFilter();
    }
}
