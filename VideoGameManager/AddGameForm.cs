using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using VideoGameManager.Domain;
using VideoGameManager.Views;

namespace VideoGameManager
{
    /// <summary>
    /// Passive view for the "add or edit a game" screen. It reports what the user typed and
    /// raises events; validation and persistence live behind the presenter.
    /// </summary>
    public partial class AddGameForm : VideoGameManager.UI.Controls.ChromelessForm, IAddGameView
    {
        private const string GenrePlaceholder = "Genre";
        private const string PlatformPlaceholder = "Platform";
        private const string ScorePlaceholder = "Score";

        private const string AddingTitle = "Video Game Manager | Add Game";
        private const string EditingTitle = "Video Game Manager | Edit Game";

        private readonly ErrorProvider _errors;

        private readonly Presenters.AddGamePresenter? _presenter;
        private readonly ILogger<AddGameForm>? _logger;

        /// <summary>
        /// Set once a load requested through <see cref="LoadForEditing"/> fails. Saving stays
        /// disabled from then on, regardless of what <see cref="Views.IView.IsBusy"/> does
        /// afterwards, so a form left half-filled by a failed load can never be written over
        /// the row that could not be read.
        /// </summary>
        private bool _saveBlocked;

        /// <summary>Parameterless constructor for the Visual Studio designer only.</summary>
        public AddGameForm()
        {
            InitializeComponent();

            _errors = new ErrorProvider { ContainerControl = this, BlinkStyle = ErrorBlinkStyle.NeverBlink };
        }

        /// <summary>The constructor the container uses. It wires the presenter to this view.</summary>
        public AddGameForm(
            Services.IGameService games,
            ILogger<Presenters.AddGamePresenter> presenterLogger,
            ILogger<AddGameForm> logger)
            : this()
        {
            _presenter = new Presenters.AddGamePresenter(this, games, presenterLogger);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public event EventHandler? SaveRequested;

        public event EventHandler<GameEditRequestedEventArgs>? EditRequested;

        public string GameName => txtName.Text;

        public string? Genre => Selected(cmbGenre, GenrePlaceholder);

        /// <summary>
        /// The combo box below is single-select because today's catalogue holds one
        /// platform per game, even though the schema and the domain model can hold more
        /// than one. Wrapping the single selection in a list keeps this view honest about
        /// that shape without adding a multi-select control in this pass: no selection
        /// (placeholder) yields an empty list, not <c>null</c>, so the validator reports a
        /// missing field the same way it would for any other required value.
        /// </summary>
        public IReadOnlyList<string> Platforms
        {
            get
            {
                string? selected = Selected(cmbPlatform, PlatformPlaceholder);
                return selected is null ? Array.Empty<string>() : new[] { selected };
            }
        }

        public string? ScoreText => Selected(cmbScore, ScorePlaceholder);

        public string? CoverUrl => txtCoverUrl.Text;

        /// <summary>
        /// The list holds the three <see cref="PlayStatus"/> names in declaration order, so
        /// the selected index is also the underlying enum value; there is no placeholder
        /// because a game always has a play state, even one nobody set on purpose.
        /// </summary>
        public PlayStatus Status => (PlayStatus)cmbStatus.SelectedIndex;

        public bool IsFavourite => chkFavourite.Checked;

        bool Views.IView.IsBusy
        {
            set
            {
                btnSave.Enabled = !value && !_saveBlocked;
                Cursor = value ? Cursors.WaitCursor : Cursors.Default;
            }
        }

        public void ResetInput()
        {
            txtName.Clear();
            txtCoverUrl.Clear();
            cmbGenre.SelectedIndex = 0;
            cmbPlatform.SelectedIndex = 0;
            cmbScore.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
            chkFavourite.Checked = false;
            ClearFieldErrors();
        }

        public void ShowGame(Game game)
        {
            txtName.Text = game.Name;
            SelectOrAdd(cmbGenre, game.Genre);
            SelectOrAdd(cmbPlatform, game.Platforms != null && game.Platforms.Count > 0 ? game.Platforms[0] : null);
            SelectOrAdd(cmbScore, game.Score.HasValue
                ? game.Score.Value.ToString("0.#", CultureInfo.InvariantCulture)
                : null);
            txtCoverUrl.Text = game.CoverUrl ?? string.Empty;
            cmbStatus.SelectedIndex = (int)game.Status;
            chkFavourite.Checked = game.IsFavourite;
            ClearFieldErrors();
        }

        public void ShowEditing(bool isEditing)
        {
            Text = isEditing ? EditingTitle : AddingTitle;
            btnSave.Text = isEditing ? "Update" : "Save";
        }

        public void ShowLoadFailed(string message)
        {
            _saveBlocked = true;
            btnSave.Enabled = false;
            ShowError(message);
        }

        public void CloseAfterSave()
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>Raises <see cref="EditRequested"/> so the presenter can load the game.
        /// Calling this before the dialog is shown is optional; without it the screen adds a
        /// new game, exactly as it always has.</summary>
        public void LoadForEditing(int gameId)
        {
            EditRequested?.Invoke(this, new GameEditRequestedEventArgs(gameId));
        }

        /// <summary>
        /// Maps a domain property name onto the control that holds it, so the presenter
        /// never needs to know which control exists.
        /// </summary>
        public void ShowFieldError(string field, string message)
        {
            Control? target = ControlFor(field);
            if (target is null)
            {
                _logger?.LogWarning(
                    "Validation reported the field {Field} on {Screen}, which has no matching control; showing a general error instead",
                    field,
                    nameof(AddGameForm));
                ShowError(message);
                return;
            }

            _errors.SetError(target, message);
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

        private Control? ControlFor(string field)
        {
            if (field == nameof(Game.Name)) return frameName;
            if (field == nameof(Game.Genre)) return frameGenre;
            if (field == nameof(Game.Platforms)) return framePlatform;
            if (field == nameof(Game.Score)) return frameScore;
            if (field == nameof(Game.CoverUrl)) return frameCoverUrl;
            if (field == nameof(Game.Status)) return frameStatus;
            return null;
        }

        /// <summary>Returns the selection, or <c>null</c> while the placeholder is selected.</summary>
        private static string? Selected(ComboBox combo, string placeholder)
        {
            string text = combo.Text;
            return string.IsNullOrEmpty(text) || text == placeholder ? null : text;
        }

        /// <summary>
        /// Selects <paramref name="value"/> in a placeholder-first combo, adding it to the
        /// list first when the fixed list does not already contain it.
        /// </summary>
        /// <remarks>
        /// A stored value can fall outside the fixed list this screen offers -- most often a
        /// score that does not land on the half-point grid, such as one recorded before this
        /// list existed. A plain <c>DropDownList</c> cannot show a value that is not one of
        /// its items, and leaving the placeholder selected in that case would read back as
        /// "no value" and silently blank the real one out the next time the row is saved.
        /// </remarks>
        private static void SelectOrAdd(ComboBox combo, string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                combo.SelectedIndex = 0;
                return;
            }

            int index = combo.Items.IndexOf(value);
            if (index < 0)
            {
                index = combo.Items.Add(value);
            }

            combo.SelectedIndex = index;
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            SaveRequested?.Invoke(this, EventArgs.Empty);
        }

        private void AddGameForm_Load(object? sender, EventArgs e)
        {
            cmbPlatform.Items.Clear();
            cmbPlatform.Items.Add(PlatformPlaceholder);
            cmbPlatform.Items.AddRange(new string[] { "PC", "PlayStation", "PS5", "Xbox", "Switch" });
            cmbPlatform.SelectedIndex = 0;

            cmbGenre.Items.Clear();
            cmbGenre.Items.Add(GenrePlaceholder);
            cmbGenre.Items.AddRange(new string[]
            {
                "Action", "Adventure", "Horror", "Platformer", "Puzzle", "Racing",
                "Roguelike", "RPG", "Sandbox", "Simulation", "Sports", "Strategy"
            });
            cmbGenre.SelectedIndex = 0;

            // Scores run in half points, because the catalogue already holds values such as
            // 9.5 and 8.6 that a whole-number list could not express.
            cmbScore.Items.Clear();
            cmbScore.Items.Add(ScorePlaceholder);
            for (int half = 0; half <= 20; half++)
            {
                cmbScore.Items.Add((half / 2.0).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture));
            }
            cmbScore.SelectedIndex = 0;

            // The items are added in PlayStatus declaration order (Backlog, Playing,
            // Finished), so the selected index doubles as the enum's underlying value; see
            // the Status property above.
            cmbStatus.Items.Clear();
            cmbStatus.Items.AddRange(new string[] { "Backlog", "Playing", "Finished" });
            cmbStatus.SelectedIndex = 0;
        }
    }
}
