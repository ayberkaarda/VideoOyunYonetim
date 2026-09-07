using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VideoGameManager.Domain;

namespace VideoGameManager.Services
{
    /// <summary>
    /// Writes a snapshot of the game catalogue to a text stream in one file format.
    /// </summary>
    /// <remarks>
    /// An exporter writes to a <see cref="TextWriter"/> the caller already owns. It never opens
    /// a file, never knows a path and never shows a dialog: choosing where the output goes is
    /// the presentation layer's job, and keeping that decision out of here is what lets a test
    /// capture the output in a <see cref="StringWriter"/> instead of touching disk.
    /// </remarks>
    public interface IGameExporter
    {
        /// <summary>
        /// Name of the format, shown to a user choosing where to save (for example in a file
        /// save dialog's list of formats).
        /// </summary>
        string Format { get; }

        /// <summary>
        /// File extension for this format, including the leading dot (for example <c>.csv</c>).
        /// </summary>
        string FileExtension { get; }

        /// <summary>
        /// Writes every game in <paramref name="games"/> to <paramref name="writer"/>.
        /// </summary>
        /// <param name="games">Games to export. An empty list produces a valid, empty export.</param>
        /// <param name="writer">Destination the caller owns; this method never closes it.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="games"/> or <paramref name="writer"/> is <c>null</c>.
        /// </exception>
        Task WriteAsync(IReadOnlyList<Game> games, TextWriter writer, CancellationToken ct = default);
    }
}
