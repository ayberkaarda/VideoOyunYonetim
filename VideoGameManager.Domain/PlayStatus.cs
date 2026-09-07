namespace VideoGameManager.Domain
{
    /// <summary>
    /// How far the owner has got with a game.
    /// </summary>
    /// <remarks>
    /// The numbers are part of the storage format: they are what the database column holds and
    /// what its check constraint accepts, so an existing member may never be renumbered and a new
    /// one takes the next free number. <see cref="Backlog"/> is zero because that is the state a
    /// game arrives in, which lets the column default to it and lets every row that existed
    /// before the column did carry the same, correct answer.
    /// </remarks>
    public enum PlayStatus
    {
        /// <summary>Owned but not started.</summary>
        Backlog = 0,

        /// <summary>Started and still being played.</summary>
        Playing = 1,

        /// <summary>Played through to the end.</summary>
        Finished = 2,
    }
}
