using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using VideoGameManager.Domain;
using VideoGameManager.Services;
using Xunit;

namespace VideoGameManager.Tests.Services
{
    public class RecommendationServiceTests
    {
        [Fact]
        public void Constructor_NullStrategies_ThrowsArgumentNullException()
        {
            Action act = () => new RecommendationService(null, NullLogger<RecommendationService>.Instance);

            act.Should().Throw<ArgumentNullException>().WithParameterName("strategies");
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            Action act = () => new RecommendationService(new IRecommendationStrategy[] { Strategy("Random") }, null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public void Constructor_NoStrategyRegistered_ThrowsArgumentException()
        {
            Action act = () => Build(new IRecommendationStrategy[0]);

            act.Should().Throw<ArgumentException>().WithParameterName("strategies");
        }

        [Fact]
        public void Constructor_StrategyThatIsNull_ThrowsArgumentException()
        {
            Action act = () => Build(Strategy("Random"), null);

            act.Should().Throw<ArgumentException>().WithParameterName("strategies");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_StrategyWithoutAName_ThrowsArgumentException(string name)
        {
            Action act = () => Build(Strategy(name));

            act.Should().Throw<ArgumentException>().WithParameterName("strategies");
        }

        [Fact]
        public void Constructor_TwoStrategiesSharingANameInAnotherCase_ThrowsArgumentException()
        {
            Action act = () => Build(Strategy("Random"), Strategy("random"));

            act.Should().Throw<ArgumentException>().WithParameterName("strategies");
        }

        [Fact]
        public void AvailableStrategies_ListsEveryNameInRegistrationOrder()
        {
            RecommendationService service = Build(Strategy("Random"), Strategy("GenreWeighted"));

            service.AvailableStrategies.Should().Equal("Random", "GenreWeighted");
        }

        [Fact]
        public async Task RecommendAsync_NoNameGiven_UsesTheFirstRegisteredStrategy()
        {
            Game expected = new Game { Name = "Celeste" };
            IRecommendationStrategy first = Strategy("Random", expected);
            IRecommendationStrategy second = Strategy("GenreWeighted", new Game { Name = "Hades" });
            RecommendationService service = Build(first, second);

            Game recommendation = await service.RecommendAsync();

            recommendation.Should().BeSameAs(expected);
            await second.DidNotReceiveWithAnyArgs().PickAsync();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task RecommendAsync_BlankName_UsesTheFirstRegisteredStrategy(string name)
        {
            Game expected = new Game { Name = "Celeste" };
            RecommendationService service = Build(Strategy("Random", expected), Strategy("GenreWeighted"));

            Game recommendation = await service.RecommendAsync(name);

            recommendation.Should().BeSameAs(expected);
        }

        [Theory]
        [InlineData("GenreWeighted")]
        [InlineData("genreweighted")]
        [InlineData("GENREWEIGHTED")]
        [InlineData("  GenreWeighted  ")]
        public async Task RecommendAsync_NamedStrategy_IsMatchedIgnoringCaseAndSurroundingSpace(string name)
        {
            Game expected = new Game { Name = "Hades" };
            IRecommendationStrategy first = Strategy("Random", new Game { Name = "Celeste" });
            RecommendationService service = Build(first, Strategy("GenreWeighted", expected));

            Game recommendation = await service.RecommendAsync(name);

            recommendation.Should().BeSameAs(expected);
            await first.DidNotReceiveWithAnyArgs().PickAsync();
        }

        [Fact]
        public async Task RecommendAsync_UnknownName_ThrowsArgumentException()
        {
            RecommendationService service = Build(Strategy("Random"));

            Func<Task> act = () => service.RecommendAsync("Weighted");

            await act.Should().ThrowAsync<ArgumentException>().WithParameterName("strategyName");
        }

        [Fact]
        public async Task RecommendAsync_StrategyMatchedNothing_ReturnsNullRatherThanThrowing()
        {
            RecommendationService service = Build(Strategy("Random"));

            Game recommendation = await service.RecommendAsync();

            recommendation.Should().BeNull();
        }

        [Fact]
        public async Task RecommendAsync_PassesTheTokenToTheStrategy()
        {
            CancellationToken ct = new CancellationTokenSource().Token;
            IRecommendationStrategy strategy = Strategy("Random", new Game { Name = "Celeste" });
            RecommendationService service = Build(strategy);

            await service.RecommendAsync(null, ct);

            await strategy.Received(1).PickAsync(ct);
        }

        [Fact]
        public async Task RecommendAsync_StrategyFailed_LetsTheFailureTravel()
        {
            DataAccessException failure = new DataAccessException("the catalogue could not be read.");
            IRecommendationStrategy strategy = Substitute.For<IRecommendationStrategy>();
            strategy.Name.Returns("Random");
            strategy.PickAsync(Arg.Any<CancellationToken>()).Throws(failure);
            RecommendationService service = Build(strategy);

            Func<Task> act = () => service.RecommendAsync();

            (await act.Should().ThrowAsync<DataAccessException>()).And.Should().BeSameAs(failure);
        }

        private static RecommendationService Build(params IRecommendationStrategy[] strategies) =>
            new RecommendationService(strategies, NullLogger<RecommendationService>.Instance);

        /// <summary>
        /// A strategy that answers to <paramref name="name"/> and always suggests
        /// <paramref name="pick"/>, which may be <c>null</c> to mean nothing qualified.
        /// </summary>
        private static IRecommendationStrategy Strategy(string name, Game pick = null)
        {
            IRecommendationStrategy strategy = Substitute.For<IRecommendationStrategy>();
            strategy.Name.Returns(name);
            strategy.PickAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(pick));
            return strategy;
        }
    }
}
