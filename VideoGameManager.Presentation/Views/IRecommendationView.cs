using System;
using System.Collections.Generic;
using VideoGameManager.Domain;

namespace VideoGameManager.Views
{
    /// <summary>The "recommend me something" screen.</summary>
    public interface IRecommendationView : IView
    {
        /// <summary>
        /// Names of the strategies the picker offers, in the order they should be listed.
        /// The view renders each one as readable text; the identifier itself is what
        /// <see cref="SelectedStrategy"/> returns.
        /// </summary>
        IReadOnlyList<string> Strategies { set; }

        /// <summary>The identifier of the strategy currently picked, never the display text.</summary>
        string? SelectedStrategy { get; }

        /// <summary>Raised once the screen has finished loading, so the presenter can fill the picker.</summary>
        event EventHandler? Loaded;

        /// <summary>
        /// The user asked for a pick using the currently selected strategy. Raised once per
        /// press, so asking again with the same strategy may well return a different game.
        /// </summary>
        event EventHandler? RecommendationRequested;

        /// <summary>Renders the pick, or clears the panel when passed <c>null</c>.</summary>
        void ShowGame(Game? game);

        /// <summary>
        /// Replaces the pick panel with a readable status message, for example when no
        /// recommendation could be fetched. The screen stays open and usable; nothing pops up.
        /// </summary>
        void ShowLoadError(string message);
    }
}
