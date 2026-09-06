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

            services.AddScoped<IGameService, GameService>();
            services.AddScoped<IReviewService, ReviewService>();

            // Registered as IEnumerable<IRecommendationStrategy>: Phase 5 adds the
            // genre-weighted strategy with one more line here and no other change.
            services.AddScoped<IRecommendationStrategy, RandomStrategy>();
            services.AddScoped<IRecommendationService, RecommendationService>();

            return services;
        }
    }
}
