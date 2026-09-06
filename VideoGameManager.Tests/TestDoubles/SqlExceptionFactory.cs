using System.Runtime.CompilerServices;
using Microsoft.Data.SqlClient;

namespace VideoGameManager.Tests.TestDoubles
{
    /// <summary>
    /// Hands out a <see cref="SqlException"/> instance that a test can make a repository throw.
    /// </summary>
    /// <remarks>
    /// The provider gives the type no public or internal-but-reachable constructor: an instance
    /// normally only exists because the driver built one from a server response. The service
    /// layer catches this exact type, so a test that cannot produce one cannot prove the
    /// translation into a data access failure happens. Allocating the object without running a
    /// constructor is the only way to get one without a live server.
    /// <para>
    /// The instance carries no error collection, so nothing may read its message or its error
    /// list. Tests use it purely as an identity that is thrown and then found again as an inner
    /// exception.
    /// </para>
    /// </remarks>
    internal static class SqlExceptionFactory
    {
        /// <summary>
        /// Creates an instance that can be thrown and compared by reference.
        /// </summary>
        /// <returns>A provider exception of the type the service layer translates.</returns>
        internal static SqlException Create() =>
            (SqlException)RuntimeHelpers.GetUninitializedObject(typeof(SqlException));
    }
}
