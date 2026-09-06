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
            this.layoutRoot = new VideoGameManager.UI.Controls.LayoutGrid();
            this.lblGameCaption = new System.Windows.Forms.Label();
            this.frameGames = new VideoGameManager.UI.Controls.InputFrame();
            this.cmbGames = new System.Windows.Forms.ComboBox();
            this.lblCommentCaption = new System.Windows.Forms.Label();
            this.frameComment = new VideoGameManager.UI.Controls.InputFrame();
            this.txtComment = new System.Windows.Forms.TextBox();
            this.panelActions = new System.Windows.Forms.FlowLayoutPanel();
            this.btnSave = new VideoGameManager.UI.Controls.FlatButton();
            this.layoutRoot.SuspendLayout();
            this.frameGames.SuspendLayout();
            this.frameComment.SuspendLayout();
            this.panelActions.SuspendLayout();
            this.SuspendLayout();
            //
            // layoutRoot
            //
            this.layoutRoot.ColumnCount = 2;
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.Controls.Add(this.lblGameCaption, 0, 0);
            this.layoutRoot.Controls.Add(this.frameGames, 1, 0);
            this.layoutRoot.Controls.Add(this.lblCommentCaption, 0, 1);
            this.layoutRoot.Controls.Add(this.frameComment, 1, 1);
            this.layoutRoot.Controls.Add(this.panelActions, 1, 2);
            this.layoutRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutRoot.Name = "layoutRoot";
            this.layoutRoot.RowCount = 3;
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, VideoGameManager.UI.Theming.Theme.Metrics.ListRow));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.layoutRoot.TabIndex = 0;
            //
            // lblGameCaption
            //
            this.lblGameCaption.AutoSize = false;
            this.lblGameCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblGameCaption.Font = VideoGameManager.UI.Theming.Theme.Fonts.BodyStrong;
            this.lblGameCaption.ForeColor = VideoGameManager.UI.Theming.Theme.TextSecondary;
            this.lblGameCaption.Name = "lblGameCaption";
            this.lblGameCaption.TabIndex = 0;
            this.lblGameCaption.Text = "Select Game";
            this.lblGameCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // frameGames
            //
            this.frameGames.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.frameGames.Controls.Add(this.cmbGames);
            this.frameGames.Name = "frameGames";
            this.frameGames.TabIndex = 0;
            //
            // cmbGames
            //
            this.cmbGames.AccessibleName = "Select Game";
            this.cmbGames.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbGames.FormattingEnabled = true;
            this.cmbGames.Name = "cmbGames";
            this.cmbGames.TabIndex = 0;
            //
            // lblCommentCaption
            //
            this.lblCommentCaption.AutoSize = false;
            this.lblCommentCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCommentCaption.Font = VideoGameManager.UI.Theming.Theme.Fonts.BodyStrong;
            this.lblCommentCaption.ForeColor = VideoGameManager.UI.Theming.Theme.TextSecondary;
            this.lblCommentCaption.Name = "lblCommentCaption";
            this.lblCommentCaption.Padding = new System.Windows.Forms.Padding(0, VideoGameManager.UI.Theming.Theme.Space.S, 0, 0);
            this.lblCommentCaption.TabIndex = 0;
            this.lblCommentCaption.Text = "Your Review";
            this.lblCommentCaption.TextAlign = System.Drawing.ContentAlignment.TopRight;
            //
            // frameComment
            //
            this.frameComment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.frameComment.Controls.Add(this.txtComment);
            this.frameComment.Name = "frameComment";
            this.frameComment.TabIndex = 1;
            //
            // txtComment
            //
            this.txtComment.AcceptsReturn = true;
            this.txtComment.AccessibleName = "Your Review";
            this.txtComment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtComment.Multiline = true;
            this.txtComment.Name = "txtComment";
            this.txtComment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtComment.TabIndex = 0;
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
            this.panelActions.TabIndex = 2;
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
            // ReviewGameForm
            //
            this.AcceptButton = this.btnSave;
            this.ClientSize = new System.Drawing.Size(640, 420);
            this.Controls.Add(this.layoutRoot);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ReviewGameForm";
            this.Text = "Video Game Manager | Review Game";
            this.Load += new System.EventHandler(this.ReviewGameForm_Load);
            this.frameGames.ResumeLayout(false);
            this.frameComment.ResumeLayout(false);
            this.frameComment.PerformLayout();
            this.panelActions.ResumeLayout(false);
            this.layoutRoot.ResumeLayout(false);
            this.layoutRoot.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private VideoGameManager.UI.Controls.LayoutGrid layoutRoot;
        private System.Windows.Forms.Label lblGameCaption;
        private VideoGameManager.UI.Controls.InputFrame frameGames;
        private System.Windows.Forms.ComboBox cmbGames;
        private System.Windows.Forms.Label lblCommentCaption;
        private VideoGameManager.UI.Controls.InputFrame frameComment;
        private System.Windows.Forms.TextBox txtComment;
        private System.Windows.Forms.FlowLayoutPanel panelActions;
        private VideoGameManager.UI.Controls.FlatButton btnSave;
    }
}
