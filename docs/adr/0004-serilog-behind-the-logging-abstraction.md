# ADR 0004 - Serilog behind `Microsoft.Extensions.Logging`

Status: accepted

## Context

Every failure in the application used to end in `MessageBox.Show(ex.Message)`: the user saw
a server name and a stack frame, and nothing was written down. Once the screens moved onto
presenters, the raw text stopped reaching the user - but it still went nowhere, so a report
of "it crashed once" left no trace to read.

The application needs a log file. It also has four projects, three of which must stay
usable without a desktop host: `Domain`, `Data` and `Services` are referenced by the test
project and are meant to run on a build agent with no UI.

## Decision

Use Serilog with a rolling file sink, but let the rest of the codebase see only
`Microsoft.Extensions.Logging.ILogger<T>`.

- The WinForms host is the only project that references Serilog. It configures the sink,
  registers the provider with `AddLogging`, and flushes on exit.
- `Services` and `Data` take `ILogger<T>` through their constructors and reference
  `Microsoft.Extensions.Logging.Abstractions` only.
- `Domain` keeps no dependency at all, logging included.

Log destination: a daily rolling file under `%LOCALAPPDATA%\VideoGameManager\logs`, seven
files retained. The directory and the minimum level are read from the `Serilog` section of
`appsettings.json`, and both have defaults, so the application starts with no configuration
present.

## Rationale

The specification names Serilog, and the file sink is what a desktop application without a
log server needs. Fronting it with the framework abstraction costs one extra package and
buys the dependency direction: a unit test can pass `NullLogger<T>.Instance` and never
touch a sink, and swapping the sink later changes one file in one project.

Writing under `LocalApplicationData` rather than next to the executable keeps the log out of
the build output, where `dotnet clean` or a rebuild would delete it, and works when the
application sits in a directory the user cannot write to.

`Serilog.Settings.Configuration` was the alternative to reading two keys by hand. It was
left out: it would pull the whole Serilog configuration surface into `appsettings.json` for
two settings that this application actually varies.

## Consequences

- An exception is logged where it is swallowed - in a presenter `catch` or in a global
  handler - and nowhere else. Services translate provider failures and rethrow without
  logging, so one failure produces one entry rather than a chain of duplicates.
- Services log completed writes at `Information` and rejected input at `Warning`, which
  makes the log readable as a history of what the catalogue did.
- The log holds no connection string, password or user name.
- Formatting is pinned to the invariant culture. On a machine with a comma decimal
  separator the log would otherwise disagree with the English user interface about what a
  score looks like.
- The file sink is best effort: if the directory cannot be created, the application starts
  anyway without a log rather than refusing to run.
