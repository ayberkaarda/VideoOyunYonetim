using System;
using System.Threading;
using System.Threading.Tasks;
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

        public BrowseGamesPresenter(IGameListView view, IGameService games)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _games = games ?? throw new ArgumentNullException(nameof(games));

            _view.Loaded += OnLoaded;
            _view.SelectionChanged += OnSelectionChanged;
        }

        private async void OnLoaded(object sender, EventArgs e)
        {
            try
            {
                await LoadAsync(CancellationToken.None).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _view.ShowError(Messages.ForUser(ex));
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
                _view.ShowError(Messages.ForUser(ex));
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
