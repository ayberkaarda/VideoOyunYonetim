using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoGameManager
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
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

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ControlBox = false;
            this.MaximizeBox = false; // Maksimize butonunu gizle
            this.MinimizeBox = false; // Minimize butonunu gizle

        }

        private void btnCloseWindow_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnReview_Click(object sender, EventArgs e)
        {
            ReviewGameForm reviewGameForm = new ReviewGameForm();
            reviewGameForm.ShowDialog();

        }
    }
}
