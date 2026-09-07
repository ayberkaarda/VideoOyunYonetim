using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using VideoGameManager.Data;
using VideoGameManager.Domain;
using VideoGameManager.Services;
using VideoGameManager.Tests.TestDoubles;
using Xunit;

namespace VideoGameManager.Tests.Services
{
    public class GameServiceTests
    {
        private readonly IGameRepository _games = Substitute.For<IGameRepository>();
        private readonly GameService _service;

        public GameServiceTests()
        {
            _service = new GameService(_games, NullLogger<GameService>.Instance);
        }

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            Action act = () => new GameService(null!, NullLogger<GameService>.Instance);

            act.Should().Throw<ArgumentNullException>().WithParameterName("games");
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            Action act = () => new GameService(_games, null!);

            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task SearchAsync_PageBelowOne_ThrowsArgumentOutOfRangeException(int page)
        {
            Func<Task> act = () => _service.SearchAsync(GameFilter.None, page, 20);

            await act.Should().ThrowAsync<ArgumentOutOfRangeException>().WithParameterName("page");
            await _games.DidNotReceiveWithAnyArgs().ListAsync(default, default, default);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task SearchAsync_PageSizeBelowOne_ThrowsArgumentOutOfRangeException(int pageSize)
        {
            Func<Task> act = () => _service.SearchAsync(GameFilter.None, 1, pageSize);

            await act.Should().ThrowAsync<ArgumentOutOfRangeException>().WithParameterName("pageSize");
            await _games.DidNotReceiveWithAnyArgs().ListAsync(default, default, default);
        }

        [Fact]
        public async Task SearchAsync_ValidArguments_PassesThemToRepositoryUnchanged()
        {
            GameFilter filter = new GameFilter(Name: "hollow", Genre: "Metroidvania", MinScore: 8.0);
            PagedResult<Game> page = new PagedResult<Game>(new List<Game> { ValidGame() }, 1, 2, 25);
            CancellationToken ct = new CancellationTokenSource().Token;

            _games.ListAsync(filter, 2, 25, GameSortField.Score, true, ct).Returns(Task.FromResult(page));

            PagedResult<Game> result = await _service.SearchAsync(filter, 2, 25, GameSortField.Score, true, ct);

            result.Should().BeSameAs(page);
            await _games.Received(1).ListAsync(filter, 2, 25, GameSortField.Score, true, ct);
        }

        [Fact]
        public async Task SearchAsync_RepositoryThrowsProviderFailure_ThrowsDataAccessException()
        {
            SqlException provider = SqlExceptionFactory.Create();
            _games.ListAsync(Arg.Any<GameFilter>(), Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<GameSortField>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Throws(provider);

            Func<Task> act = () => _service.SearchAsync(GameFilter.None, 1, 20);

            (await act.Should().ThrowAsync<DataAccessException>())
                .And.InnerException.Should().BeSameAs(provider);
        }

        [Fact]
        public async Task SearchAsync_RepositoryThrowsSomethingElse_LetsItTravelUnchanged()
        {
            TimeoutException unrelated = new TimeoutException("the wait ran out");
            _games.ListAsync(Arg.Any<GameFilter>(), Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<GameSortField>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Throws(unrelated);

            Func<Task> act = () => _service.SearchAsync(GameFilter.None, 1, 20);

            (await act.Should().ThrowAsync<TimeoutException>()).And.Should().BeSameAs(unrelated);
        }

        [Fact]
        public async Task GetAsync_ReturnsWhatTheRepositoryFound()
        {
            Game stored = ValidGame();
            _games.GetAsync(7, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Game?>(stored));

            Game? found = await _service.GetAsync(7);

            found.Should().BeSameAs(stored);
        }

        [Fact]
        public async Task GetAsync_NoSuchGame_ReturnsNull()
        {
            _games.GetAsync(7, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Game?>(null));

            Game? found = await _service.GetAsync(7);

            found.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_NullGame_ThrowsArgumentNullException()
        {
            Func<Task> act = () => _service.AddAsync(null!);

            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("game");
        }

        [Fact]
        public async Task AddAsync_InvalidGame_IsRejectedWithoutReachingTheRepository()
        {
            Game game = ValidGame();
            game.Name = "   ";
            game.Genre = null;

            Result<int> result = await _service.AddAsync(game);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Field == nameof(Game.Name));
            result.Errors.Should().Contain(e => e.Field == nameof(Game.Genre));
            await _games.DidNotReceiveWithAnyArgs().AddAsync(default!);
        }

        [Fact]
        public async Task AddAsync_RejectedGame_ProducesNoValue()
        {
            Game game = ValidGame();
            game.Name = null!;

            Result<int> result = await _service.AddAsync(game);

            Func<int> readValue = () => result.Value;
            readValue.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public async Task AddAsync_ScoreOutsideTheRange_IsRejected()
        {
            Game game = ValidGame();
            game.Score = ScoreRange.Max + 0.1;

            Result<int> result = await _service.AddAsync(game);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Field == nameof(Game.Score));
        }

        [Fact]
        public async Task AddAsync_CoverUrlThatIsNotAnAbsoluteWebAddress_IsRejected()
        {
            Game game = ValidGame();
            game.CoverUrl = "cover.png";

            Result<int> result = await _service.AddAsync(game);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Field == nameof(Game.CoverUrl));
        }

        [Fact]
        public async Task AddAsync_ValidGame_ReturnsTheIdentityTheRepositoryAssigned()
        {
            _games.AddAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(42));

            Result<int> result = await _service.AddAsync(ValidGame());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(42);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public async Task AddAsync_TextWithSurroundingSpace_IsTrimmedBeforeItIsStored()
        {
            Game game = new Game
            {
                Name = "  Hollow Knight  ",
                Genre = "\tMetroidvania ",
                Platforms = new string[] { " PC ", "Switch\t" },
                Score = 9.4,
                CoverUrl = "  https://example.invalid/cover.png ",
            };

            Game stored = await CaptureAdd(game);

            stored.Name.Should().Be("Hollow Knight");
            stored.Genre.Should().Be("Metroidvania");
            stored.CoverUrl.Should().Be("https://example.invalid/cover.png");
            stored.Platforms.Should().Equal("PC", "Switch");
        }

        [Fact]
        public async Task AddAsync_BlankCoverUrl_IsStoredAsNothingAtAll()
        {
            Game game = ValidGame();
            game.CoverUrl = "   ";

            Game stored = await CaptureAdd(game);

            stored.CoverUrl.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_BlankPlatformEntries_AreDropped()
        {
            Game game = ValidGame();
            game.Platforms = new string[] { "PC", "   ", "Switch", null! };

            Game stored = await CaptureAdd(game);

            stored.Platforms.Should().Equal("PC", "Switch");
        }

        [Fact]
        public async Task AddAsync_PlatformsRepeatedInAnotherCase_AreStoredOnce()
        {
            Game game = ValidGame();
            game.Platforms = new string[] { "PC", "pc ", "Switch", "SWITCH" };

            Game stored = await CaptureAdd(game);

            stored.Platforms.Should().Equal("PC", "Switch");
        }

        [Fact]
        public async Task AddAsync_PlatformListThatWasNeverSupplied_IsRejected()
        {
            Game game = ValidGame();
            game.Platforms = null!;

            Result<int> result = await _service.AddAsync(game);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Field == nameof(Game.Platforms));
            await _games.DidNotReceiveWithAnyArgs().AddAsync(default!);
        }

        [Fact]
        public async Task AddAsync_PlatformListThatOnlyHeldBlanks_IsRejected()
        {
            Game game = ValidGame();
            game.Platforms = new string[] { " ", "\t" };

            Result<int> result = await _service.AddAsync(game);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Field == nameof(Game.Platforms));
        }

        [Fact]
        public async Task AddAsync_ReviewTextOnTheIncomingGame_IsNotCarriedToTheRepository()
        {
            Game game = new Game
            {
                Name = "Hollow Knight",
                Genre = "Metroidvania",
                Platforms = new string[] { "PC" },
                Score = 9.4,
                LatestReview = "written elsewhere",
            };

            Game stored = await CaptureAdd(game);

            stored.LatestReview.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_PlayStateAndFavouriteFlag_ReachTheRepositoryUnchanged()
        {
            // The service hands the repository a copy it builds property by property, so a
            // property the copy forgets is written as the default of its type instead of what the
            // user chose. Nothing fails when that happens: the save reports success and the value
            // is simply gone. This test is what notices.
            Game game = ValidGame();
            game.Status = PlayStatus.Playing;
            game.IsFavourite = true;

            Game stored = await CaptureAdd(game);

            stored.Status.Should().Be(PlayStatus.Playing);
            stored.IsFavourite.Should().BeTrue();
        }

        [Fact]
        public async Task AddAsync_GameLeftInItsDefaultState_StillReachesTheRepositoryAsBacklog()
        {
            Game stored = await CaptureAdd(ValidGame());

            stored.Status.Should().Be(PlayStatus.Backlog);
            stored.IsFavourite.Should().BeFalse();
        }

        [Fact]
        public async Task AddAsync_StatusThatIsNotADefinedState_IsRejectedWithoutReachingTheRepository()
        {
            Game game = ValidGame();
            game.Status = (PlayStatus)9;

            Result<int> result = await _service.AddAsync(game);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Field == nameof(Game.Status));
            await _games.DidNotReceiveWithAnyArgs().AddAsync(default!);
        }

        [Fact]
        public async Task AddAsync_RepositoryThrowsProviderFailure_ThrowsDataAccessException()
        {
            SqlException provider = SqlExceptionFactory.Create();
            _games.AddAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>()).Throws(provider);

            Func<Task> act = () => _service.AddAsync(ValidGame());

            (await act.Should().ThrowAsync<DataAccessException>())
                .And.InnerException.Should().BeSameAs(provider);
        }

        [Fact]
        public async Task AddAsync_RepositoryThrowsSomethingElse_LetsItTravelUnchanged()
        {
            InvalidOperationException unrelated = new InvalidOperationException("not a provider failure");
            _games.AddAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>()).Throws(unrelated);

            Func<Task> act = () => _service.AddAsync(ValidGame());

            (await act.Should().ThrowAsync<InvalidOperationException>()).And.Should().BeSameAs(unrelated);
        }

        [Fact]
        public async Task UpdateAsync_NullGame_ThrowsArgumentNullException()
        {
            Func<Task> act = () => _service.UpdateAsync(null!);

            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("game");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        public async Task UpdateAsync_WithoutAnIdentity_IsRejectedBeforeValidation(int id)
        {
            Game game = ValidGame(id);

            // The name is broken as well, so a single error on the identity proves the
            // identity check ran first and stopped before the validator did.
            game.Name = null!;

            Result result = await _service.UpdateAsync(game);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle().Which.Field.Should().Be(nameof(Game.Id));
            await _games.DidNotReceiveWithAnyArgs().UpdateAsync(default!);
        }

        [Fact]
        public async Task UpdateAsync_InvalidGame_IsRejectedWithoutReachingTheRepository()
        {
            Game game = ValidGame(12);
            game.Genre = "  ";

            Result result = await _service.UpdateAsync(game);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Field == nameof(Game.Genre));
            await _games.DidNotReceiveWithAnyArgs().UpdateAsync(default!);
        }

        [Fact]
        public async Task UpdateAsync_ValidGame_IsTrimmedBeforeItIsStored()
        {
            Game game = ValidGame(12);
            game.Name = "  Celeste  ";

            Game? stored = null;
            _games.UpdateAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>()).Returns(call =>
            {
                stored = call.Arg<Game>();
                return Task.FromResult(true);
            });

            Result result = await _service.UpdateAsync(game);

            result.IsSuccess.Should().BeTrue();
            stored!.Id.Should().Be(12);
            stored.Name.Should().Be("Celeste");
        }

        [Fact]
        public async Task UpdateAsync_PlayStateAndFavouriteFlag_ReachTheRepositoryUnchanged()
        {
            // Marking a game finished is an update and nothing else, so a copy that drops the
            // play state would undo the very change the user asked for, and report success.
            Game game = ValidGame(12);
            game.Status = PlayStatus.Finished;
            game.IsFavourite = true;

            Game stored = await CaptureUpdate(game);

            stored.Status.Should().Be(PlayStatus.Finished);
            stored.IsFavourite.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_GameLeftInItsDefaultState_StillReachesTheRepositoryAsBacklog()
        {
            Game stored = await CaptureUpdate(ValidGame(12));

            stored.Status.Should().Be(PlayStatus.Backlog);
            stored.IsFavourite.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_StatusThatIsNotADefinedState_IsRejectedWithoutReachingTheRepository()
        {
            Game game = ValidGame(12);
            game.Status = (PlayStatus)9;

            Result result = await _service.UpdateAsync(game);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Field == nameof(Game.Status));
            await _games.DidNotReceiveWithAnyArgs().UpdateAsync(default!);
        }

        [Fact]
        public async Task UpdateAsync_RepositoryChangedNoRow_IsRejectedAsAGameThatIsGone()
        {
            Game game = ValidGame(12);
            _games.UpdateAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(false));

            Result result = await _service.UpdateAsync(game);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Should().Match<ValidationError>(e =>
                    e.Field == nameof(Game.Id) && e.Message.Contains("no longer exists"));
        }

        [Fact]
        public async Task UpdateAsync_RepositoryThrowsProviderFailure_ThrowsDataAccessException()
        {
            SqlException provider = SqlExceptionFactory.Create();
            Game game = ValidGame(12);
            _games.UpdateAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>()).Throws(provider);

            Func<Task> act = () => _service.UpdateAsync(game);

            (await act.Should().ThrowAsync<DataAccessException>())
                .And.InnerException.Should().BeSameAs(provider);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeleteAsync_WithoutAnIdentity_IsRejected(int id)
        {
            Result result = await _service.DeleteAsync(id);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle().Which.Field.Should().Be(nameof(Game.Id));
            await _games.DidNotReceiveWithAnyArgs().DeleteAsync(default);
        }

        [Fact]
        public async Task DeleteAsync_RepositoryRemovedNoRow_IsRejectedAsAGameThatIsGone()
        {
            _games.DeleteAsync(9, Arg.Any<CancellationToken>()).Returns(Task.FromResult(false));

            Result result = await _service.DeleteAsync(9);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Should().Match<ValidationError>(e =>
                    e.Field == nameof(Game.Id) && e.Message.Contains("no longer exists"));
        }

        [Fact]
        public async Task DeleteAsync_RepositoryRemovedTheRow_Succeeds()
        {
            _games.DeleteAsync(9, Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));

            Result result = await _service.DeleteAsync(9);

            result.IsSuccess.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public async Task DeleteAsync_RepositoryThrowsProviderFailure_ThrowsDataAccessException()
        {
            SqlException provider = SqlExceptionFactory.Create();
            _games.DeleteAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Throws(provider);

            Func<Task> act = () => _service.DeleteAsync(9);

            (await act.Should().ThrowAsync<DataAccessException>())
                .And.InnerException.Should().BeSameAs(provider);
        }

        [Fact]
        public async Task GetGenresAsync_ReturnsWhatTheRepositoryFound()
        {
            IReadOnlyList<string> genres = new string[] { "Action", "Racing" };
            _games.GetGenresAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(genres));

            IReadOnlyList<string> found = await _service.GetGenresAsync();

            found.Should().BeSameAs(genres);
        }

        [Fact]
        public async Task GetPlatformsAsync_ReturnsWhatTheRepositoryFound()
        {
            IReadOnlyList<string> platforms = new string[] { "PC", "Switch" };
            _games.GetPlatformsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(platforms));

            IReadOnlyList<string> found = await _service.GetPlatformsAsync();

            found.Should().BeSameAs(platforms);
        }

        [Fact]
        public async Task GetGenresAsync_RepositoryThrowsProviderFailure_ThrowsDataAccessException()
        {
            SqlException provider = SqlExceptionFactory.Create();
            _games.GetGenresAsync(Arg.Any<CancellationToken>()).Throws(provider);

            Func<Task> act = () => _service.GetGenresAsync();

            (await act.Should().ThrowAsync<DataAccessException>())
                .And.InnerException.Should().BeSameAs(provider);
        }

        [Fact]
        public async Task GetPlatformsAsync_RepositoryThrowsProviderFailure_ThrowsDataAccessException()
        {
            SqlException provider = SqlExceptionFactory.Create();
            _games.GetPlatformsAsync(Arg.Any<CancellationToken>()).Throws(provider);

            Func<Task> act = () => _service.GetPlatformsAsync();

            (await act.Should().ThrowAsync<DataAccessException>())
                .And.InnerException.Should().BeSameAs(provider);
        }

        /// <summary>
        /// Runs an add and hands back the game that actually reached the repository, which is
        /// where the normalisation the service performs becomes observable.
        /// </summary>
        private async Task<Game> CaptureAdd(Game game)
        {
            Game? stored = null;

            _games.AddAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>()).Returns(call =>
            {
                stored = call.Arg<Game>();
                return Task.FromResult(1);
            });

            Result<int> result = await _service.AddAsync(game);

            result.IsSuccess.Should().BeTrue("the game under test is meant to pass validation");
            stored.Should().NotBeNull();

            return stored!;
        }

        /// <summary>
        /// Runs an update and hands back the game that actually reached the repository, which is
        /// where the normalisation the service performs becomes observable.
        /// </summary>
        private async Task<Game> CaptureUpdate(Game game)
        {
            Game? stored = null;

            _games.UpdateAsync(Arg.Any<Game>(), Arg.Any<CancellationToken>()).Returns(call =>
            {
                stored = call.Arg<Game>();
                return Task.FromResult(true);
            });

            Result result = await _service.UpdateAsync(game);

            result.IsSuccess.Should().BeTrue("the game under test is meant to pass validation");
            stored.Should().NotBeNull();

            return stored!;
        }

        private static Game ValidGame(int id = 0) => new Game
        {
            Id = id,
            Name = "Hollow Knight",
            Genre = "Metroidvania",
            Platforms = new string[] { "PC" },
            Score = 9.4,
            CoverUrl = "https://example.invalid/cover.png",
        };
    }

    public class DataAccessExceptionTests
    {
        [Fact]
        public void Constructor_WithoutArguments_CarriesADefaultMessage()
        {
            DataAccessException exception = new DataAccessException();

            exception.Message.Should().NotBeEmpty();
        }

        [Fact]
        public void Constructor_WithMessage_KeepsIt()
        {
            DataAccessException exception = new DataAccessException("the catalogue is unavailable");

            exception.Message.Should().Be("the catalogue is unavailable");
            exception.InnerException.Should().BeNull();
        }

        [Fact]
        public void Constructor_WithInnerException_KeepsBoth()
        {
            InvalidOperationException cause = new InvalidOperationException("cause");

            DataAccessException exception = new DataAccessException("wrapper", cause);

            exception.Message.Should().Be("wrapper");
            exception.InnerException.Should().BeSameAs(cause);
        }
    }
}
