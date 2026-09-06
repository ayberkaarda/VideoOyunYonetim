using System;
using System.Data;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace VideoGameManager
{
    public partial class ReviewGameForm : VideoGameManager.UI.Controls.ChromelessForm
    {
        public ReviewGameForm()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cmbGames.SelectedIndex <= 0)
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

            // A DropDownList with no selection paints its whole item area with the
            // selection colour once it takes focus, which reads as a broken control. The
            // placeholder at index 0 keeps something selected, and btnSave_Click rejects
            // it, so nothing can be saved against it.
            cmbGames.Items.Add("Select a game");

            foreach (DataRow row in dt.Rows)
            {
                cmbGames.Items.Add(row["Name"].ToString());
            }

            cmbGames.SelectedIndex = 0;
        }
    }
}
