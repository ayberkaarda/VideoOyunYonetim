namespace VideoGameManager
{
    partial class StatisticsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StatisticsForm));
            this.layoutRoot = new VideoGameManager.UI.Controls.LayoutGrid();
            this.headlineCard = new VideoGameManager.UI.Controls.RoundedPanel();
            this.layoutHeadline = new VideoGameManager.UI.Controls.LayoutGrid();
            this.lblTotalGamesValue = new System.Windows.Forms.Label();
            this.lblReviewCountValue = new System.Windows.Forms.Label();
            this.lblAverageScoreValue = new System.Windows.Forms.Label();
            this.lblTotalGamesCaption = new System.Windows.Forms.Label();
            this.lblReviewCountCaption = new System.Windows.Forms.Label();
            this.lblAverageScoreCaption = new System.Windows.Forms.Label();
            this.chartCard = new VideoGameManager.UI.Controls.RoundedPanel();
            this.layoutChart = new VideoGameManager.UI.Controls.LayoutGrid();
            this.lblChartTitle = new System.Windows.Forms.Label();
            this.lblChartLegend = new System.Windows.Forms.Label();
            this.chartGenres = new VideoGameManager.UI.Controls.BarChart();
            this.lblStatus = new System.Windows.Forms.Label();
            this.panelActions = new System.Windows.Forms.FlowLayoutPanel();
            this.btnRefresh = new VideoGameManager.UI.Controls.FlatButton();
            this.btnClose = new VideoGameManager.UI.Controls.FlatButton();
            this.layoutRoot.SuspendLayout();
            this.headlineCard.SuspendLayout();
            this.layoutHeadline.SuspendLayout();
            this.chartCard.SuspendLayout();
            this.layoutChart.SuspendLayout();
            this.panelActions.SuspendLayout();
            this.SuspendLayout();
            //
            // layoutRoot
            //
            this.layoutRoot.ColumnCount = 1;
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.Controls.Add(this.headlineCard, 0, 0);
            this.layoutRoot.Controls.Add(this.chartCard, 0, 1);
            this.layoutRoot.Controls.Add(this.panelActions, 0, 2);
            this.layoutRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutRoot.Name = "layoutRoot";
            this.layoutRoot.RowCount = 3;
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 96F));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.layoutRoot.TabIndex = 0;
            //
            // headlineCard
            //
            this.headlineCard.AccessibleName = "Catalogue totals";
            this.headlineCard.Controls.Add(this.layoutHeadline);
            this.headlineCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.headlineCard.Name = "headlineCard";
            this.headlineCard.Padding = new System.Windows.Forms.Padding(VideoGameManager.UI.Theming.Theme.Space.M);
            this.headlineCard.TabIndex = 0;
            //
            // layoutHeadline
            //
            this.layoutHeadline.ColumnCount = 3;
            this.layoutHeadline.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.layoutHeadline.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.layoutHeadline.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.34F));
            this.layoutHeadline.Controls.Add(this.lblTotalGamesValue, 0, 0);
            this.layoutHeadline.Controls.Add(this.lblReviewCountValue, 1, 0);
            this.layoutHeadline.Controls.Add(this.lblAverageScoreValue, 2, 0);
            this.layoutHeadline.Controls.Add(this.lblTotalGamesCaption, 0, 1);
            this.layoutHeadline.Controls.Add(this.lblReviewCountCaption, 1, 1);
            this.layoutHeadline.Controls.Add(this.lblAverageScoreCaption, 2, 1);
            this.layoutHeadline.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutHeadline.Name = "layoutHeadline";
            this.layoutHeadline.RowCount = 2;
            this.layoutHeadline.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.layoutHeadline.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.layoutHeadline.TabIndex = 0;
            //
            // lblTotalGamesValue
            //
            this.lblTotalGamesValue.AutoSize = false;
            this.lblTotalGamesValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTotalGamesValue.Font = VideoGameManager.UI.Theming.Theme.Fonts.Title;
            this.lblTotalGamesValue.ForeColor = VideoGameManager.UI.Theming.Theme.TextPrimary;
            this.lblTotalGamesValue.Name = "lblTotalGamesValue";
            this.lblTotalGamesValue.TabIndex = 0;
            this.lblTotalGamesValue.Text = "";
            this.lblTotalGamesValue.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            //
            // lblReviewCountValue
            //
            this.lblReviewCountValue.AutoSize = false;
            this.lblReviewCountValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblReviewCountValue.Font = VideoGameManager.UI.Theming.Theme.Fonts.Title;
            this.lblReviewCountValue.ForeColor = VideoGameManager.UI.Theming.Theme.TextPrimary;
            this.lblReviewCountValue.Name = "lblReviewCountValue";
            this.lblReviewCountValue.TabIndex = 0;
            this.lblReviewCountValue.Text = "";
            this.lblReviewCountValue.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            //
            // lblAverageScoreValue
            //
            this.lblAverageScoreValue.AutoSize = false;
            this.lblAverageScoreValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAverageScoreValue.Font = VideoGameManager.UI.Theming.Theme.Fonts.Title;
            this.lblAverageScoreValue.ForeColor = VideoGameManager.UI.Theming.Theme.TextPrimary;
            this.lblAverageScoreValue.Name = "lblAverageScoreValue";
            this.lblAverageScoreValue.TabIndex = 0;
            this.lblAverageScoreValue.Text = "";
            this.lblAverageScoreValue.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            //
            // lblTotalGamesCaption
            //
            this.lblTotalGamesCaption.AutoSize = false;
            this.lblTotalGamesCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTotalGamesCaption.Font = VideoGameManager.UI.Theming.Theme.Fonts.Caption;
            this.lblTotalGamesCaption.ForeColor = VideoGameManager.UI.Theming.Theme.TextSecondary;
            this.lblTotalGamesCaption.Name = "lblTotalGamesCaption";
            this.lblTotalGamesCaption.TabIndex = 0;
            this.lblTotalGamesCaption.Text = "Games";
            this.lblTotalGamesCaption.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            //
            // lblReviewCountCaption
            //
            this.lblReviewCountCaption.AutoSize = false;
            this.lblReviewCountCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblReviewCountCaption.Font = VideoGameManager.UI.Theming.Theme.Fonts.Caption;
            this.lblReviewCountCaption.ForeColor = VideoGameManager.UI.Theming.Theme.TextSecondary;
            this.lblReviewCountCaption.Name = "lblReviewCountCaption";
            this.lblReviewCountCaption.TabIndex = 0;
            this.lblReviewCountCaption.Text = "Reviews";
            this.lblReviewCountCaption.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            //
            // lblAverageScoreCaption
            //
            this.lblAverageScoreCaption.AutoSize = false;
            this.lblAverageScoreCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAverageScoreCaption.Font = VideoGameManager.UI.Theming.Theme.Fonts.Caption;
            this.lblAverageScoreCaption.ForeColor = VideoGameManager.UI.Theming.Theme.TextSecondary;
            this.lblAverageScoreCaption.Name = "lblAverageScoreCaption";
            this.lblAverageScoreCaption.TabIndex = 0;
            this.lblAverageScoreCaption.Text = "Average score";
            this.lblAverageScoreCaption.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            //
            // chartCard
            //
            this.chartCard.AccessibleName = "Games by genre";
            this.chartCard.Controls.Add(this.layoutChart);
            this.chartCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartCard.Name = "chartCard";
            this.chartCard.Padding = new System.Windows.Forms.Padding(VideoGameManager.UI.Theming.Theme.Space.L);
            this.chartCard.TabIndex = 1;
            //
            // layoutChart
            //
            this.layoutChart.CellSpacing = VideoGameManager.UI.Theming.Theme.Space.S;
            this.layoutChart.ColumnCount = 1;
            this.layoutChart.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutChart.Controls.Add(this.lblChartTitle, 0, 0);
            this.layoutChart.Controls.Add(this.lblChartLegend, 0, 1);
            this.layoutChart.Controls.Add(this.chartGenres, 0, 2);
            this.layoutChart.Controls.Add(this.lblStatus, 0, 2);
            this.layoutChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutChart.Name = "layoutChart";
            this.layoutChart.RowCount = 3;
            this.layoutChart.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.layoutChart.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.layoutChart.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutChart.TabIndex = 0;
            //
            // lblChartTitle
            //
            this.lblChartTitle.AutoSize = false;
            this.lblChartTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblChartTitle.Font = VideoGameManager.UI.Theming.Theme.Fonts.BodyStrong;
            this.lblChartTitle.ForeColor = VideoGameManager.UI.Theming.Theme.TextPrimary;
            this.lblChartTitle.Name = "lblChartTitle";
            this.lblChartTitle.TabIndex = 0;
            this.lblChartTitle.Text = "Games by genre";
            this.lblChartTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblChartLegend
            //
            this.lblChartLegend.AutoSize = false;
            this.lblChartLegend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblChartLegend.Font = VideoGameManager.UI.Theming.Theme.Fonts.Caption;
            this.lblChartLegend.ForeColor = VideoGameManager.UI.Theming.Theme.TextSecondary;
            this.lblChartLegend.Name = "lblChartLegend";
            this.lblChartLegend.TabIndex = 0;
            this.lblChartLegend.Text = "Bar length is the number of games. The figure on the right is that genre\'s average score.";
            this.lblChartLegend.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // chartGenres
            //
            this.chartGenres.AccessibleName = "Games by genre";
            this.chartGenres.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartGenres.EmptyText = "No games in the catalogue yet.";
            this.chartGenres.Name = "chartGenres";
            this.chartGenres.TabIndex = 0;
            //
            // lblStatus
            //
            this.lblStatus.AccessibleName = "Statistics status";
            this.lblStatus.AutoSize = false;
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Font = VideoGameManager.UI.Theming.Theme.Fonts.Body;
            this.lblStatus.ForeColor = VideoGameManager.UI.Theming.Theme.TextSecondary;
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Padding = new System.Windows.Forms.Padding(VideoGameManager.UI.Theming.Theme.Space.M);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.Text = "";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblStatus.Visible = false;
            //
            // panelActions
            //
            this.panelActions.AutoSize = true;
            this.panelActions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelActions.BackColor = System.Drawing.Color.Transparent;
            this.panelActions.Controls.Add(this.btnRefresh);
            this.panelActions.Controls.Add(this.btnClose);
            this.panelActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelActions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.panelActions.Margin = new System.Windows.Forms.Padding(0, VideoGameManager.UI.Theming.Theme.Space.M, 0, 0);
            this.panelActions.Name = "panelActions";
            this.panelActions.TabIndex = 2;
            this.panelActions.WrapContents = false;
            //
            // btnRefresh
            //
            this.btnRefresh.AccessibleName = "Refresh";
            this.btnRefresh.Kind = VideoGameManager.UI.Controls.ButtonKind.Primary;
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(140, VideoGameManager.UI.Theming.Theme.Metrics.Button);
            this.btnRefresh.TabIndex = 0;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            //
            // btnClose
            //
            this.btnClose.AccessibleName = "Close";
            this.btnClose.Kind = VideoGameManager.UI.Controls.ButtonKind.Secondary;
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(120, VideoGameManager.UI.Theming.Theme.Metrics.Button);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            // StatisticsForm
            //
            this.AcceptButton = this.btnRefresh;
            this.ClientSize = new System.Drawing.Size(720, 640);
            this.Controls.Add(this.layoutRoot);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "StatisticsForm";
            this.Text = "Video Game Manager | Statistics";
            this.Load += new System.EventHandler(this.StatisticsForm_Load);
            this.layoutHeadline.ResumeLayout(false);
            this.headlineCard.ResumeLayout(false);
            this.layoutChart.ResumeLayout(false);
            this.chartCard.ResumeLayout(false);
            this.panelActions.ResumeLayout(false);
            this.layoutRoot.ResumeLayout(false);
            this.layoutRoot.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private VideoGameManager.UI.Controls.LayoutGrid layoutRoot;
        private VideoGameManager.UI.Controls.RoundedPanel headlineCard;
        private VideoGameManager.UI.Controls.LayoutGrid layoutHeadline;
        private System.Windows.Forms.Label lblTotalGamesValue;
        private System.Windows.Forms.Label lblReviewCountValue;
        private System.Windows.Forms.Label lblAverageScoreValue;
        private System.Windows.Forms.Label lblTotalGamesCaption;
        private System.Windows.Forms.Label lblReviewCountCaption;
        private System.Windows.Forms.Label lblAverageScoreCaption;
        private VideoGameManager.UI.Controls.RoundedPanel chartCard;
        private VideoGameManager.UI.Controls.LayoutGrid layoutChart;
        private System.Windows.Forms.Label lblChartTitle;
        private System.Windows.Forms.Label lblChartLegend;
        private VideoGameManager.UI.Controls.BarChart chartGenres;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.FlowLayoutPanel panelActions;
        private VideoGameManager.UI.Controls.FlatButton btnRefresh;
        private VideoGameManager.UI.Controls.FlatButton btnClose;
    }
}
