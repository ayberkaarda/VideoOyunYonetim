namespace VideoOyunY
{
    partial class OyunEkleForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OyunEkleForm));
            this.label1 = new System.Windows.Forms.Label();
            this.btnKaydet = new System.Windows.Forms.Button();
            this.cmbPlatform = new System.Windows.Forms.ComboBox();
            this.cmbTur = new System.Windows.Forms.ComboBox();
            this.txtAd = new System.Windows.Forms.TextBox();
            this.txtResimLink = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbPuan = new System.Windows.Forms.ComboBox();
            this.btnasagial = new System.Windows.Forms.Button();
            this.btnkapat = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.4F);
            this.label1.Location = new System.Drawing.Point(130, 102);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 15);
            this.label1.TabIndex = 9;
            this.label1.Text = "Oyun ismi Girin";
            // 
            // btnKaydet
            // 
            this.btnKaydet.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.btnKaydet.Location = new System.Drawing.Point(294, 188);
            this.btnKaydet.Name = "btnKaydet";
            this.btnKaydet.Size = new System.Drawing.Size(121, 32);
            this.btnKaydet.TabIndex = 8;
            this.btnKaydet.Text = "Kaydet";
            this.btnKaydet.UseVisualStyleBackColor = true;
            this.btnKaydet.Click += new System.EventHandler(this.btnKaydet_Click);
            // 
            // cmbPlatform
            // 
            this.cmbPlatform.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPlatform.FormattingEnabled = true;
            this.cmbPlatform.Items.AddRange(new object[] {
            "PC",
            "Mobil",
            "Tablet",
            "XBOX",
            "PSP",
            "PS3",
            "PS4",
            "PS5"});
            this.cmbPlatform.Location = new System.Drawing.Point(158, 130);
            this.cmbPlatform.Name = "cmbPlatform";
            this.cmbPlatform.Size = new System.Drawing.Size(121, 21);
            this.cmbPlatform.TabIndex = 7;
            // 
            // cmbTur
            // 
            this.cmbTur.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTur.FormattingEnabled = true;
            this.cmbTur.Location = new System.Drawing.Point(285, 130);
            this.cmbTur.Name = "cmbTur";
            this.cmbTur.Size = new System.Drawing.Size(121, 21);
            this.cmbTur.TabIndex = 6;
            // 
            // txtAd
            // 
            this.txtAd.Location = new System.Drawing.Point(226, 99);
            this.txtAd.MaxLength = 107;
            this.txtAd.Name = "txtAd";
            this.txtAd.Size = new System.Drawing.Size(248, 20);
            this.txtAd.TabIndex = 5;
            // 
            // txtResimLink
            // 
            this.txtResimLink.Location = new System.Drawing.Point(226, 160);
            this.txtResimLink.Name = "txtResimLink";
            this.txtResimLink.Size = new System.Drawing.Size(248, 20);
            this.txtResimLink.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.4F);
            this.label2.Location = new System.Drawing.Point(148, 163);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 15);
            this.label2.TabIndex = 11;
            this.label2.Text = "Resim Linki";
            // 
            // cmbPuan
            // 
            this.cmbPuan.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPuan.FormattingEnabled = true;
            this.cmbPuan.Location = new System.Drawing.Point(412, 130);
            this.cmbPuan.Name = "cmbPuan";
            this.cmbPuan.Size = new System.Drawing.Size(121, 21);
            this.cmbPuan.TabIndex = 12;
            // 
            // btnasagial
            // 
            this.btnasagial.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.btnasagial.FlatAppearance.BorderSize = 0;
            this.btnasagial.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnasagial.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnasagial.ForeColor = System.Drawing.Color.White;
            this.btnasagial.Location = new System.Drawing.Point(569, 0);
            this.btnasagial.Name = "btnasagial";
            this.btnasagial.Size = new System.Drawing.Size(58, 27);
            this.btnasagial.TabIndex = 24;
            this.btnasagial.Text = "-";
            this.btnasagial.UseVisualStyleBackColor = false;
            this.btnasagial.Click += new System.EventHandler(this.btnasagial_Click);
            // 
            // btnkapat
            // 
            this.btnkapat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.btnkapat.FlatAppearance.BorderSize = 0;
            this.btnkapat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnkapat.ForeColor = System.Drawing.Color.White;
            this.btnkapat.Location = new System.Drawing.Point(624, 0);
            this.btnkapat.Name = "btnkapat";
            this.btnkapat.Size = new System.Drawing.Size(58, 27);
            this.btnkapat.TabIndex = 23;
            this.btnkapat.Text = "X";
            this.btnkapat.UseVisualStyleBackColor = false;
            this.btnkapat.Click += new System.EventHandler(this.btnkapat_Click);
            // 
            // OyunEkleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(681, 345);
            this.Controls.Add(this.btnasagial);
            this.Controls.Add(this.btnkapat);
            this.Controls.Add(this.cmbPuan);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtResimLink);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnKaydet);
            this.Controls.Add(this.cmbPlatform);
            this.Controls.Add(this.cmbTur);
            this.Controls.Add(this.txtAd);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OyunEkleForm";
            this.Text = "Detaylı Oyun | Oyun Ekle";
            this.Load += new System.EventHandler(this.OyunEkleForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnKaydet;
        private System.Windows.Forms.ComboBox cmbPlatform;
        private System.Windows.Forms.ComboBox cmbTur;
        private System.Windows.Forms.TextBox txtAd;
        private System.Windows.Forms.TextBox txtResimLink;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbPuan;
        private System.Windows.Forms.Button btnasagial;
        private System.Windows.Forms.Button btnkapat;
    }
}