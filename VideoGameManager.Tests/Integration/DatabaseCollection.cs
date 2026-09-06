using Xunit;

namespace VideoGameManager.Tests.Integration
{
    /// <summary>
    /// Ties every integration test class to the one SQL Server container.
    /// </summary>
    /// <remarks>
    /// A collection fixture is created once before the first test in the collection and disposed
    /// after the last, so the container starts once for the whole run. Test classes in one
    /// collection also never run at the same time, which is what lets a test empty the tables
    /// before it starts without disturbing another.
    /// </remarks>
    [CollectionDefinition(Name)]
    public sealed class DatabaseCollection : ICollectionFixture<SqlServerFixture>
    {
        /// <summary>
        /// Name the test classes refer to the collection by.
        /// </summary>
        public const string Name = "SqlServer";
    }
}
