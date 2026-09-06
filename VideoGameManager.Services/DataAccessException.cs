using System;

namespace VideoGameManager.Services
{
    /// <summary>
    /// A database call failed for a reason the caller cannot fix by correcting its input.
    /// </summary>
    /// <remarks>
    /// Repositories let provider exceptions travel; the service layer catches them and rethrows
    /// them as this type. That keeps every caller above the service free of a reference to the
    /// database provider, and it draws the line between a rejected input, which comes back as a
    /// result, and a broken connection, which is thrown.
    /// <para>
    /// The provider exception is kept as <see cref="Exception.InnerException"/> for the log. The
    /// message on this exception is the one a user may see, so it does not repeat provider
    /// text.
    /// </para>
    /// </remarks>
    public sealed class DataAccessException : Exception
    {
        /// <summary>
        /// Creates the exception with a default message.
        /// </summary>
        public DataAccessException()
            : base("The database could not be reached.")
        {
        }

        /// <summary>
        /// Creates the exception with a message.
        /// </summary>
        /// <param name="message">What failed, in terms the caller can show a user.</param>
        public DataAccessException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates the exception with a message and the provider failure behind it.
        /// </summary>
        /// <param name="message">What failed, in terms the caller can show a user.</param>
        /// <param name="innerException">The provider exception, kept for the log.</param>
        public DataAccessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
