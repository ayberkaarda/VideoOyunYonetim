using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoOyunY
{
    public partial class OyunListeleForm : Form
    {
        public OyunListeleForm()
        {
            InitializeComponent();
        }
        private void OyunlariListele()
        {
            lstBoxOyunlar.Items.Clear(); // Önce liste temizlenir

            string query = "SELECT Ad FROM Oyunlar";
            DataTable oyunlar = DatabaseHelper.ExecuteQuery(query);

            foreach (DataRow row in oyunlar.Rows)
            {
                lstBoxOyunlar.Items.Add(row["Ad"].ToString());
                
            }
        }

        private void btnCikis_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OyunListeleForm_Load(object sender, EventArgs e)
        {
            OyunlariListele();
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage; // Resimi kutuya sığdırmak için
            lblAd.Text = "";
            lblTur.Text = "";
            lblPlatform.Text = "";
            lblPuan.Text = "";
            lblYorum.Text = "";
        }

        private void lstBoxOyunlar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstBoxOyunlar.SelectedItem != null)
            {
                string secilenOyun = lstBoxOyunlar.SelectedItem.ToString();

                string query = "SELECT * FROM Oyunlar WHERE Ad = @Ad";
                DataTable table = DatabaseHelper.ExecuteQuery(query, new SqlParameter("@Ad", secilenOyun));

                if (table.Rows.Count > 0)
                {
                    DataRow row = table.Rows[0];

                    lblAd.Text = row["Ad"].ToString();
                    lblTur.Text = row["Tur"].ToString();
                    lblPlatform.Text = row["Platform"].ToString();
                    lblPuan.Text = row["Puan"].ToString();
                    lblYorum.Text = row["Yorum"].ToString();

                    // Resmi de yüklüyorsan:
                    try
                    {
                        pictureBox1.Load(row["ResimLink"].ToString());
                    }
                    catch
                    {
                        pictureBox1.Image = null;
                    }
                }
            }
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
