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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
        }

        private void btnOyunEkle_Click(object sender, EventArgs e)
        {
            new OyunEkleForm().ShowDialog();
        }

        private void btnListele_Click(object sender, EventArgs e)
        {
            new OyunListeleForm().ShowDialog();
        }

        private void btnOneri_Click(object sender, EventArgs e)
        {
           OyunOneriForm oyunOneriForm = new OyunOneriForm();
            oyunOneriForm.ShowDialog();
        }

        private void btnCikis_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ControlBox = false;
            this.MaximizeBox = false; // Maksimize butonunu gizle
            this.MinimizeBox = false; // Minimize butonunu gizle

        }

        private void btnkapat_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnasagial_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnDegerlendir_Click(object sender, EventArgs e)
        {
            OyunDegerlendirForm oyunDegerlendirForm = new OyunDegerlendirForm();
            oyunDegerlendirForm.ShowDialog();

        }
    }
}
