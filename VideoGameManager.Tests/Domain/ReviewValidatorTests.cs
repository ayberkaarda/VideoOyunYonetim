using System;
using System.Linq;
using FluentAssertions;
using VideoGameManager.Domain;
using Xunit;

namespace VideoGameManager.Tests.Domain
{
    public class ReviewValidatorTests
    {
        private static Review CreateValidReview()
        {
            return new Review
            {
                GameId = 1,
                Score = 7.5,
                Body = "A solid game.",
            };
        }

        [Fact]
        public void Validate_NullReview_Throws()
        {
            Action act = () => ReviewValidator.Validate(null!);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Validate_FullyValidReview_IsValidWithNoErrors()
        {
            Review review = CreateValidReview();

            ValidationResult result = ReviewValidator.Validate(review);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_GameIdZeroOrNegative_IsRejected(int gameId)
        {
            Review review = new Review
            {
                GameId = gameId,
                Score = 7.5,
                Body = "A solid game.",
            };

            ValidationResult result = ReviewValidator.Validate(review);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.Field == nameof(Review.GameId) && e.Message == "The reviewed game was not identified.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_BlankBody_IsRejected(string? body)
        {
            Review review = CreateValidReview();
            review.Body = body!;

            ValidationResult result = ReviewValidator.Validate(review);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.Field == nameof(Review.Body) && e.Message == "A review needs some text.");
        }

        [Fact]
        public void Validate_BodyWithText_IsAccepted()
        {
            Review review = CreateValidReview();
            review.Body = "Great story, weak combat.";

            ValidationResult result = ReviewValidator.Validate(review);

            result.Errors.Should().NotContain(e => e.Field == nameof(Review.Body));
        }

        [Fact]
        public void Validate_NullScore_IsAccepted()
        {
            Review review = CreateValidReview();
            review.Score = null;

            ValidationResult result = ReviewValidator.Validate(review);

            result.Errors.Should().NotContain(e => e.Field == nameof(Review.Score));
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(10.0)]
        public void Validate_ScoreAtBoundary_IsAccepted(double score)
        {
            Review review = CreateValidReview();
            review.Score = score;

            ValidationResult result = ReviewValidator.Validate(review);

            result.Errors.Should().NotContain(e => e.Field == nameof(Review.Score));
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(10.1)]
        public void Validate_ScoreOutsideBoundary_IsRejected(double score)
        {
            Review review = CreateValidReview();
            review.Score = score;

            ValidationResult result = ReviewValidator.Validate(review);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.Field == nameof(Review.Score) &&
                e.Message == $"Score must be between {ScoreRange.Min} and {ScoreRange.Max}.");
        }

        [Fact]
        public void Validate_WrongInEveryWay_ReportsAllThreeErrors()
        {
            Review review = new Review
            {
                GameId = 0,
                Body = "   ",
                Score = 99,
            };

            ValidationResult result = ReviewValidator.Validate(review);

            result.IsValid.Should().BeFalse();
            result.Errors.Select(e => e.Field).Should().BeEquivalentTo(new[]
            {
                nameof(Review.GameId),
                nameof(Review.Body),
                nameof(Review.Score),
            });
        }
    }
}
