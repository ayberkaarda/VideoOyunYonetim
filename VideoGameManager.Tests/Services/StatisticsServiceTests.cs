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
using VideoGameManager.Services;
using VideoGameManager.Tests.TestDoubles;
using Xunit;

namespace VideoGameManager.Tests.Services
{
    public class StatisticsServiceTests
    {
        private readonly IGameRepository _games = Substitute.For<IGameRepository>();
        private readonly StatisticsService _service;

        public StatisticsServiceTests()
        {
            _service = new StatisticsService(_games, NullLogger<StatisticsService>.Instance);
        }

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            Action act = () => new StatisticsService(null!, NullLogger<StatisticsService>.Instance);

            act.Should().Throw<ArgumentNullException>().WithParameterName("games");
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            Action act = () => new StatisticsService(_games, null!);

            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public async Task GetAsync_RepositoryAnswered_ReturnsItsResultUnchanged()
        {
            CatalogueStatistics statistics = new CatalogueStatistics(
                42, 17, 7.5,
                new List<GenreDistribution> { new GenreDistribution("Metroidvania", 5, 8.1) });

            _games.GetStatisticsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(statistics));

            CatalogueStatistics result = await _service.GetAsync();

            result.Should().BeSameAs(statistics);
        }

        [Fact]
        public async Task GetAsync_PassesTheTokenToTheRepository()
        {
            CancellationToken ct = new CancellationTokenSource().Token;
            CatalogueStatistics statistics = new CatalogueStatistics(0, 0, null, new List<GenreDistribution>());
            _games.GetStatisticsAsync(ct).Returns(Task.FromResult(statistics));

            await _service.GetAsync(ct);

            await _games.Received(1).GetStatisticsAsync(ct);
        }

        [Fact]
        public async Task GetAsync_EmptyCatalogue_IsReturnedAsIs()
        {
            CatalogueStatistics empty = new CatalogueStatistics(0, 0, null, new List<GenreDistribution>());
            _games.GetStatisticsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(empty));

            CatalogueStatistics result = await _service.GetAsync();

            result.Should().BeSameAs(empty);
            result.TotalGames.Should().Be(0);
            result.ReviewCount.Should().Be(0);
            result.AverageScore.Should().BeNull();
            result.ByGenre.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAsync_RepositoryThrowsProviderFailure_ThrowsDataAccessException()
        {
            SqlException provider = SqlExceptionFactory.Create();
            _games.GetStatisticsAsync(Arg.Any<CancellationToken>()).Throws(provider);

            Func<Task> act = () => _service.GetAsync();

            (await act.Should().ThrowAsync<DataAccessException>())
                .And.InnerException.Should().BeSameAs(provider);
        }

        [Fact]
        public async Task GetAsync_RepositoryThrowsSomethingElse_LetsItTravelUnchanged()
        {
            TimeoutException unrelated = new TimeoutException("the wait ran out");
            _games.GetStatisticsAsync(Arg.Any<CancellationToken>()).Throws(unrelated);

            Func<Task> act = () => _service.GetAsync();

            (await act.Should().ThrowAsync<TimeoutException>()).And.Should().BeSameAs(unrelated);
        }
    }
}
