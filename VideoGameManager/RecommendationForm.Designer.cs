namespace VideoGameManager
{
    partial class RecommendationForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RecommendationForm));
            this.btnRecommend = new System.Windows.Forms.Button();
            this.lblScore = new System.Windows.Forms.Label();
            this.lblPlatform = new System.Windows.Forms.Label();
            this.lblGenre = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.lblScoreCaption = new System.Windows.Forms.Label();
            this.lblPlatformCaption = new System.Windows.Forms.Label();
            this.lblGenreCaption = new System.Windows.Forms.Label();
            this.lblNameCaption = new System.Windows.Forms.Label();
            this.picCover = new System.Windows.Forms.PictureBox();
            this.btnMinimize = new System.Windows.Forms.Button();
            this.btnCloseWindow = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picCover)).BeginInit();
            this.SuspendLayout();
            // 
            // btnRecommend
            // 
            this.btnRecommend.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.btnRecommend.Location = new System.Drawing.Point(383, 218);
            this.btnRecommend.Name = "btnRecommend";
            this.btnRecommend.Size = new System.Drawing.Size(121, 32);
            this.btnRecommend.TabIndex = 2;
            this.btnRecommend.Text = "Get Recommendation";
            this.btnRecommend.UseVisualStyleBackColor = true;
            this.btnRecommend.Click += new System.EventHandler(this.btnRecommend_Click);
            // 
            // lblScore
            // 
            this.lblScore.AutoSize = true;
            this.lblScore.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblScore.Location = new System.Drawing.Point(350, 126);
            this.lblScore.Name = "lblScore";
            this.lblScore.Size = new System.Drawing.Size(48, 21);
            this.lblScore.TabIndex = 20;
            this.lblScore.Text = "";
            // 
            // lblPlatform
            // 
            this.lblPlatform.AutoSize = true;
            this.lblPlatform.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblPlatform.Location = new System.Drawing.Point(350, 96);
            this.lblPlatform.Name = "lblPlatform";
            this.lblPlatform.Size = new System.Drawing.Size(73, 21);
            this.lblPlatform.TabIndex = 19;
            this.lblPlatform.Text = "";
            // 
            // lblGenre
            // 
            this.lblGenre.AutoSize = true;
            this.lblGenre.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblGenre.Location = new System.Drawing.Point(350, 67);
            this.lblGenre.Name = "lblGenre";
            this.lblGenre.Size = new System.Drawing.Size(36, 21);
            this.lblGenre.TabIndex = 18;
            this.lblGenre.Text = "";
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblName.Location = new System.Drawing.Point(350, 34);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(32, 21);
            this.lblName.TabIndex = 17;
            this.lblName.Text = "";
            // 
            // lblScoreCaption
            // 
            this.lblScoreCaption.AutoSize = false;
            this.lblScoreCaption.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblScoreCaption.Location = new System.Drawing.Point(262, 126);
            this.lblScoreCaption.Name = "lblScoreCaption";
            this.lblScoreCaption.Size = new System.Drawing.Size(80, 21);
            this.lblScoreCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblScoreCaption.TabIndex = 16;
            this.lblScoreCaption.Text = "Score:";
            // 
            // lblPlatformCaption
            // 
            this.lblPlatformCaption.AutoSize = false;
            this.lblPlatformCaption.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblPlatformCaption.Location = new System.Drawing.Point(262, 95);
            this.lblPlatformCaption.Name = "lblPlatformCaption";
            this.lblPlatformCaption.Size = new System.Drawing.Size(80, 21);
            this.lblPlatformCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblPlatformCaption.TabIndex = 15;
            this.lblPlatformCaption.Text = "Platform:";
            // 
            // lblGenreCaption
            // 
            this.lblGenreCaption.AutoSize = false;
            this.lblGenreCaption.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblGenreCaption.Location = new System.Drawing.Point(262, 65);
            this.lblGenreCaption.Name = "lblGenreCaption";
            this.lblGenreCaption.Size = new System.Drawing.Size(80, 21);
            this.lblGenreCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblGenreCaption.TabIndex = 14;
            this.lblGenreCaption.Text = "Genre:";
            // 
            // lblNameCaption
            // 
            this.lblNameCaption.AutoSize = false;
            this.lblNameCaption.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblNameCaption.Location = new System.Drawing.Point(262, 32);
            this.lblNameCaption.Name = "lblNameCaption";
            this.lblNameCaption.Size = new System.Drawing.Size(80, 21);
            this.lblNameCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblNameCaption.TabIndex = 13;
            this.lblNameCaption.Text = "Name:";
            // 
            // picCover
            // 
            this.picCover.Location = new System.Drawing.Point(21, 32);
            this.picCover.Name = "picCover";
            this.picCover.Size = new System.Drawing.Size(227, 297);
            this.picCover.TabIndex = 12;
            this.picCover.TabStop = false;
            // 
            // btnMinimize
            // 
            this.btnMinimize.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.btnMinimize.FlatAppearance.BorderSize = 0;
            this.btnMinimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMinimize.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnMinimize.ForeColor = System.Drawing.Color.White;
            this.btnMinimize.Location = new System.Drawing.Point(532, 0);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Size = new System.Drawing.Size(58, 27);
            this.btnMinimize.TabIndex = 22;
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
            this.btnCloseWindow.Location = new System.Drawing.Point(587, 0);
            this.btnCloseWindow.Name = "btnCloseWindow";
            this.btnCloseWindow.Size = new System.Drawing.Size(58, 27);
            this.btnCloseWindow.TabIndex = 21;
            this.btnCloseWindow.Text = "X";
            this.btnCloseWindow.UseVisualStyleBackColor = false;
            this.btnCloseWindow.Click += new System.EventHandler(this.btnCloseWindow_Click);
            // 
            // RecommendationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(645, 450);
            this.Controls.Add(this.btnMinimize);
            this.Controls.Add(this.btnCloseWindow);
            this.Controls.Add(this.lblScore);
            this.Controls.Add(this.lblPlatform);
            this.Controls.Add(this.lblGenre);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.lblScoreCaption);
            this.Controls.Add(this.lblPlatformCaption);
            this.Controls.Add(this.lblGenreCaption);
            this.Controls.Add(this.lblNameCaption);
            this.Controls.Add(this.picCover);
            this.Controls.Add(this.btnRecommend);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RecommendationForm";
            this.Text = "Video Game Manager | Recommendation";
            this.Load += new System.EventHandler(this.RecommendationForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picCover)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnRecommend;
        private System.Windows.Forms.Label lblScore;
        private System.Windows.Forms.Label lblPlatform;
        private System.Windows.Forms.Label lblGenre;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblScoreCaption;
        private System.Windows.Forms.Label lblPlatformCaption;
        private System.Windows.Forms.Label lblGenreCaption;
        private System.Windows.Forms.Label lblNameCaption;
        private System.Windows.Forms.PictureBox picCover;
        private System.Windows.Forms.Button btnMinimize;
        private System.Windows.Forms.Button btnCloseWindow;
    }
}