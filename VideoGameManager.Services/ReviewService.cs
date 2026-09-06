using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="reviews">Repository the service delegates to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="reviews"/> is <c>null</c>.</exception>
        public ReviewService(IReviewRepository reviews)
        {
            _reviews = reviews ?? throw new ArgumentNullException(nameof(reviews));
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
                Body = string.IsNullOrWhiteSpace(body) ? null : body.Trim(),
                CreatedAt = DateTimeOffset.UtcNow,
            };

            ValidationResult validation = ReviewValidator.Validate(review);

            if (!validation.IsValid)
            {
                return Result.Invalid(validation);
            }

            int id = await DatabaseCall
                .RunAsync(() => _reviews.AddAsync(review, ct), WriteFailed)
                .ConfigureAwait(false);

            return id > 0
                ? Result.Success()
                : Result.Invalid(new ValidationError(nameof(Review.GameId), "That game no longer exists."));
        }
    }
}
