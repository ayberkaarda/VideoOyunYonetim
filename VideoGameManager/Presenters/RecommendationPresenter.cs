using System;
using System.Threading;
using System.Threading.Tasks;
using VideoGameManager.Domain;
using VideoGameManager.Services;
using VideoGameManager.Views;

namespace VideoGameManager.Presenters
{
    /// <summary>
    /// Drives <see cref="IRecommendationView"/>. The picking rule lives in
    /// <see cref="IRecommendationService"/>, so Phase 5 can add a genre-weighted strategy
    /// without touching this class or the form.
    /// </summary>
    public sealed class RecommendationPresenter
    {
        private readonly IRecommendationView _view;
        private readonly IRecommendationService _recommendations;

        public RecommendationPresenter(IRecommendationView view, IRecommendationService recommendations)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _recommendations = recommendations ?? throw new ArgumentNullException(nameof(recommendations));

            _view.RecommendationRequested += OnRecommendationRequested;
        }

        private async void OnRecommendationRequested(object sender, EventArgs e)
        {
            try
            {
                await RecommendAsync(CancellationToken.None).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _view.ShowError(Messages.ForUser(ex));
            }
        }

        private async Task RecommendAsync(CancellationToken ct)
        {
            _view.IsBusy = true;
            try
            {
                Game game = await _recommendations.RecommendAsync(null, ct).ConfigureAwait(true);

                if (game is null)
                {
                    _view.ShowGame(null);
                    _view.ShowInfo("No games found in the database.");
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
