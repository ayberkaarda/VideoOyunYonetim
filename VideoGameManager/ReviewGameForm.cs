using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace VideoGameManager
{
    public partial class ReviewGameForm : Form
    {
        public ReviewGameForm()
        {
            InitializeComponent();
            
        }
       
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cmbGames.SelectedItem == null)
            {
                MessageBox.Show("Please select a game.");
                return;
            }

            string selectedName = cmbGames.Text;
            string comment = txtComment.Text;

            string query = "UPDATE dbo.Game SET Comment = @comment WHERE Name = @name";
            SqlParameter[] parameters = {
        new SqlParameter("@comment", comment),
        new SqlParameter("@name", selectedName)
    };

            try
            {
                DatabaseHelper.ExecuteNonQuery(query, parameters);
                MessageBox.Show("Review saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

        }

        private void ReviewGameForm_Load(object sender, EventArgs e)
        {
            string query = "SELECT Name FROM dbo.Game ORDER BY Name ASC";
            DataTable dt = DatabaseHelper.ExecuteQuery(query);

            cmbGames.Items.Clear();
            foreach (DataRow row in dt.Rows)
            {
                cmbGames.Items.Add(row["Name"].ToString());
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void btnCloseWindow_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            
        }
    }
}

