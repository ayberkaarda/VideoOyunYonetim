using System;
using System.Threading;
using System.Threading.Tasks;
using VideoGameManager.Data;
using VideoGameManager.Domain;

namespace VideoGameManager.Services
{
    /// <summary>
    /// Suggests a game the user has bought but not started yet, so the pile of unplayed games is
    /// what the suggestion draws from first.
    /// </summary>
    /// <remarks>
    /// An empty backlog is the normal state for a user who finishes what they start, so it is not
    /// treated as a failure: the strategy then draws from the whole catalogue instead and the
    /// button always answers.
    /// </remarks>
    public sealed class BacklogFirstStrategy : IRecommendationStrategy
    {
        /// <summary>
        /// The name this strategy is selected by.
        /// </summary>
        public const string StrategyName = "BacklogFirst";

        private const string FailureMessage =
            "A game could not be suggested because the catalogue could not be read.";

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
        public BacklogFirstStrategy(IGameRepository games, double minimumScore = ScoreRange.Min)
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
        /// comparison without a word. That matters most here, because a game the user has not
        /// started is the one least likely to carry a score of its own, and hiding those would
        /// empty the backlog of exactly what it is for.
        /// </remarks>
        private double? ScoreFilter => MinimumScore > ScoreRange.Min ? MinimumScore : (double?)null;

        /// <inheritdoc />
        public async Task<Game> PickAsync(CancellationToken ct = default)
        {
            Game fromBacklog = await PickAsync(PlayStatus.Backlog, ct).ConfigureAwait(false);

            if (fromBacklog != null)
            {
                return fromBacklog;
            }

            // Nothing in the backlog reaches the threshold; suggest from the whole catalogue
            // rather than leaving the request unanswered.
            return await PickAsync(null, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Asks the repository for a random game, optionally confined to one play status.
        /// </summary>
        private Task<Game> PickAsync(PlayStatus? status, CancellationToken ct) =>
            DatabaseCall.RunAsync(
                () => _games.GetRandomAsync(new GameFilter(MinScore: ScoreFilter, Status: status), ct),
                FailureMessage);
    }
}
