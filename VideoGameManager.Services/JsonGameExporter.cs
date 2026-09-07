using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using VideoGameManager.Domain;

namespace VideoGameManager.Services
{
    /// <summary>
    /// Writes the catalogue as a JSON array, one object per game.
    /// </summary>
    /// <remarks>
    /// <see cref="Domain.PlayStatus"/> is written by name (for example <c>"Backlog"</c>) rather
    /// than by its underlying number, because a bare number in an export file means nothing to
    /// whoever opens it later without the source code next to them. Output is indented for a
    /// human reader, and ordinary non-ASCII characters are left as themselves instead of being
    /// turned into <c>\uXXXX</c> escapes, so a game title stays readable in the file.
    /// </remarks>
    public sealed class JsonGameExporter : IGameExporter
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() },
        };

        /// <inheritdoc />
        public string Format => "JSON";

        /// <inheritdoc />
        public string FileExtension => ".json";

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// <paramref name="games"/> or <paramref name="writer"/> is <c>null</c>.
        /// </exception>
        public async Task WriteAsync(IReadOnlyList<Game> games, TextWriter writer, CancellationToken ct = default)
        {
            if (games == null)
            {
                throw new ArgumentNullException(nameof(games));
            }

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            List<ExportedGame> records = new List<ExportedGame>(games.Count);

            foreach (Game game in games)
            {
                ct.ThrowIfCancellationRequested();
                records.Add(ExportedGame.From(game));
            }

            string json = JsonSerializer.Serialize(records, Options);
            await writer.WriteAsync(json).ConfigureAwait(false);
        }

        /// <summary>
        /// Plain data shape written to JSON. Kept separate from <see cref="Game"/> so the export
        /// shape and the domain entity can change independently, and so that
        /// <see cref="Game.LatestReview"/> -- deliberately left out of the export -- has no
        /// property here for a later edit to accidentally wire back in.
        /// </summary>
        private sealed class ExportedGame
        {
            public int Id { get; set; }

            public string Name { get; set; } = string.Empty;

            public string? Genre { get; set; }

            public IReadOnlyList<string> Platforms { get; set; } = Array.Empty<string>();

            public double? Score { get; set; }

            public PlayStatus Status { get; set; }

            public bool IsFavourite { get; set; }

            public string? CoverUrl { get; set; }

            public static ExportedGame From(Game game) => new ExportedGame
            {
                Id = game.Id,
                Name = game.Name,
                Genre = game.Genre,
                Platforms = game.Platforms,
                Score = game.Score,
                Status = game.Status,
                IsFavourite = game.IsFavourite,
                CoverUrl = game.CoverUrl,
            };
        }
    }
}
