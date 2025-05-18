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
    public partial class OyunEkleForm : Form
    {
        public OyunEkleForm()
        {
            InitializeComponent();
        }

        private void btnKaydet_Click(object sender, EventArgs e)
        {
            string ad = txtAd.Text.Trim();
            string tur = cmbTur.Text;
            string platform = cmbPlatform.Text;
            string puanStr = cmbPuan.Text;
            string resimLink = txtResimLink.Text.Trim();

            if (string.IsNullOrEmpty(ad))
            {
                MessageBox.Show("Oyun adı boş olamaz.");
                return;
            }

            if (tur == "Tür")
            {
                MessageBox.Show("Lütfen bir tür seçin.");
                return;
            }

            if (platform == "Platform")
            {
                MessageBox.Show("Lütfen bir platform seçin.");
                return;
            }

            if (puanStr == "Puan")
            {
                MessageBox.Show("Lütfen bir puan seçin.");
                return;
            }

            if (!float.TryParse(puanStr, out float puan))
            {
                MessageBox.Show("Puan geçerli bir sayı olmalıdır.");
                return;
            }

            string query = @"INSERT INTO Oyunlar (Ad, Tur, Platform, Puan, ResimLink)
                     VALUES (@Ad, @Tur, @Platform, @Puan, @ResimLink)";

            SqlParameter[] parameters = new SqlParameter[]
            {
        new SqlParameter("@Ad", ad),
        new SqlParameter("@Tur", tur),
        new SqlParameter("@Platform", platform),
        new SqlParameter("@Puan", puan),
        new SqlParameter("@ResimLink", resimLink)
            };

            try
            {
                DatabaseHelper.ExecuteNonQuery(query, parameters);
                MessageBox.Show("Oyun başarıyla eklendi.");

                // Temizle ve varsayılanlara döndür
                txtAd.Clear();
                txtResimLink.Clear();
                cmbTur.SelectedIndex = 0;
                cmbPlatform.SelectedIndex = 0;
                cmbPuan.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata oluştu: " + ex.Message);
            }
        }

        private void OyunEkleForm_Load(object sender, EventArgs e)
        {
            cmbPlatform.Items.Clear();
            cmbPlatform.Items.Add("Platform"); // Varsayılan metin
            cmbPlatform.Items.AddRange(new string[] { "PC", "PlayStation", "Xbox", "Switch" });
            cmbPlatform.SelectedIndex = 0;

            cmbTur.Items.Clear();
            cmbTur.Items.Add("Tür"); // Varsayılan metin
            cmbTur.Items.AddRange(new string[] { "Aksiyon", "Macera", "RPG", "Strateji", "Spor", "Yarış", "Simülasyon", "Korku", "Bulmaca", "Platform" });
            cmbTur.SelectedIndex = 0;

            cmbPuan.Items.Clear();
            cmbPuan.Items.Add("Puan"); // Varsayılan metin
            for (int i = 1; i <= 10; i++)
            {
                cmbPuan.Items.Add(i.ToString());
            }
            cmbPuan.SelectedIndex = 0;
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
