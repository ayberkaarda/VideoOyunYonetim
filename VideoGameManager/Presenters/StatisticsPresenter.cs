using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VideoGameManager.Data;
using VideoGameManager.Services;
using VideoGameManager.Views;

namespace VideoGameManager.Presenters
{
    /// <summary>
    /// Drives <see cref="IStatisticsView"/>. The aggregation happens in the database behind
    /// <see cref="IStatisticsService"/>, so this class only decides when to ask and what the
    /// screen shows when the answer does not arrive.
    /// </summary>
    public sealed class StatisticsPresenter
    {
        private readonly IStatisticsView _view;
        private readonly IStatisticsService _statistics;
        private readonly ILogger<StatisticsPresenter> _logger;

        /// <summary>Wires the presenter to a view and the statistics service.</summary>
        /// <param name="view">The screen to drive.</param>
        /// <param name="statistics">Where the figures come from.</param>
        /// <param name="logger">Where the technical detail of a failure is written.</param>
        public StatisticsPresenter(
            IStatisticsView view,
            IStatisticsService statistics,
            ILogger<StatisticsPresenter> logger)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _statistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _view.Loaded += OnLoadRequested;
            _view.RefreshRequested += OnLoadRequested;
        }

        // A failed read renders inline where the figures normally are, rather than as a
        // dialog: the refresh action can be used repeatedly while the database is
        // unreachable, and a dialog on every attempt would repeat with it.
        private async void OnLoadRequested(object sender, EventArgs e)
        {
            try
            {
                await LoadAsync(CancellationToken.None).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reading the catalogue statistics failed on {Screen}", nameof(VideoGameManager.StatisticsForm));
                _view.ShowUnavailable(Messages.ForUser(ex));
            }
        }

        private async Task LoadAsync(CancellationToken ct)
        {
            _view.IsBusy = true;
            try
            {
                CatalogueStatistics statistics = await _statistics.GetAsync(ct).ConfigureAwait(true);

                if (statistics is null)
                {
                    _view.ShowUnavailable("No statistics are available for this catalogue.");
                    return;
                }

                _view.ShowStatistics(statistics);
            }
            finally
            {
                _view.IsBusy = false;
            }
        }
    }
}
