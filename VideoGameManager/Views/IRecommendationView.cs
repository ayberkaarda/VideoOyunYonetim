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
    }
}
