using System;

namespace VideoGameManager.Services
{
    /// <summary>
    /// A file offered for import is not a document of the format that was asked to read it.
    /// </summary>
    /// <remarks>
    /// This is the whole-file failure, and it is deliberately separate from a single entry that
    /// breaks a domain rule. One bad entry is skipped and the rest of the file still arrives;
    /// a file whose text cannot be parsed at all yields nothing, because a reader that stopped
    /// halfway would import a part of a file the user believes was rejected.
    /// <para>
    /// The parser's own exception is kept as <see cref="Exception.InnerException"/> for the log.
    /// The message on this exception is the one a user may see, so it names no parser, no
    /// position and no internal type.
    /// </para>
    /// </remarks>
    public sealed class ImportFormatException : Exception
    {
        /// <summary>
        /// Creates the exception with a default message.
        /// </summary>
        public ImportFormatException()
            : base("The file is not in the expected format.")
        {
        }

        /// <summary>
        /// Creates the exception with a message.
        /// </summary>
        /// <param name="message">What is wrong with the file, in terms a user can read.</param>
        public ImportFormatException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates the exception with a message and the parser failure behind it.
        /// </summary>
        /// <param name="message">What is wrong with the file, in terms a user can read.</param>
        /// <param name="innerException">The parser exception, kept for the log.</param>
        public ImportFormatException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
