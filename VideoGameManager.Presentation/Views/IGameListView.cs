using System;
using System.Collections.Generic;
using VideoGameManager.Data;
using VideoGameManager.Domain;

namespace VideoGameManager.Views
{
    /// <summary>
    /// One file format the catalogue can be written to, as far as the screen that saves it
    /// needs to know.
    /// </summary>
    /// <remarks>
    /// The view builds the save dialog's filter out of these, so it needs both halves: the
    /// name a user recognises and the extension the file gets. It deliberately carries no
    /// writer, so the view cannot start an export on its own and the exporters stay behind
    /// the presenter.
    /// </remarks>
    public sealed class ExportFormat
    {
        /// <summary>Creates a format entry.</summary>
        /// <param name="name">Name shown to the user, for example <c>CSV</c>.</param>
        /// <param name="fileExtension">Extension including the leading dot, for example <c>.csv</c>.</param>
        public ExportFormat(string name, string fileExtension)
        {
            Name = name;
            FileExtension = fileExtension;
        }

        /// <summary>Name shown to the user.</summary>
        public string Name { get; }

        /// <summary>File extension, including the leading dot.</summary>
        public string FileExtension { get; }
    }

    /// <summary>
    /// Says which format the user picked and where the file should go.
    /// </summary>
    /// <remarks>
    /// Choosing the path is the view's job because it owns the dialog; opening the file and
    /// writing to it is the presenter's, because that is where the exporters live.
    /// </remarks>
    public sealed class ExportRequestedEventArgs : EventArgs
    {
        /// <summary>Creates the arguments.</summary>
        /// <param name="format">Name of the chosen format, matching an <see cref="ExportFormat.Name"/>.</param>
        /// <param name="filePath">Full path of the file to write.</param>
        public ExportRequestedEventArgs(string format, string filePath)
        {
            Format = format;
            FilePath = filePath;
        }

        /// <summary>Name of the chosen format.</summary>
        public string Format { get; }

        /// <summary>Full path of the file to write.</summary>
        public string FilePath { get; }
    }

    /// <summary>
    /// The "browse games" screen. The list carries game ids, so the presenter fetches
    /// details by id rather than looking a title back up.
    /// </summary>
    /// <remarks>
    /// Every member here is either a value the presenter reads, a value it renders, or an
    /// event it reacts to. Nothing on this interface mentions a WinForms type, which is what
    /// lets the presenter be driven by a substituted view.
    /// </remarks>
    public interface IGameListView : IView
    {
        /// <summary>Fills the list with one page of games. Selection is cleared.</summary>
        IReadOnlyList<Game> Games { set; }

        /// <summary>Id of the highlighted row, or <c>null</c> when nothing is selected.</summary>
        int? SelectedGameId { get; }

        /// <summary>
        /// Genres offered by the genre filter. The view adds its own "any genre" entry in
        /// front of these; the presenter supplies only the real ones.
        /// </summary>
        IReadOnlyList<string> Genres { set; }

        /// <summary>
        /// Platforms offered by the platform filter. The view adds its own "any platform"
        /// entry in front of these.
        /// </summary>
        IReadOnlyList<string> Platforms { set; }

        /// <summary>Formats the export dialog offers.</summary>
        IReadOnlyList<ExportFormat> ExportFormats { set; }

        /// <summary>Text typed into the search field. Empty when nothing was typed.</summary>
        string SearchText { get; }

        /// <summary>Chosen genre, or <c>null</c> when the "any genre" entry is selected.</summary>
        string? SelectedGenre { get; }

        /// <summary>Chosen platform, or <c>null</c> when the "any platform" entry is selected.</summary>
        string? SelectedPlatform { get; }

        /// <summary>Chosen play state, or <c>null</c> when every state is wanted.</summary>
        PlayStatus? SelectedStatus { get; }

        /// <summary><c>true</c> when the list should be narrowed to favourites.</summary>
        bool OnlyFavourites { get; }

        /// <summary>Column the list is ordered by.</summary>
        GameSortField SortField { get; }

        /// <summary><c>true</c> to order from high to low.</summary>
        bool SortDescending { get; }

        /// <summary>The screen is on screen and wants its first page.</summary>
        event EventHandler? Loaded;

        /// <summary>A different row was highlighted.</summary>
        event EventHandler? SelectionChanged;

        /// <summary>
        /// A filter or the sort order changed: the search text settled after the user
        /// stopped typing, a filter was picked, or the sort was changed.
        /// </summary>
        event EventHandler? FilterChanged;

        /// <summary>The user asked for the page before the current one.</summary>
        event EventHandler? PreviousPageRequested;

        /// <summary>The user asked for the page after the current one.</summary>
        event EventHandler? NextPageRequested;

        /// <summary>The user asked to edit the highlighted game.</summary>
        event EventHandler? EditRequested;

        /// <summary>The user asked to delete the highlighted game.</summary>
        event EventHandler? DeleteRequested;

        /// <summary>The user picked a format and a file to export to.</summary>
        event EventHandler<ExportRequestedEventArgs>? ExportRequested;

        /// <summary>Renders the detail panel, or empties it when passed <c>null</c>.</summary>
        void ShowDetails(Game? game);

        /// <summary>
        /// Reports where the user is in the result set and how big it is, so the paging
        /// controls can enable or disable themselves.
        /// </summary>
        /// <param name="page">One-based number of the page on screen.</param>
        /// <param name="pageCount">How many pages the result set spans. Zero when nothing matched.</param>
        /// <param name="totalCount">How many games match the current filter in total.</param>
        void ShowPage(int page, int pageCount, int totalCount);

        /// <summary>
        /// Replaces the list with a readable status message, for example when the list could
        /// not be loaded or when nothing matches. The screen stays open and usable; nothing
        /// pops up.
        /// </summary>
        void ShowListUnavailable(string message);

        /// <summary>
        /// Replaces the detail panel with a readable status message, for example when the
        /// selected row's details could not be fetched. The list itself is left alone.
        /// </summary>
        void ShowDetailsUnavailable(string message);

        /// <summary>
        /// Opens the editing screen for one game and returns once the user has closed it.
        /// </summary>
        /// <param name="gameId">Identity of the game to edit.</param>
        /// <remarks>
        /// Opening a child window is the view's business: it owns the window handle and the
        /// container scope the child is resolved from. Deciding whether a game is selected at
        /// all, and reloading the list once the child closes, stays with the presenter, which
        /// is why this is a plain command rather than a click the view handles by itself.
        /// </remarks>
        void OpenEditor(int gameId);
    }
}
