using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VideoGameManager.Domain;

namespace VideoGameManager.Services
{
    /// <summary>
    /// Writes the catalogue as comma-separated values.
    /// </summary>
    /// <remarks>
    /// Rows are terminated with a carriage return followed by a line feed (<c>"\r\n"</c>),
    /// written literally so the ending stays the same wherever the file is produced, regardless
    /// of the writer's own <see cref="TextWriter.NewLine"/> setting. A field is quoted only when
    /// it contains a comma, a double quote, a carriage return or a line feed; an embedded double
    /// quote is doubled, which is the quoting rule spreadsheet applications expect. Numbers are
    /// formatted with <see cref="CultureInfo.InvariantCulture"/> so a decimal point is always a
    /// point: on a machine whose regional settings format numbers with a comma, a plain
    /// <c>ToString()</c> would turn a single score into two columns without raising an error.
    /// </remarks>
    public sealed class CsvGameExporter : IGameExporter
    {
        private const string RowEnding = "\r\n";
        private static readonly char[] CharsThatForceQuoting = { ',', '"', '\r', '\n' };

        /// <inheritdoc />
        public string Format => "CSV";

        /// <inheritdoc />
        public string FileExtension => ".csv";

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

            await WriteRowAsync(writer, HeaderFields()).ConfigureAwait(false);

            foreach (Game game in games)
            {
                ct.ThrowIfCancellationRequested();
                await WriteRowAsync(writer, FieldsFor(game)).ConfigureAwait(false);
            }
        }

        private static IEnumerable<string> HeaderFields()
        {
            yield return "Id";
            yield return "Name";
            yield return "Genre";
            yield return "Platforms";
            yield return "Score";
            yield return "Status";
            yield return "IsFavourite";
            yield return "CoverUrl";
        }

        /// <summary>
        /// Builds the raw field values for one game, in the fixed column order. A missing value
        /// becomes an empty field rather than the literal text "null", which a spreadsheet would
        /// otherwise read as a real data value.
        /// </summary>
        private static IEnumerable<string> FieldsFor(Game game)
        {
            yield return game.Id.ToString(CultureInfo.InvariantCulture);
            yield return game.Name ?? string.Empty;
            yield return game.Genre ?? string.Empty;
            yield return string.Join("; ", game.Platforms);
            yield return game.Score.HasValue ? game.Score.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
            yield return game.Status.ToString();
            yield return game.IsFavourite.ToString(CultureInfo.InvariantCulture);
            yield return game.CoverUrl ?? string.Empty;
        }

        private static async Task WriteRowAsync(TextWriter writer, IEnumerable<string> fields)
        {
            bool first = true;

            foreach (string field in fields)
            {
                if (!first)
                {
                    await writer.WriteAsync(',').ConfigureAwait(false);
                }

                first = false;
                await writer.WriteAsync(Escape(field)).ConfigureAwait(false);
            }

            await writer.WriteAsync(RowEnding).ConfigureAwait(false);
        }

        /// <summary>
        /// Quotes a field only when it needs it, doubling any quote already inside it. A field
        /// that needs no quoting is returned unchanged so a plain export stays easy to read.
        /// </summary>
        private static string Escape(string field)
        {
            if (string.IsNullOrEmpty(field))
            {
                return string.Empty;
            }

            if (field.IndexOfAny(CharsThatForceQuoting) < 0)
            {
                return field;
            }

            return "\"" + field.Replace("\"", "\"\"") + "\"";
        }
    }
}
