using System;
using System.Data;
using System.Windows.Forms;

namespace VideoGameManager
{
    public partial class RecommendationForm : VideoGameManager.UI.Controls.ChromelessForm
    {
        public RecommendationForm()
        {
            InitializeComponent();
        }

        private void btnRecommend_Click(object sender, EventArgs e)
        {
            string query = "SELECT TOP 1 Name, Genre, [Platform], Score, CoverUrl FROM dbo.Game ORDER BY NEWID()";
            DataTable table = DatabaseHelper.ExecuteQuery(query);

            DataRow row = null;

            if (table.Rows.Count > 0)
            {
                row = table.Rows[0];

                lblName.Text = row["Name"].ToString();
                lblGenre.Text = row["Genre"].ToString();
                lblPlatform.Text = row["Platform"].ToString();
                lblScore.Text = row["Score"].ToString();
            }
            else
            {
                MessageBox.Show("No games found in the database.");
                return;
            }

            try
            {
                picCover.Load(row["CoverUrl"].ToString());
            }
            catch
            {
                picCover.Image = null;
            }
        }

        private void RecommendationForm_Load(object sender, EventArgs e)
        {
            picCover.SizeMode = PictureBoxSizeMode.Zoom;
        }
    }
}
