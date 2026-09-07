using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VideoGameManager.Domain;
using VideoGameManager.Services;
using VideoGameManager.Views;

namespace VideoGameManager.Presenters
{
    /// <summary>
    /// Drives <see cref="IRecommendationView"/>. The picking rule lives behind
    /// <see cref="IRecommendationService"/>, so a future strategy change does not touch this
    /// class or the form.
    /// </summary>
    public sealed class RecommendationPresenter
    {
        private readonly IRecommendationView _view;
        private readonly IRecommendationService _recommendations;
        private readonly ILogger<RecommendationPresenter> _logger;

        /// <summary>Wires the presenter to a view and the recommendation service.</summary>
        /// <param name="view">The screen to drive. Its events are subscribed to here.</param>
        /// <param name="recommendations">Supplies both the list of strategies and the picks.</param>
        /// <param name="logger">Where the technical detail of a failure is written.</param>
        /// <exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
        public RecommendationPresenter(
            IRecommendationView view,
            IRecommendationService recommendations,
            ILogger<RecommendationPresenter> logger)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _recommendations = recommendations ?? throw new ArgumentNullException(nameof(recommendations));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _view.Loaded += OnLoaded;
            _view.RecommendationRequested += OnRecommendationRequested;
        }

        // Filling the picker touches no database, only the service's in-memory list of
        // registered strategies, so this stays synchronous and needs no try/catch of its own.
        private void OnLoaded(object sender, EventArgs e)
        {
            _view.Strategies = _recommendations.AvailableStrategies;
        }

        // A failed pick renders inline where the pick is normally shown, rather than as a
        // dialog: the button can be clicked repeatedly while the database is unreachable,
        // and a dialog on every click would repeat with it.
        private async void OnRecommendationRequested(object sender, EventArgs e)
        {
            try
            {
                await RecommendAsync(CancellationToken.None).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fetching a recommendation failed on {Screen}", nameof(RecommendationPresenter));
                _view.ShowLoadError(Messages.ForUser(ex));
            }
        }

        private async Task RecommendAsync(CancellationToken ct)
        {
            _view.IsBusy = true;
            try
            {
                Game game = await _recommendations.RecommendAsync(_view.SelectedStrategy, ct).ConfigureAwait(true);

                if (game is null)
                {
                    _view.ShowGame(null);
                    _view.ShowInfo("No game matched this recommendation strategy.");
                    return;
                }

                _view.ShowGame(game);
            }
            finally
            {
                _view.IsBusy = false;
            }
        }
    }
}
