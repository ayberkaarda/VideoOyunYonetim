using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using VideoGameManager.Domain;
using VideoGameManager.UI.Controls;
using VideoGameManager.Views;

namespace VideoGameManager
{
    /// <summary>
    /// Passive view for the "browse games" screen. It renders what it is given and reports
    /// which row is highlighted; the queries live behind the presenter.
    /// </summary>
    public partial class BrowseGamesForm : VideoGameManager.UI.Controls.ChromelessForm, IGameListView
    {
        private readonly Presenters.BrowseGamesPresenter _presenter;

        /// <summary>Parameterless constructor for the Visual Studio designer only.</summary>
        public BrowseGamesForm()
        {
            InitializeComponent();

            picCover.Provider = new VideoGameManager.UI.CachedCoverImageProvider();
        }

        /// <summary>The constructor the container uses. It wires the presenter to this view.</summary>
        public BrowseGamesForm(
            Services.IGameService games,
            ILogger<Presenters.BrowseGamesPresenter> presenterLogger,
            ILogger<VideoGameManager.UI.CachedCoverImageProvider> coverLogger)
            : this()
        {
            picCover.Provider = new VideoGameManager.UI.CachedCoverImageProvider(coverLogger);
            _presenter = new Presenters.BrowseGamesPresenter(this, games, presenterLogger);
        }

        public event EventHandler Loaded;

        public event EventHandler SelectionChanged;

        IReadOnlyList<Game> Views.IGameListView.Games
        {
            set
            {
                lblListStatus.Visible = false;
                lstGames.Visible = true;

                lstGames.Items.Clear();
                foreach (Game game in value)
                {
                    lstGames.Items.Add(new GameRow(game));
                }
            }
        }

        public int? SelectedGameId => lstGames.SelectedItem is GameRow row ? row.Id : (int?)null;

        bool Views.IView.IsBusy
        {
            set => Cursor = value ? Cursors.WaitCursor : Cursors.Default;
        }

        public void ShowDetails(Game game)
        {
            if (game is null)
            {
                lblName.Text = string.Empty;
                lblGenre.Text = string.Empty;
                lblPlatform.Text = string.Empty;
                badgeScore.Score = null;
                lblComment.Text = string.Empty;
                _ = picCover.LoadCoverAsync(null);
                return;
            }

            lblName.Text = game.Name;
            lblGenre.Text = game.Genre;
            lblPlatform.Text = JoinPlatforms(game.Platforms);
            badgeScore.Score = game.Score;
            lblComment.Text = game.LatestReview;

            // LoadCoverAsync cancels a load still in flight for a previous selection, so
            // arrowing through the list quickly can never leave a stale cover on screen.
            // It drives IsLoading itself and swallows provider failures.
            _ = picCover.LoadCoverAsync(game.CoverUrl);
        }

        public void ShowListUnavailable(string message)
        {
            lstGames.Items.Clear();
            lstGames.Visible = false;
            lblListStatus.Text = message;
            lblListStatus.Visible = true;

            ShowDetails(null);
        }

        public void ShowDetailsUnavailable(string message)
        {
            lblName.Text = string.Empty;
            lblGenre.Text = string.Empty;
            lblPlatform.Text = string.Empty;
            badgeScore.Score = null;
            lblComment.Text = message;
            _ = picCover.LoadCoverAsync(null);
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

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BrowseGamesForm_Load(object sender, EventArgs e)
        {
            Loaded?.Invoke(this, EventArgs.Empty);
        }

        private void lstGames_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectionChanged?.Invoke(this, EventArgs.Empty);
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

        /// <summary>Adapts a <see cref="Game"/> to the two-line row the list draws.</summary>
        private sealed class GameRow : IGameListItem
        {
            private readonly Game _game;

            public GameRow(Game game) => _game = game;

            public int Id => _game.Id;

            public string PrimaryText => _game.Name;

            public string SecondaryText => string.Join(
                "  -  ",
                Parts(JoinPlatforms(_game.Platforms), _game.Genre));

            public double? Score => _game.Score;

            public override string ToString() => _game.Name;

            private static IEnumerable<string> Parts(params string[] values)
            {
                foreach (string value in values)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        yield return value;
                    }
                }
            }
        }
    }
}
