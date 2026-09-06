using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using VideoGameManager.Domain;
using VideoGameManager.Views;

namespace VideoGameManager
{
    /// <summary>
    /// Passive view for the "review a game" screen. The picker holds
    /// <see cref="GameListItem"/> objects, so the presenter writes against the selected id
    /// instead of matching a title.
    /// </summary>
    public partial class ReviewGameForm : VideoGameManager.UI.Controls.ChromelessForm, IReviewGameView
    {
        private const string GamePlaceholder = "Select a game";

        private readonly ErrorProvider _errors;

        private readonly Presenters.ReviewGamePresenter _presenter;
        private readonly ILogger<ReviewGameForm> _logger;

        /// <summary>Parameterless constructor for the Visual Studio designer only.</summary>
        public ReviewGameForm()
        {
            InitializeComponent();

            _errors = new ErrorProvider { ContainerControl = this, BlinkStyle = ErrorBlinkStyle.NeverBlink };
        }

        /// <summary>The constructor the container uses. It wires the presenter to this view.</summary>
        public ReviewGameForm(
            Services.IGameService games,
            Services.IReviewService reviews,
            ILogger<Presenters.ReviewGamePresenter> presenterLogger,
            ILogger<ReviewGameForm> logger)
            : this()
        {
            _presenter = new Presenters.ReviewGamePresenter(this, games, reviews, presenterLogger);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public event EventHandler Loaded;

        public event EventHandler SaveRequested;

        public int? SelectedGameId => cmbGames.SelectedItem is GameListItem item ? item.Id : (int?)null;

        public string ReviewText => txtComment.Text;

        bool Views.IView.IsBusy
        {
            set
            {
                btnSave.Enabled = !value;
                Cursor = value ? Cursors.WaitCursor : Cursors.Default;
            }
        }

        public void SetGames(IReadOnlyList<GameListItem> games)
        {
            cmbGames.Items.Clear();

            // A DropDownList with no selection paints its whole item area with the
            // selection colour once it takes focus, which reads as a broken control. The
            // placeholder keeps something selected; it is not a GameListItem, so
            // SelectedGameId stays null until a real game is chosen.
            cmbGames.Items.Add(GamePlaceholder);

            foreach (GameListItem game in games)
            {
                cmbGames.Items.Add(game);
            }

            cmbGames.SelectedIndex = 0;
        }

        public void ClearReviewText()
        {
            txtComment.Clear();
        }

        public void ShowFieldError(string field, string message)
        {
            Control target = ControlFor(field);
            if (target is null)
            {
                _logger?.LogWarning(
                    "Validation reported the field {Field} on {Screen}, which has no matching control; showing a general error instead",
                    field,
                    nameof(ReviewGameForm));
                ShowError(message);
                return;
            }

            _errors.SetError(target, message);
        }

        /// <summary>
        /// Maps a domain property name onto the control that holds it. This screen has no
        /// input for <see cref="Review.Score"/>, so a validation error naming it falls
        /// through to the general error display rather than being pinned to an unrelated
        /// control.
        /// </summary>
        private Control ControlFor(string field)
        {
            if (field == nameof(Review.Body)) return frameComment;
            if (field == nameof(Review.GameId)) return frameGames;
            return null;
        }

        public void ClearFieldErrors()
        {
            _errors.Clear();
        }

        public void ShowError(string message)
        {
            MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public void ShowInfo(string message)
        {
            MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public bool Confirm(string message)
        {
            return MessageBox.Show(this, message, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                   == DialogResult.Yes;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ReviewGameForm_Load(object sender, EventArgs e)
        {
            Loaded?.Invoke(this, EventArgs.Empty);
        }
    }
}
