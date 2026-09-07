using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VideoGameManager.Logging;
using VideoGameManager.Services;
using VideoGameManager.UI.Dialogs;

namespace VideoGameManager
{
    internal static class Program
    {
        /// <summary>
        /// Name of the connection string in the configuration. The data access layer reads
        /// the same name; this copy exists only so the startup dialog can say which server
        /// was tried without the presentation layer taking a reference on a database driver.
        /// </summary>
        private const string ConnectionStringName = "VideoGameManager";

        private const string ApplicationTitle = "Video Game Manager";

        private const string MessageLoopFailure =
            "Something went wrong and the last action could not be completed. " +
            "The application is still running.";

        private const string BackgroundFailure =
            "A background operation failed. The application is still running.";

        private const string FatalFailure =
            "Video Game Manager has to close because of an unexpected problem.";

        private const string DatabaseUnreachable =
            "The database is not reachable, so the game library cannot be loaded.";

        private const string MigrationFailure =
            "The database schema could not be updated. Video Game Manager will not run " +
            "against an outdated schema, so the application will now close.";

        private const string SettingsUnreadable =
            "Video Game Manager cannot start because its settings file is missing or " +
            "cannot be read.";

        /// <summary>
        /// A control created on the thread that owns the windows. The handlers below can
        /// run on a worker thread or on the finalizer thread, and a message box has to be
        /// shown from the thread that owns the windows or not at all.
        /// </summary>
        private static Control? _uiMarshal;

        /// <summary>Where the log files went, for the "see the log" line in a message box.</summary>
        private static string? _logFolder;

        /// <summary>
        /// The main entry point for the application. It builds the configuration and the
        /// service container, then resolves the main form from it: no form constructs
        /// another form or a service directly.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            // The UI is English-only, so formatting must not follow the machine's
            // regional settings: on a Turkish Windows a score of 8.6 would render
            // as "8,6".
            CultureInfo culture = CultureInfo.GetCultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            // This has to happen before the first window handle exists, otherwise an
            // exception that reaches the message loop still ends the process.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += OnMessageLoopException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            // --gallery opens the UI library's visual test page instead of the app. It
            // touches no database, so the theme and the controls can be reviewed without
            // a running SQL Server.
            bool galleryOnly = args != null && args.Length > 0 && args[0] == "--gallery";

            // Reading the settings comes first but must not crash before there is
            // anywhere to record why: the logger falls back to its defaults when the
            // configuration is missing, so the failure below still reaches a file.
            Exception? configurationFailure;
            IConfiguration? configuration = BuildConfiguration(galleryOnly, out configurationFailure);

            LogSetupResult logging = LogSetup.Configure(configuration);
            _logFolder = logging.Directory;

            try
            {
                Log.Information(
                    "{Application} {Version} starting. Log file: {LogFile}",
                    ApplicationTitle,
                    ApplicationVersion(),
                    logging.FilePath ?? "(none)");

                if (logging.Failure != null)
                {
                    Log.Warning(
                        logging.Failure,
                        "Writing to a log file is switched off: {LogFolder} could not be opened.",
                        logging.Directory);
                }

                if (configurationFailure != null || configuration == null)
                {
                    Log.Fatal(configurationFailure, "The settings file could not be read, so the application cannot start.");
                    ShowUserMessage(WithLogHint(SettingsUnreadable));
                    return;
                }

                // Touching Handle forces the window handle into existence on this thread,
                // which is what later lets a failure raised on a worker or the finalizer
                // thread be shown from the thread that owns the windows.
                _uiMarshal = new Control();
                _ = _uiMarshal.Handle;

                if (galleryOnly)
                {
                    Application.Run(new UI.DesignGallery());
                    return;
                }

                using (ServiceProvider provider = BuildContainer(configuration))
                {
                    DatabaseAvailability availability = ConfirmDatabase(provider, configuration);
                    if (availability == DatabaseAvailability.Quit)
                    {
                        return;
                    }

                    if (availability == DatabaseAvailability.Reachable && !ApplyMigrations(provider))
                    {
                        return;
                    }

                    Application.Run(provider.GetRequiredService<MainForm>());
                }
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "Startup failed and the application could not be opened.");
                ShowUserMessage(WithLogHint(FatalFailure));
            }
            finally
            {
                if (_uiMarshal != null)
                {
                    _uiMarshal.Dispose();
                    _uiMarshal = null;
                }

                Log.Information("{Application} exiting.", ApplicationTitle);
                Log.CloseAndFlush();
            }
        }

        // ------------------------------------------------------------------
        // Composition
        // ------------------------------------------------------------------

        private static IConfiguration? BuildConfiguration(bool galleryOnly, out Exception? failure)
        {
            failure = null;

            try
            {
                // appsettings.json is committed and holds a placeholder. The real password
                // lives in appsettings.Development.json, which is git-ignored and overrides it.
                // The gallery is a UI-only page, so it stays runnable on a machine that has
                // neither file: the settings are only required on the path that connects.
                return new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: galleryOnly, reloadOnChange: false)
                    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables("VIDEOGAMEMANAGER_")
                    .Build();
            }
            catch (Exception exception)
            {
                // A missing or malformed settings file is reported by the caller once the
                // logger exists. Returning null here leaves the logger on its defaults
                // rather than leaving the user with a stack trace and no window.
                failure = exception;
                return null;
            }
        }

        private static ServiceProvider BuildContainer(IConfiguration configuration)
        {
            ServiceCollection services = new ServiceCollection();

            services.AddSingleton(configuration);

            // Every layer asks for ILogger<T> and gets it from here, so Serilog stays a
            // reference of this project alone.
            services.AddLogging(builder => builder.ClearProviders().AddSerilog(Log.Logger, dispose: false));

            // Data access and business services register themselves, so this project
            // never references VideoGameManager.Data at compile time.
            services.AddVideoGameManager();

            services.AddTransient<MainForm>();
            services.AddTransient<AddGameForm>();
            services.AddTransient<BrowseGamesForm>();
            services.AddTransient<RecommendationForm>();
            services.AddTransient<ReviewGameForm>();
            services.AddTransient<StatisticsForm>();

            return services.BuildServiceProvider();
        }

        private static string ApplicationVersion()
        {
            Version? version = typeof(Program).Assembly.GetName().Version;
            return version == null ? "unknown" : version.ToString();
        }

        // ------------------------------------------------------------------
        // Startup database check
        // ------------------------------------------------------------------

        /// <summary>
        /// What the startup database check settled on, once the user has been asked
        /// anything there was to ask. Distinct from a plain <see cref="bool"/> because the
        /// caller treats "reachable" and "unreachable, continuing anyway" differently:
        /// only the former is a database worth running migrations against.
        /// </summary>
        private enum DatabaseAvailability
        {
            /// <summary>The database answered, either on the first try or on a retry.</summary>
            Reachable,

            /// <summary>The user chose to open the main window with no reachable database.</summary>
            Unavailable,

            /// <summary>The user closed the connection dialog instead of retrying or continuing.</summary>
            Quit
        }

        /// <summary>
        /// Checks that the database answers before the first window opens, and asks the
        /// user what to do when it does not.
        /// </summary>
        /// <param name="provider">The container, used to open a scope per check.</param>
        /// <param name="configuration">Where the connection string is read from, so the
        /// dialog can name the target.</param>
        private static DatabaseAvailability ConfirmDatabase(IServiceProvider provider, IConfiguration configuration)
        {
            DatabaseStatus status = ProbeDatabase(provider);

            if (status != null && status.IsReachable)
            {
                Log.Information("Database answered the startup check.");
                return DatabaseAvailability.Reachable;
            }

            string? reported = status == null ? null : status.Message;
            Log.Error(
                status == null ? null : status.Failure,
                "The database did not answer the startup check. {Reason}",
                string.IsNullOrWhiteSpace(reported) ? DatabaseUnreachable : reported);

            using (ConnectionProblemDialog dialog = new ConnectionProblemDialog(
                reported,
                ConnectionTarget.FromConnectionString(configuration.GetConnectionString(ConnectionStringName)),
                () => CheckDatabaseAsync(provider),
                provider.GetRequiredService<ILoggerFactory>().CreateLogger<ConnectionProblemDialog>()))
            {
                DialogResult choice = dialog.ShowDialog();

                if (choice == DialogResult.OK)
                {
                    Log.Information("The database answered a retry; opening the main window.");
                    return DatabaseAvailability.Reachable;
                }

                if (choice == DialogResult.Continue)
                {
                    Log.Warning("Opening the main window with no reachable database, at the user's request.");
                    return DatabaseAvailability.Unavailable;
                }

                Log.Information("Startup ended at the connection dialog.");
                return DatabaseAvailability.Quit;
            }
        }

        /// <summary>
        /// Applies any pending schema migrations, now that the database is known to be
        /// reachable. Runs before the message loop starts, the same as
        /// <see cref="ProbeDatabase"/>.
        /// </summary>
        /// <returns><see langword="true"/> to continue opening the main window.</returns>
        private static bool ApplyMigrations(IServiceProvider provider)
        {
            Data.MigrationOutcome outcome;

            using (IServiceScope scope = provider.CreateScope())
            {
                Services.IDatabaseMigrationService migrations =
                    scope.ServiceProvider.GetRequiredService<Services.IDatabaseMigrationService>();

                // The message loop has not started yet, so there is neither a UI thread
                // waiting on this call nor a captured synchronization context to deadlock
                // against; the same reasoning as ProbeDatabase's bypass just above.
                outcome = Task.Run(() => migrations.ApplyPendingAsync()).GetAwaiter().GetResult(); // bypass-ok: no message loop and no synchronization context exist yet, so neither side of the deadlock is present
            }

            if (outcome.Log != null && outcome.Log.Count > 0)
            {
                Log.Debug("Migration runner log: {Lines}", string.Join(" | ", outcome.Log));
            }

            if (outcome.Succeeded)
            {
                if (outcome.AppliedScripts != null && outcome.AppliedScripts.Count > 0)
                {
                    Log.Information(
                        "Applied {Count} pending database migration(s): {Scripts}",
                        outcome.AppliedScripts.Count,
                        string.Join(", ", outcome.AppliedScripts));
                }

                return true;
            }

            Log.Fatal(outcome.Failure, "Applying pending database migrations failed.");
            ShowUserMessage(WithLogHint(MigrationFailure));
            return false;
        }

        private static DatabaseStatus ProbeDatabase(IServiceProvider provider)
        {
            try
            {
                // The check is pushed onto a worker thread on purpose, so its awaits
                // resume there instead of being posted back to this thread. This is the
                // one call that runs before the message loop starts, which is why it can
                // be waited on: every later attempt is made from the dialog, where the
                // loop is running and the result is awaited.
                return Task.Run(() => CheckDatabaseAsync(provider)).GetAwaiter().GetResult(); // bypass-ok: no message loop and no synchronization context exist yet, so neither side of the deadlock is present
            }
            catch (Exception exception)
            {
                Log.Error(exception, "The startup database check itself failed.");
                return DatabaseStatus.Unreachable(DatabaseUnreachable, exception);
            }
        }

        private static async Task<DatabaseStatus> CheckDatabaseAsync(IServiceProvider provider)
        {
            // The health check is scoped like every other service, so each attempt gets
            // its own scope and its own connection.
            using (IServiceScope scope = provider.CreateScope())
            {
                IDatabaseHealthService health = scope.ServiceProvider.GetRequiredService<IDatabaseHealthService>();
                return await health.CheckAsync().ConfigureAwait(false);
            }
        }

        // ------------------------------------------------------------------
        // Application-wide failure handling
        // ------------------------------------------------------------------

        private static void OnMessageLoopException(object sender, ThreadExceptionEventArgs e)
        {
            Log.Error(e.Exception, "An exception reached the message loop unhandled.");
            ShowUserMessage(WithLogHint(MessageLoopFailure));
        }

        private static void OnDomainException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Fatal(
                e.ExceptionObject as Exception,
                "An exception reached the application domain unhandled. Terminating: {Terminating}",
                e.IsTerminating);

            // The runtime tears the process down as soon as this returns, and the finally
            // block in Main does not run on that path, so the file is flushed here.
            Log.CloseAndFlush();

            ShowUserMessage(WithLogHint(FatalFailure));
        }

        private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.Error(e.Exception, "A background task failed and nothing observed its result.");

            // Marking it observed keeps the runtime from ending the process over work
            // whose result nobody was waiting for.
            e.SetObserved();

            // This runs on the finalizer thread, so the message is queued rather than
            // waited for: blocking that thread would stall every other cleanup.
            PostUserMessage(WithLogHint(BackgroundFailure));
        }

        private static string WithLogHint(string message)
        {
            string? folder = _logFolder;
            if (string.IsNullOrEmpty(folder))
            {
                return message;
            }

            return message + Environment.NewLine + Environment.NewLine + "Details were written to: " + folder;
        }

        private static void ShowUserMessage(string message)
        {
            try
            {
                Control? marshal = _uiMarshal;
                if (marshal != null && marshal.IsHandleCreated && marshal.InvokeRequired)
                {
                    marshal.Invoke(new Action<string>(ShowMessageBox), message);
                    return;
                }

                ShowMessageBox(message);
            }
            catch (Exception failure)
            {
                // Reporting a failure must never become one. The entry written just above
                // is what a diagnosis relies on; losing the dialog is the smaller loss.
                Log.Error(failure, "The failure dialog could not be shown.");
            }
        }

        private static void PostUserMessage(string message)
        {
            try
            {
                Control? marshal = _uiMarshal;
                if (marshal != null && marshal.IsHandleCreated)
                {
                    marshal.BeginInvoke(new Action<string>(ShowMessageBox), message);
                }
            }
            catch (Exception failure)
            {
                Log.Error(failure, "The failure dialog could not be queued.");
            }
        }

        private static void ShowMessageBox(string message)
        {
            // The exception text itself is deliberately not shown. It names drivers,
            // hosts and file paths, which helps nobody in front of the screen and says
            // more about the machine than a message on screen should. The full detail
            // is in the log file.
            MessageBox.Show(message, ApplicationTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
