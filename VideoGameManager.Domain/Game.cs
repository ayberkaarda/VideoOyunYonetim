namespace VideoGameManager.Domain
{
    /// <summary>
    /// A single entry in the game catalogue.
    /// </summary>
    /// <remarks>
    /// <see cref="Genre"/> and <see cref="Platform"/> are plain strings for now. They become
    /// lookup tables later; the repository contract does not change when they do, because it
    /// already exposes them as strings here.
    /// </remarks>
    public sealed class Game
    {
        /// <summary>
        /// Database identity. Zero for a game that has not been stored yet.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Title of the game. Required, at most 100 characters.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Genre the game belongs to, such as <c>RPG</c> or <c>Racing</c>. Required.
        /// </summary>
        public string Genre { get; set; }

        /// <summary>
        /// Platform the game is played on, such as <c>PC</c> or <c>Switch</c>. Required.
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// Rating between <see cref="ScoreRange.Min"/> and <see cref="ScoreRange.Max"/>,
        /// or <c>null</c> when the game has not been rated.
        /// </summary>
        public double? Score { get; set; }

        /// <summary>
        /// Absolute http or https address of the cover image. Optional.
        /// </summary>
        public string CoverUrl { get; set; }

        /// <summary>
        /// Free text review left by the owner of the catalogue. Optional.
        /// </summary>
        public string Comment { get; set; }
    }
}
