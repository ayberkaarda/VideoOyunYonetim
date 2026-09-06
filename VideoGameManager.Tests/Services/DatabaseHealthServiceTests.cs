using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using VideoGameManager.Data;
using VideoGameManager.Services;
using VideoGameManager.Tests.TestDoubles;
using Xunit;

namespace VideoGameManager.Tests.Services
{
    public class DatabaseHealthServiceTests
    {
        private readonly IDatabaseProbe _probe = Substitute.For<IDatabaseProbe>();
        private readonly DatabaseHealthService _service;

        public DatabaseHealthServiceTests()
        {
            _service = new DatabaseHealthService(_probe, NullLogger<DatabaseHealthService>.Instance);
        }

        [Fact]
        public void Constructor_NullProbe_ThrowsArgumentNullException()
        {
            Action act = () => new DatabaseHealthService(null, NullLogger<DatabaseHealthService>.Instance);

            act.Should().Throw<ArgumentNullException>().WithParameterName("probe");
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            Action act = () => new DatabaseHealthService(_probe, null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public async Task CheckAsync_ProbeAnswered_ReportsTheDatabaseAsReachable()
        {
            _probe.PingAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

            DatabaseStatus status = await _service.CheckAsync();

            status.IsReachable.Should().BeTrue();
            status.Message.Should().BeEmpty();
            status.Failure.Should().BeNull();
        }

        [Fact]
        public async Task CheckAsync_PassesTheTokenToTheProbe()
        {
            CancellationToken ct = new CancellationTokenSource().Token;
            _probe.PingAsync(ct).Returns(Task.CompletedTask);

            await _service.CheckAsync(ct);

            await _probe.Received(1).PingAsync(ct);
        }

        [Fact]
        public async Task CheckAsync_ServerCouldNotBeReached_ReportsItAsUnreachableAndKeepsTheFailure()
        {
            SqlException provider = SqlExceptionFactory.Create();
            _probe.PingAsync(Arg.Any<CancellationToken>()).Throws(provider);

            DatabaseStatus status = await _service.CheckAsync();

            status.IsReachable.Should().BeFalse();
            status.Message.Should().Contain("could not be reached");
            status.Failure.Should().BeSameAs(provider);
        }

        [Fact]
        public async Task CheckAsync_ConnectionIsNotConfigured_ReportsItAsAConfigurationProblem()
        {
            InvalidOperationException misconfigured =
                new InvalidOperationException("the connection string entry is missing");
            _probe.PingAsync(Arg.Any<CancellationToken>()).Throws(misconfigured);

            DatabaseStatus status = await _service.CheckAsync();

            status.IsReachable.Should().BeFalse();
            status.Message.Should().Contain("not set up correctly");
            status.Failure.Should().BeSameAs(misconfigured);
        }

        [Fact]
        public async Task CheckAsync_TheWaitWasCancelled_RethrowsInsteadOfCallingTheDatabaseDown()
        {
            // Giving up on the wait is not evidence about the database, so it must not come back
            // as a health verdict that a screen would show as an outage.
            OperationCanceledException cancelled = new OperationCanceledException();
            _probe.PingAsync(Arg.Any<CancellationToken>()).Throws(cancelled);

            Func<Task> act = () => _service.CheckAsync();

            (await act.Should().ThrowAsync<OperationCanceledException>()).And.Should().BeSameAs(cancelled);
        }

        [Fact]
        public async Task CheckAsync_ProbeFailedForAnUnexpectedReason_LetsItTravelUnchanged()
        {
            TimeoutException unrelated = new TimeoutException("the wait ran out");
            _probe.PingAsync(Arg.Any<CancellationToken>()).Throws(unrelated);

            Func<Task> act = () => _service.CheckAsync();

            (await act.Should().ThrowAsync<TimeoutException>()).And.Should().BeSameAs(unrelated);
        }
    }

    public class DatabaseStatusTests
    {
        [Fact]
        public void Reachable_CarriesNoMessageAndNoFailure()
        {
            DatabaseStatus status = DatabaseStatus.Reachable();

            status.IsReachable.Should().BeTrue();
            status.Message.Should().BeEmpty();
            status.Failure.Should().BeNull();
        }

        [Fact]
        public void Unreachable_CarriesTheMessageAndTheFailure()
        {
            InvalidOperationException failure = new InvalidOperationException("cause");

            DatabaseStatus status = DatabaseStatus.Unreachable("the server did not answer", failure);

            status.IsReachable.Should().BeFalse();
            status.Message.Should().Be("the server did not answer");
            status.Failure.Should().BeSameAs(failure);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Unreachable_WithoutAMessage_ThrowsArgumentException(string message)
        {
            Action act = () => DatabaseStatus.Unreachable(message, new InvalidOperationException("cause"));

            act.Should().Throw<ArgumentException>().WithParameterName("message");
        }

        [Fact]
        public void Unreachable_WithoutAFailure_ThrowsArgumentNullException()
        {
            Action act = () => DatabaseStatus.Unreachable("the server did not answer", null);

            act.Should().Throw<ArgumentNullException>().WithParameterName("failure");
        }
    }
}
