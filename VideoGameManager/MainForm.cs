using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace VideoGameManager
{
    /// <summary>
    /// The main menu. It resolves the screen it opens from the container rather than
    /// constructing it, so each screen arrives with its presenter and services already
    /// wired.
    /// </summary>
    public partial class MainForm : VideoGameManager.UI.Controls.ChromelessForm
    {
        private readonly IServiceProvider? _provider;

        /// <summary>Parameterless constructor for the Visual Studio designer only.</summary>
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>The constructor the container uses.</summary>
        public MainForm(IServiceProvider provider) : this()
        {
            _provider = provider;
        }

        /// <summary>
        /// Opens a screen as a modal dialog. Each one is transient, so it is disposed when
        /// the dialog closes.
        /// </summary>
        private void ShowDialog<TForm>() where TForm : Form
        {
            if (_provider is null)
            {
                return;
            }

            using (IServiceScope scope = _provider.CreateScope())
            using (TForm form = scope.ServiceProvider.GetRequiredService<TForm>())
            {
                form.ShowDialog(this);
            }
        }

        private void btnAddGame_Click(object sender, EventArgs e)
        {
            ShowDialog<AddGameForm>();
        }

        private void btnBrowseGames_Click(object sender, EventArgs e)
        {
            ShowDialog<BrowseGamesForm>();
        }

        private void btnRecommend_Click(object sender, EventArgs e)
        {
            ShowDialog<RecommendationForm>();
        }

        private void btnReview_Click(object sender, EventArgs e)
        {
            ShowDialog<ReviewGameForm>();
        }

        private void btnStatistics_Click(object sender, EventArgs e)
        {
            ShowDialog<StatisticsForm>();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
