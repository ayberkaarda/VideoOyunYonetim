using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VideoGameManager.Data;
using VideoGameManager.Domain;

namespace VideoGameManager.Services
{
    /// <summary>
    /// Suggests a game from a genre the user has reviewed well, choosing the genre at random but
    /// with a bias towards the ones their reviews favour.
    /// </summary>
    /// <remarks>
    /// The repository only reports raw facts, one row per genre: how many scored reviews it has
    /// and what they average. The weighting and the draw live here so that both can be exercised
    /// without a database.
    /// <para>
    /// The strategy always answers when the catalogue holds anything at all. A user who has
    /// written no reviews yet, or who has rated nothing above the midpoint, gets the same pick
    /// the random strategy would have made rather than an empty screen.
    /// </para>
    /// </remarks>
    public sealed class GenreWeightedStrategy : IRecommendationStrategy
    {
        /// <summary>
        /// The name this strategy is selected by.
        /// </summary>
        public const string StrategyName = "GenreWeighted";

        /// <summary>
        /// The score that counts as indifference: the midpoint of the score range. A genre the
        /// user rates at or below this carries no pull at all, and one rated above it pulls
        /// harder the further above it sits and the more reviews agree.
        /// </summary>
        public const double Neutral = (ScoreRange.Min + ScoreRange.Max) / 2.0;

        private const string FailureMessage =
            "A game could not be suggested because the catalogue could not be read.";

        private readonly IGameRepository _games;
        private readonly Random _random;

        /// <summary>
        /// Creates the strategy.
        /// </summary>
        /// <param name="games">Repository the strategy reads affinities and games from.</param>
        /// <param name="minimumScore">
        /// Lowest score a game must reach to be suggested. Defaults to
        /// <see cref="ScoreRange.Min"/>, meaning every game qualifies.
        /// </param>
        /// <param name="random">
        /// Source of the draw. <c>null</c> takes <see cref="Random.Shared"/>. A test passes a
        /// seeded generator here so that the draw can be observed.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="games"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="minimumScore"/> falls outside the score range.
        /// </exception>
        public GenreWeightedStrategy(IGameRepository games, double minimumScore = ScoreRange.Min, Random random = null)
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
            _random = random ?? Random.Shared;
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
        public async Task<Game> PickAsync(CancellationToken ct = default)
        {
            IReadOnlyList<GenreReviewSummary> affinities = await DatabaseCall
                .RunAsync(() => _games.GetGenreAffinitiesAsync(ct), FailureMessage)
                .ConfigureAwait(false);

            string genre = ChooseGenre(affinities);

            if (genre != null)
            {
                Game fromFavouredGenre = await PickAsync(genre, ct).ConfigureAwait(false);

                if (fromFavouredGenre != null)
                {
                    return fromFavouredGenre;
                }
            }

            // Either the reviews point nowhere or the genre they point at holds no game the
            // threshold accepts. The button still has to answer, so fall back to the whole
            // catalogue.
            return await PickAsync(null, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// How strongly the user's reviews pull towards one genre. Zero or less means no pull.
        /// </summary>
        private static double Weigh(GenreReviewSummary affinity) =>
            Math.Max(0.0, affinity.AverageReviewScore - Neutral) * affinity.ScoredReviewCount;

        /// <summary>
        /// Draws one genre, each with a probability proportional to its weight.
        /// </summary>
        /// <returns>The chosen genre, or <c>null</c> when no genre carries any weight.</returns>
        private string ChooseGenre(IReadOnlyList<GenreReviewSummary> affinities)
        {
            if (affinities == null)
            {
                return null;
            }

            List<string> genres = new List<string>(affinities.Count);
            List<double> weights = new List<double>(affinities.Count);
            double total = 0.0;

            foreach (GenreReviewSummary affinity in affinities)
            {
                double weight = Weigh(affinity);

                if (weight <= 0.0)
                {
                    continue;
                }

                genres.Add(affinity.Genre);
                weights.Add(weight);
                total += weight;
            }

            if (genres.Count == 0)
            {
                return null;
            }

            // Walk the weights until the running sum passes a point drawn uniformly from the
            // total, which lands in each genre's stretch of the line in proportion to its
            // weight. The last genre is returned without a comparison so that rounding in the
            // running sum can never leave the draw with no answer at all.
            double target = _random.NextDouble() * total;
            double running = 0.0;

            for (int index = 0; index < genres.Count - 1; index++)
            {
                running += weights[index];

                if (target < running)
                {
                    return genres[index];
                }
            }

            return genres[genres.Count - 1];
        }

        /// <summary>
        /// Asks the repository for a random game, optionally confined to one genre.
        /// </summary>
        private Task<Game> PickAsync(string genre, CancellationToken ct) =>
            DatabaseCall.RunAsync(
                () => _games.GetRandomAsync(new GameFilter(Genre: genre, MinScore: ScoreFilter), ct),
                FailureMessage);
    }
}
