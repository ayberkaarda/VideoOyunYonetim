using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VideoGameManager.Data;
using VideoGameManager.Domain;

namespace VideoGameManager.Services
{
    /// <summary>
    /// Default <see cref="IReviewService"/>: validates, then delegates to the repository.
    /// </summary>
    public sealed class ReviewService : IReviewService
    {
        private const string ReadFailed = "The reviews could not be read.";
        private const string WriteFailed = "The review could not be saved.";

        private readonly IReviewRepository _reviews;
        private readonly ILogger<ReviewService> _logger;

        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="reviews">Repository the service delegates to.</param>
        /// <param name="logger">Logger completed mutations and rejected writes are recorded on.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="reviews"/> or <paramref name="logger"/> is <c>null</c>.
        /// </exception>
        public ReviewService(IReviewRepository reviews, ILogger<ReviewService> logger)
        {
            _reviews = reviews ?? throw new ArgumentNullException(nameof(reviews));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public Task<IReadOnlyList<Review>> GetForGameAsync(int gameId, CancellationToken ct = default) =>
            DatabaseCall.RunAsync(() => _reviews.GetForGameAsync(gameId, ct), ReadFailed);

        /// <inheritdoc />
        public async Task<Result> AddAsync(int gameId, double? score, string body, CancellationToken ct = default)
        {
            Review review = new Review
            {
                GameId = gameId,
                Score = score,
                Body = string.IsNullOrWhiteSpace(body) ? string.Empty : body.Trim(),
                CreatedAt = DateTimeOffset.UtcNow,
            };

            ValidationResult validation = ReviewValidator.Validate(review);

            if (!validation.IsValid)
            {
                _logger.LogWarning(
                    "Review add rejected: {ErrorCount} validation error(s) on game {GameId}.",
                    validation.Errors.Count, gameId);
                return Result.Invalid(validation);
            }

            int id = await DatabaseCall
                .RunAsync(() => _reviews.AddAsync(review, ct), WriteFailed)
                .ConfigureAwait(false);

            if (id > 0)
            {
                _logger.LogInformation("Review added: {ReviewId} for game {GameId}", id, gameId);
                return Result.Success();
            }

            _logger.LogWarning("Review add rejected: game {GameId} no longer exists.", gameId);
            return Result.Invalid(new ValidationError(nameof(Review.GameId), "That game no longer exists."));
        }
    }
}
