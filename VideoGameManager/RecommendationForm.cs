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
    public partial class RecommendationForm : Form
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

        private void btnCloseWindow_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}
