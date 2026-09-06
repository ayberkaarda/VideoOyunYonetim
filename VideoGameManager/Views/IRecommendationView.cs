using System;
using VideoGameManager.Domain;

namespace VideoGameManager.Views
{
    /// <summary>The "recommend me something" screen.</summary>
    public interface IRecommendationView : IView
    {
        event EventHandler RecommendationRequested;

        /// <summary>Renders the pick, or clears the panel when passed <c>null</c>.</summary>
        void ShowGame(Game game);

        /// <summary>
        /// Replaces the pick panel with a readable status message, for example when no
        /// recommendation could be fetched. The screen stays open and usable; nothing pops up.
        /// </summary>
        void ShowLoadError(string message);
    }
}
