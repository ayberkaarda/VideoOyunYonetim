using System;
using System.Collections.Generic;
using System.Globalization;

namespace VideoGameManager.Domain
{
    /// <summary>
    /// The single place where the rules for a <see cref="Review"/> live.
    /// </summary>
    /// <remarks>
    /// Reviews are validated here rather than in the service for the same reason games are: a
    /// rule that is written twice drifts apart. The service runs this before it reaches a
    /// repository.
    /// </remarks>
    public static class ReviewValidator
    {
        /// <summary>
        /// Checks every rule and returns all of the broken ones.
        /// </summary>
        /// <param name="review">Review to check.</param>
        /// <returns>
        /// <see cref="ValidationResult.Ok"/> when the review is valid, otherwise a result
        /// listing each broken rule.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="review"/> is <c>null</c>.</exception>
        public static ValidationResult Validate(Review review)
        {
            if (review == null)
            {
                throw new ArgumentNullException(nameof(review));
            }

            List<ValidationError> errors = new List<ValidationError>();

            if (review.GameId <= 0)
            {
                errors.Add(new ValidationError(nameof(Review.GameId), "The reviewed game was not identified."));
            }

            if (string.IsNullOrWhiteSpace(review.Body))
            {
                errors.Add(new ValidationError(nameof(Review.Body), "A review needs some text."));
            }

            if (!ScoreRange.Contains(review.Score))
            {
                errors.Add(new ValidationError(
                    nameof(Review.Score),
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Score must be between {0} and {1}.",
                        ScoreRange.Min,
                        ScoreRange.Max)));
            }

            return errors.Count == 0 ? ValidationResult.Ok : new ValidationResult(errors);
        }
    }
}
