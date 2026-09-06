using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VideoGameManager.Domain;

namespace VideoGameManager.Services
{
    /// <summary>
    /// Default <see cref="IRecommendationService"/>: routes a request to the named strategy.
    /// </summary>
    /// <remarks>
    /// The service takes every registered strategy, so adding one is a registration and nothing
    /// else.
    /// </remarks>
    public sealed class RecommendationService : IRecommendationService
    {
        private readonly IReadOnlyList<IRecommendationStrategy> _strategies;
        private readonly IReadOnlyList<string> _names;
        private readonly ILogger<RecommendationService> _logger;

        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="strategies">
        /// Every available strategy. The first one registered is the default.
        /// </param>
        /// <param name="logger">Logger an empty pick is recorded on.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="strategies"/> or <paramref name="logger"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// No strategy was registered, or two strategies share a name.
        /// </exception>
        public RecommendationService(IEnumerable<IRecommendationStrategy> strategies, ILogger<RecommendationService> logger)
        {
            if (strategies == null)
            {
                throw new ArgumentNullException(nameof(strategies));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            List<IRecommendationStrategy> ordered = new List<IRecommendationStrategy>(strategies);

            if (ordered.Count == 0)
            {
                throw new ArgumentException("At least one recommendation strategy is required.", nameof(strategies));
            }

            List<string> names = new List<string>(ordered.Count);
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (IRecommendationStrategy strategy in ordered)
            {
                if (strategy == null)
                {
                    throw new ArgumentException("A registered recommendation strategy is null.", nameof(strategies));
                }

                if (string.IsNullOrWhiteSpace(strategy.Name))
                {
                    throw new ArgumentException("A recommendation strategy has no name.", nameof(strategies));
                }

                if (!seen.Add(strategy.Name))
                {
                    throw new ArgumentException(
                        "Two recommendation strategies share the name '" + strategy.Name + "'.",
                        nameof(strategies));
                }

                names.Add(strategy.Name);
            }

            _strategies = ordered;
            _names = names.AsReadOnly();
        }

        /// <inheritdoc />
        public IReadOnlyList<string> AvailableStrategies => _names;

        /// <inheritdoc />
        /// <exception cref="ArgumentException">
        /// No strategy carries <paramref name="strategyName"/>. The caller chooses from
        /// <see cref="AvailableStrategies"/>, so an unknown name is a defect and not user input.
        /// </exception>
        public async Task<Game> RecommendAsync(string strategyName = null, CancellationToken ct = default)
        {
            IRecommendationStrategy strategy = Select(strategyName);
            Game recommendation = await strategy.PickAsync(ct).ConfigureAwait(false);

            if (recommendation == null)
            {
                _logger.LogInformation(
                    "Recommendation strategy {Strategy} found no matching game.", strategy.Name);
            }

            return recommendation;
        }

        private IRecommendationStrategy Select(string strategyName)
        {
            if (string.IsNullOrWhiteSpace(strategyName))
            {
                return _strategies[0];
            }

            foreach (IRecommendationStrategy strategy in _strategies)
            {
                if (string.Equals(strategy.Name, strategyName.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return strategy;
                }
            }

            throw new ArgumentException(
                "There is no recommendation strategy named '" + strategyName + "'.", nameof(strategyName));
        }
    }
}
