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

        string ReviewText { get; }

        event EventHandler Loaded;

        event EventHandler SaveRequested;

        /// <summary>
        /// Fills the picker. Ids travel with the items, so a review is written against the
        /// selected id rather than a matched title.
        /// </summary>
        void SetGames(IReadOnlyList<GameListItem> games);

        void ClearReviewText();
    }

    /// <summary>A game as the picker shows it: the title the user reads, the id the presenter uses.</summary>
    public sealed class GameListItem
    {
        public GameListItem(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }

        public string Name { get; }

        /// <summary>The combo box renders this.</summary>
        public override string ToString() => Name;
    }
}
