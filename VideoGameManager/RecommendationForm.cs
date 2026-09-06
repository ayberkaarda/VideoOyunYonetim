using System;
using System.Globalization;
using System.Windows.Forms;
using VideoGameManager.Domain;
using VideoGameManager.Views;

namespace VideoGameManager
{
    /// <summary>
    /// Passive view for the recommendation screen. It renders what it is given and raises
    /// events; the picking rule and the database live behind the presenter.
    /// </summary>
    public partial class RecommendationForm : VideoGameManager.UI.Controls.ChromelessForm, IRecommendationView
    {
        private readonly Presenters.RecommendationPresenter _presenter;

        /// <summary>Parameterless constructor for the Visual Studio designer only.</summary>
        public RecommendationForm()
        {
            InitializeComponent();
        }

        /// <summary>The constructor the container uses. It wires the presenter to this view.</summary>
        public RecommendationForm(Services.IRecommendationService recommendations) : this()
        {
            _presenter = new Presenters.RecommendationPresenter(this, recommendations);
        }

        public event EventHandler RecommendationRequested;

        bool Views.IView.IsBusy
        {
            set
            {
                btnRecommend.Enabled = !value;
                Cursor = value ? Cursors.WaitCursor : Cursors.Default;
            }
        }

        public void ShowGame(Game game)
        {
            if (game is null)
            {
                lblName.Text = string.Empty;
                lblGenre.Text = string.Empty;
                lblPlatform.Text = string.Empty;
                lblScore.Text = string.Empty;
                picCover.Image = null;
                return;
            }

            lblName.Text = game.Name;
            lblGenre.Text = game.Genre;
            lblPlatform.Text = game.Platform;
            lblScore.Text = game.Score?.ToString("0.#", CultureInfo.CurrentCulture) ?? string.Empty;

            LoadCover(game.CoverUrl);
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

        /// <summary>
        /// Cover art comes from a remote address that may be slow, missing or no longer an
        /// image. A failure leaves the box empty rather than interrupting the user.
        /// </summary>
        private void LoadCover(string coverUrl)
        {
            picCover.Image = null;

            if (string.IsNullOrWhiteSpace(coverUrl))
            {
                return;
            }

            try
            {
                picCover.LoadAsync(coverUrl);
            }
            catch (Exception)
            {
                picCover.Image = null;
            }
        }

        private void btnRecommend_Click(object sender, EventArgs e)
        {
            RecommendationRequested?.Invoke(this, EventArgs.Empty);
        }

        private void RecommendationForm_Load(object sender, EventArgs e)
        {
            picCover.SizeMode = PictureBoxSizeMode.Zoom;
        }
    }
}
