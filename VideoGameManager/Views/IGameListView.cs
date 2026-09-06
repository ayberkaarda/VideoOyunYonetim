using System;
using System.Collections.Generic;
using VideoGameManager.Domain;

namespace VideoGameManager.Views
{
    /// <summary>
    /// The "browse games" screen. The list carries game ids, so the presenter fetches
    /// details by id rather than looking a title back up.
    /// </summary>
    public interface IGameListView : IView
    {
        /// <summary>Fills the list. Selection is cleared.</summary>
        IReadOnlyList<Game> Games { set; }

        /// <summary>Id of the highlighted row, or <c>null</c> when nothing is selected.</summary>
        int? SelectedGameId { get; }

        event EventHandler Loaded;

        event EventHandler SelectionChanged;

        /// <summary>Renders the detail panel, or empties it when passed <c>null</c>.</summary>
        void ShowDetails(Game game);

        /// <summary>
        /// Replaces the list with a readable status message, for example when the list could
        /// not be loaded. The screen stays open and usable; nothing pops up.
        /// </summary>
        void ShowListUnavailable(string message);

        /// <summary>
        /// Replaces the detail panel with a readable status message, for example when the
        /// selected row's details could not be fetched. The list itself is left alone.
        /// </summary>
        void ShowDetailsUnavailable(string message);
    }
}
