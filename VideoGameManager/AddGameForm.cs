using System;
using System.Windows.Forms;
using VideoGameManager.Domain;
using VideoGameManager.Views;

namespace VideoGameManager
{
    /// <summary>
    /// Passive view for the "add a game" screen. It reports what the user typed and raises
    /// events; validation and persistence live behind the presenter.
    /// </summary>
    public partial class AddGameForm : VideoGameManager.UI.Controls.ChromelessForm, IAddGameView
    {
        private const string GenrePlaceholder = "Genre";
        private const string PlatformPlaceholder = "Platform";
        private const string ScorePlaceholder = "Score";

        private readonly ErrorProvider _errors;

        private readonly Presenters.AddGamePresenter _presenter;

        /// <summary>Parameterless constructor for the Visual Studio designer only.</summary>
        public AddGameForm()
        {
            InitializeComponent();

            _errors = new ErrorProvider { ContainerControl = this, BlinkStyle = ErrorBlinkStyle.NeverBlink };
        }

        /// <summary>The constructor the container uses. It wires the presenter to this view.</summary>
        public AddGameForm(Services.IGameService games) : this()
        {
            _presenter = new Presenters.AddGamePresenter(this, games);
        }

        public event EventHandler SaveRequested;

        public string GameName => txtName.Text;

        public string Genre => Selected(cmbGenre, GenrePlaceholder);

        public string Platform => Selected(cmbPlatform, PlatformPlaceholder);

        public string ScoreText => Selected(cmbScore, ScorePlaceholder);

        public string CoverUrl => txtCoverUrl.Text;

        bool Views.IView.IsBusy
        {
            set
            {
                btnSave.Enabled = !value;
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
            ClearFieldErrors();
        }

        /// <summary>
        /// Maps a domain property name onto the control that holds it, so the presenter
        /// never needs to know which control exists.
        /// </summary>
        public void ShowFieldError(string field, string message)
        {
            Control target = ControlFor(field);
            if (target is null)
            {
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

        private Control ControlFor(string field)
        {
            if (field == nameof(Game.Name)) return frameName;
            if (field == nameof(Game.Genre)) return frameGenre;
            if (field == nameof(Game.Platform)) return framePlatform;
            if (field == nameof(Game.Score)) return frameScore;
            if (field == nameof(Game.CoverUrl)) return frameCoverUrl;
            return null;
        }

        /// <summary>Returns the selection, or <c>null</c> while the placeholder is selected.</summary>
        private static string Selected(ComboBox combo, string placeholder)
        {
            string text = combo.Text;
            return string.IsNullOrEmpty(text) || text == placeholder ? null : text;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveRequested?.Invoke(this, EventArgs.Empty);
        }

        private void AddGameForm_Load(object sender, EventArgs e)
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
        }
    }
}
