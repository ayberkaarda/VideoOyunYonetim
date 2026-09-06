# ADR 0006 - Test stack: xUnit, FluentAssertions 7, NSubstitute, Testcontainers

Status: accepted

## Context

The project needs a test suite that a grader can run with one command, that proves the
service layer behaves as described, and that exercises the repositories against a real
SQL Server rather than a fake. Two constraints shaped the choices.

The first is the database. The repositories are Dapper over hand-written T-SQL, and most
of what can go wrong in them is invisible to an in-memory substitute: `OFFSET/FETCH`
paging, `LIKE` escaping, the collation that decides whether a case-insensitive search
finds anything, the transaction that rolls a genre row back when the game it belonged to
turned out not to exist. A repository test that does not talk to SQL Server tests the
mock.

The second is that the desktop project targets `net10.0-windows` while Domain, Data and
Services target `net10.0`. A test project can reference one group or the other, not both.

## Decision

One test project, `VideoGameManager.Tests`, targeting `net10.0` and referencing Domain,
Data and Services.

| Package | Version | Role |
|---|---|---|
| xunit | 2.9.3 | test framework |
| xunit.runner.visualstudio | 3.1.5 | VSTest adapter |
| Microsoft.NET.Test.Sdk | 18.9.0 | `dotnet test` host |
| FluentAssertions | 7.2.0 | assertions |
| NSubstitute | 6.2.0 | substitutes for the repository interfaces |
| Testcontainers.MsSql | 4.15.0 | throwaway SQL Server for integration tests |
| coverlet.collector | 6.0.4 | line coverage, via `--collect:"XPlat Code Coverage"` |

Unit tests substitute the repository interfaces and never open a connection. Integration
tests raise their own container and never connect to the development database.

## Rationale

**xUnit 2.9.3 rather than the v3 line.** xUnit v3 is the newer package and would also
work, but it defaults to the Microsoft Testing Platform runner, and the coverage figure
this phase has to produce is collected through the VSTest data collector
(`--collect:"XPlat Code Coverage"`). The 2.x line plus `Microsoft.NET.Test.Sdk` is the
combination that path is built for, and coverage is a gate here, not a nicety.

**FluentAssertions pinned to 7.2.0.** From 8.0 the package is published under the Xceed
Community License, which grants free use for non-commercial purposes only and requires a
paid commercial licence for anything else. This repository is published under MIT, and a
test-only dependency that narrows what a reader may do with the code is a poor trade for
assertion syntax. 7.2.0 is the last release under Apache-2.0 - the package metadata
states `<license type="expression">Apache-2.0</license>` - and it carries every assertion
this suite uses. The version is pinned rather than floated so an upgrade is a deliberate
act.

**Testcontainers rather than LocalDB.** LocalDB is a Windows-only, machine-wide
installation that a Linux build agent cannot have, and its version is whatever the
developer's machine happens to carry. Testcontainers starts
`mcr.microsoft.com/mssql/server:2022-latest` - the same image, by the same tag, that the
development database and CI use - so the schema is exercised on the same engine
everywhere. The image is set explicitly rather than left to the library's default tag,
because the point is that the three environments match.

**Coverage through coverlet.** `dotnet test --collect:"XPlat Code Coverage"` writes a
Cobertura report whose per-assembly `line-rate` is the number the ≥80% service-layer
target is measured against. No separate tool, no global install.

## Consequences

- The integration tests need a working Docker daemon. Where there is none they fail
  rather than skip: a suite that reports green without having run is worse than a red
  one, because it is the report a reviewer trusts.
- The first integration run on a machine pays for pulling the SQL Server image, and every
  run pays a container start. One collection fixture serves the whole integration run, so
  that cost is paid once per `dotnet test`, not once per class.
- The integration tests create their own uniquely named database inside their own
  container. They never open a connection to the development container, whose volume
  holds the only copy of the user's catalogue.
- The schema under test comes from the migration chain: the fixture calls the migrator
  rather than running any DDL of its own. That means the migration scripts are covered by
  every integration run, and a schema change that breaks them cannot pass.
- Presenters are not covered. They live in the desktop project, which targets
  `net10.0-windows`, and a `net10.0` test project cannot reference it. Testing them means
  a second test project on the Windows target framework, which would also mean the test
  suite could no longer run in full on a Linux agent. Presenters hold no WinForms types by
  design, so this stays open rather than closed.
