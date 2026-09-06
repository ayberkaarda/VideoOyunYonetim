using System;
using System.Windows.Forms;

namespace VideoGameManager
{
    public partial class MainForm : VideoGameManager.UI.Controls.ChromelessForm
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnAddGame_Click(object sender, EventArgs e)
        {
            new AddGameForm().ShowDialog();
        }

        private void btnBrowseGames_Click(object sender, EventArgs e)
        {
            new BrowseGamesForm().ShowDialog();
        }

        private void btnRecommend_Click(object sender, EventArgs e)
        {
            RecommendationForm recommendationForm = new RecommendationForm();
            recommendationForm.ShowDialog();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnReview_Click(object sender, EventArgs e)
        {
            ReviewGameForm reviewGameForm = new ReviewGameForm();
            reviewGameForm.ShowDialog();
        }
    }
}
