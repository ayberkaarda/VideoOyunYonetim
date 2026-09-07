using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using VideoGameManager.Domain;

namespace VideoGameManager.Services
{
    /// <summary>
    /// Reads a catalogue back out of the JSON array <see cref="JsonGameExporter"/> writes.
    /// </summary>
    /// <remarks>
    /// The two are a round trip: the field names, the array of platform names and the play
    /// state written by name are read back here, so a file this application exported is a file
    /// it can import. The reader is deliberately more forgiving than the writer, because an
    /// import file is often edited by hand or produced elsewhere: property names are matched
    /// without regard to case, a number written as a quoted string is accepted, comments and a
    /// trailing comma are ignored, and every field except the title may simply be absent.
    /// <para>
    /// A missing field becomes the same value an unset one has on a new game rather than an
    /// invented one, and the game is then handed to the caller as it stands. Whether it is fit
    /// to store is <see cref="GameValidator"/>'s question, not this class's: an entry with no
    /// title or no platform is returned like any other and refused later, so the rules stay in
    /// one place.
    /// </para>
    /// <para>
    /// The identity in the file is read past on purpose. Importing describes new rows in
    /// whatever catalogue is receiving them, and carrying a number from another database would
    /// either collide with a row that already holds it or claim a row that means something
    /// else entirely.
    /// </para>
    /// </remarks>
    public sealed class JsonGameImporter : IGameImporter
    {
        private const string NotJson =
            "The file could not be read as JSON.";

        private const string NotAnArrayOfGames =
            "The file is not a JSON array of games.";

        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters = { new JsonStringEnumConverter() },
        };

        /// <inheritdoc />
        public string Format => "JSON";

        /// <inheritdoc />
        public string FileExtension => ".json";

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <c>null</c>.</exception>
        /// <exception cref="ImportFormatException">
        /// The text is not JSON, or it is JSON that is not an array of objects.
        /// </exception>
        public async Task<IReadOnlyList<Game>> ReadAsync(TextReader reader, CancellationToken ct = default)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            ct.ThrowIfCancellationRequested();

            // The whole document is read before any of it is parsed. A JSON array only makes
            // sense complete, so there is nothing to be gained by streaming it, and reading it
            // in one go keeps a half-read file from producing a half-imported catalogue.
            string text = await reader.ReadToEndAsync(ct).ConfigureAwait(false);

            List<ImportedGame?>? records = Parse(text);

            if (records == null)
            {
                // The document was the literal null, which is valid JSON and no catalogue.
                throw new ImportFormatException(NotAnArrayOfGames);
            }

            List<Game> games = new List<Game>(records.Count);

            foreach (ImportedGame? record in records)
            {
                ct.ThrowIfCancellationRequested();

                if (record == null)
                {
                    // A null where an object should be. Treated as the file being the wrong
                    // shape rather than as one bad entry, because there is nothing here to
                    // report back to the user about which game was meant.
                    throw new ImportFormatException(NotAnArrayOfGames);
                }

                games.Add(record.ToGame());
            }

            return games;
        }

        private static List<ImportedGame?>? Parse(string text)
        {
            try
            {
                return JsonSerializer.Deserialize<List<ImportedGame?>>(text, Options);
            }
            catch (JsonException ex)
            {
                throw new ImportFormatException(NotJson, ex);
            }
            catch (NotSupportedException ex)
            {
                // Raised instead of JsonException when a value has a type the converter cannot
                // read at all, for example an object where the play state should be.
                throw new ImportFormatException(NotJson, ex);
            }
        }

        /// <summary>
        /// Plain data shape read from JSON, matching what <see cref="JsonGameExporter"/> writes.
        /// </summary>
        /// <remarks>
        /// Every property is optional so that an incomplete entry is a game that fails
        /// validation rather than a file that fails to parse: the difference decides whether
        /// one bad row is skipped or the whole import is refused, and only the first of those
        /// is right for a missing title.
        /// </remarks>
        private sealed class ImportedGame
        {
            public string? Name { get; set; }

            public string? Genre { get; set; }

            public List<string?>? Platforms { get; set; }

            public double? Score { get; set; }

            public PlayStatus? Status { get; set; }

            public bool? IsFavourite { get; set; }

            public string? CoverUrl { get; set; }

            public Game ToGame() => new Game
            {
                Name = Name ?? string.Empty,
                Genre = Genre,
                Platforms = ReadPlatforms(Platforms),
                Score = Score,
                Status = Status ?? PlayStatus.Backlog,
                IsFavourite = IsFavourite ?? false,
                CoverUrl = CoverUrl,
            };

            /// <summary>
            /// Copies the platform names, turning an absent list into an empty one and a null
            /// entry into an empty name.
            /// </summary>
            /// <remarks>
            /// Nothing is dropped and nothing is repaired. A blank entry stays in the list so
            /// that the validator sees it and rejects the game; removing it here would turn a
            /// file that names no platform for a game into a file that quietly names one fewer.
            /// </remarks>
            private static IReadOnlyList<string> ReadPlatforms(List<string?>? platforms)
            {
                if (platforms == null || platforms.Count == 0)
                {
                    return Array.Empty<string>();
                }

                List<string> names = new List<string>(platforms.Count);

                foreach (string? platform in platforms)
                {
                    names.Add(platform ?? string.Empty);
                }

                return names;
            }
        }
    }
}
