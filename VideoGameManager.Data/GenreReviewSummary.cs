namespace VideoGameManager.Data
{
    /// <summary>
    /// What the catalogue's reviews say about one genre.
    /// </summary>
    /// <remarks>
    /// These are counts and an average and nothing more. Turning them into a preference -- deciding
    /// how many reviews are enough to trust an average, or how strongly a liked genre should be
    /// favoured over an unrated one -- is a business rule and lives in the service layer, so that
    /// it can be read, changed and tested without a database.
    /// </remarks>
    /// <param name="Genre">Name of the genre.</param>
    /// <param name="ScoredReviewCount">
    /// How many reviews of games in this genre carry a score. Never zero: a genre nobody scored
    /// is left out of the answer rather than reported as an average of nothing.
    /// </param>
    /// <param name="AverageReviewScore">Mean of those review scores.</param>
    public sealed record GenreReviewSummary(string Genre, int ScoredReviewCount, double AverageReviewScore);
}
