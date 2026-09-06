namespace VideoGameManager.Domain
{
    /// <summary>
    /// The inclusive range a <see cref="Game.Score"/> may take.
    /// </summary>
    public static class ScoreRange
    {
        /// <summary>
        /// Lowest accepted score.
        /// </summary>
        public const double Min = 0.0;

        /// <summary>
        /// Highest accepted score.
        /// </summary>
        public const double Max = 10.0;

        /// <summary>
        /// Tells whether a score is inside the range. A <c>null</c> score counts as valid,
        /// because rating a game is optional.
        /// </summary>
        /// <param name="score">Score to check.</param>
        /// <returns><c>true</c> when the score is <c>null</c> or within the range.</returns>
        public static bool Contains(double? score) =>
            !score.HasValue || (score.Value >= Min && score.Value <= Max);
    }
}
