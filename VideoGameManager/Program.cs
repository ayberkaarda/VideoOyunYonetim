using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace VideoGameManager
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
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

            Application.Run(new MainForm());
        }
    }
}
