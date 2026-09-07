using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using VideoGameManager.Domain;
using VideoGameManager.Views;

namespace VideoGameManager
{
    /// <summary>
    /// Passive view for the recommendation screen. It renders what it is given and raises
    /// events; the picking rule and the database live behind the presenter.
    /// </summary>
    public partial class RecommendationForm : VideoGameManager.UI.Controls.ChromelessForm, IRecommendationView
    {
        private readonly Presenters.RecommendationPresenter? _presenter;

        /// <summary>Parameterless constructor for the Visual Studio designer only.</summary>
        public RecommendationForm()
        {
            InitializeComponent();
        }

        /// <summary>The constructor the container uses. It wires the presenter to this view.</summary>
        /// <param name="recommendations">The picking service behind the presenter.</param>
        /// <param name="covers">
        /// The shared cover cache. It is handed in rather than created here so that every
        /// screen draws from the same one and artwork survives a window closing.
        /// </param>
        /// <param name="presenterLogger">Logger for the presenter.</param>
        public RecommendationForm(
            Services.IRecommendationService recommendations,
            UI.Controls.ICoverImageProvider covers,
            ILogger<Presenters.RecommendationPresenter> presenterLogger)
            : this()
        {
            _presenter = new Presenters.RecommendationPresenter(this, recommendations, presenterLogger);
            picCover.Provider = covers;
        }

        public event EventHandler? Loaded;

        public event EventHandler? RecommendationRequested;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<string> Strategies
        {
            set
            {
                cmbStrategy.Items.Clear();

                if (value != null)
                {
                    foreach (string name in value)
                    {
                        cmbStrategy.Items.Add(new StrategyItem(name, DisplayNameFor(name)));
                    }
                }

                if (cmbStrategy.Items.Count > 0)
                {
                    cmbStrategy.SelectedIndex = 0;
                }
            }
        }

        public string? SelectedStrategy => cmbStrategy.SelectedItem is StrategyItem item ? item.Identifier : null;

        bool Views.IView.IsBusy
        {
            set
            {
                btnRecommend.Enabled = !value;
                Cursor = value ? Cursors.WaitCursor : Cursors.Default;
            }
        }

        public void ShowGame(Game? game)
        {
            lblStatus.Visible = false;
            layoutDetails.Visible = true;

            if (game is null)
            {
                lblName.Text = string.Empty;
                lblGenre.Text = string.Empty;
                lblPlatform.Text = string.Empty;
                lblScore.Text = string.Empty;
                _ = picCover.LoadCoverAsync(null);
                return;
            }

            lblName.Text = game.Name;
            lblGenre.Text = game.Genre;
            lblPlatform.Text = JoinPlatforms(game.Platforms);
            lblScore.Text = game.Score?.ToString("0.#", CultureInfo.CurrentCulture) ?? string.Empty;

            // The box owns the fetch: it cancels whatever was in flight, runs its loading
            // state and settles on either the artwork or its empty state. Nothing here has
            // to be awaited, and a failure is reported by the cache rather than surfacing
            // as an exception on this thread.
            _ = picCover.LoadCoverAsync(game.CoverUrl);
        }

        public void ShowLoadError(string message)
        {
            _ = picCover.LoadCoverAsync(null);
            layoutDetails.Visible = false;
            lblStatus.Text = message;
            lblStatus.Visible = true;
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

        private void btnRecommend_Click(object? sender, EventArgs e)
        {
            RecommendationRequested?.Invoke(this, EventArgs.Empty);
        }

        private void RecommendationForm_Load(object? sender, EventArgs e)
        {
            Loaded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Joins a game's platforms for display in one label. A game can carry more than
        /// one platform; today's catalogue has exactly one per game, so the joined text
        /// looks the same as before. A missing or empty list renders as an empty string
        /// rather than a null-reference failure.
        /// </summary>
        private static string JoinPlatforms(IReadOnlyList<string> platforms)
        {
            return platforms is null || platforms.Count == 0
                ? string.Empty
                : string.Join(", ", platforms);
        }

        /// <summary>
        /// Turns a strategy identifier into text a person reads comfortably. This is the one
        /// place that mapping lives; an identifier with no entry here still shows up in the
        /// picker, spelled exactly as the service returned it, rather than being dropped.
        /// </summary>
        private static string DisplayNameFor(string identifier)
        {
            switch (identifier)
            {
                case "Random":
                    return "Random";
                case "GenreWeighted":
                    return "Genre weighted";
                case "BacklogFirst":
                    return "Backlog first";
                default:
                    return identifier;
            }
        }

        /// <summary>
        /// One entry in the strategy picker: the identifier the service expects, paired with
        /// the text the combo box shows. <see cref="ToString"/> is what the combo box renders.
        /// </summary>
        private sealed class StrategyItem
        {
            internal StrategyItem(string identifier, string displayText)
            {
                Identifier = identifier;
                DisplayText = displayText;
            }

            internal string Identifier { get; }

            private string DisplayText { get; }

            public override string ToString() => DisplayText;
        }
    }
}
