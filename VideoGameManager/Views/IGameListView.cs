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
    }
}
