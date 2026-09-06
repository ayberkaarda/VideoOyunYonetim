using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoGameManager
{
    public partial class BrowseGamesForm : Form
    {
        public BrowseGamesForm()
        {
            InitializeComponent();
        }
        private void LoadGames()
        {
            lstGames.Items.Clear();

            string query = "SELECT Name FROM dbo.Game ORDER BY Name";
            DataTable games = DatabaseHelper.ExecuteQuery(query);

            foreach (DataRow row in games.Rows)
            {
                lstGames.Items.Add(row["Name"].ToString());
                
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BrowseGamesForm_Load(object sender, EventArgs e)
        {
            LoadGames();
            picCover.SizeMode = PictureBoxSizeMode.Zoom; // Keep the cover aspect ratio.
            lblName.Text = "";
            lblGenre.Text = "";
            lblPlatform.Text = "";
            lblScore.Text = "";
            lblComment.Text = "";
        }

        private void lstGames_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstGames.SelectedItem != null)
            {
                string selectedName = lstGames.SelectedItem.ToString();

                string query = "SELECT Name, Genre, [Platform], Score, CoverUrl, Comment FROM dbo.Game WHERE Name = @Name";
                DataTable table = DatabaseHelper.ExecuteQuery(query, new SqlParameter("@Name", selectedName));

                if (table.Rows.Count > 0)
                {
                    DataRow row = table.Rows[0];

                    lblName.Text = row["Name"].ToString();
                    lblGenre.Text = row["Genre"].ToString();
                    lblPlatform.Text = row["Platform"].ToString();
                    lblScore.Text = row["Score"].ToString();
                    lblComment.Text = row["Comment"].ToString();

                    // Load the cover art if the row has one.
                    try
                    {
                        picCover.Load(row["CoverUrl"].ToString());
                    }
                    catch
                    {
                        picCover.Image = null;
                    }
                }
            }
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
