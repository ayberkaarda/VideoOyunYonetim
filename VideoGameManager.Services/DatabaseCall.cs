using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace VideoGameManager.Services
{
    /// <summary>
    /// Turns provider failures into <see cref="DataAccessException"/>.
    /// </summary>
    /// <remarks>
    /// Repositories deliberately do not catch, so the translation happens once, here, instead of
    /// in every service method.
    /// </remarks>
    internal static class DatabaseCall
    {
        /// <summary>
        /// Runs a database call and rewrites a provider failure.
        /// </summary>
        /// <typeparam name="T">Type the call returns.</typeparam>
        /// <param name="operation">The call to run.</param>
        /// <param name="message">Message to put on the wrapping exception.</param>
        /// <returns>Whatever the call returned.</returns>
        /// <exception cref="DataAccessException">The provider reported a failure.</exception>
        internal static async Task<T> RunAsync<T>(Func<Task<T>> operation, string message)
        {
            try
            {
                return await operation().ConfigureAwait(false);
            }
            catch (SqlException ex)
            {
                throw new DataAccessException(message, ex);
            }
        }
    }
}
