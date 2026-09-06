using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VideoGameManager.Services;

namespace VideoGameManager
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application. It builds the configuration and the
        /// service container, then resolves the main form from it: no form constructs
        /// another form or a service directly.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // The UI is English-only, so formatting must not follow the machine's
            // regional settings: on a Turkish Windows a score of 8.6 would render
            // as "8,6".
            CultureInfo culture = CultureInfo.GetCultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // --gallery opens the UI library's visual test page instead of the app. It
            // touches no database, so the theme and the controls can be reviewed without
            // a running SQL Server.
            if (args.Length > 0 && args[0] == "--gallery")
            {
                Application.Run(new UI.DesignGallery());
                return;
            }

            using (ServiceProvider provider = BuildContainer())
            {
                Application.Run(provider.GetRequiredService<MainForm>());
            }
        }

        private static ServiceProvider BuildContainer()
        {
            // appsettings.json is committed and holds a placeholder. The real password
            // lives in appsettings.Development.json, which is git-ignored and overrides it.
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables("VIDEOGAMEMANAGER_")
                .Build();

            ServiceCollection services = new ServiceCollection();

            services.AddSingleton(configuration);
            // Data access and business services register themselves, so this project
            // never references VideoGameManager.Data at compile time.
            services.AddVideoGameManager();

            services.AddTransient<MainForm>();
            services.AddTransient<AddGameForm>();
            services.AddTransient<BrowseGamesForm>();
            services.AddTransient<RecommendationForm>();
            services.AddTransient<ReviewGameForm>();

            return services.BuildServiceProvider();
        }
    }
}
