namespace VideoGameManager
{
    partial class AddGameForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddGameForm));
            this.layoutRoot = new VideoGameManager.UI.Controls.LayoutGrid();
            this.lblNameCaption = new System.Windows.Forms.Label();
            this.frameName = new VideoGameManager.UI.Controls.InputFrame();
            this.txtName = new System.Windows.Forms.TextBox();
            this.layoutSelections = new VideoGameManager.UI.Controls.LayoutGrid();
            this.framePlatform = new VideoGameManager.UI.Controls.InputFrame();
            this.cmbPlatform = new System.Windows.Forms.ComboBox();
            this.frameGenre = new VideoGameManager.UI.Controls.InputFrame();
            this.cmbGenre = new System.Windows.Forms.ComboBox();
            this.frameScore = new VideoGameManager.UI.Controls.InputFrame();
            this.cmbScore = new System.Windows.Forms.ComboBox();
            this.lblStatusCaption = new System.Windows.Forms.Label();
            this.layoutStatusRow = new VideoGameManager.UI.Controls.LayoutGrid();
            this.frameStatus = new VideoGameManager.UI.Controls.InputFrame();
            this.cmbStatus = new System.Windows.Forms.ComboBox();
            this.chkFavourite = new System.Windows.Forms.CheckBox();
            this.lblCoverUrlCaption = new System.Windows.Forms.Label();
            this.frameCoverUrl = new VideoGameManager.UI.Controls.InputFrame();
            this.txtCoverUrl = new System.Windows.Forms.TextBox();
            this.panelActions = new System.Windows.Forms.FlowLayoutPanel();
            this.btnSave = new VideoGameManager.UI.Controls.FlatButton();
            this.layoutRoot.SuspendLayout();
            this.frameName.SuspendLayout();
            this.layoutSelections.SuspendLayout();
            this.framePlatform.SuspendLayout();
            this.frameGenre.SuspendLayout();
            this.frameScore.SuspendLayout();
            this.layoutStatusRow.SuspendLayout();
            this.frameStatus.SuspendLayout();
            this.frameCoverUrl.SuspendLayout();
            this.panelActions.SuspendLayout();
            this.SuspendLayout();
            //
            // layoutRoot
            //
            this.layoutRoot.ColumnCount = 2;
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.Controls.Add(this.lblNameCaption, 0, 0);
            this.layoutRoot.Controls.Add(this.frameName, 1, 0);
            this.layoutRoot.Controls.Add(this.layoutSelections, 1, 1);
            this.layoutRoot.Controls.Add(this.lblStatusCaption, 0, 2);
            this.layoutRoot.Controls.Add(this.layoutStatusRow, 1, 2);
            this.layoutRoot.Controls.Add(this.lblCoverUrlCaption, 0, 3);
            this.layoutRoot.Controls.Add(this.frameCoverUrl, 1, 3);
            this.layoutRoot.Controls.Add(this.panelActions, 1, 4);
            this.layoutRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutRoot.Name = "layoutRoot";
            this.layoutRoot.RowCount = 5;
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, VideoGameManager.UI.Theming.Theme.Metrics.ListRow));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, VideoGameManager.UI.Theming.Theme.Metrics.ListRow));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, VideoGameManager.UI.Theming.Theme.Metrics.ListRow));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, VideoGameManager.UI.Theming.Theme.Metrics.ListRow));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.layoutRoot.TabIndex = 0;
            //
            // lblNameCaption
            //
            this.lblNameCaption.AutoSize = false;
            this.lblNameCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNameCaption.Font = VideoGameManager.UI.Theming.Theme.Fonts.BodyStrong;
            this.lblNameCaption.ForeColor = VideoGameManager.UI.Theming.Theme.TextSecondary;
            this.lblNameCaption.Name = "lblNameCaption";
            this.lblNameCaption.TabIndex = 0;
            this.lblNameCaption.Text = "Game Name";
            this.lblNameCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // frameName
            //
            this.frameName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.frameName.Controls.Add(this.txtName);
            this.frameName.Name = "frameName";
            this.frameName.TabIndex = 0;
            //
            // txtName
            //
            this.txtName.AccessibleName = "Game Name";
            this.txtName.MaxLength = 107;
            this.txtName.Name = "txtName";
            this.txtName.TabIndex = 0;
            //
            // layoutSelections
            //
            this.layoutSelections.ColumnCount = 3;
            this.layoutSelections.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.layoutSelections.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.layoutSelections.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.layoutSelections.Controls.Add(this.framePlatform, 0, 0);
            this.layoutSelections.Controls.Add(this.frameGenre, 1, 0);
            this.layoutSelections.Controls.Add(this.frameScore, 2, 0);
            this.layoutSelections.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutSelections.Margin = new System.Windows.Forms.Padding(0);
            this.layoutSelections.Name = "layoutSelections";
            this.layoutSelections.RowCount = 1;
            this.layoutSelections.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutSelections.TabIndex = 1;
            //
            // framePlatform
            //
            this.framePlatform.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.framePlatform.Controls.Add(this.cmbPlatform);
            this.framePlatform.Name = "framePlatform";
            this.framePlatform.TabIndex = 0;
            //
            // cmbPlatform
            //
            this.cmbPlatform.AccessibleName = "Platform";
            this.cmbPlatform.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPlatform.FormattingEnabled = true;
            this.cmbPlatform.Name = "cmbPlatform";
            this.cmbPlatform.TabIndex = 0;
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
            this.cmbGenre.AccessibleName = "Genre";
            this.cmbGenre.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbGenre.FormattingEnabled = true;
            this.cmbGenre.Name = "cmbGenre";
            this.cmbGenre.TabIndex = 0;
            //
            // frameScore
            //
            this.frameScore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.frameScore.Controls.Add(this.cmbScore);
            this.frameScore.Name = "frameScore";
            this.frameScore.TabIndex = 2;
            //
            // cmbScore
            //
            this.cmbScore.AccessibleName = "Score";
            this.cmbScore.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbScore.FormattingEnabled = true;
            this.cmbScore.Name = "cmbScore";
            this.cmbScore.TabIndex = 0;
            //
            // lblStatusCaption
            //
            this.lblStatusCaption.AutoSize = false;
            this.lblStatusCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatusCaption.Font = VideoGameManager.UI.Theming.Theme.Fonts.BodyStrong;
            this.lblStatusCaption.ForeColor = VideoGameManager.UI.Theming.Theme.TextSecondary;
            this.lblStatusCaption.Name = "lblStatusCaption";
            this.lblStatusCaption.TabIndex = 0;
            this.lblStatusCaption.Text = "Status";
            this.lblStatusCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // layoutStatusRow
            //
            this.layoutStatusRow.ColumnCount = 2;
            this.layoutStatusRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.layoutStatusRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.layoutStatusRow.Controls.Add(this.frameStatus, 0, 0);
            this.layoutStatusRow.Controls.Add(this.chkFavourite, 1, 0);
            this.layoutStatusRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutStatusRow.Margin = new System.Windows.Forms.Padding(0);
            this.layoutStatusRow.Name = "layoutStatusRow";
            this.layoutStatusRow.RowCount = 1;
            this.layoutStatusRow.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutStatusRow.TabIndex = 2;
            //
            // frameStatus
            //
            this.frameStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.frameStatus.Controls.Add(this.cmbStatus);
            this.frameStatus.Name = "frameStatus";
            this.frameStatus.TabIndex = 0;
            //
            // cmbStatus
            //
            this.cmbStatus.AccessibleName = "Status";
            this.cmbStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStatus.FormattingEnabled = true;
            this.cmbStatus.Name = "cmbStatus";
            this.cmbStatus.TabIndex = 0;
            //
            // chkFavourite
            //
            this.chkFavourite.AccessibleName = "Favourite";
            this.chkFavourite.BackColor = System.Drawing.Color.Transparent;
            this.chkFavourite.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkFavourite.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkFavourite.Font = VideoGameManager.UI.Theming.Theme.Fonts.Body;
            this.chkFavourite.ForeColor = VideoGameManager.UI.Theming.Theme.TextPrimary;
            this.chkFavourite.Name = "chkFavourite";
            this.chkFavourite.TabIndex = 1;
            this.chkFavourite.Text = "Favourite";
            this.chkFavourite.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.chkFavourite.UseVisualStyleBackColor = false;
            //
            // lblCoverUrlCaption
            //
            this.lblCoverUrlCaption.AutoSize = false;
            this.lblCoverUrlCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCoverUrlCaption.Font = VideoGameManager.UI.Theming.Theme.Fonts.BodyStrong;
            this.lblCoverUrlCaption.ForeColor = VideoGameManager.UI.Theming.Theme.TextSecondary;
            this.lblCoverUrlCaption.Name = "lblCoverUrlCaption";
            this.lblCoverUrlCaption.TabIndex = 0;
            this.lblCoverUrlCaption.Text = "Cover URL";
            this.lblCoverUrlCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // frameCoverUrl
            //
            this.frameCoverUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.frameCoverUrl.Controls.Add(this.txtCoverUrl);
            this.frameCoverUrl.Name = "frameCoverUrl";
            this.frameCoverUrl.TabIndex = 3;
            //
            // txtCoverUrl
            //
            this.txtCoverUrl.AccessibleName = "Cover URL";
            this.txtCoverUrl.Name = "txtCoverUrl";
            this.txtCoverUrl.TabIndex = 0;
            //
            // panelActions
            //
            this.panelActions.AutoSize = true;
            this.panelActions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelActions.BackColor = System.Drawing.Color.Transparent;
            this.panelActions.Controls.Add(this.btnSave);
            this.panelActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelActions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.panelActions.Margin = new System.Windows.Forms.Padding(0, VideoGameManager.UI.Theming.Theme.Space.M, 0, 0);
            this.panelActions.Name = "panelActions";
            this.panelActions.TabIndex = 4;
            this.panelActions.WrapContents = false;
            //
            // btnSave
            //
            this.btnSave.AccessibleName = "Save";
            this.btnSave.Kind = VideoGameManager.UI.Controls.ButtonKind.Primary;
            this.btnSave.Name = "btnSave";
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Save";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            //
            // AddGameForm
            //
            this.AcceptButton = this.btnSave;
            this.ClientSize = new System.Drawing.Size(620, 338);
            this.Controls.Add(this.layoutRoot);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AddGameForm";
            this.Text = "Video Game Manager | Add Game";
            this.Load += new System.EventHandler(this.AddGameForm_Load);
            this.frameName.ResumeLayout(false);
            this.framePlatform.ResumeLayout(false);
            this.frameGenre.ResumeLayout(false);
            this.frameScore.ResumeLayout(false);
            this.frameStatus.ResumeLayout(false);
            this.layoutStatusRow.ResumeLayout(false);
            this.frameCoverUrl.ResumeLayout(false);
            this.layoutSelections.ResumeLayout(false);
            this.panelActions.ResumeLayout(false);
            this.layoutRoot.ResumeLayout(false);
            this.layoutRoot.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private VideoGameManager.UI.Controls.LayoutGrid layoutRoot;
        private System.Windows.Forms.Label lblNameCaption;
        private VideoGameManager.UI.Controls.InputFrame frameName;
        private System.Windows.Forms.TextBox txtName;
        private VideoGameManager.UI.Controls.LayoutGrid layoutSelections;
        private VideoGameManager.UI.Controls.InputFrame framePlatform;
        private System.Windows.Forms.ComboBox cmbPlatform;
        private VideoGameManager.UI.Controls.InputFrame frameGenre;
        private System.Windows.Forms.ComboBox cmbGenre;
        private VideoGameManager.UI.Controls.InputFrame frameScore;
        private System.Windows.Forms.ComboBox cmbScore;
        private System.Windows.Forms.Label lblStatusCaption;
        private VideoGameManager.UI.Controls.LayoutGrid layoutStatusRow;
        private VideoGameManager.UI.Controls.InputFrame frameStatus;
        private System.Windows.Forms.ComboBox cmbStatus;
        private System.Windows.Forms.CheckBox chkFavourite;
        private System.Windows.Forms.Label lblCoverUrlCaption;
        private VideoGameManager.UI.Controls.InputFrame frameCoverUrl;
        private System.Windows.Forms.TextBox txtCoverUrl;
        private System.Windows.Forms.FlowLayoutPanel panelActions;
        private VideoGameManager.UI.Controls.FlatButton btnSave;
    }
}
