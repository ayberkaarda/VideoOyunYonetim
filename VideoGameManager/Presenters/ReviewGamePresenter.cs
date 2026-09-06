using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VideoGameManager.Data;
using VideoGameManager.Domain;
using VideoGameManager.Services;
using VideoGameManager.Views;

namespace VideoGameManager.Presenters
{
    /// <summary>
    /// Drives <see cref="IReviewGameView"/>. The picker carries game ids, so a review is
    /// written against the selected id. The screen used to match on the title, which wrote
    /// to the wrong row as soon as two games shared a name.
    /// </summary>
    public sealed class ReviewGamePresenter
    {
        private const int PageSize = 1000;

        private readonly IReviewGameView _view;
        private readonly IGameService _games;
        private readonly IReviewService _reviews;
        private readonly ILogger<ReviewGamePresenter> _logger;

        public ReviewGamePresenter(
            IReviewGameView view,
            IGameService games,
            IReviewService reviews,
            ILogger<ReviewGamePresenter> logger)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _games = games ?? throw new ArgumentNullException(nameof(games));
            _reviews = reviews ?? throw new ArgumentNullException(nameof(reviews));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _view.Loaded += OnLoaded;
            _view.SaveRequested += OnSaveRequested;
        }

        private async void OnLoaded(object sender, EventArgs e)
        {
            try
            {
                await LoadGamesAsync(CancellationToken.None).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loading the game picker failed on {Screen}", nameof(VideoGameManager.ReviewGameForm));
                _view.ShowError(Messages.ForUser(ex));
            }
        }

        private async void OnSaveRequested(object sender, EventArgs e)
        {
            try
            {
                await SaveAsync(CancellationToken.None).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saving a review failed on {Screen}", nameof(VideoGameManager.ReviewGameForm));
                _view.ShowError(Messages.ForUser(ex));
            }
        }

        private async Task LoadGamesAsync(CancellationToken ct)
        {
            _view.IsBusy = true;
            try
            {
                PagedResult<Game> page = await _games
                    .SearchAsync(new GameFilter(), 1, PageSize, GameSortField.Name, false, ct)
                    .ConfigureAwait(true);

                List<GameListItem> items = new List<GameListItem>(page.Items.Count);
                foreach (Game game in page.Items)
                {
                    items.Add(new GameListItem(game.Id, game.Name));
                }

                _view.SetGames(items);
            }
            finally
            {
                _view.IsBusy = false;
            }
        }

        private async Task SaveAsync(CancellationToken ct)
        {
            _view.ClearFieldErrors();

            int? gameId = _view.SelectedGameId;
            if (gameId is null)
            {
                _view.ShowError("Please select a game.");
                return;
            }

            _view.IsBusy = true;
            try
            {
                Result result = await _reviews
                    .AddAsync(gameId.Value, null, _view.ReviewText, ct)
                    .ConfigureAwait(true);

                if (!result.IsSuccess)
                {
                    _view.ShowFieldErrors(result.Validation.Errors);
                    return;
                }

                _view.ClearReviewText();
                _view.ShowInfo("Review saved successfully.");
            }
            finally
            {
                _view.IsBusy = false;
            }
        }
    }
}
