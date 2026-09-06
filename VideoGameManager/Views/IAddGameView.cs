using System;
using System.Collections.Generic;

namespace VideoGameManager.Views
{
    /// <summary>
    /// The "add a game" screen. Combo boxes carry a placeholder entry, so the view returns
    /// <c>null</c> (or, for platforms, an empty list) when nothing real is selected and the
    /// domain validator reports the missing field. No WinForms type appears here.
    /// </summary>
    public interface IAddGameView : IValidatingView
    {
        string GameName { get; }

        /// <summary>Selected genre, or <c>null</c> while the placeholder is selected.</summary>
        string Genre { get; }

        /// <summary>
        /// Selected platforms. The catalogue can hold more than one platform per game, but
        /// this screen still offers a single-select control, so the list has either zero
        /// entries (placeholder selected) or exactly one. Never <c>null</c>.
        /// </summary>
        IReadOnlyList<string> Platforms { get; }

        /// <summary>Selected score as typed text, or <c>null</c> while the placeholder is selected.</summary>
        string ScoreText { get; }

        string CoverUrl { get; }

        event EventHandler SaveRequested;

        /// <summary>Empties the fields and returns the combo boxes to their placeholders.</summary>
        void ResetInput();
    }
}
