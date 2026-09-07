using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using VideoGameManager.Domain;
using VideoGameManager.Services;
using Xunit;

namespace VideoGameManager.Tests.Services
{
    /// <summary>
    /// Exercises <see cref="JsonGameImporter"/> against text rather than files: the importer
    /// reads a <see cref="TextReader"/> the caller owns, so nothing here touches disk.
    /// </summary>
    public class JsonGameImporterTests
    {
        private readonly JsonGameImporter _importer = new JsonGameImporter();

        [Fact]
        public void Format_IsJson()
        {
            _importer.Format.Should().Be("JSON");
        }

        [Fact]
        public void FileExtension_IsDotJson()
        {
            _importer.FileExtension.Should().Be(".json");
        }

        [Fact]
        public void Format_MatchesTheExporterThatWritesTheSameFormat()
        {
            // The screen offers one entry per format and matches the chosen name back to a
            // reader or a writer, so the two halves have to agree on the spelling.
            JsonGameExporter exporter = new JsonGameExporter();

            _importer.Format.Should().Be(exporter.Format);
            _importer.FileExtension.Should().Be(exporter.FileExtension);
        }

        [Fact]
        public async Task ReadAsync_NullReader_ThrowsArgumentNullException()
        {
            Func<Task> act = () => _importer.ReadAsync(null!);

            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("reader");
        }

        [Fact]
        public async Task ReadAsync_EmptyArray_ReturnsNoGames()
        {
            IReadOnlyList<Game> games = await ReadAsync("[]");

            games.Should().BeEmpty();
        }

        [Fact]
        public async Task ReadAsync_WellFormedEntry_ReadsEveryField()
        {
            IReadOnlyList<Game> games = await ReadAsync(@"
            [
              {
                ""Id"": 7,
                ""Name"": ""Hades"",
                ""Genre"": ""Action"",
                ""Platforms"": [ ""PC"", ""Switch"" ],
                ""Score"": 9.5,
                ""Status"": ""Playing"",
                ""IsFavourite"": true,
                ""CoverUrl"": ""https://example.com/hades.png""
              }
            ]");

            games.Should().HaveCount(1);
            Game game = games[0];
            game.Name.Should().Be("Hades");
            game.Genre.Should().Be("Action");
            game.Platforms.Should().Equal("PC", "Switch");
            game.Score.Should().Be(9.5);
            game.Status.Should().Be(PlayStatus.Playing);
            game.IsFavourite.Should().BeTrue();
            game.CoverUrl.Should().Be("https://example.com/hades.png");
        }

        [Fact]
        public async Task ReadAsync_IdentityInTheFile_IsNotCarriedOver()
        {
            // An imported game describes a new row. Keeping the number from the file would
            // either collide with a row that already holds it or claim one that means
            // something else in this catalogue.
            IReadOnlyList<Game> games = await ReadAsync(@"[ { ""Id"": 812, ""Name"": ""Hades"" } ]");

            games[0].Id.Should().Be(0);
        }

        [Fact]
        public async Task ReadAsync_PropertyNamesInAnotherCase_AreStillRecognised()
        {
            IReadOnlyList<Game> games = await ReadAsync(@"[ { ""name"": ""Hades"", ""genre"": ""Action"" } ]");

            games[0].Name.Should().Be("Hades");
            games[0].Genre.Should().Be("Action");
        }

        [Fact]
        public async Task ReadAsync_AbsentFields_TakeTheValuesANewGameHas()
        {
            IReadOnlyList<Game> games = await ReadAsync(@"[ { ""Name"": ""Hades"" } ]");

            Game game = games[0];
            game.Genre.Should().BeNull();
            game.Platforms.Should().BeEmpty();
            game.Score.Should().BeNull();
            game.Status.Should().Be(PlayStatus.Backlog);
            game.IsFavourite.Should().BeFalse();
            game.CoverUrl.Should().BeNull();
        }

        [Fact]
        public async Task ReadAsync_NotJsonAtAll_ThrowsImportFormatException()
        {
            Func<Task> act = () => ReadAsync("this file is not JSON at all");

            await act.Should().ThrowAsync<ImportFormatException>();
        }

        [Fact]
        public async Task ReadAsync_EmptyFile_ThrowsImportFormatException()
        {
            Func<Task> act = () => ReadAsync(string.Empty);

            await act.Should().ThrowAsync<ImportFormatException>();
        }

        [Fact]
        public async Task ReadAsync_TruncatedJson_ThrowsImportFormatException()
        {
            Func<Task> act = () => ReadAsync(@"[ { ""Name"": ""Hades"" ");

            await act.Should().ThrowAsync<ImportFormatException>();
        }

        [Fact]
        public async Task ReadAsync_JsonThatIsNotAnArrayOfGames_ThrowsImportFormatException()
        {
            // Valid JSON, wrong shape: a single object rather than the array the export
            // writes. Refused whole, because there is no telling what else in it is misread.
            Func<Task> act = () => ReadAsync(@"{ ""Name"": ""Hades"" }");

            await act.Should().ThrowAsync<ImportFormatException>();
        }

        [Fact]
        public async Task ReadAsync_TheLiteralNull_ThrowsImportFormatException()
        {
            Func<Task> act = () => ReadAsync("null");

            await act.Should().ThrowAsync<ImportFormatException>();
        }

        [Fact]
        public async Task ReadAsync_ArrayHoldingANull_ThrowsImportFormatException()
        {
            Func<Task> act = () => ReadAsync(@"[ { ""Name"": ""Hades"" }, null ]");

            await act.Should().ThrowAsync<ImportFormatException>();
        }

        [Fact]
        public async Task ReadAsync_ArrayOfSomethingOtherThanObjects_ThrowsImportFormatException()
        {
            Func<Task> act = () => ReadAsync(@"[ ""Hades"", ""Celeste"" ]");

            await act.Should().ThrowAsync<ImportFormatException>();
        }

        [Fact]
        public async Task ReadAsync_EntryWithNoName_IsReturnedAndRejectedByTheValidator()
        {
            // A missing title is one bad entry, not a bad file: it comes back like any other
            // so that the caller can count it, and the domain is what refuses it.
            IReadOnlyList<Game> games = await ReadAsync(@"
            [
              { ""Genre"": ""Action"", ""Platforms"": [ ""PC"" ] }
            ]");

            games.Should().HaveCount(1);
            games[0].Name.Should().BeEmpty();
            GameValidator.Validate(games[0]).IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task ReadAsync_EntryWithNoPlatform_IsReturnedAndRejectedByTheValidator()
        {
            IReadOnlyList<Game> games = await ReadAsync(@"[ { ""Name"": ""Hades"", ""Genre"": ""Action"" } ]");

            games[0].Platforms.Should().BeEmpty();
            GameValidator.Validate(games[0]).IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task ReadAsync_BlankPlatformEntry_IsKeptSoTheValidatorCanSeeIt()
        {
            // Dropping it would turn "this game names no platform" into "this game names one
            // fewer platform", and a game with no platform at all would then pass unnoticed.
            IReadOnlyList<Game> games = await ReadAsync(
                @"[ { ""Name"": ""Hades"", ""Genre"": ""Action"", ""Platforms"": [ null ] } ]");

            games[0].Platforms.Should().HaveCount(1);
            GameValidator.Validate(games[0]).IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task ReadAsync_ScoreOutsideTheAllowedRange_IsReturnedAndRejectedByTheValidator()
        {
            IReadOnlyList<Game> games = await ReadAsync(@"
            [
              { ""Name"": ""Hades"", ""Genre"": ""Action"", ""Platforms"": [ ""PC"" ], ""Score"": 42 }
            ]");

            games[0].Score.Should().Be(42);
            GameValidator.Validate(games[0]).IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task ReadAsync_OneBadEntryAmongGoodOnes_StillReturnsAllOfThem()
        {
            IReadOnlyList<Game> games = await ReadAsync(@"
            [
              { ""Name"": ""Hades"", ""Genre"": ""Action"", ""Platforms"": [ ""PC"" ], ""Score"": 9.5 },
              { ""Genre"": ""Action"", ""Platforms"": [ ""PC"" ] },
              { ""Name"": ""Celeste"", ""Genre"": ""Platformer"", ""Platforms"": [ ""PC"" ], ""Score"": 9.0 }
            ]");

            games.Should().HaveCount(3);
            GameValidator.Validate(games[0]).IsValid.Should().BeTrue();
            GameValidator.Validate(games[1]).IsValid.Should().BeFalse();
            GameValidator.Validate(games[2]).IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task ReadAsync_ScoreWrittenAsAQuotedNumber_IsStillRead()
        {
            IReadOnlyList<Game> games = await ReadAsync(@"[ { ""Name"": ""Hades"", ""Score"": ""9.5"" } ]");

            games[0].Score.Should().Be(9.5);
        }

        [Fact]
        public async Task ReadAsync_CommentsAndATrailingComma_AreIgnored()
        {
            IReadOnlyList<Game> games = await ReadAsync(@"
            [
              // added by hand
              { ""Name"": ""Hades"", ""Genre"": ""Action"", ""Platforms"": [ ""PC"" ], },
            ]");

            games.Should().HaveCount(1);
            games[0].Name.Should().Be("Hades");
        }

        [Fact]
        public async Task ReadAsync_CancelledToken_ThrowsOperationCanceledException()
        {
            using CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();
            using StringReader reader = new StringReader("[]");

            Func<Task> act = () => _importer.ReadAsync(reader, cts.Token);

            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task ReadAsync_WhatTheExporterWrote_ComesBackAsTheSameGames()
        {
            // The round trip is the point of the format: a file this application exported is
            // a file it can import, field for field, without a conversion step in between.
            Game[] originals =
            {
                new Game
                {
                    Id = 1,
                    Name = "Hades",
                    Genre = "Action",
                    Platforms = new List<string> { "PC", "Switch" },
                    Score = 9.5,
                    Status = PlayStatus.Finished,
                    IsFavourite = true,
                    CoverUrl = "https://example.com/hades.png",
                },
                new Game
                {
                    Id = 2,
                    Name = "No Metadata",
                    Genre = null,
                    Platforms = new string[0],
                    Score = null,
                    Status = PlayStatus.Backlog,
                    IsFavourite = false,
                    CoverUrl = null,
                },
            };

            string json;
            using (StringWriter writer = new StringWriter())
            {
                await new JsonGameExporter().WriteAsync(originals, writer);
                json = writer.ToString();
            }

            IReadOnlyList<Game> read = await ReadAsync(json);

            // Identity is excluded on purpose: an imported game is a new row, and the
            // importer reads past the number in the file for that reason.
            read.Should().BeEquivalentTo(
                originals,
                options => options.Excluding(game => game.Id).WithStrictOrdering());
        }

        [Fact]
        public async Task ReadAsync_AWholeExportedCatalogue_KeepsEveryTitleInOrder()
        {
            Game[] originals =
            {
                SampleGame("Hades"),
                SampleGame("Celeste"),
                SampleGame("Hollow Knight"),
            };

            string json;
            using (StringWriter writer = new StringWriter())
            {
                await new JsonGameExporter().WriteAsync(originals, writer);
                json = writer.ToString();
            }

            IReadOnlyList<Game> read = await ReadAsync(json);

            read.Should().HaveCount(3);
            read[0].Name.Should().Be("Hades");
            read[1].Name.Should().Be("Celeste");
            read[2].Name.Should().Be("Hollow Knight");
        }

        private async Task<IReadOnlyList<Game>> ReadAsync(string json)
        {
            using StringReader reader = new StringReader(json);
            return await _importer.ReadAsync(reader);
        }

        private static Game SampleGame(string name) => new Game
        {
            Name = name,
            Genre = "Action",
            Platforms = new List<string> { "PC" },
            Score = 8.0,
            Status = PlayStatus.Backlog,
        };
    }
}
