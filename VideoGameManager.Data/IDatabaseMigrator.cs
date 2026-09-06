namespace VideoGameManager.Data
{
    /// <summary>
    /// Brings a database up to the schema this assembly ships.
    /// </summary>
    /// <remarks>
    /// The schema is only ever changed by running these scripts. Editing a table by hand
    /// leaves a database that no clean install can be rebuilt to match.
    /// </remarks>
    public interface IDatabaseMigrator
    {
        /// <summary>
        /// Applies every script the journal does not list. The database must already exist.
        /// </summary>
        /// <returns>
        /// What the run did. A failure is reported here, not thrown.
        /// </returns>
        MigrationOutcome Apply();

        /// <summary>
        /// Creates the database with the required collation when it is missing, then applies.
        /// </summary>
        /// <returns>
        /// What the run did. A failure is reported here, not thrown.
        /// </returns>
        MigrationOutcome CreateAndApply();
    }
}
