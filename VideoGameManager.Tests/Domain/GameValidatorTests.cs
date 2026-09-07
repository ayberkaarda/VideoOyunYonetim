using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using VideoGameManager.Domain;
using Xunit;

namespace VideoGameManager.Tests.Domain
{
    public class GameValidatorTests
    {
        private static Game CreateValidGame()
        {
            return new Game
            {
                Name = "Test Game",
                Genre = "RPG",
                Platforms = new List<string> { "PC" },
                Score = 8.5,
                CoverUrl = "https://example.com/cover.jpg",
            };
        }

        [Fact]
        public void Validate_NullGame_Throws()
        {
            Action act = () => GameValidator.Validate(null!);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Validate_FullyValidGame_IsValidWithNoErrors()
        {
            Game game = CreateValidGame();

            ValidationResult result = GameValidator.Validate(game);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_NameMissing_ReportsRequiredError(string? name)
        {
            Game game = CreateValidGame();
            game.Name = name!;

            ValidationResult result = GameValidator.Validate(game);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Field == nameof(Game.Name) && e.Message == "Name is required.");
        }

        [Fact]
        public void Validate_NameOverMaxLengthOnlyBecauseOfSurroundingSpaces_IsAccepted()
        {
            Game game = CreateValidGame();
            string exactLengthName = new string('A', GameValidator.MaxNameLength);
            game.Name = "  " + exactLengthName + "  ";

            ValidationResult result = GameValidator.Validate(game);

            result.Errors.Should().NotContain(e => e.Field == nameof(Game.Name));
        }

        [Fact]
        public void Validate_NameOverMaxLengthAfterTrimming_ReportsError()
        {
            Game game = CreateValidGame();
            game.Name = new string('A', GameValidator.MaxNameLength + 1);

            ValidationResult result = GameValidator.Validate(game);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.Field == nameof(Game.Name) &&
                e.Message == $"Name must be {GameValidator.MaxNameLength} characters or fewer.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_GenreMissing_ReportsRequiredError(string? genre)
        {
            Game game = CreateValidGame();
            game.Genre = genre;

            ValidationResult result = GameValidator.Validate(game);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Field == nameof(Game.Genre) && e.Message == "Genre is required.");
        }

        [Fact]
        public void Validate_GenreOverMaxLengthOnlyBecauseOfSurroundingSpaces_IsAccepted()
        {
            Game game = CreateValidGame();
            string exactLengthGenre = new string('B', GameValidator.MaxGenreLength);
            game.Genre = "  " + exactLengthGenre + "  ";

            ValidationResult result = GameValidator.Validate(game);

            result.Errors.Should().NotContain(e => e.Field == nameof(Game.Genre));
        }

        [Fact]
        public void Validate_GenreOverMaxLengthAfterTrimming_ReportsError()
        {
            Game game = CreateValidGame();
            game.Genre = new string('B', GameValidator.MaxGenreLength + 1);

            ValidationResult result = GameValidator.Validate(game);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.Field == nameof(Game.Genre) &&
                e.Message == $"Genre must be {GameValidator.MaxGenreLength} characters or fewer.");
        }

        [Fact]
        public void Validate_NullPlatforms_ReportsAtLeastOneRequired()
        {
            Game game = CreateValidGame();
            game.Platforms = null!;

            ValidationResult result = GameValidator.Validate(game);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.Field == nameof(Game.Platforms) && e.Message == "At least one platform is required.");
        }

        [Fact]
        public void Validate_EmptyPlatforms_ReportsAtLeastOneRequired()
        {
            Game game = CreateValidGame();
            game.Platforms = new List<string>();

            ValidationResult result = GameValidator.Validate(game);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.Field == nameof(Game.Platforms) && e.Message == "At least one platform is required.");
        }

        [Fact]
        public void Validate_BlankPlatformEntry_IsRejected()
        {
            Game game = CreateValidGame();
            game.Platforms = new List<string> { "PC", "   " };

            ValidationResult result = GameValidator.Validate(game);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.Field == nameof(Game.Platforms) && e.Message == "A platform name cannot be blank.");
        }

        [Fact]
        public void Validate_PlatformEntryOverMaxLengthAfterTrimming_IsRejected()
        {
            Game game = CreateValidGame();
            game.Platforms = new List<string> { new string('C', GameValidator.MaxPlatformLength + 1) };

            ValidationResult result = GameValidator.Validate(game);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.Field == nameof(Game.Platforms) &&
                e.Message == $"A platform name must be {GameValidator.MaxPlatformLength} characters or fewer.");
        }

        [Fact]
        public void Validate_PlatformEntryOverMaxLengthOnlyBecauseOfSurroundingSpaces_IsAccepted()
        {
            Game game = CreateValidGame();
            string exactLengthPlatform = new string('C', GameValidator.MaxPlatformLength);
            game.Platforms = new List<string> { "  " + exactLengthPlatform + "  " };

            ValidationResult result = GameValidator.Validate(game);

            result.Errors.Should().NotContain(e =>
                e.Field == nameof(Game.Platforms) &&
                e.Message.Contains("characters or fewer"));
        }

        [Theory]
        [InlineData("PC", "PC")]
        [InlineData("PC", "pc")]
        [InlineData("PC", "  PC  ")]
        [InlineData("PC", "  pc  ")]
        public void Validate_SamePlatformTwice_IsRejectedCaseInsensitivelyAndIgnoringWhitespace(
            string first, string second)
        {
            Game game = CreateValidGame();
            game.Platforms = new List<string> { first, second };

            ValidationResult result = GameValidator.Validate(game);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.Field == nameof(Game.Platforms) && e.Message == "The same platform cannot be listed twice.");
        }

        [Fact]
        public void Validate_SeveralBrokenPlatformEntries_ReportsEachKindOfProblemOnlyOnce()
        {
            Game game = CreateValidGame();
            string tooLong = new string('D', GameValidator.MaxPlatformLength + 1);
            game.Platforms = new List<string>
            {
                "",
                "   ",
                tooLong,
                tooLong,
                "PC",
                "pc",
                "PS5",
            };

            ValidationResult result = GameValidator.Validate(game);

            List<ValidationError> platformErrors = result.Errors
                .Where(e => e.Field == nameof(Game.Platforms))
                .ToList();

            platformErrors.Should().HaveCount(3);
            platformErrors.Should().ContainSingle(e => e.Message == "A platform name cannot be blank.");
            platformErrors.Should().ContainSingle(e =>
                e.Message == $"A platform name must be {GameValidator.MaxPlatformLength} characters or fewer.");
            platformErrors.Should().ContainSingle(e => e.Message == "The same platform cannot be listed twice.");
        }

        [Fact]
        public void Validate_NullScore_IsAccepted()
        {
            Game game = CreateValidGame();
            game.Score = null;

            ValidationResult result = GameValidator.Validate(game);

            result.Errors.Should().NotContain(e => e.Field == nameof(Game.Score));
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(10.0)]
        public void Validate_ScoreAtBoundary_IsAccepted(double score)
        {
            Game game = CreateValidGame();
            game.Score = score;

            ValidationResult result = GameValidator.Validate(game);

            result.Errors.Should().NotContain(e => e.Field == nameof(Game.Score));
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(10.1)]
        public void Validate_ScoreOutsideBoundary_IsRejected(double score)
        {
            Game game = CreateValidGame();
            game.Score = score;

            ValidationResult result = GameValidator.Validate(game);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.Field == nameof(Game.Score) &&
                e.Message == $"Score must be between {ScoreRange.Min} and {ScoreRange.Max}.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("https://example.com/cover.jpg")]
        [InlineData("http://example.com/cover.jpg")]
        public void Validate_AcceptableCoverUrl_IsAccepted(string? coverUrl)
        {
            Game game = CreateValidGame();
            game.CoverUrl = coverUrl;

            ValidationResult result = GameValidator.Validate(game);

            result.Errors.Should().NotContain(e => e.Field == nameof(Game.CoverUrl));
        }

        [Theory]
        [InlineData("/relative/path/cover.jpg")]
        [InlineData("ftp://example.com/cover.jpg")]
        [InlineData("file:///C:/covers/cover.jpg")]
        [InlineData("not a url")]
        public void Validate_UnacceptableCoverUrl_IsRejected(string coverUrl)
        {
            Game game = CreateValidGame();
            game.CoverUrl = coverUrl;

            ValidationResult result = GameValidator.Validate(game);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e =>
                e.Field == nameof(Game.CoverUrl) &&
                e.Message == "Cover URL must be an absolute http or https address.");
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("   ", true)]
        [InlineData("https://example.com/cover.jpg", true)]
        [InlineData("http://example.com/cover.jpg", true)]
        [InlineData("/relative/path/cover.jpg", false)]
        [InlineData("ftp://example.com/cover.jpg", false)]
        [InlineData("file:///C:/covers/cover.jpg", false)]
        [InlineData("not a url", false)]
        public void IsAcceptableCoverUrl_ReturnsExpectedResult(string? coverUrl, bool expected)
        {
            GameValidator.IsAcceptableCoverUrl(coverUrl).Should().Be(expected);
        }

        [Fact]
        public void Validate_SeveralFieldsWrong_ReportsEveryErrorWithCorrectField()
        {
            Game game = new Game
            {
                Name = string.Empty,
                Genre = string.Empty,
                Platforms = null!,
                Score = 99,
                CoverUrl = "not a url",
            };

            ValidationResult result = GameValidator.Validate(game);

            result.IsValid.Should().BeFalse();
            result.Errors.Select(e => e.Field).Should().BeEquivalentTo(new[]
            {
                nameof(Game.Name),
                nameof(Game.Genre),
                nameof(Game.Platforms),
                nameof(Game.Score),
                nameof(Game.CoverUrl),
            });
        }

        [Theory]
        [InlineData(PlayStatus.Backlog)]
        [InlineData(PlayStatus.Playing)]
        [InlineData(PlayStatus.Finished)]
        public void Validate_EveryDefinedStatus_IsAccepted(PlayStatus status)
        {
            Game game = CreateValidGame();
            game.Status = status;

            ValidationResult result = GameValidator.Validate(game);

            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(3)]
        [InlineData(255)]
        [InlineData(-1)]
        public void Validate_StatusThatIsNotADefinedState_ReportsError(int raw)
        {
            // A cast from an arbitrary number produces an enum value with no name and no
            // complaint, so this is the only way an invalid state can arrive. Left unchecked it
            // would reach the database and be refused there by the check constraint.
            Game game = CreateValidGame();
            game.Status = (PlayStatus)raw;

            ValidationResult result = GameValidator.Validate(game);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e =>
                e.Field == nameof(Game.Status) &&
                e.Message == "Status must be one of backlog, playing or finished.");
        }
    }

    /// <summary>
    /// The state a game starts life in, which is also the state every row that predates these
    /// properties is read back with.
    /// </summary>
    public class GameDefaultsTests
    {
        [Fact]
        public void NewGame_WithoutAnyStateSet_IsInTheBacklogAndNotAFavourite()
        {
            Game game = new Game();

            game.Status.Should().Be(PlayStatus.Backlog);
            game.IsFavourite.Should().BeFalse();
        }

        [Fact]
        public void PlayStatus_KeepsTheNumbersTheDatabaseColumnStores()
        {
            // The numbers are the storage format: the column holds them and its check constraint
            // accepts exactly these three. Renumbering a member here would silently re-label every
            // row already written.
            ((int)PlayStatus.Backlog).Should().Be(0);
            ((int)PlayStatus.Playing).Should().Be(1);
            ((int)PlayStatus.Finished).Should().Be(2);
        }
    }
}
