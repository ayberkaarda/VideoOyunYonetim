using System.Data.Common;

namespace VideoGameManager.Data
{
    /// <summary>
    /// Hands out closed database connections.
    /// </summary>
    /// <remarks>
    /// Repositories take this instead of a connection string, so the provider and the
    /// credentials stay in one place and tests can point the same repositories at a
    /// throwaway database.
    /// </remarks>
    public interface IDbConnectionFactory
    {
        /// <summary>
        /// Creates a new, unopened connection. The caller owns it and must dispose it.
        /// </summary>
        /// <returns>A connection that has not been opened yet.</returns>
        DbConnection Create();
    }
}
