namespace VideoGameManager
{
    partial class ReviewGameForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReviewGameForm));
            this.btnMinimize = new System.Windows.Forms.Button();
            this.btnCloseWindow = new System.Windows.Forms.Button();
            this.cmbGames = new System.Windows.Forms.ComboBox();
            this.txtComment = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.lblCommentCaption = new System.Windows.Forms.Label();
            this.lblGameCaption = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnMinimize
            // 
            this.btnMinimize.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.btnMinimize.FlatAppearance.BorderSize = 0;
            this.btnMinimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMinimize.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnMinimize.ForeColor = System.Drawing.Color.White;
            this.btnMinimize.Location = new System.Drawing.Point(736, -2);
            this.btnMinimize.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Size = new System.Drawing.Size(77, 33);
            this.btnMinimize.TabIndex = 11;
            this.btnMinimize.Text = "-";
            this.btnMinimize.UseVisualStyleBackColor = false;
            this.btnMinimize.Click += new System.EventHandler(this.btnMinimize_Click);
            // 
            // btnCloseWindow
            // 
            this.btnCloseWindow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.btnCloseWindow.FlatAppearance.BorderSize = 0;
            this.btnCloseWindow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCloseWindow.ForeColor = System.Drawing.Color.White;
            this.btnCloseWindow.Location = new System.Drawing.Point(811, -2);
            this.btnCloseWindow.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCloseWindow.Name = "btnCloseWindow";
            this.btnCloseWindow.Size = new System.Drawing.Size(77, 33);
            this.btnCloseWindow.TabIndex = 10;
            this.btnCloseWindow.Text = "X";
            this.btnCloseWindow.UseVisualStyleBackColor = false;
            this.btnCloseWindow.Click += new System.EventHandler(this.btnCloseWindow_Click);
            // 
            // cmbGames
            // 
            this.cmbGames.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbGames.FormattingEnabled = true;
            this.cmbGames.Location = new System.Drawing.Point(403, 159);
            this.cmbGames.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cmbGames.Name = "cmbGames";
            this.cmbGames.Size = new System.Drawing.Size(160, 24);
            this.cmbGames.TabIndex = 12;
            // 
            // txtComment
            // 
            this.txtComment.Location = new System.Drawing.Point(319, 191);
            this.txtComment.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtComment.Multiline = true;
            this.txtComment.Name = "txtComment";
            this.txtComment.Size = new System.Drawing.Size(329, 74);
            this.txtComment.TabIndex = 14;
            // 
            // btnSave
            // 
            this.btnSave.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.btnSave.Location = new System.Drawing.Point(413, 273);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(161, 39);
            this.btnSave.TabIndex = 15;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // lblCommentCaption
            // 
            this.lblCommentCaption.AutoSize = true;
            this.lblCommentCaption.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblCommentCaption.Location = new System.Drawing.Point(215, 191);
            this.lblCommentCaption.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCommentCaption.Name = "lblCommentCaption";
            this.lblCommentCaption.Size = new System.Drawing.Size(72, 17);
            this.lblCommentCaption.TabIndex = 16;
            this.lblCommentCaption.Text = "Your Review";
            // 
            // lblGameCaption
            // 
            this.lblGameCaption.AutoSize = true;
            this.lblGameCaption.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblGameCaption.Location = new System.Drawing.Point(316, 161);
            this.lblGameCaption.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblGameCaption.Name = "lblGameCaption";
            this.lblGameCaption.Size = new System.Drawing.Size(62, 17);
            this.lblGameCaption.TabIndex = 17;
            this.lblGameCaption.Text = "Select Game";
            this.lblGameCaption.Click += new System.EventHandler(this.label2_Click);
            // 
            // ReviewGameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::VideoGameManager.Properties.Resources._30th_anniversary_hub_background_desktop_04_en_02oct24;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(887, 458);
            this.Controls.Add(this.lblGameCaption);
            this.Controls.Add(this.lblCommentCaption);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtComment);
            this.Controls.Add(this.cmbGames);
            this.Controls.Add(this.btnMinimize);
            this.Controls.Add(this.btnCloseWindow);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "ReviewGameForm";
            this.Text = "Video Game Manager | Review Game";
            this.Load += new System.EventHandler(this.ReviewGameForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnMinimize;
        private System.Windows.Forms.Button btnCloseWindow;
        private System.Windows.Forms.ComboBox cmbGames;
        private System.Windows.Forms.TextBox txtComment;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblCommentCaption;
        private System.Windows.Forms.Label lblGameCaption;
    }
}