namespace VideoOyunY
{
    partial class OyunDegerlendirForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OyunDegerlendirForm));
            this.btnasagial = new System.Windows.Forms.Button();
            this.btnkapat = new System.Windows.Forms.Button();
            this.cmbOyunlar = new System.Windows.Forms.ComboBox();
            this.txtYorum = new System.Windows.Forms.TextBox();
            this.btnKaydet = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnasagial
            // 
            this.btnasagial.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.btnasagial.FlatAppearance.BorderSize = 0;
            this.btnasagial.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnasagial.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnasagial.ForeColor = System.Drawing.Color.White;
            this.btnasagial.Location = new System.Drawing.Point(736, -2);
            this.btnasagial.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnasagial.Name = "btnasagial";
            this.btnasagial.Size = new System.Drawing.Size(77, 33);
            this.btnasagial.TabIndex = 11;
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
            this.btnkapat.Location = new System.Drawing.Point(811, -2);
            this.btnkapat.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnkapat.Name = "btnkapat";
            this.btnkapat.Size = new System.Drawing.Size(77, 33);
            this.btnkapat.TabIndex = 10;
            this.btnkapat.Text = "X";
            this.btnkapat.UseVisualStyleBackColor = false;
            this.btnkapat.Click += new System.EventHandler(this.btnkapat_Click);
            // 
            // cmbOyunlar
            // 
            this.cmbOyunlar.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOyunlar.FormattingEnabled = true;
            this.cmbOyunlar.Location = new System.Drawing.Point(403, 159);
            this.cmbOyunlar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cmbOyunlar.Name = "cmbOyunlar";
            this.cmbOyunlar.Size = new System.Drawing.Size(160, 24);
            this.cmbOyunlar.TabIndex = 12;
            // 
            // txtYorum
            // 
            this.txtYorum.Location = new System.Drawing.Point(319, 191);
            this.txtYorum.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtYorum.Multiline = true;
            this.txtYorum.Name = "txtYorum";
            this.txtYorum.Size = new System.Drawing.Size(329, 74);
            this.txtYorum.TabIndex = 14;
            // 
            // btnKaydet
            // 
            this.btnKaydet.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.btnKaydet.Location = new System.Drawing.Point(413, 273);
            this.btnKaydet.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnKaydet.Name = "btnKaydet";
            this.btnKaydet.Size = new System.Drawing.Size(161, 39);
            this.btnKaydet.TabIndex = 15;
            this.btnKaydet.Text = "Kaydet";
            this.btnKaydet.UseVisualStyleBackColor = true;
            this.btnKaydet.Click += new System.EventHandler(this.btnKaydet_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label1.Location = new System.Drawing.Point(215, 191);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 17);
            this.label1.TabIndex = 16;
            this.label1.Text = "Yorum Ekle";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label2.Location = new System.Drawing.Point(316, 161);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 17);
            this.label2.TabIndex = 17;
            this.label2.Text = "Oyun Seç";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // OyunDegerlendirForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::VideoOyunY.Properties.Resources._30th_anniversary_hub_background_desktop_04_en_02oct24;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(887, 458);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnKaydet);
            this.Controls.Add(this.txtYorum);
            this.Controls.Add(this.cmbOyunlar);
            this.Controls.Add(this.btnasagial);
            this.Controls.Add(this.btnkapat);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "OyunDegerlendirForm";
            this.Text = "Detaylı Oyun | Değerlendir";
            this.Load += new System.EventHandler(this.OyunDegerlendirForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnasagial;
        private System.Windows.Forms.Button btnkapat;
        private System.Windows.Forms.ComboBox cmbOyunlar;
        private System.Windows.Forms.TextBox txtYorum;
        private System.Windows.Forms.Button btnKaydet;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}