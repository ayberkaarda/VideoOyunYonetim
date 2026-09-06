using System;
using VideoGameManager.Services;

namespace VideoGameManager.Presenters
{
    /// <summary>
    /// Turns an exception into something a user can read. The raw message never reaches
    /// the screen: it names servers, tables and drivers, which is both confusing and a
    /// disclosure. Phase 2 adds Serilog so the detail is written to a log file instead.
    /// </summary>
    internal static class Messages
    {
        public const string DatabaseUnreachable =
            "The database is not reachable. Start it with " +
            "\"docker compose -f db/docker-compose.yml up -d\" and try again.";

        public const string Unexpected =
            "Something went wrong. Please try again.";

        public static string ForUser(Exception exception)
        {
            return exception is DataAccessException ? DatabaseUnreachable : Unexpected;
        }
    }
}
