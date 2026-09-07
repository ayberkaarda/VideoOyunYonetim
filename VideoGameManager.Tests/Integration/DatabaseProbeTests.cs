using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using VideoGameManager.Data;
using Xunit;

namespace VideoGameManager.Tests.Integration
{
    /// <summary>
    /// Exercises <see cref="SqlDatabaseProbe"/> against a real SQL Server, both when the server
    /// answers and when it does not.
    /// </summary>
    /// <remarks>
    /// Health checks elsewhere in the suite substitute <see cref="IDatabaseProbe"/>, so the
    /// probe itself is never actually run there: this is the only place a real connection is
    /// opened and closed the way <see cref="SqlDatabaseProbe"/> does it.
    /// </remarks>
    [Collection(DatabaseCollection.Name)]
    public sealed class DatabaseProbeTests
    {
        /// <summary>
        /// A port nothing listens on. The failure comes back as a refused TCP connection rather
        /// than a timeout, so the negative case runs quickly.
        /// </summary>
        private const string UnreachableConnectionString =
            "Server=127.0.0.1,1;Database=master;TrustServerCertificate=True;Connect Timeout=2;";

        private readonly SqlServerFixture _fixture;

        /// <summary>Creates the test class against the shared server.</summary>
        /// <param name="fixture">The running SQL Server.</param>
        public DatabaseProbeTests(SqlServerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task PingAsync_ServerReachable_CompletesWithoutThrowing()
        {
            SqlDatabaseProbe probe = new SqlDatabaseProbe(_fixture.Connections);

            Func<Task> act = () => probe.PingAsync();

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task PingAsync_ServerUnreachable_ThrowsSqlException()
        {
            IDbConnectionFactory unreachable = SqlConnectionFactory.ForConnectionString(UnreachableConnectionString);
            SqlDatabaseProbe probe = new SqlDatabaseProbe(unreachable);

            Func<Task> act = () => probe.PingAsync();

            await act.Should().ThrowAsync<SqlException>();
        }
    }
}
