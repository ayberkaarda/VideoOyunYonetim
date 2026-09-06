using System;
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
    /// Drives <see cref="IGameListView"/>. Details are fetched by id, so two games sharing
    /// a title no longer collapse onto whichever row the database returned first.
    /// </summary>
    public sealed class BrowseGamesPresenter
    {
        private const int PageSize = 1000;

        private readonly IGameListView _view;
        private readonly IGameService _games;
        private readonly ILogger<BrowseGamesPresenter> _logger;

        public BrowseGamesPresenter(IGameListView view, IGameService games, ILogger<BrowseGamesPresenter> logger)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _games = games ?? throw new ArgumentNullException(nameof(games));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _view.Loaded += OnLoaded;
            _view.SelectionChanged += OnSelectionChanged;
        }

        // A failed load or a failed detail fetch renders inline, in the space the list or
        // the details normally occupy, instead of a dialog: the database can drop out while
        // the user is arrowing through the list, and a dialog on every keystroke would be
        // both disruptive and, because the row stays selected, would reopen on the next one.
        private async void OnLoaded(object sender, EventArgs e)
        {
            try
            {
                await LoadAsync(CancellationToken.None).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loading the game list failed on {Screen}", nameof(VideoGameManager.BrowseGamesForm));
                _view.ShowListUnavailable(Messages.ForUser(ex));
            }
        }

        private async void OnSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                await ShowSelectedAsync(CancellationToken.None).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loading game details failed on {Screen}", nameof(VideoGameManager.BrowseGamesForm));
                _view.ShowDetailsUnavailable(Messages.ForUser(ex));
            }
        }

        private async Task LoadAsync(CancellationToken ct)
        {
            _view.IsBusy = true;
            try
            {
                PagedResult<Game> page = await _games
                    .SearchAsync(new GameFilter(), 1, PageSize, GameSortField.Name, false, ct)
                    .ConfigureAwait(true);

                _view.Games = page.Items;
                _view.ShowDetails(null);
            }
            finally
            {
                _view.IsBusy = false;
            }
        }

        private async Task ShowSelectedAsync(CancellationToken ct)
        {
            int? id = _view.SelectedGameId;
            if (id is null)
            {
                _view.ShowDetails(null);
                return;
            }

            Game game = await _games.GetAsync(id.Value, ct).ConfigureAwait(true);
            _view.ShowDetails(game);
        }
    }
}
