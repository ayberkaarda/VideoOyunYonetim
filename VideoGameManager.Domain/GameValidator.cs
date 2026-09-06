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

            if (string.IsNullOrWhiteSpace(game.Platform))
            {
                errors.Add(new ValidationError(nameof(Game.Platform), "Platform is required."));
            }

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
