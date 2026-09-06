using System;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace VideoGameManager.Logging
{
    /// <summary>
    /// What <see cref="LogSetup.Configure"/> managed to set up, so the caller can say so
    /// in the first log line instead of guessing.
    /// </summary>
    internal sealed class LogSetupResult
    {
        private readonly string _directory;
        private readonly string _filePath;
        private readonly Exception _failure;

        internal LogSetupResult(string directory, string filePath, Exception failure)
        {
            _directory = directory;
            _filePath = filePath;
            _failure = failure;
        }

        /// <summary>Gets the folder the log files were meant to go to.</summary>
        public string Directory
        {
            get { return _directory; }
        }

        /// <summary>
        /// Gets the file name pattern that is written to, or null when the file sink
        /// could not be opened.
        /// </summary>
        public string FilePath
        {
            get { return _filePath; }
        }

        /// <summary>
        /// Gets why the file sink could not be opened, or null when it was opened.
        /// </summary>
        public Exception Failure
        {
            get { return _failure; }
        }
    }

    /// <summary>
    /// Builds the application logger from configuration and installs it as the static
    /// logger, which is what the dependency injection container then hands out as
    /// <c>ILogger&lt;T&gt;</c>.
    /// </summary>
    internal static class LogSetup
    {
        private const string FileNamePattern = "videogamemanager-.log";

        private const string OutputTemplate =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}";

        /// <summary>
        /// Configures <see cref="Log.Logger"/> from the given configuration.
        /// </summary>
        /// <param name="configuration">The application configuration, which may be null.</param>
        /// <returns>Where the log went, and why it did not go to a file if it did not.</returns>
        public static LogSetupResult Configure(IConfiguration configuration)
        {
            LogOptions options = LogOptions.FromConfiguration(configuration);

            LoggerConfiguration builder = new LoggerConfiguration()
                .MinimumLevel.Is(options.MinimumLevel)
                .Enrich.FromLogContext();

            string filePath = null;
            Exception failure = null;

            try
            {
                System.IO.Directory.CreateDirectory(options.Directory);
                filePath = Path.Combine(options.Directory, FileNamePattern);

                builder = builder.WriteTo.File(
                    path: filePath,
                    outputTemplate: OutputTemplate,

                    // The machine's regional settings must not reach the file. With a
                    // locale that uses a comma as the decimal separator a score of 8.6
                    // is written as "8,6" and the timestamp layout shifts as well, so
                    // two machines produce log files that cannot be compared or parsed
                    // by the same tool.
                    formatProvider: CultureInfo.InvariantCulture,

                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: options.RetainedFileCountLimit,

                    // A second copy of the application must not be left with no log at
                    // all because the first one holds the file open.
                    shared: true,

                    encoding: Encoding.UTF8);
            }
            catch (Exception exception)
            {
                // A log file is a diagnostic aid, not a precondition for running: an
                // unwritable folder degrades the application to no file rather than
                // stopping it from starting. The reason travels back to the caller,
                // which reports it instead of discarding it.
                failure = exception;
                filePath = null;
            }

            Log.Logger = builder.CreateLogger();

            return new LogSetupResult(options.Directory, filePath, failure);
        }
    }
}
