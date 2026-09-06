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
    public sealed class AddGamePresenter
    {
        private readonly IAddGameView _view;
        private readonly IGameService _games;
        private readonly ILogger<AddGamePresenter> _logger;

        public AddGamePresenter(IAddGameView view, IGameService games, ILogger<AddGamePresenter> logger)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _games = games ?? throw new ArgumentNullException(nameof(games));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _view.SaveRequested += OnSaveRequested;
        }

        // async void is confined to the event handler, and it cannot let an exception
        // escape onto the UI message loop.
        private async void OnSaveRequested(object sender, EventArgs e)
        {
            try
            {
                await SaveAsync(CancellationToken.None).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saving a new game failed on {Screen}", nameof(VideoGameManager.AddGameForm));
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
                Name = _view.GameName?.Trim(),
                Genre = _view.Genre,
                Platform = _view.Platform,
                Score = score,
                CoverUrl = string.IsNullOrWhiteSpace(_view.CoverUrl) ? null : _view.CoverUrl.Trim()
            };

            _view.IsBusy = true;
            try
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
            finally
            {
                _view.IsBusy = false;
            }
        }

        /// <summary>
        /// Reads the score box. The placeholder yields <c>null</c>, which the domain
        /// validator treats as "no score" rather than as an error. Parsing is culture
        /// invariant, so a machine set to a comma decimal separator behaves the same.
        /// </summary>
        private bool TryReadScore(out double? score)
        {
            score = null;
            string text = _view.ScoreText;

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
