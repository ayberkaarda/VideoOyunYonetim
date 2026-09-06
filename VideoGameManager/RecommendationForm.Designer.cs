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
            this.layoutRoot = new VideoGameManager.UI.Controls.LayoutGrid();
            this.coverCard = new VideoGameManager.UI.Controls.RoundedPanel();
            this.picCover = new System.Windows.Forms.PictureBox();
            this.layoutDetails = new VideoGameManager.UI.Controls.LayoutGrid();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblNameCaption = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.lblGenreCaption = new System.Windows.Forms.Label();
            this.lblGenre = new System.Windows.Forms.Label();
            this.lblPlatformCaption = new System.Windows.Forms.Label();
            this.lblPlatform = new System.Windows.Forms.Label();
            this.lblScoreCaption = new System.Windows.Forms.Label();
            this.lblScore = new System.Windows.Forms.Label();
            this.panelActions = new System.Windows.Forms.FlowLayoutPanel();
            this.btnRecommend = new VideoGameManager.UI.Controls.FlatButton();
            ((System.ComponentModel.ISupportInitialize)(this.picCover)).BeginInit();
            this.layoutRoot.SuspendLayout();
            this.coverCard.SuspendLayout();
            this.layoutDetails.SuspendLayout();
            this.panelActions.SuspendLayout();
            this.SuspendLayout();
            //
            // layoutRoot
            //
            this.layoutRoot.ColumnCount = 2;
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 36F));
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 64F));
            this.layoutRoot.Controls.Add(this.coverCard, 0, 0);
            this.layoutRoot.Controls.Add(this.layoutDetails, 1, 0);
            this.layoutRoot.Controls.Add(this.lblStatus, 1, 0);
            this.layoutRoot.Controls.Add(this.panelActions, 1, 1);
            this.layoutRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutRoot.Name = "layoutRoot";
            this.layoutRoot.RowCount = 2;
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.layoutRoot.TabIndex = 0;
            //
            // coverCard
            //
            this.coverCard.AccessibleName = "Cover artwork";
            this.coverCard.Controls.Add(this.picCover);
            this.coverCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.coverCard.Name = "coverCard";
            this.coverCard.Padding = new System.Windows.Forms.Padding(VideoGameManager.UI.Theming.Theme.Space.M);
            this.coverCard.TabIndex = 0;
            //
            // picCover
            //
            this.picCover.AccessibleName = "Cover artwork";
            this.picCover.BackColor = VideoGameManager.UI.Theming.Theme.SurfaceSunken;
            this.picCover.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picCover.Name = "picCover";
            this.picCover.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picCover.TabIndex = 0;
            this.picCover.TabStop = false;
            //
            // layoutDetails
            //
            this.layoutDetails.ColumnCount = 2;
            this.layoutDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.layoutDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutDetails.Controls.Add(this.lblNameCaption, 0, 0);
            this.layoutDetails.Controls.Add(this.lblName, 1, 0);
            this.layoutDetails.Controls.Add(this.lblGenreCaption, 0, 1);
            this.layoutDetails.Controls.Add(this.lblGenre, 1, 1);
            this.layoutDetails.Controls.Add(this.lblPlatformCaption, 0, 2);
            this.layoutDetails.Controls.Add(this.lblPlatform, 1, 2);
            this.layoutDetails.Controls.Add(this.lblScoreCaption, 0, 3);
            this.layoutDetails.Controls.Add(this.lblScore, 1, 3);
            this.layoutDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutDetails.Name = "layoutDetails";
            this.layoutDetails.RowCount = 5;
            this.layoutDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, VideoGameManager.UI.Theming.Theme.Metrics.Input));
            this.layoutDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, VideoGameManager.UI.Theming.Theme.Metrics.Input));
            this.layoutDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, VideoGameManager.UI.Theming.Theme.Metrics.Input));
            this.layoutDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, VideoGameManager.UI.Theming.Theme.Metrics.Input));
            this.layoutDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutDetails.TabIndex = 1;
            //
            // lblStatus
            //
            this.lblStatus.AccessibleName = "Recommendation status";
            this.lblStatus.AutoSize = false;
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Font = VideoGameManager.UI.Theming.Theme.Fonts.Body;
            this.lblStatus.ForeColor = VideoGameManager.UI.Theming.Theme.TextSecondary;
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Padding = new System.Windows.Forms.Padding(VideoGameManager.UI.Theming.Theme.Space.M);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblStatus.Visible = false;
            //
            // lblNameCaption
            //
            this.lblNameCaption.AutoSize = false;
            this.lblNameCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNameCaption.Font = VideoGameManager.UI.Theming.Theme.Fonts.BodyStrong;
            this.lblNameCaption.ForeColor = VideoGameManager.UI.Theming.Theme.TextSecondary;
            this.lblNameCaption.Name = "lblNameCaption";
            this.lblNameCaption.TabIndex = 0;
            this.lblNameCaption.Text = "Name:";
            this.lblNameCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblName
            //
            this.lblName.AutoEllipsis = true;
            this.lblName.AutoSize = false;
            this.lblName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblName.Font = VideoGameManager.UI.Theming.Theme.Fonts.BodyLarge;
            this.lblName.ForeColor = VideoGameManager.UI.Theming.Theme.TextPrimary;
            this.lblName.Name = "lblName";
            this.lblName.TabIndex = 0;
            this.lblName.Text = "";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblGenreCaption
            //
            this.lblGenreCaption.AutoSize = false;
            this.lblGenreCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGenreCaption.Font = VideoGameManager.UI.Theming.Theme.Fonts.BodyStrong;
            this.lblGenreCaption.ForeColor = VideoGameManager.UI.Theming.Theme.TextSecondary;
            this.lblGenreCaption.Name = "lblGenreCaption";
            this.lblGenreCaption.TabIndex = 0;
            this.lblGenreCaption.Text = "Genre:";
            this.lblGenreCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblGenre
            //
            this.lblGenre.AutoEllipsis = true;
            this.lblGenre.AutoSize = false;
            this.lblGenre.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGenre.Font = VideoGameManager.UI.Theming.Theme.Fonts.BodyLarge;
            this.lblGenre.ForeColor = VideoGameManager.UI.Theming.Theme.TextPrimary;
            this.lblGenre.Name = "lblGenre";
            this.lblGenre.TabIndex = 0;
            this.lblGenre.Text = "";
            this.lblGenre.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblPlatformCaption
            //
            this.lblPlatformCaption.AutoSize = false;
            this.lblPlatformCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPlatformCaption.Font = VideoGameManager.UI.Theming.Theme.Fonts.BodyStrong;
            this.lblPlatformCaption.ForeColor = VideoGameManager.UI.Theming.Theme.TextSecondary;
            this.lblPlatformCaption.Name = "lblPlatformCaption";
            this.lblPlatformCaption.TabIndex = 0;
            this.lblPlatformCaption.Text = "Platform:";
            this.lblPlatformCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblPlatform
            //
            this.lblPlatform.AutoEllipsis = true;
            this.lblPlatform.AutoSize = false;
            this.lblPlatform.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPlatform.Font = VideoGameManager.UI.Theming.Theme.Fonts.BodyLarge;
            this.lblPlatform.ForeColor = VideoGameManager.UI.Theming.Theme.TextPrimary;
            this.lblPlatform.Name = "lblPlatform";
            this.lblPlatform.TabIndex = 0;
            this.lblPlatform.Text = "";
            this.lblPlatform.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblScoreCaption
            //
            this.lblScoreCaption.AutoSize = false;
            this.lblScoreCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblScoreCaption.Font = VideoGameManager.UI.Theming.Theme.Fonts.BodyStrong;
            this.lblScoreCaption.ForeColor = VideoGameManager.UI.Theming.Theme.TextSecondary;
            this.lblScoreCaption.Name = "lblScoreCaption";
            this.lblScoreCaption.TabIndex = 0;
            this.lblScoreCaption.Text = "Score:";
            this.lblScoreCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblScore
            //
            this.lblScore.AutoEllipsis = true;
            this.lblScore.AutoSize = false;
            this.lblScore.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblScore.Font = VideoGameManager.UI.Theming.Theme.Fonts.BodyLarge;
            this.lblScore.ForeColor = VideoGameManager.UI.Theming.Theme.TextPrimary;
            this.lblScore.Name = "lblScore";
            this.lblScore.TabIndex = 0;
            this.lblScore.Text = "";
            this.lblScore.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // panelActions
            //
            this.panelActions.AutoSize = true;
            this.panelActions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelActions.BackColor = System.Drawing.Color.Transparent;
            this.panelActions.Controls.Add(this.btnRecommend);
            this.panelActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelActions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.panelActions.Margin = new System.Windows.Forms.Padding(0, VideoGameManager.UI.Theming.Theme.Space.M, 0, 0);
            this.panelActions.Name = "panelActions";
            this.panelActions.TabIndex = 2;
            this.panelActions.WrapContents = false;
            //
            // btnRecommend
            //
            this.btnRecommend.AccessibleName = "Get Recommendation";
            this.btnRecommend.Kind = VideoGameManager.UI.Controls.ButtonKind.Primary;
            this.btnRecommend.Name = "btnRecommend";
            this.btnRecommend.Size = new System.Drawing.Size(220, VideoGameManager.UI.Theming.Theme.Metrics.Button);
            this.btnRecommend.TabIndex = 0;
            this.btnRecommend.Text = "Get Recommendation";
            this.btnRecommend.Click += new System.EventHandler(this.btnRecommend_Click);
            //
            // RecommendationForm
            //
            this.AcceptButton = this.btnRecommend;
            this.ClientSize = new System.Drawing.Size(700, 460);
            this.Controls.Add(this.layoutRoot);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RecommendationForm";
            this.Text = "Video Game Manager | Recommendation";
            this.Load += new System.EventHandler(this.RecommendationForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picCover)).EndInit();
            this.coverCard.ResumeLayout(false);
            this.layoutDetails.ResumeLayout(false);
            this.panelActions.ResumeLayout(false);
            this.layoutRoot.ResumeLayout(false);
            this.layoutRoot.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private VideoGameManager.UI.Controls.LayoutGrid layoutRoot;
        private VideoGameManager.UI.Controls.RoundedPanel coverCard;
        private System.Windows.Forms.PictureBox picCover;
        private VideoGameManager.UI.Controls.LayoutGrid layoutDetails;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblNameCaption;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblGenreCaption;
        private System.Windows.Forms.Label lblGenre;
        private System.Windows.Forms.Label lblPlatformCaption;
        private System.Windows.Forms.Label lblPlatform;
        private System.Windows.Forms.Label lblScoreCaption;
        private System.Windows.Forms.Label lblScore;
        private System.Windows.Forms.FlowLayoutPanel panelActions;
        private VideoGameManager.UI.Controls.FlatButton btnRecommend;
    }
}
