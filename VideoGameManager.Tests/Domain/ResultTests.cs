using System;
using VideoGameManager.Domain;
using FluentAssertions;
using Xunit;

namespace VideoGameManager.Tests.Domain
{
    public class ResultTests
    {
        [Fact]
        public void Success_IsSuccessfulAndCarriesNoErrors()
        {
            Result result = Result.Success();

            result.IsSuccess.Should().BeTrue();
            result.Errors.Should().BeEmpty();
            result.Validation.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Invalid_WithValidValidationResult_Throws()
        {
            Action act = () => Result.Invalid(VideoGameManager.Domain.ValidationResult.Ok);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Invalid_WithNullValidationResult_Throws()
        {
            Action act = () => Result.Invalid((VideoGameManager.Domain.ValidationResult)null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Invalid_WithErrors_IsUnsuccessfulAndExposesThem()
        {
            ValidationError error = new ValidationError("Field", "Message");

            Result result = Result.Invalid(error);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle().Which.Should().Be(error);
            result.Validation.Errors.Should().ContainSingle().Which.Should().Be(error);
        }
    }

    public class ResultOfTTests
    {
        [Fact]
        public void Success_IsSuccessfulAndCarriesTheValue()
        {
            Result<string> result = Result<string>.Success("value");

            result.IsSuccess.Should().BeTrue();
            result.Errors.Should().BeEmpty();
            result.Value.Should().Be("value");
        }

        [Fact]
        public void Invalid_WithValidValidationResult_Throws()
        {
            Action act = () => Result<string>.Invalid(VideoGameManager.Domain.ValidationResult.Ok);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Invalid_WithNullValidationResult_Throws()
        {
            Action act = () => Result<string>.Invalid((VideoGameManager.Domain.ValidationResult)null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Invalid_WithErrors_IsUnsuccessfulAndExposesThemThroughBothProperties()
        {
            ValidationError error = new ValidationError("Field", "Message");

            Result<string> result = Result<string>.Invalid(error);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle().Which.Should().Be(error);
            result.Validation.Errors.Should().ContainSingle().Which.Should().Be(error);
        }

        [Fact]
        public void Value_OnRejectedResult_Throws()
        {
            Result<string> result = Result<string>.Invalid(new ValidationError("Field", "Message"));

            Action act = () => _ = result.Value;

            act.Should().Throw<InvalidOperationException>();
        }
    }
}
