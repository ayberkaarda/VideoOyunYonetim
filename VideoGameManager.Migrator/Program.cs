using System;
using System.Globalization;
using VideoGameManager.Data;

namespace VideoGameManager.Migrator
{
    /// <summary>
    /// Command line that brings a database up to the current schema.
    /// </summary>
    /// <remarks>
    /// Kept separate from the desktop application so a database can be prepared before the
    /// application is ever started, and so a build server can create a throwaway database
    /// with the same scripts the application would apply.
    /// </remarks>
    public static class Program
    {
        private const int ExitSuccess = 0;
        private const int ExitFailed = 1;
        private const int ExitUsage = 2;

        /// <summary>
        /// The shape of the argument the operator types, shown in the help text. Every
        /// secret in it is a placeholder; no value here is real.
        /// </summary>
        // secret-guard: bypass-ok Help text rather than a value -- the password is the literal placeholder <password>.
        private const string ExampleConnectionString =
            "Server=localhost,1433;Database=VideoGameManager;User Id=sa;Password=<password>;TrustServerCertificate=True";

        private const string Usage =
            "Applies pending schema migrations, creating the database when it does not exist.\r\n" +
            "\r\n" +
            "  dotnet run --project VideoGameManager.Migrator -- \"<connection string>\"\r\n" +
            "\r\n" +
            "Example:\r\n" +
            "  dotnet run --project VideoGameManager.Migrator -- \\\r\n" +
            "    \"" + ExampleConnectionString + "\"\r\n" +
            "\r\n" +
            "TrustServerCertificate is needed against a local or containerised server, which\r\n" +
            "presents a certificate it signed itself; without it the connection fails during\r\n" +
            "the handshake and the error talks about the connection rather than the certificate.";

        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="args">A single argument: the connection string to migrate.</param>
        /// <returns>
        /// <c>0</c> when the database is up to date, <c>1</c> when a migration failed and
        /// <c>2</c> when the arguments were wrong.
        /// </returns>
        public static int Main(string[] args)
        {
            // Script names and counts are written the same way whatever the machine's
            // regional settings are, so output can be compared between machines.
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            if (args == null || args.Length != 1 || string.IsNullOrWhiteSpace(args[0]))
            {
                Console.Error.WriteLine(Usage);
                return ExitUsage;
            }

            MigrationOutcome outcome = DatabaseMigrator
                .ForConnectionString(args[0])
                .CreateAndApply();

            foreach (string line in outcome.Log)
            {
                Console.WriteLine(line);
            }

            if (!outcome.Succeeded)
            {
                // The message goes to stderr so a script that pipes stdout still sees it, and
                // the whole exception is printed because the audience here is an operator
                // running the tool by hand, not a user of the application.
                Console.Error.WriteLine("Migration failed after applying " +
                    outcome.AppliedScripts.Count + " script(s).");

                if (outcome.Failure != null)
                {
                    Console.Error.WriteLine(outcome.Failure.ToString());
                }

                return ExitFailed;
            }

            foreach (string script in outcome.AppliedScripts)
            {
                Console.WriteLine("Applied: " + script);
            }

            Console.WriteLine(outcome.AppliedScripts.Count + " script(s) applied.");
            Console.WriteLine("Database is up to date.");
            return ExitSuccess;
        }
    }
}
