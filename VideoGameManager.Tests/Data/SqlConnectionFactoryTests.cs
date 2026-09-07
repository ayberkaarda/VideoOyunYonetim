using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using VideoGameManager.Data;
using Xunit;

namespace VideoGameManager.Tests.Data
{
    /// <summary>
    /// Exercises <see cref="SqlConnectionFactory"/> without a database: everything here is
    /// about how the connection string is read from configuration, not about the server that
    /// eventually receives it.
    /// </summary>
    public class SqlConnectionFactoryTests
    {
        [Fact]
        public void Constructor_NullConfiguration_ThrowsArgumentNullException()
        {
            Action act = () => new SqlConnectionFactory(null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
        }

        [Fact]
        public void Constructor_ConnectionStringMissing_ThrowsInvalidOperationException()
        {
            IConfiguration configuration = new ConfigurationBuilder().Build();

            Action act = () => new SqlConnectionFactory(configuration);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*ConnectionStrings:VideoGameManager*");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ConnectionStringBlank_ThrowsInvalidOperationException(string value)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:VideoGameManager"] = value,
                })
                .Build();

            Action act = () => new SqlConnectionFactory(configuration);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*ConnectionStrings:VideoGameManager*");
        }

        [Fact]
        public void Create_ConfiguredConnectionString_ReturnsAConnectionCarryingIt()
        {
            const string connectionString = "Server=localhost;Database=VideoGameManager;Trusted_Connection=True;";
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:VideoGameManager"] = connectionString,
                })
                .Build();
            SqlConnectionFactory factory = new SqlConnectionFactory(configuration);

            using (SqlConnection connection = (SqlConnection)factory.Create())
            {
                connection.ConnectionString.Should().Be(connectionString);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ForConnectionString_BlankConnectionString_ThrowsArgumentException(string value)
        {
            Action act = () => SqlConnectionFactory.ForConnectionString(value);

            act.Should().Throw<ArgumentException>().WithParameterName("connectionString");
        }
    }
}
