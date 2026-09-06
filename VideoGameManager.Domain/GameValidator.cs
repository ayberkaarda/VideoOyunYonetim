using System;
using System.Collections.Generic;
using System.Globalization;

namespace VideoGameManager.Domain
{
    /// <summary>
    /// The single place where the rules for a <see cref="Game"/> live.
    /// </summary>
    /// <remarks>
    /// The service layer runs this before it reaches a repository, so an invalid game never
    /// becomes a SQL statement. The presentation layer only renders the result; it does not
    /// repeat the rules.
    /// </remarks>
    public static class GameValidator
    {
        /// <summary>
        /// Longest accepted <see cref="Game.Name"/>. Matches the width of the database column,
        /// so a name that passes validation cannot be truncated on the way in.
        /// </summary>
        public const int MaxNameLength = 100;

        /// <summary>
        /// Longest accepted <see cref="Game.Genre"/>. Matches the width of the name column in the
        /// genre lookup table, so a genre that passes validation cannot be truncated on the way in.
        /// </summary>
        public const int MaxGenreLength = 50;

        /// <summary>
        /// Longest accepted entry in <see cref="Game.Platforms"/>. Matches the width of the name
        /// column in the platform lookup table, for the same reason.
        /// </summary>
        public const int MaxPlatformLength = 50;

        /// <summary>
        /// Checks every rule and returns all of the broken ones.
        /// </summary>
        /// <param name="game">Game to check.</param>
        /// <returns>
        /// <see cref="ValidationResult.Ok"/> when the game is valid, otherwise a result listing
        /// each broken rule.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="game"/> is <c>null</c>.</exception>
        public static ValidationResult Validate(Game game)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            List<ValidationError> errors = new List<ValidationError>();

            if (string.IsNullOrWhiteSpace(game.Name))
            {
                errors.Add(new ValidationError(nameof(Game.Name), "Name is required."));
            }
            else if (game.Name.Trim().Length > MaxNameLength)
            {
                errors.Add(new ValidationError(
                    nameof(Game.Name),
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Name must be {0} characters or fewer.",
                        MaxNameLength)));
            }

            if (string.IsNullOrWhiteSpace(game.Genre))
            {
                errors.Add(new ValidationError(nameof(Game.Genre), "Genre is required."));
            }
            else if (game.Genre.Trim().Length > MaxGenreLength)
            {
                errors.Add(new ValidationError(
                    nameof(Game.Genre),
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Genre must be {0} characters or fewer.",
                        MaxGenreLength)));
            }

            AddPlatformErrors(game.Platforms, errors);

            if (!ScoreRange.Contains(game.Score))
            {
                errors.Add(new ValidationError(
                    nameof(Game.Score),
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Score must be between {0} and {1}.",
                        ScoreRange.Min,
                        ScoreRange.Max)));
            }

            if (!IsAcceptableCoverUrl(game.CoverUrl))
            {
                errors.Add(new ValidationError(
                    nameof(Game.CoverUrl),
                    "Cover URL must be an absolute http or https address."));
            }

            return errors.Count == 0 ? ValidationResult.Ok : new ValidationResult(errors);
        }

        /// <summary>
        /// Checks the platform list and appends whatever it breaks.
        /// </summary>
        /// <remarks>
        /// Each kind of problem is reported once rather than once per entry, because the screens
        /// show a single message beside the platform field and a list of near-identical messages
        /// would tell the user nothing extra.
        /// </remarks>
        /// <param name="platforms">Platform list to check.</param>
        /// <param name="errors">List the broken rules are appended to.</param>
        private static void AddPlatformErrors(IReadOnlyList<string> platforms, List<ValidationError> errors)
        {
            if (platforms == null || platforms.Count == 0)
            {
                errors.Add(new ValidationError(nameof(Game.Platforms), "At least one platform is required."));
                return;
            }

            bool blankReported = false;
            bool tooLongReported = false;
            bool repeatReported = false;
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string platform in platforms)
            {
                if (string.IsNullOrWhiteSpace(platform))
                {
                    if (!blankReported)
                    {
                        blankReported = true;
                        errors.Add(new ValidationError(nameof(Game.Platforms), "A platform name cannot be blank."));
                    }

                    continue;
                }

                string trimmed = platform.Trim();

                if (trimmed.Length > MaxPlatformLength && !tooLongReported)
                {
                    tooLongReported = true;
                    errors.Add(new ValidationError(
                        nameof(Game.Platforms),
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "A platform name must be {0} characters or fewer.",
                            MaxPlatformLength)));
                }

                if (!seen.Add(trimmed) && !repeatReported)
                {
                    repeatReported = true;
                    errors.Add(new ValidationError(
                        nameof(Game.Platforms),
                        "The same platform cannot be listed twice."));
                }
            }
        }

        /// <summary>
        /// Tells whether a cover address is acceptable. An empty address is fine; a present one
        /// must be an absolute http or https URI.
        /// </summary>
        /// <param name="coverUrl">Address to check.</param>
        /// <returns><c>true</c> when the address is empty or a usable http/https URI.</returns>
        public static bool IsAcceptableCoverUrl(string coverUrl)
        {
            if (string.IsNullOrWhiteSpace(coverUrl))
            {
                return true;
            }

            if (!Uri.TryCreate(coverUrl.Trim(), UriKind.Absolute, out Uri uri))
            {
                return false;
            }

            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }
    }
}
