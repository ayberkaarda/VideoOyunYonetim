using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using VideoGameManager.Data;
using VideoGameManager.UI.Controls;
using VideoGameManager.Views;

namespace VideoGameManager
{
    /// <summary>
    /// Passive view for the statistics screen. It renders the figures it is given and
    /// raises events; the aggregation and the database live behind the presenter.
    /// </summary>
    public partial class StatisticsForm : VideoGameManager.UI.Controls.ChromelessForm, IStatisticsView
    {
        /// <summary>Stands in for a figure that does not exist, never for a figure of zero.</summary>
        private const string NoValueText = "\u2014";

        /// <summary>Label for the games that belong to no genre at all.</summary>
        /// <remarks>
        /// Those games arrive as a row whose genre is null. The row is drawn like any other
        /// so that the bars still add up to the total printed above them; dropping it would
        /// leave the chart quietly contradicting that total.
        /// </remarks>
        private const string NoGenreLabel = "(no genre)";

        private const string LoadingText = "Loading\u2026";

        private readonly Presenters.StatisticsPresenter _presenter;

        /// <summary>Parameterless constructor for the Visual Studio designer only.</summary>
        public StatisticsForm()
        {
            InitializeComponent();
        }

        /// <summary>The constructor the container uses. It wires the presenter to this view.</summary>
        public StatisticsForm(
            Services.IStatisticsService statistics,
            ILogger<Presenters.StatisticsPresenter> presenterLogger)
            : this()
        {
            _presenter = new Presenters.StatisticsPresenter(this, statistics, presenterLogger);
        }

        public event EventHandler Loaded;

        public event EventHandler RefreshRequested;

        bool Views.IView.IsBusy
        {
            set
            {
                btnRefresh.Enabled = !value;
                Cursor = value ? Cursors.WaitCursor : Cursors.Default;

                // Only the busy state puts the message up; clearing it is left to whichever
                // of ShowStatistics or ShowUnavailable answers, because the presenter drops
                // the busy flag before it reports either one.
                if (value)
                {
                    ShowMessageOverChart(LoadingText);
                }
            }
        }

        public void ShowStatistics(CatalogueStatistics statistics)
        {
            if (statistics is null)
            {
                ShowUnavailable("No statistics are available for this catalogue.");
                return;
            }

            lblTotalGamesValue.Text = statistics.TotalGames.ToString(CultureInfo.InvariantCulture);
            lblReviewCountValue.Text = statistics.ReviewCount.ToString(CultureInfo.InvariantCulture);

            // An unscored catalogue has no average. Showing 0.0 would read as "everything
            // scored zero", which is a different and much worse claim than "nothing scored".
            lblAverageScoreValue.Text = RatingBadge.FormatScore(statistics.AverageScore);

            chartGenres.SetItems(ToBars(statistics.ByGenre));

            lblStatus.Visible = false;
            chartGenres.Visible = true;
        }

        public void ShowUnavailable(string message)
        {
            // The headline figures are blanked as well: leaving the previous reading on
            // screen next to a failure message would present stale numbers as current.
            lblTotalGamesValue.Text = NoValueText;
            lblReviewCountValue.Text = NoValueText;
            lblAverageScoreValue.Text = NoValueText;
            chartGenres.SetItems(null);

            ShowMessageOverChart(message);
        }

        public void ShowError(string message)
        {
            MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public void ShowInfo(string message)
        {
            MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public bool Confirm(string message)
        {
            return MessageBox.Show(this, message, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                   == DialogResult.Yes;
        }

        /// <summary>
        /// Turns the per-genre rows into bars. The numbers are formatted here, with an
        /// explicit culture, so the chart never has to guess how a figure should read.
        /// </summary>
        /// <param name="byGenre">The breakdown, largest first, as the service returned it.</param>
        /// <returns>One bar per row, in the order given.</returns>
        private static IEnumerable<BarChartItem> ToBars(IReadOnlyList<GenreDistribution> byGenre)
        {
            List<BarChartItem> bars = new List<BarChartItem>();

            if (byGenre is null)
            {
                return bars;
            }

            foreach (GenreDistribution row in byGenre)
            {
                if (row is null)
                {
                    continue;
                }

                string label = string.IsNullOrWhiteSpace(row.Genre) ? NoGenreLabel : row.Genre;

                bars.Add(new BarChartItem(
                    label,
                    row.GameCount,
                    row.GameCount.ToString(CultureInfo.InvariantCulture),
                    RatingBadge.FormatScore(row.AverageScore)));
            }

            return bars;
        }

        /// <summary>Puts a single line of text where the chart normally is.</summary>
        private void ShowMessageOverChart(string message)
        {
            chartGenres.Visible = false;
            lblStatus.Text = message;
            lblStatus.Visible = true;
        }

        private void StatisticsForm_Load(object sender, EventArgs e)
        {
            Loaded?.Invoke(this, EventArgs.Empty);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshRequested?.Invoke(this, EventArgs.Empty);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
