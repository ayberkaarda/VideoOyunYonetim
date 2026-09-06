using FluentAssertions;
using VideoGameManager.Domain;
using Xunit;

namespace VideoGameManager.Tests.Domain
{
    public class ScoreRangeTests
    {
        [Fact]
        public void Min_IsZero()
        {
            ScoreRange.Min.Should().Be(0.0);
        }

        [Fact]
        public void Max_IsTen()
        {
            ScoreRange.Max.Should().Be(10.0);
        }

        [Fact]
        public void Contains_NullScore_ReturnsTrue()
        {
            ScoreRange.Contains(null).Should().BeTrue();
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(10.0)]
        [InlineData(5.0)]
        public void Contains_ValueInsideRange_ReturnsTrue(double score)
        {
            ScoreRange.Contains(score).Should().BeTrue();
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(10.1)]
        public void Contains_ValueJustOutsideRange_ReturnsFalse(double score)
        {
            ScoreRange.Contains(score).Should().BeFalse();
        }
    }
}
