using System;
using System.Collections.Generic;
using VideoGameManager.Domain;

namespace VideoGameManager.Views
{
    /// <summary>
    /// The "add or edit a game" screen. Combo boxes carry a placeholder entry, so the view
    /// returns <c>null</c> (or, for platforms, an empty list) when nothing real is selected
    /// and the domain validator reports the missing field. No WinForms type appears here.
    /// </summary>
    public interface IAddGameView : IValidatingView
    {
        /// <summary>Title as typed, before trimming. Never <c>null</c>; empty when nothing was typed.</summary>
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

        /// <summary>
        /// Cover image address as typed. Optional, so an empty value is valid; when one is
        /// given the domain validator decides whether the address is usable.
        /// </summary>
        string CoverUrl { get; }

        /// <summary>How far the owner has got with the game. Always one of the defined values.</summary>
        PlayStatus Status { get; }

        /// <summary>Whether the owner marked the game as a favourite.</summary>
        bool IsFavourite { get; }

        /// <summary>
        /// The user asked to save what is on screen. The view performs no checking of its
        /// own: the presenter reads the fields, has the domain validate them, and either
        /// writes the game or reports the failed fields back.
        /// </summary>
        event EventHandler SaveRequested;

        /// <summary>
        /// Raised when the caller asks this screen to load an existing game for editing.
        /// The presenter reads the identity from the event argument and fetches the game.
        /// </summary>
        event EventHandler<GameEditRequestedEventArgs> EditRequested;

        /// <summary>Empties the fields and returns the combo boxes to their placeholders.</summary>
        void ResetInput();

        /// <summary>Fills every field from a game that was loaded for editing.</summary>
        void ShowGame(Game game);

        /// <summary>
        /// Switches the title and the save button between their adding and editing wording.
        /// </summary>
        void ShowEditing(bool isEditing);

        /// <summary>
        /// Reports that the game requested for editing could not be loaded. The screen shows
        /// the message and stops the user from saving, so an empty or half-filled form can
        /// never be written over the row that failed to load.
        /// </summary>
        void ShowLoadFailed(string message);

        /// <summary>Closes the dialog with <c>DialogResult.OK</c>
        /// after a successful update, so the caller knows to reload the row it just changed.</summary>
        void CloseAfterSave();
    }

    /// <summary>Carries the identity a caller passed to <c>LoadForEditing</c> onto the event
    /// the presenter subscribes to.</summary>
    public sealed class GameEditRequestedEventArgs : EventArgs
    {
        /// <summary>Creates the arguments.</summary>
        /// <param name="gameId">Identity of the game the screen should load for editing.</param>
        public GameEditRequestedEventArgs(int gameId)
        {
            GameId = gameId;
        }

        /// <summary>Identity of the game to load for editing.</summary>
        public int GameId { get; }
    }
}
