namespace VideoOyunY
{
    partial class OyunListeleForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OyunListeleForm));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnCikis = new System.Windows.Forms.Button();
            this.lstBoxOyunlar = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lblPuan = new System.Windows.Forms.Label();
            this.lblPlatform = new System.Windows.Forms.Label();
            this.lblTur = new System.Windows.Forms.Label();
            this.lblAd = new System.Windows.Forms.Label();
            this.btnasagial = new System.Windows.Forms.Button();
            this.btnkapat = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.lblYorum = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(408, 36);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(303, 366);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label1.Location = new System.Drawing.Point(792, 36);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 21);
            this.label1.TabIndex = 2;
            this.label1.Text = "Ad:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label2.Location = new System.Drawing.Point(787, 76);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 21);
            this.label2.TabIndex = 3;
            this.label2.Text = "Tür:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label3.Location = new System.Drawing.Point(741, 113);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 21);
            this.label3.TabIndex = 4;
            this.label3.Text = "Platform:";
            // 
            // btnCikis
            // 
            this.btnCikis.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.btnCikis.FlatAppearance.BorderSize = 0;
            this.btnCikis.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCikis.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.btnCikis.ForeColor = System.Drawing.Color.White;
            this.btnCikis.Location = new System.Drawing.Point(1070, 369);
            this.btnCikis.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCikis.Name = "btnCikis";
            this.btnCikis.Size = new System.Drawing.Size(161, 39);
            this.btnCikis.TabIndex = 5;
            this.btnCikis.Text = "Çıkış";
            this.btnCikis.UseVisualStyleBackColor = false;
            this.btnCikis.Click += new System.EventHandler(this.btnCikis_Click);
            // 
            // lstBoxOyunlar
            // 
            this.lstBoxOyunlar.FormattingEnabled = true;
            this.lstBoxOyunlar.ItemHeight = 16;
            this.lstBoxOyunlar.Location = new System.Drawing.Point(16, 36);
            this.lstBoxOyunlar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lstBoxOyunlar.Name = "lstBoxOyunlar";
            this.lstBoxOyunlar.Size = new System.Drawing.Size(313, 372);
            this.lstBoxOyunlar.TabIndex = 6;
            this.lstBoxOyunlar.SelectedIndexChanged += new System.EventHandler(this.lstBoxOyunlar_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label4.Location = new System.Drawing.Point(769, 151);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 21);
            this.label4.TabIndex = 7;
            this.label4.Text = "Puan:";
            // 
            // lblPuan
            // 
            this.lblPuan.AutoSize = true;
            this.lblPuan.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblPuan.Location = new System.Drawing.Point(837, 151);
            this.lblPuan.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPuan.Name = "lblPuan";
            this.lblPuan.Size = new System.Drawing.Size(48, 21);
            this.lblPuan.TabIndex = 11;
            this.lblPuan.Text = "Puan:";
            // 
            // lblPlatform
            // 
            this.lblPlatform.AutoSize = true;
            this.lblPlatform.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblPlatform.Location = new System.Drawing.Point(837, 114);
            this.lblPlatform.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPlatform.Name = "lblPlatform";
            this.lblPlatform.Size = new System.Drawing.Size(73, 21);
            this.lblPlatform.TabIndex = 10;
            this.lblPlatform.Text = "Platform:";
            // 
            // lblTur
            // 
            this.lblTur.AutoSize = true;
            this.lblTur.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblTur.Location = new System.Drawing.Point(837, 79);
            this.lblTur.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTur.Name = "lblTur";
            this.lblTur.Size = new System.Drawing.Size(36, 21);
            this.lblTur.TabIndex = 9;
            this.lblTur.Text = "Tür:";
            // 
            // lblAd
            // 
            this.lblAd.AutoSize = true;
            this.lblAd.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblAd.Location = new System.Drawing.Point(843, 38);
            this.lblAd.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblAd.Name = "lblAd";
            this.lblAd.Size = new System.Drawing.Size(32, 21);
            this.lblAd.TabIndex = 8;
            this.lblAd.Text = "Ad:";
            // 
            // btnasagial
            // 
            this.btnasagial.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.btnasagial.FlatAppearance.BorderSize = 0;
            this.btnasagial.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnasagial.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnasagial.ForeColor = System.Drawing.Color.White;
            this.btnasagial.Location = new System.Drawing.Point(1107, -1);
            this.btnasagial.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnasagial.Name = "btnasagial";
            this.btnasagial.Size = new System.Drawing.Size(77, 33);
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
            this.btnkapat.Location = new System.Drawing.Point(1181, -1);
            this.btnkapat.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnkapat.Name = "btnkapat";
            this.btnkapat.Size = new System.Drawing.Size(77, 33);
            this.btnkapat.TabIndex = 23;
            this.btnkapat.Text = "X";
            this.btnkapat.UseVisualStyleBackColor = false;
            this.btnkapat.Click += new System.EventHandler(this.btnkapat_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label5.Location = new System.Drawing.Point(719, 190);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(130, 21);
            this.label5.TabIndex = 25;
            this.label5.Text = "Kullanıcı Yorumu:";
            // 
            // lblYorum
            // 
            this.lblYorum.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblYorum.Location = new System.Drawing.Point(894, 190);
            this.lblYorum.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblYorum.Name = "lblYorum";
            this.lblYorum.Size = new System.Drawing.Size(325, 168);
            this.lblYorum.TabIndex = 26;
            this.lblYorum.Text = "Yorum";
            // 
            // OyunListeleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(1259, 434);
            this.Controls.Add(this.lblYorum);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnasagial);
            this.Controls.Add(this.btnkapat);
            this.Controls.Add(this.lblPuan);
            this.Controls.Add(this.lblPlatform);
            this.Controls.Add(this.lblTur);
            this.Controls.Add(this.lblAd);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lstBoxOyunlar);
            this.Controls.Add(this.btnCikis);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "OyunListeleForm";
            this.Text = "Detaylı Oyun | Oyun Listele";
            this.Load += new System.EventHandler(this.OyunListeleForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnCikis;
        private System.Windows.Forms.ListBox lstBoxOyunlar;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblPuan;
        private System.Windows.Forms.Label lblPlatform;
        private System.Windows.Forms.Label lblTur;
        private System.Windows.Forms.Label lblAd;
        private System.Windows.Forms.Button btnasagial;
        private System.Windows.Forms.Button btnkapat;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblYorum;
    }
}