using System;
using System.Globalization;
using System.IO;
using System.Security;
using Microsoft.Extensions.Configuration;
using Serilog.Events;

namespace VideoGameManager.Logging
{
    /// <summary>
    /// The logging settings read from the <c>Serilog</c> section of the application
    /// configuration.
    /// </summary>
    /// <remarks>
    /// Every entry is optional and every unusable value falls back to the default. A
    /// typo in a diagnostic setting must never be the reason the application refuses to
    /// start, because the person who would read the resulting error is the same person
    /// who no longer has a log file to read it in.
    /// </remarks>
    internal sealed class LogOptions
    {
        /// <summary>Name of the configuration section the settings are read from.</summary>
        public const string SectionName = "Serilog";

        /// <summary>Number of daily files kept before the oldest one is deleted.</summary>
        public const int DefaultRetainedFileCountLimit = 7;

        private const LogEventLevel DefaultMinimumLevel = LogEventLevel.Information;
        private const string FolderName = "VideoGameManager";
        private const string LogFolderName = "logs";

        private readonly LogEventLevel _minimumLevel;
        private readonly string _directory;
        private readonly int _retainedFileCountLimit;

        private LogOptions(LogEventLevel minimumLevel, string directory, int retainedFileCountLimit)
        {
            _minimumLevel = minimumLevel;
            _directory = directory;
            _retainedFileCountLimit = retainedFileCountLimit;
        }

        /// <summary>Gets the lowest level that is written.</summary>
        public LogEventLevel MinimumLevel
        {
            get { return _minimumLevel; }
        }

        /// <summary>Gets the folder the log files are written to.</summary>
        public string Directory
        {
            get { return _directory; }
        }

        /// <summary>Gets how many daily files are kept.</summary>
        public int RetainedFileCountLimit
        {
            get { return _retainedFileCountLimit; }
        }

        /// <summary>
        /// Reads the settings, substituting the default for anything missing or unusable.
        /// </summary>
        /// <param name="configuration">The application configuration, which may be null.</param>
        /// <returns>A usable set of options; never null.</returns>
        public static LogOptions FromConfiguration(IConfiguration configuration)
        {
            IConfigurationSection section = configuration == null
                ? null
                : configuration.GetSection(SectionName);

            return new LogOptions(
                ReadMinimumLevel(section),
                ReadDirectory(section),
                ReadRetainedFileCountLimit(section));
        }

        /// <summary>
        /// Gets the folder used when the configuration names none: a per-user location
        /// that is writable without elevation and is not wiped by a reinstall of the
        /// application.
        /// </summary>
        /// <returns>The default log folder.</returns>
        public static string DefaultDirectory()
        {
            string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(root ?? string.Empty, FolderName, LogFolderName);
        }

        private static LogEventLevel ReadMinimumLevel(IConfigurationSection section)
        {
            string configured = section == null ? null : section["MinimumLevel"];
            if (string.IsNullOrWhiteSpace(configured))
            {
                return DefaultMinimumLevel;
            }

            LogEventLevel level;
            if (!Enum.TryParse(configured.Trim(), true, out level) ||
                !Enum.IsDefined(typeof(LogEventLevel), level))
            {
                return DefaultMinimumLevel;
            }

            return level;
        }

        private static string ReadDirectory(IConfigurationSection section)
        {
            string configured = section == null ? null : section["Directory"];
            if (string.IsNullOrWhiteSpace(configured))
            {
                return DefaultDirectory();
            }

            try
            {
                // Expanding first lets the setting be written as %LOCALAPPDATA%\... instead
                // of a path that only exists on one machine.
                return Path.GetFullPath(Environment.ExpandEnvironmentVariables(configured.Trim()));
            }
            catch (ArgumentException)
            {
                // The value is not a path at all. Fall back rather than refuse to start.
                return DefaultDirectory();
            }
            catch (NotSupportedException)
            {
                return DefaultDirectory();
            }
            catch (PathTooLongException)
            {
                return DefaultDirectory();
            }
            catch (SecurityException)
            {
                return DefaultDirectory();
            }
        }

        private static int ReadRetainedFileCountLimit(IConfigurationSection section)
        {
            string configured = section == null ? null : section["RetainedFileCountLimit"];

            int parsed;
            if (!int.TryParse(configured, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
            {
                return DefaultRetainedFileCountLimit;
            }

            // Zero or a negative count would mean "keep nothing", which is never what
            // someone editing this setting is asking for.
            return parsed < 1 ? DefaultRetainedFileCountLimit : parsed;
        }
    }
}
