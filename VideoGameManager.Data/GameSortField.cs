namespace VideoGameManager.Data
{
    /// <summary>
    /// Column a game listing is ordered by.
    /// </summary>
    /// <remarks>
    /// The sort is an enum rather than a column name so that no caller can push an identifier
    /// into the ORDER BY clause. The repository turns it into a bound parameter.
    /// </remarks>
    public enum GameSortField
    {
        /// <summary>Order by the title.</summary>
        Name = 0,

        /// <summary>Order by the score.</summary>
        Score = 1,
    }
}
