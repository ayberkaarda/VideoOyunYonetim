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
        /// <remarks>
        /// Empty until something fills it in, rather than absent. <see cref="ReviewValidator"/>
        /// rejects a blank body, so an unset one is already refused and does not need a second,
        /// nullable spelling of the same thing.
        /// </remarks>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// Moment the review was written.
        /// </summary>
        public DateTimeOffset CreatedAt { get; init; }
    }
}
