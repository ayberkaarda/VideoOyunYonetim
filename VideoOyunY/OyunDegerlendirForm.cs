using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace VideoOyunY
{
    public partial class OyunDegerlendirForm : Form
    {
        public OyunDegerlendirForm()
        {
            InitializeComponent();
            
        }
       
        private void btnKaydet_Click(object sender, EventArgs e)
        {
            if (cmbOyunlar.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir oyun seçin.");
                return;
            }

            string secilenOyun = cmbOyunlar.Text; // ComboBox bağlıysa doğrudan .Text kullanılabilir
            string yorum = txtYorum.Text;

            string query = "UPDATE Oyunlar SET Yorum = @yorum WHERE Ad = @ad";
            SqlParameter[] parameters = {
        new SqlParameter("@yorum", yorum),
        new SqlParameter("@ad", secilenOyun)
    };

            try
            {
                DatabaseHelper.ExecuteNonQuery(query, parameters);
                MessageBox.Show("Yorum başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }

        }

        private void OyunDegerlendirForm_Load(object sender, EventArgs e)
        {
            string query = "SELECT Ad FROM Oyunlar ORDER BY Ad ASC";
            DataTable dt = DatabaseHelper.ExecuteQuery(query);

            cmbOyunlar.Items.Clear();
            foreach (DataRow row in dt.Rows)
            {
                cmbOyunlar.Items.Add(row["Ad"].ToString());
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void btnkapat_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnasagial_Click(object sender, EventArgs e)
        {
            
        }
    }
}

