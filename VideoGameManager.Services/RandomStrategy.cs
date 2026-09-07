using System;
using System.Threading;
using System.Threading.Tasks;
using VideoGameManager.Data;
using VideoGameManager.Domain;

namespace VideoGameManager.Services
{
    /// <summary>
    /// Suggests a game picked uniformly at random from those scoring at least
    /// <see cref="MinimumScore"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="MinimumScore"/> defaults to <see cref="ScoreRange.Min"/>, which is no
    /// threshold at all and reproduces exactly what the application has always done. The
    /// project description used to promise a "random high-scoring game" while the query it
    /// described applied no threshold; making the threshold a setting resolves that without
    /// silently changing the behaviour anyone has seen.
    /// </remarks>
    public sealed class RandomStrategy : IRecommendationStrategy
    {
        /// <summary>
        /// The name this strategy is selected by.
        /// </summary>
        public const string StrategyName = "Random";

        private readonly IGameRepository _games;

        /// <summary>
        /// Creates the strategy.
        /// </summary>
        /// <param name="games">Repository the strategy samples from.</param>
        /// <param name="minimumScore">
        /// Lowest score a game must reach to be suggested. Defaults to
        /// <see cref="ScoreRange.Min"/>, meaning every game qualifies.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="games"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="minimumScore"/> falls outside the score range.
        /// </exception>
        public RandomStrategy(IGameRepository games, double minimumScore = ScoreRange.Min)
        {
            if (games == null)
            {
                throw new ArgumentNullException(nameof(games));
            }

            if (!ScoreRange.Contains(minimumScore))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumScore), minimumScore, "The threshold must be inside the score range.");
            }

            _games = games;
            MinimumScore = minimumScore;
        }

        /// <inheritdoc />
        public string Name => StrategyName;

        /// <summary>
        /// Lowest score a game must reach to be suggested.
        /// </summary>
        public double MinimumScore { get; }

        /// <summary>
        /// The threshold as the filter should carry it, which is nothing at all when it sits at
        /// the bottom of the score range.
        /// </summary>
        /// <remarks>
        /// A threshold at the bottom of the range is not a threshold. Asking the catalogue for
        /// games scoring at or above zero is not the same question as asking for every game: a
        /// game nobody has rated has no score to compare against, so it drops out of that
        /// comparison without a word. An unrated game is unknown, not worthless, and leaving the
        /// score out of the filter altogether is what keeps it a candidate.
        /// </remarks>
        private double? ScoreFilter => MinimumScore > ScoreRange.Min ? MinimumScore : (double?)null;

        /// <inheritdoc />
        public Task<Game?> PickAsync(CancellationToken ct = default) =>
            DatabaseCall.RunAsync(
                () => _games.GetRandomAsync(new GameFilter(MinScore: ScoreFilter), ct),
                "A game could not be suggested because the catalogue could not be read.");
    }
}
