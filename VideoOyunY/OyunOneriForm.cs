using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoOyunY
{
    public partial class OyunOneriForm : Form
    {
        public OyunOneriForm()
        {
            InitializeComponent();
        }

        private void btnOneri_Click(object sender, EventArgs e)
        {
            string query = "SELECT TOP 1 * FROM Oyunlar ORDER BY NEWID()";
            DataTable table = DatabaseHelper.ExecuteQuery(query);

            DataRow row = null; // row'u if dışında tanımla

            if (table.Rows.Count > 0)
            {
                row = table.Rows[0];

                lblAd.Text = row["Ad"].ToString();
                lblTur.Text = row["Tur"].ToString();
                lblPlatform.Text = row["Platform"].ToString();
                lblPuan.Text = row["Puan"].ToString();
            }
            else
            {
                MessageBox.Show("Veritabanında oyun bulunamadı.");
                return; // devam etmesin
            }

            try
            {
                pictureBox1.Load(row["ResimLink"].ToString());
            }
            catch
            {
                pictureBox1.Image = null;
            }
        }

        private void OyunOneriForm_Load(object sender, EventArgs e)
        {
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void btnkapat_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnasagial_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}
