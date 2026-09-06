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
    public partial class AddGameForm : Form
    {
        public AddGameForm()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string genre = cmbGenre.Text;
            string platform = cmbPlatform.Text;
            string scoreText = cmbScore.Text;
            string coverUrl = txtCoverUrl.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Game name cannot be empty.");
                return;
            }

            if (genre == "Genre")
            {
                MessageBox.Show("Please select a genre.");
                return;
            }

            if (platform == "Platform")
            {
                MessageBox.Show("Please select a platform.");
                return;
            }

            if (scoreText == "Score")
            {
                MessageBox.Show("Please select a score.");
                return;
            }

            if (!float.TryParse(scoreText, out float score))
            {
                MessageBox.Show("Score must be a valid number.");
                return;
            }

            string query = @"INSERT INTO dbo.Game (Name, Genre, [Platform], Score, CoverUrl)
                             VALUES (@Name, @Genre, @Platform, @Score, @CoverUrl)";

            SqlParameter[] parameters = new SqlParameter[]
            {
        new SqlParameter("@Name", name),
        new SqlParameter("@Genre", genre),
        new SqlParameter("@Platform", platform),
        new SqlParameter("@Score", score),
        new SqlParameter("@CoverUrl", coverUrl)
            };

            try
            {
                DatabaseHelper.ExecuteNonQuery(query, parameters);
                MessageBox.Show("Game added successfully.");

                // Clear the form and go back to the default selections.
                txtName.Clear();
                txtCoverUrl.Clear();
                cmbGenre.SelectedIndex = 0;
                cmbPlatform.SelectedIndex = 0;
                cmbScore.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void AddGameForm_Load(object sender, EventArgs e)
        {
            cmbPlatform.Items.Clear();
            cmbPlatform.Items.Add("Platform"); // Placeholder entry
            cmbPlatform.Items.AddRange(new string[] { "PC", "PlayStation", "PS5", "Xbox", "Switch" });
            cmbPlatform.SelectedIndex = 0;

            cmbGenre.Items.Clear();
            cmbGenre.Items.Add("Genre"); // Placeholder entry
            cmbGenre.Items.AddRange(new string[] { "Action", "Adventure", "Horror", "Platformer", "Puzzle", "Racing", "Roguelike", "RPG", "Sandbox", "Simulation", "Sports", "Strategy" });
            cmbGenre.SelectedIndex = 0;

            cmbScore.Items.Clear();
            cmbScore.Items.Add("Score"); // Placeholder entry
            for (int i = 1; i <= 10; i++)
            {
                cmbScore.Items.Add(i.ToString());
            }
            cmbScore.SelectedIndex = 0;
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
