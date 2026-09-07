using Microsoft.Extensions.DependencyInjection;
using VideoGameManager.Data;

namespace VideoGameManager.Services
{
    /// <summary>
    /// Registers the application core. The presentation layer calls this instead of
    /// registering repositories itself, so it never needs a compile-time reference to
    /// <c>VideoGameManager.Data</c> and the dependency direction stays one way.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the data access and business services. An <c>IConfiguration</c> must
        /// already be registered: <see cref="SqlConnectionFactory"/> reads the connection
        /// string named <see cref="SqlConnectionFactory.ConnectionStringName"/> from it.
        /// </summary>
        public static IServiceCollection AddVideoGameManager(this IServiceCollection services)
        {
            // One connection factory for the process; it only holds a string.
            services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

            services.AddScoped<IGameRepository, GameRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IDatabaseProbe, SqlDatabaseProbe>();
            services.AddScoped<IDatabaseMigrator, DatabaseMigrator>();

            services.AddScoped<IGameService, GameService>();
            services.AddScoped<IDatabaseMigrationService, DatabaseMigrationService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IDatabaseHealthService, DatabaseHealthService>();
            services.AddScoped<IStatisticsService, StatisticsService>();

            // Registered as a set, like the strategies below: the screen that saves a file
            // offers one entry per exporter and asks the chosen one to write, so adding a
            // third format costs one line here and nothing in the user interface.
            services.AddScoped<IGameExporter, CsvGameExporter>();
            services.AddScoped<IGameExporter, JsonGameExporter>();

            // Registered as IEnumerable<IRecommendationStrategy>, so a further strategy costs
            // one line here and no other change. Order is meaningful: the recommendation
            // service treats the first entry as the default when no name is given, and the
            // random pick is the one the application has always started from.
            services.AddScoped<IRecommendationStrategy, RandomStrategy>();
            services.AddScoped<IRecommendationStrategy, GenreWeightedStrategy>();
            services.AddScoped<IRecommendationStrategy, BacklogFirstStrategy>();

            services.AddScoped<IRecommendationService, RecommendationService>();

            return services;
        }
    }
}
