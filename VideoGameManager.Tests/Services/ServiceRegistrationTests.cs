using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VideoGameManager.Data;
using VideoGameManager.Services;
using Xunit;

namespace VideoGameManager.Tests.Services
{
    /// <summary>
    /// Guards the composition root. Nothing here opens a connection: the registered types only
    /// keep a connection string until a call is actually made, so the container can be built and
    /// every registration resolved against an address that leads nowhere.
    /// </summary>
    public class ServiceRegistrationTests
    {
        [Theory]
        [InlineData(typeof(IGameService))]
        [InlineData(typeof(IReviewService))]
        [InlineData(typeof(IRecommendationService))]
        [InlineData(typeof(IDatabaseHealthService))]
        [InlineData(typeof(IDatabaseMigrationService))]
        [InlineData(typeof(IStatisticsService))]
        public void AddVideoGameManager_RegistersTheService(Type serviceType)
        {
            using (ServiceProvider provider = BuildProvider())
            using (IServiceScope scope = provider.CreateScope())
            {
                object? resolved = scope.ServiceProvider.GetService(serviceType);

                resolved.Should().NotBeNull();
                resolved.Should().BeAssignableTo(serviceType);
            }
        }

        [Fact]
        public void AddVideoGameManager_RegistersBothExportersAsASet()
        {
            using (ServiceProvider provider = BuildProvider())
            using (IServiceScope scope = provider.CreateScope())
            {
                IEnumerable<IGameExporter> exporters =
                    scope.ServiceProvider.GetServices<IGameExporter>();

                exporters.Select(exporter => exporter.Format)
                    .Should().BeEquivalentTo(new[] { "CSV", "JSON" });
            }
        }

        [Fact]
        public void AddVideoGameManager_RegistersTheRecommendationStrategiesAsASet()
        {
            using (ServiceProvider provider = BuildProvider())
            using (IServiceScope scope = provider.CreateScope())
            {
                IRecommendationService service = scope.ServiceProvider.GetRequiredService<IRecommendationService>();

                service.AvailableStrategies.Should().BeEquivalentTo(new[]
                {
                    RandomStrategy.StrategyName,
                    GenreWeightedStrategy.StrategyName,
                    BacklogFirstStrategy.StrategyName,
                });
            }
        }

        [Fact]
        public void AddVideoGameManager_OffersTheRandomStrategyFirst()
        {
            // The recommendation service treats the first registered strategy as the default
            // when the caller names none, so the order of the registrations is behaviour and
            // not housekeeping.
            using (ServiceProvider provider = BuildProvider())
            using (IServiceScope scope = provider.CreateScope())
            {
                IRecommendationService service = scope.ServiceProvider.GetRequiredService<IRecommendationService>();

                service.AvailableStrategies.Should().HaveCountGreaterThan(1);
                service.AvailableStrategies[0].Should().Be(RandomStrategy.StrategyName);
            }
        }

        [Fact]
        public void AddVideoGameManager_ReturnsTheCollectionSoRegistrationsCanBeChained()
        {
            ServiceCollection services = new ServiceCollection();

            IServiceCollection returned = services.AddVideoGameManager();

            returned.Should().BeSameAs(services);
        }

        private static ServiceProvider BuildProvider()
        {
            Dictionary<string, string?> settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:" + SqlConnectionFactory.ConnectionStringName] = UnusableConnectionString(),
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            ServiceCollection services = new ServiceCollection();
            services.AddSingleton(configuration);

            // The core registration leaves logging to whichever host uses it, so the test
            // supplies loggers that discard everything.
            services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

            services.AddVideoGameManager();

            return services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true,
            });
        }

        /// <summary>
        /// Builds a connection string that the provider can parse and that can never connect: the
        /// host sits in a namespace reserved so that it never resolves, and it carries no
        /// credential of any kind.
        /// </summary>
        private static string UnusableConnectionString() =>
            new SqlConnectionStringBuilder
            {
                DataSource = "placeholder.invalid",
                InitialCatalog = "placeholder",
                IntegratedSecurity = true,
                TrustServerCertificate = true,
            }.ConnectionString;
    }
}
