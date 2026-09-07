using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VideoGameManager.Domain;

namespace VideoGameManager.Services
{
    /// <summary>
    /// Reads games out of a text stream written in one file format.
    /// </summary>
    /// <remarks>
    /// The mirror image of <see cref="IGameExporter"/>, and deliberately shaped the same way:
    /// an importer reads from a <see cref="TextReader"/> the caller already owns, never opens a
    /// file, never knows a path and never shows a dialog. Choosing which file to read is the
    /// presentation layer's job, and keeping that decision out of here is what lets a test feed
    /// an importer a <see cref="StringReader"/> instead of touching disk.
    /// <para>
    /// An importer only reads. It does not store anything, does not decide what to do with a
    /// game whose title is already in the catalogue, and does not check the domain rules: those
    /// are decisions of the caller and of the validator, and an importer that made them itself
    /// would be a second place where the rules live.
    /// </para>
    /// </remarks>
    public interface IGameImporter
    {
        /// <summary>
        /// Name of the format, shown to a user choosing a file to read (for example in an open
        /// file dialog's list of formats). Matches the <see cref="IGameExporter.Format"/> of the
        /// exporter that writes the same format, so a file written by one is offered to the
        /// other under the same name.
        /// </summary>
        string Format { get; }

        /// <summary>
        /// File extension for this format, including the leading dot (for example <c>.json</c>).
        /// </summary>
        string FileExtension { get; }

        /// <summary>
        /// Reads every game the stream describes.
        /// </summary>
        /// <param name="reader">Source the caller owns; this method never closes it.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>
        /// One game per entry in the file, in the order they appear, including entries that
        /// break a domain rule. Nothing is dropped silently: an entry the file contains is an
        /// entry the caller gets, so that a caller counting what it stored can also count what
        /// it refused. An empty file that is otherwise well formed produces an empty list.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="reader"/> is <c>null</c>.</exception>
        /// <exception cref="ImportFormatException">
        /// The stream is not a document of this format at all. Nothing is returned in that
        /// case: a file the reader cannot make sense of is refused whole rather than half read.
        /// </exception>
        Task<IReadOnlyList<Game>> ReadAsync(TextReader reader, CancellationToken ct = default);
    }
}
