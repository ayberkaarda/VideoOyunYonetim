namespace VideoGameManager
{
    partial class BrowseGamesForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BrowseGamesForm));
            this.picCover = new System.Windows.Forms.PictureBox();
            this.lblNameCaption = new System.Windows.Forms.Label();
            this.lblGenreCaption = new System.Windows.Forms.Label();
            this.lblPlatformCaption = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.lstGames = new System.Windows.Forms.ListBox();
            this.lblScoreCaption = new System.Windows.Forms.Label();
            this.lblScore = new System.Windows.Forms.Label();
            this.lblPlatform = new System.Windows.Forms.Label();
            this.lblGenre = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.btnMinimize = new System.Windows.Forms.Button();
            this.btnCloseWindow = new System.Windows.Forms.Button();
            this.lblCommentCaption = new System.Windows.Forms.Label();
            this.lblComment = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picCover)).BeginInit();
            this.SuspendLayout();
            // 
            // picCover
            // 
            this.picCover.Location = new System.Drawing.Point(408, 36);
            this.picCover.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.picCover.Name = "picCover";
            this.picCover.Size = new System.Drawing.Size(303, 366);
            this.picCover.TabIndex = 1;
            this.picCover.TabStop = false;
            // 
            // lblNameCaption
            // 
            this.lblNameCaption.AutoSize = false;
            this.lblNameCaption.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblNameCaption.Location = new System.Drawing.Point(715, 36);
            this.lblNameCaption.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblNameCaption.Name = "lblNameCaption";
            this.lblNameCaption.Size = new System.Drawing.Size(110, 21);
            this.lblNameCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblNameCaption.TabIndex = 2;
            this.lblNameCaption.Text = "Name:";
            // 
            // lblGenreCaption
            // 
            this.lblGenreCaption.AutoSize = false;
            this.lblGenreCaption.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblGenreCaption.Location = new System.Drawing.Point(715, 76);
            this.lblGenreCaption.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblGenreCaption.Name = "lblGenreCaption";
            this.lblGenreCaption.Size = new System.Drawing.Size(110, 21);
            this.lblGenreCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblGenreCaption.TabIndex = 3;
            this.lblGenreCaption.Text = "Genre:";
            // 
            // lblPlatformCaption
            // 
            this.lblPlatformCaption.AutoSize = false;
            this.lblPlatformCaption.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblPlatformCaption.Location = new System.Drawing.Point(715, 113);
            this.lblPlatformCaption.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPlatformCaption.Name = "lblPlatformCaption";
            this.lblPlatformCaption.Size = new System.Drawing.Size(110, 21);
            this.lblPlatformCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblPlatformCaption.TabIndex = 4;
            this.lblPlatformCaption.Text = "Platform:";
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(1070, 369);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(161, 39);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lstGames
            // 
            this.lstGames.FormattingEnabled = true;
            this.lstGames.ItemHeight = 16;
            this.lstGames.Location = new System.Drawing.Point(16, 36);
            this.lstGames.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lstGames.Name = "lstGames";
            this.lstGames.Size = new System.Drawing.Size(313, 372);
            this.lstGames.TabIndex = 6;
            this.lstGames.SelectedIndexChanged += new System.EventHandler(this.lstGames_SelectedIndexChanged);
            // 
            // lblScoreCaption
            // 
            this.lblScoreCaption.AutoSize = false;
            this.lblScoreCaption.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblScoreCaption.Location = new System.Drawing.Point(715, 151);
            this.lblScoreCaption.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblScoreCaption.Name = "lblScoreCaption";
            this.lblScoreCaption.Size = new System.Drawing.Size(110, 21);
            this.lblScoreCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblScoreCaption.TabIndex = 7;
            this.lblScoreCaption.Text = "Score:";
            // 
            // lblScore
            // 
            this.lblScore.AutoSize = true;
            this.lblScore.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblScore.Location = new System.Drawing.Point(840, 151);
            this.lblScore.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblScore.Name = "lblScore";
            this.lblScore.Size = new System.Drawing.Size(48, 21);
            this.lblScore.TabIndex = 11;
            this.lblScore.Text = "";
            // 
            // lblPlatform
            // 
            this.lblPlatform.AutoSize = true;
            this.lblPlatform.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblPlatform.Location = new System.Drawing.Point(840, 114);
            this.lblPlatform.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPlatform.Name = "lblPlatform";
            this.lblPlatform.Size = new System.Drawing.Size(73, 21);
            this.lblPlatform.TabIndex = 10;
            this.lblPlatform.Text = "";
            // 
            // lblGenre
            // 
            this.lblGenre.AutoSize = true;
            this.lblGenre.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblGenre.Location = new System.Drawing.Point(840, 79);
            this.lblGenre.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblGenre.Name = "lblGenre";
            this.lblGenre.Size = new System.Drawing.Size(36, 21);
            this.lblGenre.TabIndex = 9;
            this.lblGenre.Text = "";
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblName.Location = new System.Drawing.Point(840, 38);
            this.lblName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(32, 21);
            this.lblName.TabIndex = 8;
            this.lblName.Text = "";
            // 
            // btnMinimize
            // 
            this.btnMinimize.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.btnMinimize.FlatAppearance.BorderSize = 0;
            this.btnMinimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMinimize.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnMinimize.ForeColor = System.Drawing.Color.White;
            this.btnMinimize.Location = new System.Drawing.Point(1107, -1);
            this.btnMinimize.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Size = new System.Drawing.Size(77, 33);
            this.btnMinimize.TabIndex = 24;
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
            this.btnCloseWindow.Location = new System.Drawing.Point(1181, -1);
            this.btnCloseWindow.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCloseWindow.Name = "btnCloseWindow";
            this.btnCloseWindow.Size = new System.Drawing.Size(77, 33);
            this.btnCloseWindow.TabIndex = 23;
            this.btnCloseWindow.Text = "X";
            this.btnCloseWindow.UseVisualStyleBackColor = false;
            this.btnCloseWindow.Click += new System.EventHandler(this.btnCloseWindow_Click);
            // 
            // lblCommentCaption
            // 
            this.lblCommentCaption.AutoSize = false;
            this.lblCommentCaption.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblCommentCaption.Location = new System.Drawing.Point(675, 190);
            this.lblCommentCaption.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCommentCaption.Name = "lblCommentCaption";
            this.lblCommentCaption.Size = new System.Drawing.Size(150, 21);
            this.lblCommentCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblCommentCaption.TabIndex = 25;
            this.lblCommentCaption.Text = "Your Review:";
            // 
            // lblComment
            // 
            this.lblComment.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblComment.Location = new System.Drawing.Point(840, 190);
            this.lblComment.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblComment.Name = "lblComment";
            this.lblComment.Size = new System.Drawing.Size(325, 168);
            this.lblComment.TabIndex = 26;
            this.lblComment.Text = "";
            // 
            // BrowseGamesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(1259, 434);
            this.Controls.Add(this.lblComment);
            this.Controls.Add(this.lblCommentCaption);
            this.Controls.Add(this.btnMinimize);
            this.Controls.Add(this.btnCloseWindow);
            this.Controls.Add(this.lblScore);
            this.Controls.Add(this.lblPlatform);
            this.Controls.Add(this.lblGenre);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.lblScoreCaption);
            this.Controls.Add(this.lstGames);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblPlatformCaption);
            this.Controls.Add(this.lblGenreCaption);
            this.Controls.Add(this.lblNameCaption);
            this.Controls.Add(this.picCover);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "BrowseGamesForm";
            this.Text = "Video Game Manager | Browse Games";
            this.Load += new System.EventHandler(this.BrowseGamesForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picCover)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.PictureBox picCover;
        private System.Windows.Forms.Label lblNameCaption;
        private System.Windows.Forms.Label lblGenreCaption;
        private System.Windows.Forms.Label lblPlatformCaption;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ListBox lstGames;
        private System.Windows.Forms.Label lblScoreCaption;
        private System.Windows.Forms.Label lblScore;
        private System.Windows.Forms.Label lblPlatform;
        private System.Windows.Forms.Label lblGenre;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Button btnMinimize;
        private System.Windows.Forms.Button btnCloseWindow;
        private System.Windows.Forms.Label lblCommentCaption;
        private System.Windows.Forms.Label lblComment;
    }
}