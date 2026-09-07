using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VideoGameManager.Domain;
using VideoGameManager.Services;
using VideoGameManager.Views;

namespace VideoGameManager.Presenters
{
    /// <summary>
    /// Drives <see cref="IAddGameView"/>. Holds no WinForms type: the view reports what the
    /// user typed, the domain decides whether it is valid, and the service writes it.
    /// </summary>
    /// <remarks>
    /// The same screen adds a new game and edits an existing one. Which of the two is in
    /// play is tracked here, as <see cref="_editingId"/>, never on the view: the view only
    /// renders what it is told.
    /// </remarks>
    public sealed class AddGamePresenter
    {
        private const string GameNotFound =
            "This game could not be found. It may have been removed from the catalogue.";

        private readonly IAddGameView _view;
        private readonly IGameService _games;
        private readonly ILogger<AddGamePresenter> _logger;

        /// <summary>
        /// Identity of the game being edited, or <c>null</c> while the screen is adding a
        /// new one. Only ever set after a game has been loaded successfully, so a failed or
        /// still-running load can never be mistaken for an editable row.
        /// </summary>
        private int? _editingId;

        /// <summary>Wires the presenter to a view and the catalogue.</summary>
        /// <param name="view">The screen to drive. Its events are subscribed to here.</param>
        /// <param name="games">Where a game is read from and written to.</param>
        /// <param name="logger">Where the technical detail of a failure is written.</param>
        /// <exception cref="ArgumentNullException">Any argument is <c>null</c>.</exception>
        public AddGamePresenter(IAddGameView view, IGameService games, ILogger<AddGamePresenter> logger)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _games = games ?? throw new ArgumentNullException(nameof(games));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _view.SaveRequested += OnSaveRequested;
            _view.EditRequested += OnEditRequested;
        }

        // async void is confined to the event handler, and it cannot let an exception
        // escape onto the UI message loop.
        private async void OnEditRequested(object? sender, GameEditRequestedEventArgs e)
        {
            int gameId = e.GameId;

            try
            {
                await LoadForEditingAsync(gameId, CancellationToken.None).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Loading game {GameId} for editing failed on {Screen}",
                    gameId,
                    nameof(AddGamePresenter));
                _editingId = null;
                _view.ShowLoadFailed(Messages.ForUser(ex));
            }
        }

        private async Task LoadForEditingAsync(int gameId, CancellationToken ct)
        {
            _view.ShowEditing(true);
            _view.IsBusy = true;
            try
            {
                Game? game = await _games.GetAsync(gameId, ct).ConfigureAwait(true);

                if (game == null)
                {
                    _logger.LogWarning("Game {GameId} could not be loaded for editing: it no longer exists.", gameId);
                    _editingId = null;
                    _view.ShowLoadFailed(GameNotFound);
                    return;
                }

                _editingId = gameId;
                _view.ShowGame(game);
            }
            finally
            {
                _view.IsBusy = false;
            }
        }

        // async void is confined to the event handler, and it cannot let an exception
        // escape onto the UI message loop.
        private async void OnSaveRequested(object? sender, EventArgs e)
        {
            try
            {
                await SaveAsync(CancellationToken.None).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saving a game failed on {Screen}", nameof(AddGamePresenter));
                _view.ShowError(Messages.ForUser(ex));
            }
        }

        private async Task SaveAsync(CancellationToken ct)
        {
            _view.ClearFieldErrors();

            if (!TryReadScore(out double? score))
            {
                _view.ShowFieldError(nameof(Game.Score), "Score must be a number.");
                return;
            }

            Game game = new Game
            {
                Id = _editingId ?? 0,
                Name = _view.GameName?.Trim() ?? string.Empty,
                Genre = _view.Genre,
                Platforms = _view.Platforms,
                Score = score,
                CoverUrl = string.IsNullOrWhiteSpace(_view.CoverUrl) ? null : _view.CoverUrl.Trim(),
                Status = _view.Status,
                IsFavourite = _view.IsFavourite
            };

            _view.IsBusy = true;
            try
            {
                if (_editingId.HasValue)
                {
                    await UpdateAsync(game, ct).ConfigureAwait(true);
                    return;
                }

                await AddAsync(game, ct).ConfigureAwait(true);
            }
            finally
            {
                _view.IsBusy = false;
            }
        }

        private async Task AddAsync(Game game, CancellationToken ct)
        {
            Result<int> result = await _games.AddAsync(game, ct).ConfigureAwait(true);

            if (!result.IsSuccess)
            {
                _view.ShowFieldErrors(result.Validation.Errors);
                return;
            }

            _view.ResetInput();
            _view.ShowInfo("Game added successfully.");
        }

        private async Task UpdateAsync(Game game, CancellationToken ct)
        {
            Result result = await _games.UpdateAsync(game, ct).ConfigureAwait(true);

            if (!result.IsSuccess)
            {
                _view.ShowFieldErrors(result.Validation.Errors);
                return;
            }

            // Editing closes the dialog instead of clearing the form: unlike adding, there
            // is no "edit several games in a row" workflow, and the caller re-reads the row
            // once it sees DialogResult.OK.
            _view.CloseAfterSave();
        }

        /// <summary>
        /// Reads the score box. The placeholder yields <c>null</c>, which the domain
        /// validator treats as "no score" rather than as an error. Parsing is culture
        /// invariant, so a machine set to a comma decimal separator behaves the same.
        /// </summary>
        private bool TryReadScore(out double? score)
        {
            score = null;
            string? text = _view.ScoreText;

            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed))
            {
                return false;
            }

            score = parsed;
            return true;
        }
    }
}
