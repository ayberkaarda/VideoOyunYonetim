using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using VideoGameManager.Domain;
using VideoGameManager.Services;
using Xunit;

namespace VideoGameManager.Tests.Services
{
    public class JsonGameExporterTests
    {
        private readonly JsonGameExporter _exporter = new JsonGameExporter();

        [Fact]
        public void Format_IsJson()
        {
            _exporter.Format.Should().Be("JSON");
        }

        [Fact]
        public void FileExtension_IsDotJson()
        {
            _exporter.FileExtension.Should().Be(".json");
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
        public async Task WriteAsync_EmptyList_WritesAnEmptyArray()
        {
            string json = await ExportAsync();

            json.Should().Be("[]");
        }

        [Fact]
        public async Task WriteAsync_CancelledToken_ThrowsOperationCanceledException()
        {
            using CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();
            using StringWriter writer = new StringWriter();
            Game[] games = { OneGame() };

            Func<Task> act = () => _exporter.WriteAsync(games, writer, cts.Token);

            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task WriteAsync_Output_IsIndentedForAHumanReader()
        {
            string json = await ExportAsync(OneGame());

            json.Should().Contain("\n");
            json.Should().Contain("  ");
        }

        [Fact]
        public async Task WriteAsync_Status_IsWrittenAsItsNameNotItsNumber()
        {
            Game game = OneGame();
            game.Status = PlayStatus.Playing;

            string json = await ExportAsync(game);

            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement record = document.RootElement[0];
            record.GetProperty("Status").GetString().Should().Be("Playing");
        }

        [Fact]
        public async Task WriteAsync_NullScoreGenreCoverUrlAndEmptyPlatformList_StayNullOrAnEmptyArray()
        {
            Game game = new Game
            {
                Id = 5,
                Name = "No Metadata",
                Genre = null,
                Platforms = new string[0],
                Score = null,
                Status = PlayStatus.Backlog,
                IsFavourite = false,
                CoverUrl = null,
            };

            string json = await ExportAsync(game);

            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement record = document.RootElement[0];
            record.GetProperty("Genre").ValueKind.Should().Be(JsonValueKind.Null);
            record.GetProperty("Score").ValueKind.Should().Be(JsonValueKind.Null);
            record.GetProperty("CoverUrl").ValueKind.Should().Be(JsonValueKind.Null);
            JsonElement platforms = record.GetProperty("Platforms");
            platforms.ValueKind.Should().Be(JsonValueKind.Array);
            platforms.GetArrayLength().Should().Be(0);
        }

        [Fact]
        public async Task WriteAsync_PlatformsWithEntries_AreWrittenAsAJsonArrayInOrder()
        {
            Game game = OneGame();
            game.Platforms = new string[] { "PC", "Switch" };

            string json = await ExportAsync(game);

            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement platforms = document.RootElement[0].GetProperty("Platforms");
            platforms.EnumerateArray().Should().HaveCount(2);
            platforms[0].GetString().Should().Be("PC");
            platforms[1].GetString().Should().Be("Switch");
        }

        [Fact]
        public async Task WriteAsync_NameWithCommaAndQuote_RoundTripsThroughValidJson()
        {
            Game game = OneGame();
            game.Name = "Zelda, \"BOTW\"\r\nSpecial Edition";

            string json = await ExportAsync(game);

            using JsonDocument document = JsonDocument.Parse(json);
            document.RootElement[0].GetProperty("Name").GetString().Should().Be(game.Name);
        }

        [Fact]
        public async Task WriteAsync_NonAsciiCharacters_AreNotEscapedIntoUnicodeSequences()
        {
            Game game = OneGame();
            game.Name = "Café Simulator";

            string json = await ExportAsync(game);

            json.Should().Contain("Café Simulator");
            json.Should().NotContain("\\u");
        }

        [Fact]
        public async Task WriteAsync_LatestReview_IsNotIncludedInTheExportedObject()
        {
            Game game = OneGame();

            string json = await ExportAsync(game);

            json.Should().NotContain("LatestReview");
        }

        [Fact]
        public async Task WriteAsync_MultipleGames_ProducesOneObjectPerGameInOrder()
        {
            Game first = OneGame(id: 1);
            Game second = OneGame(id: 2);

            string json = await ExportAsync(first, second);

            using JsonDocument document = JsonDocument.Parse(json);
            document.RootElement.GetArrayLength().Should().Be(2);
            document.RootElement[0].GetProperty("Id").GetInt32().Should().Be(1);
            document.RootElement[1].GetProperty("Id").GetInt32().Should().Be(2);
        }

        [Fact]
        public async Task WriteAsync_IsFavourite_IsWrittenAsAJsonBoolean()
        {
            Game game = OneGame();
            game.IsFavourite = true;

            string json = await ExportAsync(game);

            using JsonDocument document = JsonDocument.Parse(json);
            document.RootElement[0].GetProperty("IsFavourite").GetBoolean().Should().BeTrue();
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
