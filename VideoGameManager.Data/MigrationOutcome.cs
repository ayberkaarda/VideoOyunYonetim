using System;
using System.Collections.Generic;

namespace VideoGameManager.Data
{
    /// <summary>
    /// What one migration run did.
    /// </summary>
    /// <remarks>
    /// A run reports its failure rather than throwing, so the caller decides whether a
    /// database that could not be upgraded is fatal or merely worth a warning.
    /// </remarks>
    /// <param name="Succeeded">
    /// <c>true</c> when every pending script ran to completion.
    /// </param>
    /// <param name="AppliedScripts">
    /// Names of the scripts this run applied, in the order they ran. Empty when the
    /// database was already up to date.
    /// </param>
    /// <param name="Log">
    /// Lines the migrator produced while it ran, kept so a caller can record them.
    /// </param>
    /// <param name="Failure">
    /// The error that stopped the run, or <c>null</c> when it succeeded.
    /// </param>
    public sealed record MigrationOutcome(
        bool Succeeded,
        IReadOnlyList<string> AppliedScripts,
        IReadOnlyList<string> Log,
        Exception? Failure);
}
