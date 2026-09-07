using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using VideoGameManager.Domain;
using VideoGameManager.Services;
using Xunit;

namespace VideoGameManager.Tests.Services
{
    public class CsvGameExporterTests
    {
        private readonly CsvGameExporter _exporter = new CsvGameExporter();

        [Fact]
        public void Format_IsCsv()
        {
            _exporter.Format.Should().Be("CSV");
        }

        [Fact]
        public void FileExtension_IsDotCsv()
        {
            _exporter.FileExtension.Should().Be(".csv");
        }

        [Fact]
        public async Task WriteAsync_NullGames_ThrowsArgumentNullException()
        {
            using StringWriter writer = new StringWriter();

            Func<Task> act = () => _exporter.WriteAsync(null, writer);

            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("games");
        }

        [Fact]
        public async Task WriteAsync_NullWriter_ThrowsArgumentNullException()
        {
            Func<Task> act = () => _exporter.WriteAsync(new Game[0], null);

            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("writer");
        }

        [Fact]
        public async Task WriteAsync_EmptyList_WritesOnlyTheHeaderRow()
        {
            string csv = await ExportAsync();

            csv.Should().Be("Id,Name,Genre,Platforms,Score,Status,IsFavourite,CoverUrl\r\n");
        }

        [Fact]
        public async Task WriteAsync_CancelledToken_ThrowsOperationCanceledExceptionAndStopsBeforeWriting()
        {
            using CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();
            using StringWriter writer = new StringWriter();
            Game[] games = { OneGame() };

            Func<Task> act = () => _exporter.WriteAsync(games, writer, cts.Token);

            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task WriteAsync_RowsAreTerminatedWithCarriageReturnLineFeed()
        {
            string csv = await ExportAsync(OneGame());

            csv.Should().EndWith("\r\n");
            csv.Should().NotContain("\n\n");
        }

        [Fact]
        public async Task WriteAsync_NameContainingCommaQuoteCarriageReturnAndLineFeed_IsQuotedWithDoubledQuotes()
        {
            Game game = new Game
            {
                Id = 1,
                Name = "Zelda, \"BOTW\"\r\nSpecial Edition",
                Genre = null,
                Platforms = new string[0],
                Score = null,
                Status = PlayStatus.Playing,
                IsFavourite = true,
                CoverUrl = null,
            };

            string csv = await ExportAsync(game);

            string expectedRow = "1,\"Zelda, \"\"BOTW\"\"\r\nSpecial Edition\",,,,Playing,True,\r\n";
            csv.Should().Be("Id,Name,Genre,Platforms,Score,Status,IsFavourite,CoverUrl\r\n" + expectedRow);
        }

        [Fact]
        public async Task WriteAsync_FieldWithNoSpecialCharacters_IsNotQuoted()
        {
            Game game = new Game
            {
                Id = 2,
                Name = "Simple Title",
                Genre = "Action",
                Platforms = new string[] { "PC", "Switch" },
                Score = 9.5,
                Status = PlayStatus.Backlog,
                IsFavourite = false,
                CoverUrl = "https://example.invalid/cover.png",
            };

            string csv = await ExportAsync(game);

            string expectedRow = "2,Simple Title,Action,PC; Switch,9.5,Backlog,False,https://example.invalid/cover.png\r\n";
            csv.Should().Be("Id,Name,Genre,Platforms,Score,Status,IsFavourite,CoverUrl\r\n" + expectedRow);
        }

        [Fact]
        public async Task WriteAsync_NullScoreGenreCoverUrlAndEmptyPlatformList_ProduceEmptyFieldsNotTheWordNull()
        {
            Game game = new Game
            {
                Id = 3,
                Name = "No Metadata",
                Genre = null,
                Platforms = new string[0],
                Score = null,
                Status = PlayStatus.Backlog,
                IsFavourite = false,
                CoverUrl = null,
            };

            string csv = await ExportAsync(game);

            string expectedRow = "3,No Metadata,,,,Backlog,False,\r\n";
            csv.Should().Be("Id,Name,Genre,Platforms,Score,Status,IsFavourite,CoverUrl\r\n" + expectedRow);
            csv.Should().NotContain("null");
        }

        [Fact]
        public async Task WriteAsync_PlatformsWithMultipleEntries_AreJoinedWithSemicolonAndSpace()
        {
            Game game = OneGame();
            game.Platforms = new string[] { "PC", "Switch", "PlayStation 5" };

            string csv = await ExportAsync(game);

            csv.Should().Contain(",PC; Switch; PlayStation 5,");
        }

        [Fact]
        public async Task WriteAsync_ScoreOnAMachineThatFormatsNumbersWithAComma_StillWritesADotAsTheDecimalSeparator()
        {
            CultureInfo original = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

            try
            {
                Game game = OneGame();
                game.Score = 9.5;

                string csv = await ExportAsync(game);

                csv.Should().Contain(",9.5,");
                csv.Should().NotContain(",9,5,");
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }

        [Fact]
        public async Task WriteAsync_MultipleGames_WritesOneRowPerGameInTheOrderGiven()
        {
            Game first = OneGame(id: 1, name: "First");
            Game second = OneGame(id: 2, name: "Second");

            string csv = await ExportAsync(first, second);

            int firstIndex = csv.IndexOf("First", StringComparison.Ordinal);
            int secondIndex = csv.IndexOf("Second", StringComparison.Ordinal);
            firstIndex.Should().BeGreaterThan(-1);
            secondIndex.Should().BeGreaterThan(firstIndex);
        }

        [Fact]
        public async Task WriteAsync_StatusAndIsFavourite_AreWrittenAsPlainText()
        {
            Game game = OneGame();
            game.Status = PlayStatus.Finished;
            game.IsFavourite = true;

            string csv = await ExportAsync(game);

            csv.Should().Contain(",Finished,True,");
        }

        private async Task<string> ExportAsync(params Game[] games)
        {
            using StringWriter writer = new StringWriter();
            await _exporter.WriteAsync(games, writer);
            return writer.ToString();
        }

        private static Game OneGame(int id = 1, string name = "Hollow Knight") => new Game
        {
            Id = id,
            Name = name,
            Genre = "Metroidvania",
            Platforms = new string[] { "PC" },
            Score = 9.4,
            Status = PlayStatus.Playing,
            IsFavourite = false,
            CoverUrl = "https://example.invalid/cover.png",
        };
    }
}
