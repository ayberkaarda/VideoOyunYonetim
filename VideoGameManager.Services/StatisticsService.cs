using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VideoGameManager.Data;

namespace VideoGameManager.Services
{
    /// <summary>
    /// Default <see cref="IStatisticsService"/>: delegates to the repository unchanged.
    /// </summary>
    public sealed class StatisticsService : IStatisticsService
    {
        private const string ReadFailed = "The catalogue statistics could not be read.";

        private readonly IGameRepository _games;
        private readonly ILogger<StatisticsService> _logger;

        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="games">Repository the service delegates to.</param>
        /// <param name="logger">Logger the service is constructed with.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="games"/> or <paramref name="logger"/> is <c>null</c>.
        /// </exception>
        public StatisticsService(IGameRepository games, ILogger<StatisticsService> logger)
        {
            _games = games ?? throw new ArgumentNullException(nameof(games));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public Task<CatalogueStatistics> GetAsync(CancellationToken ct = default) =>
            DatabaseCall.RunAsync(() => _games.GetStatisticsAsync(ct), ReadFailed);
    }
}
