using System;

namespace VideoGameManager.Views
{
    /// <summary>
    /// The "add a game" screen. Combo boxes carry a placeholder entry, so the view returns
    /// <c>null</c> when nothing real is selected and the domain validator reports the
    /// missing field. No WinForms type appears here.
    /// </summary>
    public interface IAddGameView : IValidatingView
    {
        string GameName { get; }

        /// <summary>Selected genre, or <c>null</c> while the placeholder is selected.</summary>
        string Genre { get; }

        /// <summary>Selected platform, or <c>null</c> while the placeholder is selected.</summary>
        string Platform { get; }

        /// <summary>Selected score as typed text, or <c>null</c> while the placeholder is selected.</summary>
        string ScoreText { get; }

        string CoverUrl { get; }

        event EventHandler SaveRequested;

        /// <summary>Empties the fields and returns the combo boxes to their placeholders.</summary>
        void ResetInput();
    }
}
