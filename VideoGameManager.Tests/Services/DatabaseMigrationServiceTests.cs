using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using VideoGameManager.Data;
using VideoGameManager.Services;
using Xunit;

namespace VideoGameManager.Tests.Services
{
    public class DatabaseMigrationServiceTests
    {
        private readonly IDatabaseMigrator _migrator = Substitute.For<IDatabaseMigrator>();
        private readonly DatabaseMigrationService _service;

        public DatabaseMigrationServiceTests()
        {
            _service = new DatabaseMigrationService(_migrator, NullLogger<DatabaseMigrationService>.Instance);
        }

        [Fact]
        public void Constructor_NullMigrator_ThrowsArgumentNullException()
        {
            Action act = () => new DatabaseMigrationService(null!, NullLogger<DatabaseMigrationService>.Instance);

            act.Should().Throw<ArgumentNullException>().WithParameterName("migrator");
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            Action act = () => new DatabaseMigrationService(_migrator, null!);

            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public async Task ApplyPendingAsync_ScriptsWereApplied_ReturnsTheOutcome()
        {
            MigrationOutcome outcome = Outcome(true, "0003_add_review_table.sql", "0004_add_indexes.sql");
            _migrator.Apply().Returns(outcome);

            MigrationOutcome result = await _service.ApplyPendingAsync();

            result.Should().BeSameAs(outcome);
            result.Succeeded.Should().BeTrue();
            result.AppliedScripts.Should().HaveCount(2);
        }

        [Fact]
        public async Task ApplyPendingAsync_NothingWasPending_ReturnsTheOutcome()
        {
            MigrationOutcome outcome = Outcome(true);
            _migrator.Apply().Returns(outcome);

            MigrationOutcome result = await _service.ApplyPendingAsync();

            result.Should().BeSameAs(outcome);
            result.Succeeded.Should().BeTrue();
            result.AppliedScripts.Should().BeEmpty();
        }

        [Fact]
        public async Task ApplyPendingAsync_RunFailed_ReturnsTheOutcomeRatherThanThrowing()
        {
            InvalidOperationException failure = new InvalidOperationException("a script stopped half way");
            MigrationOutcome outcome = new MigrationOutcome(
                false,
                new string[] { "0003_add_review_table.sql" },
                new string[] { "applying 0004_add_indexes.sql" },
                failure);
            _migrator.Apply().Returns(outcome);

            MigrationOutcome result = await _service.ApplyPendingAsync();

            result.Should().BeSameAs(outcome);
            result.Succeeded.Should().BeFalse();
            result.Failure.Should().BeSameAs(failure);
        }

        [Fact]
        public async Task ApplyPendingAsync_AppliesWithoutCreatingTheDatabase()
        {
            // Creating a missing database is a separate decision the start-up path makes; this
            // service only brings an existing one up to date.
            _migrator.Apply().Returns(Outcome(true));

            await _service.ApplyPendingAsync();

            _migrator.Received(1).Apply();
            _migrator.DidNotReceive().CreateAndApply();
        }

        [Fact]
        public async Task ApplyPendingAsync_MigratorThrew_LetsTheFailureTravel()
        {
            InvalidOperationException thrown = new InvalidOperationException("the runner itself broke");
            _migrator.Apply().Throws(thrown);

            Func<Task> act = () => _service.ApplyPendingAsync();

            (await act.Should().ThrowAsync<InvalidOperationException>()).And.Should().BeSameAs(thrown);
        }

        private static MigrationOutcome Outcome(bool succeeded, params string[] appliedScripts) =>
            new MigrationOutcome(succeeded, appliedScripts, new List<string>(), null);
    }
}
