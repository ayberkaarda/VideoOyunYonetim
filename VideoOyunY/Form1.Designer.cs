namespace VideoOyunY
{
    partial class Form1
    {
        /// <summary>
        ///Gerekli tasarımcı değişkeni.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///Kullanılan tüm kaynakları temizleyin.
        /// </summary>
        ///<param name="disposing">yönetilen kaynaklar dispose edilmeliyse doğru; aksi halde yanlış.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer üretilen kod

        /// <summary>
        /// Tasarımcı desteği için gerekli metot - bu metodun 
        ///içeriğini kod düzenleyici ile değiştirmeyin.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnCikis = new System.Windows.Forms.Button();
            this.btnOyunEkle = new System.Windows.Forms.Button();
            this.btnListele = new System.Windows.Forms.Button();
            this.btnOneri = new System.Windows.Forms.Button();
            this.btnkapat = new System.Windows.Forms.Button();
            this.btnasagial = new System.Windows.Forms.Button();
            this.btnDegerlendir = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnCikis
            // 
            this.btnCikis.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.btnCikis.FlatAppearance.BorderSize = 0;
            this.btnCikis.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCikis.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.btnCikis.ForeColor = System.Drawing.Color.White;
            this.btnCikis.Location = new System.Drawing.Point(220, 274);
            this.btnCikis.Name = "btnCikis";
            this.btnCikis.Size = new System.Drawing.Size(121, 32);
            this.btnCikis.TabIndex = 3;
            this.btnCikis.Text = "Çıkış";
            this.btnCikis.UseVisualStyleBackColor = false;
            this.btnCikis.Click += new System.EventHandler(this.btnCikis_Click);
            // 
            // btnOyunEkle
            // 
            this.btnOyunEkle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(158)))), ((int)(((byte)(255)))));
            this.btnOyunEkle.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOyunEkle.FlatAppearance.BorderSize = 0;
            this.btnOyunEkle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOyunEkle.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.btnOyunEkle.ForeColor = System.Drawing.Color.White;
            this.btnOyunEkle.Location = new System.Drawing.Point(86, 50);
            this.btnOyunEkle.Name = "btnOyunEkle";
            this.btnOyunEkle.Size = new System.Drawing.Size(162, 56);
            this.btnOyunEkle.TabIndex = 4;
            this.btnOyunEkle.Text = "Oyun Ekle";
            this.btnOyunEkle.UseVisualStyleBackColor = false;
            this.btnOyunEkle.Click += new System.EventHandler(this.btnOyunEkle_Click);
            // 
            // btnListele
            // 
            this.btnListele.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(108)))), ((int)(((byte)(108)))));
            this.btnListele.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnListele.FlatAppearance.BorderSize = 0;
            this.btnListele.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnListele.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.btnListele.ForeColor = System.Drawing.Color.White;
            this.btnListele.Location = new System.Drawing.Point(317, 50);
            this.btnListele.Name = "btnListele";
            this.btnListele.Size = new System.Drawing.Size(162, 56);
            this.btnListele.TabIndex = 5;
            this.btnListele.Text = "Oyunları Listele";
            this.btnListele.UseVisualStyleBackColor = false;
            this.btnListele.Click += new System.EventHandler(this.btnListele_Click);
            // 
            // btnOneri
            // 
            this.btnOneri.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(213)))), ((int)(((byte)(64)))));
            this.btnOneri.FlatAppearance.BorderSize = 0;
            this.btnOneri.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOneri.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.btnOneri.ForeColor = System.Drawing.Color.White;
            this.btnOneri.Location = new System.Drawing.Point(86, 163);
            this.btnOneri.Name = "btnOneri";
            this.btnOneri.Size = new System.Drawing.Size(162, 56);
            this.btnOneri.TabIndex = 7;
            this.btnOneri.Text = "Oyun Öneri";
            this.btnOneri.UseVisualStyleBackColor = false;
            this.btnOneri.Click += new System.EventHandler(this.btnOneri_Click);
            // 
            // btnkapat
            // 
            this.btnkapat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.btnkapat.FlatAppearance.BorderSize = 0;
            this.btnkapat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnkapat.ForeColor = System.Drawing.Color.White;
            this.btnkapat.Location = new System.Drawing.Point(515, 0);
            this.btnkapat.Name = "btnkapat";
            this.btnkapat.Size = new System.Drawing.Size(58, 27);
            this.btnkapat.TabIndex = 8;
            this.btnkapat.Text = "X";
            this.btnkapat.UseVisualStyleBackColor = false;
            this.btnkapat.Click += new System.EventHandler(this.btnkapat_Click);
            // 
            // btnasagial
            // 
            this.btnasagial.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.btnasagial.FlatAppearance.BorderSize = 0;
            this.btnasagial.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnasagial.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnasagial.ForeColor = System.Drawing.Color.White;
            this.btnasagial.Location = new System.Drawing.Point(459, 0);
            this.btnasagial.Name = "btnasagial";
            this.btnasagial.Size = new System.Drawing.Size(58, 27);
            this.btnasagial.TabIndex = 9;
            this.btnasagial.Text = "-";
            this.btnasagial.UseVisualStyleBackColor = false;
            this.btnasagial.Click += new System.EventHandler(this.btnasagial_Click);
            // 
            // btnDegerlendir
            // 
            this.btnDegerlendir.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(232)))), ((int)(((byte)(68)))));
            this.btnDegerlendir.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDegerlendir.FlatAppearance.BorderSize = 0;
            this.btnDegerlendir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDegerlendir.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.btnDegerlendir.ForeColor = System.Drawing.Color.White;
            this.btnDegerlendir.Location = new System.Drawing.Point(317, 163);
            this.btnDegerlendir.Name = "btnDegerlendir";
            this.btnDegerlendir.Size = new System.Drawing.Size(162, 56);
            this.btnDegerlendir.TabIndex = 10;
            this.btnDegerlendir.Text = "Oyun Değerlendir";
            this.btnDegerlendir.UseVisualStyleBackColor = false;
            this.btnDegerlendir.Click += new System.EventHandler(this.btnDegerlendir_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(572, 354);
            this.Controls.Add(this.btnDegerlendir);
            this.Controls.Add(this.btnasagial);
            this.Controls.Add(this.btnkapat);
            this.Controls.Add(this.btnOneri);
            this.Controls.Add(this.btnListele);
            this.Controls.Add(this.btnOyunEkle);
            this.Controls.Add(this.btnCikis);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Detaylı Oyun | Ana Menü";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnCikis;
        private System.Windows.Forms.Button btnOyunEkle;
        private System.Windows.Forms.Button btnListele;
        private System.Windows.Forms.Button btnOneri;
        private System.Windows.Forms.Button btnkapat;
        private System.Windows.Forms.Button btnasagial;
        private System.Windows.Forms.Button btnDegerlendir;
    }
}

