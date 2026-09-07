namespace VideoGameManager
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.layoutRoot = new VideoGameManager.UI.Controls.LayoutGrid();
            this.layoutCards = new VideoGameManager.UI.Controls.LayoutGrid();
            this.btnAddGame = new VideoGameManager.UI.Controls.FlatCardButton();
            this.btnBrowseGames = new VideoGameManager.UI.Controls.FlatCardButton();
            this.btnRecommend = new VideoGameManager.UI.Controls.FlatCardButton();
            this.btnReview = new VideoGameManager.UI.Controls.FlatCardButton();
            this.btnStatistics = new VideoGameManager.UI.Controls.FlatCardButton();
            this.panelActions = new System.Windows.Forms.FlowLayoutPanel();
            this.btnExit = new VideoGameManager.UI.Controls.FlatButton();
            this.layoutRoot.SuspendLayout();
            this.layoutCards.SuspendLayout();
            this.panelActions.SuspendLayout();
            this.SuspendLayout();
            //
            // layoutRoot
            //
            this.layoutRoot.ColumnCount = 1;
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.Controls.Add(this.layoutCards, 0, 0);
            this.layoutRoot.Controls.Add(this.panelActions, 0, 1);
            this.layoutRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutRoot.Name = "layoutRoot";
            this.layoutRoot.RowCount = 2;
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.layoutRoot.TabIndex = 0;
            //
            // layoutCards
            //
            this.layoutCards.ColumnCount = 2;
            this.layoutCards.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.layoutCards.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.layoutCards.Controls.Add(this.btnAddGame, 0, 0);
            this.layoutCards.Controls.Add(this.btnBrowseGames, 1, 0);
            this.layoutCards.Controls.Add(this.btnRecommend, 0, 1);
            this.layoutCards.Controls.Add(this.btnReview, 1, 1);
            this.layoutCards.Controls.Add(this.btnStatistics, 0, 2);
            this.layoutCards.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutCards.Name = "layoutCards";
            this.layoutCards.RowCount = 3;
            this.layoutCards.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.layoutCards.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.layoutCards.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.34F));
            this.layoutCards.SetColumnSpan(this.btnStatistics, 2);
            this.layoutCards.TabIndex = 0;
            //
            // btnAddGame
            //
            this.btnAddGame.AccessibleName = "Add Game";
            this.btnAddGame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAddGame.Glyph = "\uE710";
            this.btnAddGame.Name = "btnAddGame";
            this.btnAddGame.TabIndex = 0;
            this.btnAddGame.Text = "Add Game";
            this.btnAddGame.Tone = VideoGameManager.UI.Theming.CardTone.Accent;
            this.btnAddGame.Click += new System.EventHandler(this.btnAddGame_Click);
            //
            // btnBrowseGames
            //
            this.btnBrowseGames.AccessibleName = "Browse Games";
            this.btnBrowseGames.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnBrowseGames.Glyph = "\uE8FD";
            this.btnBrowseGames.Name = "btnBrowseGames";
            this.btnBrowseGames.TabIndex = 1;
            this.btnBrowseGames.Text = "Browse Games";
            this.btnBrowseGames.Tone = VideoGameManager.UI.Theming.CardTone.Success;
            this.btnBrowseGames.Click += new System.EventHandler(this.btnBrowseGames_Click);
            //
            // btnRecommend
            //
            this.btnRecommend.AccessibleName = "Recommendation";
            this.btnRecommend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnRecommend.Glyph = "\uE72C";
            this.btnRecommend.Name = "btnRecommend";
            this.btnRecommend.TabIndex = 2;
            this.btnRecommend.Text = "Recommendation";
            this.btnRecommend.Tone = VideoGameManager.UI.Theming.CardTone.Accent;
            this.btnRecommend.Click += new System.EventHandler(this.btnRecommend_Click);
            //
            // btnReview
            //
            this.btnReview.AccessibleName = "Review Game";
            this.btnReview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnReview.Glyph = "\uE734";
            this.btnReview.Name = "btnReview";
            this.btnReview.TabIndex = 3;
            this.btnReview.Text = "Review Game";
            this.btnReview.Tone = VideoGameManager.UI.Theming.CardTone.Warning;
            this.btnReview.Click += new System.EventHandler(this.btnReview_Click);
            //
            // btnStatistics
            //
            this.btnStatistics.AccessibleName = "Statistics";
            this.btnStatistics.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStatistics.Glyph = "\uE904";
            this.btnStatistics.Name = "btnStatistics";
            this.btnStatistics.TabIndex = 4;
            this.btnStatistics.Text = "Statistics";
            this.btnStatistics.Tone = VideoGameManager.UI.Theming.CardTone.Accent;
            this.btnStatistics.Click += new System.EventHandler(this.btnStatistics_Click);
            //
            // panelActions
            //
            this.panelActions.AutoSize = true;
            this.panelActions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelActions.BackColor = System.Drawing.Color.Transparent;
            this.panelActions.Controls.Add(this.btnExit);
            this.panelActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelActions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.panelActions.Name = "panelActions";
            this.panelActions.TabIndex = 1;
            this.panelActions.WrapContents = false;
            //
            // btnExit
            //
            this.btnExit.AccessibleName = "Exit";
            this.btnExit.Kind = VideoGameManager.UI.Controls.ButtonKind.Secondary;
            this.btnExit.Name = "btnExit";
            this.btnExit.TabIndex = 0;
            this.btnExit.Text = "Exit";
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            //
            // MainForm
            //
            this.AcceptButton = this.btnAddGame;
            this.ClientSize = new System.Drawing.Size(600, 485);
            this.Controls.Add(this.layoutRoot);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Video Game Manager";
            this.layoutCards.ResumeLayout(false);
            this.layoutRoot.ResumeLayout(false);
            this.layoutRoot.PerformLayout();
            this.panelActions.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private VideoGameManager.UI.Controls.LayoutGrid layoutRoot;
        private VideoGameManager.UI.Controls.LayoutGrid layoutCards;
        private VideoGameManager.UI.Controls.FlatCardButton btnAddGame;
        private VideoGameManager.UI.Controls.FlatCardButton btnBrowseGames;
        private VideoGameManager.UI.Controls.FlatCardButton btnRecommend;
        private VideoGameManager.UI.Controls.FlatCardButton btnReview;
        private VideoGameManager.UI.Controls.FlatCardButton btnStatistics;
        private System.Windows.Forms.FlowLayoutPanel panelActions;
        private VideoGameManager.UI.Controls.FlatButton btnExit;
    }
}
