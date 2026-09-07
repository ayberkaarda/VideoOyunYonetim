using System;
using System.Collections.Generic;

namespace VideoGameManager.Domain
{
    /// <summary>
    /// A single entry in the game catalogue.
    /// </summary>
    /// <remarks>
    /// <see cref="Genre"/> and <see cref="Platforms"/> are plain strings even though the database
    /// keeps them in lookup tables. The name is what the screens show and what the user types, so
    /// the translation between a name and a lookup identity belongs to the repository and never
    /// leaks into this entity or into the repository contract.
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
        /// <remarks>
        /// Empty until something fills it in, rather than absent. A game with no title is
        /// rejected by <see cref="GameValidator"/>, which treats an empty string and a blank
        /// one alike, so an unset title is already refused and does not need a second, nullable
        /// spelling of the same thing.
        /// </remarks>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Genre the game belongs to, such as <c>RPG</c> or <c>Racing</c>. Required to store a
        /// game, but absent on one that was stored before the rule existed.
        /// </summary>
        /// <remarks>
        /// The one required field that is still allowed to be missing. The genre column accepts
        /// no genre at all, so a catalogue kept before a genre was asked for still reads back,
        /// with nothing where the genre would be rather than an invented one. Validation refuses
        /// to write such a game, which is a different question from whether one can be read.
        /// </remarks>
        public string? Genre { get; set; }

        /// <summary>
        /// Platforms the game is played on, such as <c>PC</c> or <c>Switch</c>. At least one is
        /// required, and the same name may not appear twice.
        /// </summary>
        /// <remarks>
        /// A game read from the repository always carries a list here, never <c>null</c>: a game
        /// that is attached to no platform at all comes back with an empty list. Callers may
        /// therefore enumerate this without a null check.
        /// </remarks>
        public IReadOnlyList<string> Platforms { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Rating between <see cref="ScoreRange.Min"/> and <see cref="ScoreRange.Max"/>,
        /// or <c>null</c> when the game has not been rated.
        /// </summary>
        public double? Score { get; set; }

        /// <summary>
        /// Absolute http or https address of the cover image. Optional.
        /// </summary>
        public string? CoverUrl { get; set; }

        /// <summary>
        /// Text of the most recent review written for this game, or <c>null</c> when it has none.
        /// </summary>
        /// <remarks>
        /// Read-only projection. Reviews live in a table of their own, and this property is only
        /// ever filled in by the query that reads a game; no insert or update carries it back to
        /// the database. Writing a review goes through the review repository instead, which is
        /// the only place that can record its score and the moment it was written.
        /// </remarks>
        public string? LatestReview { get; init; }

        /// <summary>
        /// How far the owner has got with this game. A game that has never been marked reads as
        /// <see cref="PlayStatus.Backlog"/>.
        /// </summary>
        /// <remarks>
        /// The default is deliberate rather than incidental. A catalogue that was kept before this
        /// property existed says nothing about what its owner played, and "not started" is the
        /// only answer that invents nothing.
        /// </remarks>
        public PlayStatus Status { get; set; }

        /// <summary>
        /// Whether the owner marked this game as a favourite. <c>false</c> until they do.
        /// </summary>
        public bool IsFavourite { get; set; }
    }
}
