using System;

namespace VideoGameManager.Domain
{
    /// <summary>
    /// A review written for a game.
    /// </summary>
    public sealed class Review
    {
        /// <summary>
        /// Identity of the review.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Identity of the reviewed <see cref="Game"/>.
        /// </summary>
        public int GameId { get; init; }

        /// <summary>
        /// Rating given by the review, or <c>null</c> when the review only carries text.
        /// </summary>
        public double? Score { get; set; }

        /// <summary>
        /// Text of the review. Required.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Moment the review was written.
        /// </summary>
        public DateTimeOffset CreatedAt { get; init; }
    }
}
