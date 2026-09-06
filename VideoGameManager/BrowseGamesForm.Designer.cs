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
            this.layoutRoot = new VideoGameManager.UI.Controls.LayoutGrid();
            this.lstGames = new VideoGameManager.UI.Controls.GameListBox();
            this.lblListStatus = new System.Windows.Forms.Label();
            this.coverCard = new VideoGameManager.UI.Controls.RoundedPanel();
            this.picCover = new VideoGameManager.UI.Controls.CoverImageBox();
            this.detailsCard = new VideoGameManager.UI.Controls.RoundedPanel();
            this.layoutDetails = new VideoGameManager.UI.Controls.LayoutGrid();
            this.lblNameCaption = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.lblGenreCaption = new System.Windows.Forms.Label();
            this.lblGenre = new System.Windows.Forms.Label();
            this.lblPlatformCaption = new System.Windows.Forms.Label();
            this.lblPlatform = new System.Windows.Forms.Label();
            this.lblScoreCaption = new System.Windows.Forms.Label();
            this.badgeScore = new VideoGameManager.UI.Controls.RatingBadge();
            this.lblCommentCaption = new System.Windows.Forms.Label();
            this.lblComment = new System.Windows.Forms.Label();
            this.panelActions = new System.Windows.Forms.FlowLayoutPanel();
            this.btnClose = new VideoGameManager.UI.Controls.FlatButton();
            this.layoutRoot.SuspendLayout();
            this.coverCard.SuspendLayout();
            this.detailsCard.SuspendLayout();
            this.layoutDetails.SuspendLayout();
            this.panelActions.SuspendLayout();
            this.SuspendLayout();
            //
            // layoutRoot
            //
            this.layoutRoot.ColumnCount = 3;
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 260F));
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.Controls.Add(this.lstGames, 0, 0);
            this.layoutRoot.Controls.Add(this.lblListStatus, 0, 0);
            this.layoutRoot.Controls.Add(this.coverCard, 1, 0);
            this.layoutRoot.Controls.Add(this.detailsCard, 2, 0);
            this.layoutRoot.Controls.Add(this.panelActions, 0, 1);
            this.layoutRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutRoot.Name = "layoutRoot";
            this.layoutRoot.RowCount = 2;
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.layoutRoot.SetColumnSpan(this.panelActions, 3);
            this.layoutRoot.TabIndex = 0;
            //
            // lstGames
            //
            this.lstGames.AccessibleName = "Games";
            this.lstGames.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstGames.FormattingEnabled = true;
            this.lstGames.Name = "lstGames";
            this.lstGames.TabIndex = 0;
            this.lstGames.SelectedIndexChanged += new System.EventHandler(this.lstGames_SelectedIndexChanged);
            //
            // lblListStatus
            //
            this.lblListStatus.AccessibleName = "Games";
            this.lblListStatus.AutoSize = false;
            this.lblListStatus.BackColor = VideoGameManager.UI.Theming.Theme.SurfaceRaised;
            this.lblListStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblListStatus.Font = VideoGameManager.UI.Theming.Theme.Fonts.Body;
            this.lblListStatus.ForeColor = VideoGameManager.UI.Theming.Theme.TextSecondary;
            this.lblListStatus.Name = "lblListStatus";
            this.lblListStatus.Padding = new System.Windows.Forms.Padding(VideoGameManager.UI.Theming.Theme.Space.M);
            this.lblListStatus.TabIndex = 4;
            this.lblListStatus.Text = "";
            this.lblListStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblListStatus.Visible = false;
            //
            // coverCard
            //
            this.coverCard.AccessibleName = "Cover artwork";
            this.coverCard.Controls.Add(this.picCover);
            this.coverCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.coverCard.Name = "coverCard";
            this.coverCard.Padding = new System.Windows.Forms.Padding(VideoGameManager.UI.Theming.Theme.Space.M);
            this.coverCard.TabIndex = 1;
            //
            // picCover
            //
            this.picCover.AccessibleName = "Cover artwork";
            this.picCover.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picCover.Name = "picCover";
            this.picCover.TabIndex = 0;
            //
            // detailsCard
            //
            this.detailsCard.AccessibleName = "Game details";
            this.detailsCard.Controls.Add(this.layoutDetails);
            this.detailsCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailsCard.Name = "detailsCard";
            this.detailsCard.TabIndex = 2;
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
            this.layoutDetails.Controls.Add(this.badgeScore, 1, 3);
            this.layoutDetails.Controls.Add(this.lblCommentCaption, 0, 4);
            this.layoutDetails.Controls.Add(this.lblComment, 1, 4);
            this.layoutDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutDetails.Name = "layoutDetails";
            this.layoutDetails.RowCount = 5;
            this.layoutDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, VideoGameManager.UI.Theming.Theme.Metrics.Input));
            this.layoutDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, VideoGameManager.UI.Theming.Theme.Metrics.Input));
            this.layoutDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, VideoGameManager.UI.Theming.Theme.Metrics.Input));
            this.layoutDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, VideoGameManager.UI.Theming.Theme.Metrics.Input));
            this.layoutDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutDetails.TabIndex = 0;
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
            this.lblName.TabIndex = 1;
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
            this.lblGenreCaption.TabIndex = 2;
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
            this.lblGenre.TabIndex = 3;
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
            this.lblPlatformCaption.TabIndex = 4;
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
            this.lblPlatform.TabIndex = 5;
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
            this.lblScoreCaption.TabIndex = 6;
            this.lblScoreCaption.Text = "Score:";
            this.lblScoreCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // badgeScore
            //
            this.badgeScore.AccessibleName = "Score";
            this.badgeScore.Dock = System.Windows.Forms.DockStyle.Left;
            this.badgeScore.Name = "badgeScore";
            this.badgeScore.TabIndex = 7;
            //
            // lblCommentCaption
            //
            this.lblCommentCaption.AutoSize = false;
            this.lblCommentCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCommentCaption.Font = VideoGameManager.UI.Theming.Theme.Fonts.BodyStrong;
            this.lblCommentCaption.ForeColor = VideoGameManager.UI.Theming.Theme.TextSecondary;
            this.lblCommentCaption.Name = "lblCommentCaption";
            this.lblCommentCaption.Padding = new System.Windows.Forms.Padding(0, VideoGameManager.UI.Theming.Theme.Space.S, 0, 0);
            this.lblCommentCaption.TabIndex = 8;
            this.lblCommentCaption.Text = "Your Review:";
            this.lblCommentCaption.TextAlign = System.Drawing.ContentAlignment.TopRight;
            //
            // lblComment
            //
            this.lblComment.AutoSize = false;
            this.lblComment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblComment.Font = VideoGameManager.UI.Theming.Theme.Fonts.Body;
            this.lblComment.ForeColor = VideoGameManager.UI.Theming.Theme.TextPrimary;
            this.lblComment.Name = "lblComment";
            this.lblComment.Padding = new System.Windows.Forms.Padding(0, VideoGameManager.UI.Theming.Theme.Space.S, 0, 0);
            this.lblComment.TabIndex = 9;
            this.lblComment.Text = "";
            this.lblComment.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // panelActions
            //
            this.panelActions.AutoSize = true;
            this.panelActions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelActions.BackColor = System.Drawing.Color.Transparent;
            this.panelActions.Controls.Add(this.btnClose);
            this.panelActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelActions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.panelActions.Margin = new System.Windows.Forms.Padding(0, VideoGameManager.UI.Theming.Theme.Space.M, 0, 0);
            this.panelActions.Name = "panelActions";
            this.panelActions.TabIndex = 3;
            this.panelActions.WrapContents = false;
            //
            // btnClose
            //
            this.btnClose.AccessibleName = "Close";
            this.btnClose.Kind = VideoGameManager.UI.Controls.ButtonKind.Secondary;
            this.btnClose.Name = "btnClose";
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            // BrowseGamesForm
            //
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(1040, 560);
            this.Controls.Add(this.layoutRoot);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BrowseGamesForm";
            this.Text = "Video Game Manager | Browse Games";
            this.Load += new System.EventHandler(this.BrowseGamesForm_Load);
            this.layoutRoot.ResumeLayout(false);
            this.layoutRoot.PerformLayout();
            this.coverCard.ResumeLayout(false);
            this.detailsCard.ResumeLayout(false);
            this.layoutDetails.ResumeLayout(false);
            this.panelActions.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private VideoGameManager.UI.Controls.LayoutGrid layoutRoot;
        private VideoGameManager.UI.Controls.GameListBox lstGames;
        private System.Windows.Forms.Label lblListStatus;
        private VideoGameManager.UI.Controls.RoundedPanel coverCard;
        private VideoGameManager.UI.Controls.CoverImageBox picCover;
        private VideoGameManager.UI.Controls.RoundedPanel detailsCard;
        private VideoGameManager.UI.Controls.LayoutGrid layoutDetails;
        private System.Windows.Forms.Label lblNameCaption;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblGenreCaption;
        private System.Windows.Forms.Label lblGenre;
        private System.Windows.Forms.Label lblPlatformCaption;
        private System.Windows.Forms.Label lblPlatform;
        private System.Windows.Forms.Label lblScoreCaption;
        private VideoGameManager.UI.Controls.RatingBadge badgeScore;
        private System.Windows.Forms.Label lblCommentCaption;
        private System.Windows.Forms.Label lblComment;
        private System.Windows.Forms.FlowLayoutPanel panelActions;
        private VideoGameManager.UI.Controls.FlatButton btnClose;
    }
}
