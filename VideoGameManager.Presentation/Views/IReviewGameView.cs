using System;
using System.Collections.Generic;

namespace VideoGameManager.Views
{
    /// <summary>
    /// The "review a game" screen. The picker carries a placeholder entry at index 0, so
    /// <see cref="SelectedGameId"/> is <c>null</c> until a real game is chosen.
    /// </summary>
    public interface IReviewGameView : IValidatingView
    {
        /// <summary>Id of the chosen game, or <c>null</c> while the placeholder is selected.</summary>
        int? SelectedGameId { get; }

        /// <summary>What the user wrote, as typed. Never <c>null</c>; empty when nothing was written.</summary>
        string ReviewText { get; }

        /// <summary>
        /// The screen is on screen and needs its picker filled. Raised before the user can
        /// interact with it, so the picker is never empty by the time it is reachable.
        /// </summary>
        event EventHandler? Loaded;

        /// <summary>
        /// The user asked to save the review. The presenter reads the chosen game and the
        /// text, has the domain validate them, and reports the failed fields back when it
        /// refuses.
        /// </summary>
        event EventHandler? SaveRequested;

        /// <summary>
        /// Fills the picker. Ids travel with the items, so a review is written against the
        /// selected id rather than a matched title.
        /// </summary>
        void SetGames(IReadOnlyList<GameListItem> games);

        /// <summary>
        /// Empties the text box after a review has been written, so the next one does not
        /// start from the previous text. The chosen game is left selected.
        /// </summary>
        void ClearReviewText();
    }

    /// <summary>A game as the picker shows it: the title the user reads, the id the presenter uses.</summary>
    public sealed class GameListItem
    {
        /// <summary>Creates a picker entry.</summary>
        /// <param name="id">Identity the review is written against.</param>
        /// <param name="name">Title shown in the picker.</param>
        public GameListItem(int id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Identity of the game. Carried on the item itself so a review is never matched
        /// back to a row by title, which two games can share.
        /// </summary>
        public int Id { get; }

        /// <summary>Title shown in the picker.</summary>
        public string Name { get; }

        /// <summary>The combo box renders this.</summary>
        public override string ToString() => Name;
    }
}
