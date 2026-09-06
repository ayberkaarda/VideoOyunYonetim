using System;
using System.Collections.Generic;
using FluentAssertions;
using VideoGameManager.Domain;
using Xunit;

namespace VideoGameManager.Tests.Domain
{
    public class ValidationResultTests
    {
        [Fact]
        public void Constructor_NullErrors_Throws()
        {
            Action act = () => new ValidationResult(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_CopiesSourceSequence_SoLaterMutationDoesNotAffectResult()
        {
            List<ValidationError> source = new List<ValidationError>
            {
                new ValidationError("Field", "Message"),
            };

            ValidationResult result = new ValidationResult(source);
            source.Add(new ValidationError("Other", "Other message"));

            result.Errors.Should().HaveCount(1);
        }

        [Fact]
        public void Constructor_EmptySequence_IsValid()
        {
            ValidationResult result = new ValidationResult(new List<ValidationError>());

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Ok_IsValidAndCarriesNoErrors()
        {
            ValidationResult result = ValidationResult.Ok;

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Failed_NullErrors_Throws()
        {
            Action act = () => ValidationResult.Failed(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Failed_EmptyErrors_Throws()
        {
            Action act = () => ValidationResult.Failed();

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Failed_WithErrors_IsInvalidAndCarriesThem()
        {
            ValidationError error = new ValidationError("Field", "Message");

            ValidationResult result = ValidationResult.Failed(error);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle().Which.Should().Be(error);
        }
    }

    public class ValidationErrorTests
    {
        [Fact]
        public void Equals_SameFieldAndMessage_AreEqual()
        {
            ValidationError first = new ValidationError("Field", "Message");
            ValidationError second = new ValidationError("Field", "Message");

            first.Should().Be(second);
            (first == second).Should().BeTrue();
        }

        [Fact]
        public void Equals_DifferentMessage_AreNotEqual()
        {
            ValidationError first = new ValidationError("Field", "Message");
            ValidationError second = new ValidationError("Field", "Other message");

            first.Should().NotBe(second);
        }

        [Fact]
        public void Properties_ReturnConstructorArguments()
        {
            ValidationError error = new ValidationError("Field", "Message");

            error.Field.Should().Be("Field");
            error.Message.Should().Be("Message");
        }
    }
}
