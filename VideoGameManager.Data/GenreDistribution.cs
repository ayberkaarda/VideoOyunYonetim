namespace VideoGameManager.Data
{
    /// <summary>
    /// How much of the catalogue one genre accounts for.
    /// </summary>
    /// <param name="Genre">
    /// Name of the genre, or <c>null</c> for the games that belong to no genre at all. Those games
    /// get a row of their own rather than being dropped, so that the counts in a breakdown add up
    /// to the total the same answer reports and a gap in the data stays visible instead of
    /// quietly shrinking the catalogue.
    /// </param>
    /// <param name="GameCount">How many games carry that genre.</param>
    /// <param name="AverageScore">
    /// Mean of the scores those games carry, or <c>null</c> when none of them is scored. Unscored
    /// games are left out of the mean but still counted by <paramref name="GameCount"/>.
    /// </param>
    public sealed record GenreDistribution(string Genre, int GameCount, double? AverageScore);
}
