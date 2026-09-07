using System;

namespace VideoGameManager.Services
{
    /// <summary>
    /// Outcome of a database health check.
    /// </summary>
    /// <remarks>
    /// This is a plain report, not a thrown failure: a caller that only wants to show a banner
    /// or gate a screen should not have to catch an exception to learn the database is down.
    /// </remarks>
    public sealed class DatabaseStatus
    {
        private static readonly DatabaseStatus ReachableStatus = new DatabaseStatus(true, string.Empty, null);

        private DatabaseStatus(bool isReachable, string message, Exception? failure)
        {
            IsReachable = isReachable;
            Message = message;
            Failure = failure;
        }

        /// <summary>
        /// <c>true</c> when the check reached the database.
        /// </summary>
        public bool IsReachable { get; }

        /// <summary>
        /// A message a user can be shown. Empty when <see cref="IsReachable"/> is <c>true</c>.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The failure behind an unreachable result, kept for the log. <c>null</c> when
        /// <see cref="IsReachable"/> is <c>true</c>.
        /// </summary>
        public Exception? Failure { get; }

        /// <summary>
        /// The outcome of a check that reached the database.
        /// </summary>
        /// <returns>A reachable status.</returns>
        public static DatabaseStatus Reachable() => ReachableStatus;

        /// <summary>
        /// The outcome of a check that could not reach the database.
        /// </summary>
        /// <param name="message">Message a user can be shown. Required.</param>
        /// <param name="failure">The failure the check ran into. Required, kept for the log.</param>
        /// <returns>An unreachable status.</returns>
        /// <exception cref="ArgumentException"><paramref name="message"/> is empty.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="failure"/> is <c>null</c>.</exception>
        public static DatabaseStatus Unreachable(string message, Exception failure)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("A user-facing message is required.", nameof(message));
            }

            if (failure == null)
            {
                throw new ArgumentNullException(nameof(failure));
            }

            return new DatabaseStatus(false, message, failure);
        }
    }
}
