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
            this.layoutFilters = new VideoGameManager.UI.Controls.LayoutGrid();
            this.searchGames = new VideoGameManager.UI.Controls.SearchBox();
            this.frameGenre = new VideoGameManager.UI.Controls.InputFrame();
            this.cmbGenre = new System.Windows.Forms.ComboBox();
            this.framePlatform = new VideoGameManager.UI.Controls.InputFrame();
            this.cmbPlatform = new System.Windows.Forms.ComboBox();
            this.frameStatus = new VideoGameManager.UI.Controls.InputFrame();
            this.cmbStatus = new System.Windows.Forms.ComboBox();
            this.chkFavourites = new System.Windows.Forms.CheckBox();
            this.frameSort = new VideoGameManager.UI.Controls.InputFrame();
            this.cmbSort = new System.Windows.Forms.ComboBox();
            this.layoutList = new VideoGameManager.UI.Controls.LayoutGrid();
            this.lstGames = new VideoGameManager.UI.Controls.GameListBox();
            this.lblListStatus = new System.Windows.Forms.Label();
            this.layoutPager = new VideoGameManager.UI.Controls.LayoutGrid();
            this.lblPage = new System.Windows.Forms.Label();
            this.btnPreviousPage = new VideoGameManager.UI.Controls.FlatButton();
            this.btnNextPage = new VideoGameManager.UI.Controls.FlatButton();
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
            this.lblStatusCaption = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblFavouriteCaption = new System.Windows.Forms.Label();
            this.lblFavourite = new System.Windows.Forms.Label();
            this.lblCommentCaption = new System.Windows.Forms.Label();
            this.lblComment = new System.Windows.Forms.Label();
            this.panelActions = new System.Windows.Forms.FlowLayoutPanel();
            this.btnClose = new VideoGameManager.UI.Controls.FlatButton();
            this.btnDelete = new VideoGameManager.UI.Controls.FlatButton();
            this.btnEdit = new VideoGameManager.UI.Controls.FlatButton();
            this.btnExport = new VideoGameManager.UI.Controls.FlatButton();
            this.layoutRoot.SuspendLayout();
            this.layoutFilters.SuspendLayout();
            this.frameGenre.SuspendLayout();
            this.framePlatform.SuspendLayout();
            this.frameStatus.SuspendLayout();
            this.frameSort.SuspendLayout();
            this.layoutList.SuspendLayout();
            this.layoutPager.SuspendLayout();
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
            this.layoutRoot.Controls.Add(this.layoutFilters, 0, 0);
            this.layoutRoot.Controls.Add(this.layoutList, 0, 1);
            this.layoutRoot.Controls.Add(this.coverCard, 1, 1);
            this.layoutRoot.Controls.Add(this.detailsCard, 2, 1);
            this.layoutRoot.Controls.Add(this.panelActions, 0, 2);
            this.layoutRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutRoot.Name = "layoutRoot";
            this.layoutRoot.RowCount = 3;
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, VideoGameManager.UI.Theming.Theme.Metrics.ListRow));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.layoutRoot.SetColumnSpan(this.layoutFilters, 3);
            this.layoutRoot.SetColumnSpan(this.panelActions, 3);
            this.layoutRoot.TabIndex = 0;
            //
            // layoutFilters
            //
            this.layoutFilters.ColumnCount = 6;
            this.layoutFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.layoutFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.layoutFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            this.layoutFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            this.layoutFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 190F));
            this.layoutFilters.Controls.Add(this.searchGames, 0, 0);
            this.layoutFilters.Controls.Add(this.frameGenre, 1, 0);
            this.layoutFilters.Controls.Add(this.framePlatform, 2, 0);
            this.layoutFilters.Controls.Add(this.frameStatus, 3, 0);
            this.layoutFilters.Controls.Add(this.chkFavourites, 4, 0);
            this.layoutFilters.Controls.Add(this.frameSort, 5, 0);
            this.layoutFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutFilters.Margin = new System.Windows.Forms.Padding(0);
            this.layoutFilters.Name = "layoutFilters";
            this.layoutFilters.RowCount = 1;
            this.layoutFilters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutFilters.TabIndex = 0;
            //
            // searchGames
            //
            this.searchGames.AccessibleName = "Search games";
            this.searchGames.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.searchGames.Name = "searchGames";
            this.searchGames.PlaceholderText = "Search by name";
            this.searchGames.TabIndex = 0;
            this.searchGames.SearchTextChanged += new System.EventHandler(this.filterControl_Changed);
            //
            // frameGenre
            //
            this.frameGenre.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.frameGenre.Controls.Add(this.cmbGenre);
            this.frameGenre.Name = "frameGenre";
            this.frameGenre.TabIndex = 1;
            //
            // cmbGenre
            //
            this.cmbGenre.AccessibleName = "Genre filter";
            this.cmbGenre.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbGenre.FormattingEnabled = true;
            this.cmbGenre.Name = "cmbGenre";
            this.cmbGenre.TabIndex = 0;
            this.cmbGenre.SelectedIndexChanged += new System.EventHandler(this.filterControl_Changed);
            //
            // framePlatform
            //
            this.framePlatform.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.framePlatform.Controls.Add(this.cmbPlatform);
            this.framePlatform.Name = "framePlatform";
            this.framePlatform.TabIndex = 2;
            //
            // cmbPlatform
            //
            this.cmbPlatform.AccessibleName = "Platform filter";
            this.cmbPlatform.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPlatform.FormattingEnabled = true;
            this.cmbPlatform.Name = "cmbPlatform";
            this.cmbPlatform.TabIndex = 0;
            this.cmbPlatform.SelectedIndexChanged += new System.EventHandler(this.filterControl_Changed);
            //
            // frameStatus
            //
            this.frameStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.frameStatus.Controls.Add(this.cmbStatus);
            this.frameStatus.Name = "frameStatus";
            this.frameStatus.TabIndex = 3;
            //
            // cmbStatus
            //
            this.cmbStatus.AccessibleName = "Play state filter";
            this.cmbStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStatus.FormattingEnabled = true;
            this.cmbStatus.Name = "cmbStatus";
            this.cmbStatus.TabIndex = 0;
            this.cmbStatus.SelectedIndexChanged += new System.EventHandler(this.filterControl_Changed);
            //
            // chkFavourites
            //
            this.chkFavourites.AccessibleName = "Favourites only";
            this.chkFavourites.AutoSize = false;
            this.chkFavourites.BackColor = System.Drawing.Color.Transparent;
            this.chkFavourites.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkFavourites.Font = VideoGameManager.UI.Theming.Theme.Fonts.Body;
            this.chkFavourites.ForeColor = VideoGameManager.UI.Theming.Theme.TextPrimary;
            this.chkFavourites.Name = "chkFavourites";
            this.chkFavourites.TabIndex = 4;
            this.chkFavourites.Text = "Favourites only";
            this.chkFavourites.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkFavourites.UseVisualStyleBackColor = true;
            this.chkFavourites.CheckedChanged += new System.EventHandler(this.filterControl_Changed);
            //
            // frameSort
            //
            this.frameSort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.frameSort.Controls.Add(this.cmbSort);
            this.frameSort.Name = "frameSort";
            this.frameSort.TabIndex = 5;
            //
            // cmbSort
            //
            this.cmbSort.AccessibleName = "Sort order";
            this.cmbSort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSort.FormattingEnabled = true;
            this.cmbSort.Name = "cmbSort";
            this.cmbSort.TabIndex = 0;
            this.cmbSort.SelectedIndexChanged += new System.EventHandler(this.filterControl_Changed);
            //
            // layoutList
            //
            this.layoutList.ColumnCount = 1;
            this.layoutList.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutList.Controls.Add(this.lstGames, 0, 0);
            this.layoutList.Controls.Add(this.lblListStatus, 0, 0);
            this.layoutList.Controls.Add(this.layoutPager, 0, 1);
            this.layoutList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutList.Margin = new System.Windows.Forms.Padding(0);
            this.layoutList.Name = "layoutList";
            this.layoutList.RowCount = 2;
            this.layoutList.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutList.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72F));
            this.layoutList.TabIndex = 1;
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
            this.lblListStatus.TabIndex = 1;
            this.lblListStatus.Text = "";
            this.lblListStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblListStatus.Visible = false;
            //
            // layoutPager
            //
            this.layoutPager.ColumnCount = 2;
            this.layoutPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.layoutPager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.layoutPager.Controls.Add(this.lblPage, 0, 0);
            this.layoutPager.Controls.Add(this.btnPreviousPage, 0, 1);
            this.layoutPager.Controls.Add(this.btnNextPage, 1, 1);
            this.layoutPager.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutPager.Margin = new System.Windows.Forms.Padding(0);
            this.layoutPager.Name = "layoutPager";
            this.layoutPager.RowCount = 2;
            this.layoutPager.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.layoutPager.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.layoutPager.SetColumnSpan(this.lblPage, 2);
            this.layoutPager.TabIndex = 2;
            //
            // lblPage
            //
            this.lblPage.AccessibleName = "Page";
            this.lblPage.AutoSize = false;
            this.lblPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPage.Font = VideoGameManager.UI.Theming.Theme.Fonts.Caption;
            this.lblPage.ForeColor = VideoGameManager.UI.Theming.Theme.TextSecondary;
            this.lblPage.Name = "lblPage";
            this.lblPage.TabIndex = 0;
            this.lblPage.Text = "";
            this.lblPage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // btnPreviousPage
            //
            this.btnPreviousPage.AccessibleName = "Previous page";
            this.btnPreviousPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnPreviousPage.Enabled = false;
            this.btnPreviousPage.Kind = VideoGameManager.UI.Controls.ButtonKind.Secondary;
            this.btnPreviousPage.Name = "btnPreviousPage";
            this.btnPreviousPage.TabIndex = 1;
            this.btnPreviousPage.Text = "Previous";
            this.btnPreviousPage.Click += new System.EventHandler(this.btnPreviousPage_Click);
            //
            // btnNextPage
            //
            this.btnNextPage.AccessibleName = "Next page";
            this.btnNextPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnNextPage.Enabled = false;
            this.btnNextPage.Kind = VideoGameManager.UI.Controls.ButtonKind.Secondary;
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.TabIndex = 2;
            this.btnNextPage.Text = "Next";
            this.btnNextPage.Click += new System.EventHandler(this.btnNextPage_Click);
            //
            // coverCard
            //
            this.coverCard.AccessibleName = "Cover artwork";
            this.coverCard.Controls.Add(this.picCover);
            this.coverCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.coverCard.Name = "coverCard";
            this.coverCard.Padding = new System.Windows.Forms.Padding(VideoGameManager.UI.Theming.Theme.Space.M);
            this.coverCard.TabIndex = 2;
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
            this.detailsCard.TabIndex = 3;
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
            this.layoutDetails.Controls.Add(this.lblStatusCaption, 0, 4);
            this.layoutDetails.Controls.Add(this.lblStatus, 1, 4);
            this.layoutDetails.Controls.Add(this.lblFavouriteCaption, 0, 5);
            this.layoutDetails.Controls.Add(this.lblFavourite, 1, 5);
            this.layoutDetails.Controls.Add(this.lblCommentCaption, 0, 6);
            this.layoutDetails.Controls.Add(this.lblComment, 1, 6);
            this.layoutDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutDetails.Name = "layoutDetails";
            this.layoutDetails.RowCount = 7;
            this.layoutDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, VideoGameManager.UI.Theming.Theme.Metrics.Input));
            this.layoutDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, VideoGameManager.UI.Theming.Theme.Metrics.Input));
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
            // lblStatusCaption
            //
            this.lblStatusCaption.AutoSize = false;
            this.lblStatusCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatusCaption.Font = VideoGameManager.UI.Theming.Theme.Fonts.BodyStrong;
            this.lblStatusCaption.ForeColor = VideoGameManager.UI.Theming.Theme.TextSecondary;
            this.lblStatusCaption.Name = "lblStatusCaption";
            this.lblStatusCaption.TabIndex = 8;
            this.lblStatusCaption.Text = "Play State:";
            this.lblStatusCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblStatus
            //
            this.lblStatus.AutoEllipsis = true;
            this.lblStatus.AutoSize = false;
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Font = VideoGameManager.UI.Theming.Theme.Fonts.BodyLarge;
            this.lblStatus.ForeColor = VideoGameManager.UI.Theming.Theme.TextPrimary;
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.TabIndex = 9;
            this.lblStatus.Text = "";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblFavouriteCaption
            //
            this.lblFavouriteCaption.AutoSize = false;
            this.lblFavouriteCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFavouriteCaption.Font = VideoGameManager.UI.Theming.Theme.Fonts.BodyStrong;
            this.lblFavouriteCaption.ForeColor = VideoGameManager.UI.Theming.Theme.TextSecondary;
            this.lblFavouriteCaption.Name = "lblFavouriteCaption";
            this.lblFavouriteCaption.TabIndex = 10;
            this.lblFavouriteCaption.Text = "Favourite:";
            this.lblFavouriteCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // lblFavourite
            //
            this.lblFavourite.AutoEllipsis = true;
            this.lblFavourite.AutoSize = false;
            this.lblFavourite.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFavourite.Font = VideoGameManager.UI.Theming.Theme.Fonts.BodyLarge;
            this.lblFavourite.ForeColor = VideoGameManager.UI.Theming.Theme.TextPrimary;
            this.lblFavourite.Name = "lblFavourite";
            this.lblFavourite.TabIndex = 11;
            this.lblFavourite.Text = "";
            this.lblFavourite.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblCommentCaption
            //
            this.lblCommentCaption.AutoSize = false;
            this.lblCommentCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCommentCaption.Font = VideoGameManager.UI.Theming.Theme.Fonts.BodyStrong;
            this.lblCommentCaption.ForeColor = VideoGameManager.UI.Theming.Theme.TextSecondary;
            this.lblCommentCaption.Name = "lblCommentCaption";
            this.lblCommentCaption.Padding = new System.Windows.Forms.Padding(0, VideoGameManager.UI.Theming.Theme.Space.S, 0, 0);
            this.lblCommentCaption.TabIndex = 12;
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
            this.lblComment.TabIndex = 13;
            this.lblComment.Text = "";
            this.lblComment.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // panelActions
            //
            this.panelActions.AutoSize = true;
            this.panelActions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelActions.BackColor = System.Drawing.Color.Transparent;
            this.panelActions.Controls.Add(this.btnClose);
            this.panelActions.Controls.Add(this.btnDelete);
            this.panelActions.Controls.Add(this.btnEdit);
            this.panelActions.Controls.Add(this.btnExport);
            this.panelActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelActions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.panelActions.Margin = new System.Windows.Forms.Padding(0, VideoGameManager.UI.Theming.Theme.Space.M, 0, 0);
            this.panelActions.Name = "panelActions";
            this.panelActions.TabIndex = 4;
            this.panelActions.WrapContents = false;
            //
            // btnClose
            //
            this.btnClose.AccessibleName = "Close";
            this.btnClose.Kind = VideoGameManager.UI.Controls.ButtonKind.Secondary;
            this.btnClose.Name = "btnClose";
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            // btnDelete
            //
            this.btnDelete.AccessibleName = "Delete";
            this.btnDelete.Kind = VideoGameManager.UI.Controls.ButtonKind.Danger;
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "Delete";
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            //
            // btnEdit
            //
            this.btnEdit.AccessibleName = "Edit";
            this.btnEdit.Kind = VideoGameManager.UI.Controls.ButtonKind.Primary;
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.TabIndex = 1;
            this.btnEdit.Text = "Edit";
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            //
            // btnExport
            //
            this.btnExport.AccessibleName = "Export";
            this.btnExport.Kind = VideoGameManager.UI.Controls.ButtonKind.Secondary;
            this.btnExport.Name = "btnExport";
            this.btnExport.TabIndex = 0;
            this.btnExport.Text = "Export...";
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            //
            // BrowseGamesForm
            //
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(1120, 640);
            this.Controls.Add(this.layoutRoot);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BrowseGamesForm";
            this.Text = "Video Game Manager | Browse Games";
            this.Load += new System.EventHandler(this.BrowseGamesForm_Load);
            this.layoutRoot.ResumeLayout(false);
            this.layoutRoot.PerformLayout();
            this.layoutFilters.ResumeLayout(false);
            this.frameGenre.ResumeLayout(false);
            this.framePlatform.ResumeLayout(false);
            this.frameStatus.ResumeLayout(false);
            this.frameSort.ResumeLayout(false);
            this.layoutList.ResumeLayout(false);
            this.layoutPager.ResumeLayout(false);
            this.coverCard.ResumeLayout(false);
            this.detailsCard.ResumeLayout(false);
            this.layoutDetails.ResumeLayout(false);
            this.panelActions.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private VideoGameManager.UI.Controls.LayoutGrid layoutRoot;
        private VideoGameManager.UI.Controls.LayoutGrid layoutFilters;
        private VideoGameManager.UI.Controls.SearchBox searchGames;
        private VideoGameManager.UI.Controls.InputFrame frameGenre;
        private System.Windows.Forms.ComboBox cmbGenre;
        private VideoGameManager.UI.Controls.InputFrame framePlatform;
        private System.Windows.Forms.ComboBox cmbPlatform;
        private VideoGameManager.UI.Controls.InputFrame frameStatus;
        private System.Windows.Forms.ComboBox cmbStatus;
        private System.Windows.Forms.CheckBox chkFavourites;
        private VideoGameManager.UI.Controls.InputFrame frameSort;
        private System.Windows.Forms.ComboBox cmbSort;
        private VideoGameManager.UI.Controls.LayoutGrid layoutList;
        private VideoGameManager.UI.Controls.GameListBox lstGames;
        private System.Windows.Forms.Label lblListStatus;
        private VideoGameManager.UI.Controls.LayoutGrid layoutPager;
        private System.Windows.Forms.Label lblPage;
        private VideoGameManager.UI.Controls.FlatButton btnPreviousPage;
        private VideoGameManager.UI.Controls.FlatButton btnNextPage;
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
        private System.Windows.Forms.Label lblStatusCaption;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblFavouriteCaption;
        private System.Windows.Forms.Label lblFavourite;
        private System.Windows.Forms.Label lblCommentCaption;
        private System.Windows.Forms.Label lblComment;
        private System.Windows.Forms.FlowLayoutPanel panelActions;
        private VideoGameManager.UI.Controls.FlatButton btnClose;
        private VideoGameManager.UI.Controls.FlatButton btnDelete;
        private VideoGameManager.UI.Controls.FlatButton btnEdit;
        private VideoGameManager.UI.Controls.FlatButton btnExport;
    }
}
