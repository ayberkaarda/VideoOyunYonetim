using System;
using VideoGameManager.Data;

namespace VideoGameManager.Views
{
    /// <summary>The catalogue statistics screen.</summary>
    public interface IStatisticsView : IView
    {
        /// <summary>Raised once the screen is on screen and ready to be filled.</summary>
        event EventHandler? Loaded;

        /// <summary>Raised when the user asks for the figures to be read again.</summary>
        event EventHandler? RefreshRequested;

        /// <summary>Renders the headline figures and the per-genre breakdown.</summary>
        /// <param name="statistics">One reading of the whole catalogue.</param>
        void ShowStatistics(CatalogueStatistics statistics);

        /// <summary>
        /// Replaces the figures with a readable status message, for example when the
        /// database did not answer. The screen stays open and the refresh action stays
        /// usable; nothing pops up.
        /// </summary>
        /// <param name="message">What to show in place of the figures.</param>
        void ShowUnavailable(string message);
    }
}
