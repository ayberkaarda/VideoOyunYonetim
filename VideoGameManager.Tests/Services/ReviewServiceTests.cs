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
    public class ReviewServiceTests
    {
        private const int GameId = 12;

        private readonly IReviewRepository _reviews = Substitute.For<IReviewRepository>();
        private readonly ReviewService _service;

        public ReviewServiceTests()
        {
            _service = new ReviewService(_reviews, NullLogger<ReviewService>.Instance);
        }

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            Action act = () => new ReviewService(null!, NullLogger<ReviewService>.Instance);

            act.Should().Throw<ArgumentNullException>().WithParameterName("reviews");
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            Action act = () => new ReviewService(_reviews, null!);

            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public async Task GetForGameAsync_ReturnsWhatTheRepositoryFound()
        {
            IReadOnlyList<Review> stored = new Review[] { new Review { GameId = GameId, Body = "good" } };
            _reviews.GetForGameAsync(GameId, Arg.Any<CancellationToken>()).Returns(Task.FromResult(stored));

            IReadOnlyList<Review> found = await _service.GetForGameAsync(GameId);

            found.Should().BeSameAs(stored);
        }

        [Fact]
        public async Task GetForGameAsync_PassesTheTokenOn()
        {
            CancellationToken ct = new CancellationTokenSource().Token;
            _reviews.GetForGameAsync(GameId, ct).Returns(Task.FromResult<IReadOnlyList<Review>>(new Review[0]));

            await _service.GetForGameAsync(GameId, ct);

            await _reviews.Received(1).GetForGameAsync(GameId, ct);
        }

        [Fact]
        public async Task GetForGameAsync_RepositoryThrowsProviderFailure_ThrowsDataAccessException()
        {
            SqlException provider = SqlExceptionFactory.Create();
            _reviews.GetForGameAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Throws(provider);

            Func<Task> act = () => _service.GetForGameAsync(GameId);

            (await act.Should().ThrowAsync<DataAccessException>())
                .And.InnerException.Should().BeSameAs(provider);
        }

        [Fact]
        public async Task AddAsync_ValidReview_Succeeds()
        {
            _reviews.AddAsync(Arg.Any<Review>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(3));

            Result result = await _service.AddAsync(GameId, 8.5, "A long, slow burn.");

            result.IsSuccess.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public async Task AddAsync_ValidReview_BuildsTheReviewFromItsArguments()
        {
            Review stored = await CaptureAdd(GameId, 8.5, "A long, slow burn.");

            stored.GameId.Should().Be(GameId);
            stored.Score.Should().Be(8.5);
            stored.Body.Should().Be("A long, slow burn.");
        }

        [Fact]
        public async Task AddAsync_BodyWithSurroundingSpace_IsTrimmedBeforeItIsStored()
        {
            Review stored = await CaptureAdd(GameId, null, "  worth the wait \t");

            stored.Body.Should().Be("worth the wait");
        }

        [Fact]
        public async Task AddAsync_NoScore_StoresNoneAndLeavesTheRatingAlone()
        {
            Review stored = await CaptureAdd(GameId, null, "worth the wait");

            stored.Score.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_StampsTheMomentTheReviewWasWritten()
        {
            DateTimeOffset before = DateTimeOffset.UtcNow;

            Review stored = await CaptureAdd(GameId, null, "worth the wait");

            stored.CreatedAt.Should().BeOnOrAfter(before);
            stored.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
            stored.CreatedAt.Offset.Should().Be(TimeSpan.Zero);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\r\n")]
        public async Task AddAsync_BodyThatIsOnlySpace_IsRejectedWithoutReachingTheRepository(string? body)
        {
            Result result = await _service.AddAsync(GameId, 8.5, body!);

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Field == nameof(Review.Body));
            await _reviews.DidNotReceiveWithAnyArgs().AddAsync(default!);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-4)]
        public async Task AddAsync_GameThatWasNotIdentified_IsRejectedWithoutReachingTheRepository(int gameId)
        {
            Result result = await _service.AddAsync(gameId, 8.5, "worth the wait");

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Field == nameof(Review.GameId));
            await _reviews.DidNotReceiveWithAnyArgs().AddAsync(default!);
        }

        [Theory]
        [InlineData(-0.5)]
        [InlineData(10.5)]
        public async Task AddAsync_ScoreOutsideTheRange_IsRejectedWithoutReachingTheRepository(double score)
        {
            Result result = await _service.AddAsync(GameId, score, "worth the wait");

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Field == nameof(Review.Score));
            await _reviews.DidNotReceiveWithAnyArgs().AddAsync(default!);
        }

        [Fact]
        public async Task AddAsync_SeveralBrokenRules_AreAllReported()
        {
            Result result = await _service.AddAsync(0, 99.0, "  ");

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().HaveCount(3);
        }

        [Fact]
        public async Task AddAsync_RepositoryReportsNoIdentity_IsRejectedAsAGameThatIsGone()
        {
            _reviews.AddAsync(Arg.Any<Review>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(0));

            Result result = await _service.AddAsync(GameId, 8.5, "worth the wait");

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle()
                .Which.Should().Match<ValidationError>(e =>
                    e.Field == nameof(Review.GameId) && e.Message.Contains("no longer exists"));
        }

        [Fact]
        public async Task AddAsync_RepositoryThrowsProviderFailure_ThrowsDataAccessException()
        {
            SqlException provider = SqlExceptionFactory.Create();
            _reviews.AddAsync(Arg.Any<Review>(), Arg.Any<CancellationToken>()).Throws(provider);

            Func<Task> act = () => _service.AddAsync(GameId, 8.5, "worth the wait");

            (await act.Should().ThrowAsync<DataAccessException>())
                .And.InnerException.Should().BeSameAs(provider);
        }

        [Fact]
        public async Task AddAsync_RepositoryThrowsSomethingElse_LetsItTravelUnchanged()
        {
            InvalidOperationException unrelated = new InvalidOperationException("not a provider failure");
            _reviews.AddAsync(Arg.Any<Review>(), Arg.Any<CancellationToken>()).Throws(unrelated);

            Func<Task> act = () => _service.AddAsync(GameId, 8.5, "worth the wait");

            (await act.Should().ThrowAsync<InvalidOperationException>()).And.Should().BeSameAs(unrelated);
        }

        /// <summary>
        /// Runs an add and hands back the review that actually reached the repository, which is
        /// where the review the service builds becomes observable.
        /// </summary>
        private async Task<Review> CaptureAdd(int gameId, double? score, string body)
        {
            Review? stored = null;

            _reviews.AddAsync(Arg.Any<Review>(), Arg.Any<CancellationToken>()).Returns(call =>
            {
                stored = call.Arg<Review>();
                return Task.FromResult(3);
            });

            Result result = await _service.AddAsync(gameId, score, body);

            result.IsSuccess.Should().BeTrue("the review under test is meant to pass validation");
            stored.Should().NotBeNull();

            return stored!;
        }
    }
}
