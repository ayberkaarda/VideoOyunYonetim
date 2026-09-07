# ADR 0009 - A second test project, for code that cannot leave Windows

Status: accepted. Amends ADR 0006, which chose a single test project.

## Context

ADR 0006 settled on one test project, `VideoGameManager.Tests`, targeting `net10.0`. The
reason was worth having: one command on a Linux agent runs the whole suite, and a green
result means the whole suite is green, with nothing to qualify.

That decision was made when the code a `net10.0` project could not reach was the
presenters, and the honest answer there was not a second test project but a better home for
the presenters. ADR 0008 gave them one, and they are tested on Linux like everything else.

What is left is smaller and genuinely immovable. The cover art pipeline -
`CachedCoverImageProvider`, and the `CoverImageBox` it feeds - is `System.Drawing`. On .NET
6 and later `System.Drawing.Common` is supported on Windows only, so that code cannot be
compiled into a portable project, cannot be referenced from one, and would not run on a
Linux agent if it were.

Leaving it untested was the status quo, and the status quo is what let a set of avoidable
faults sit in it unnoticed:

- the recommendation screen never used the cache at all and re-downloaded the full-size
  artwork on every request;
- the memory cache was created per form, so it was empty again every time a window opened;
- the disk cache key did not include the size the entry was stored at;
- a failed address was retried, and paid the full timeout, on every selection.

None of these are subtle once a test looks at them. All of them survived review, a green
build and 471 passing tests, because nothing could reach the code.

## Decision

A second test project, `VideoGameManager.Tests.Desktop`, targeting `net10.0-windows` and
referencing the desktop project.

It is deliberately narrow. Code belongs there only when it cannot be tested from
`VideoGameManager.Tests` - in practice, when it needs `System.Drawing` or a `Form`.
Everything else goes in the portable project, as before. The rule is a question with one
answer: *could this test run on Linux?* If yes, it does not belong here.

Continuous integration runs both. The Linux job runs `VideoGameManager.Tests`, integration
tests included; the Windows job builds the solution, runs
`VideoGameManager.Tests.Desktop`, and checks formatting. Both jobs are required, so a
green run still means every test passed - it just takes two runners to say so.

## Rationale

**The property ADR 0006 protected is weakened, not lost.** "One command proves the suite"
becomes "each job proves the half it can, and both must pass". That is a real cost: a
contributor on Linux can no longer run everything locally. It buys coverage of code that
had none and had faults in it, which is the better trade at this size.

**The alternative was to keep the code untestable and rely on looking at it.** That is what
was happening, and the faults above are the result. A cover that silently re-downloads is
invisible in a screenshot.

**Wrapping `System.Drawing` behind an interface to fake it would test the wrapper.** What
needs proving here is the actual behaviour of the scaling and encoding - that proportions
survive, that a small image is not enlarged, that the bytes written for an unchanged image
are the bytes that arrived. A substitute proves none of that.

**The split is on a real boundary, not a convenient one.** Windows-only is a property of
the platform, not a decision this project made, and the test project boundary now matches
the target framework boundary exactly.

## Consequences

- Two test projects, and a contributor has to know which one a new test belongs in. The
  rule above is written into the contribution notes for that reason.
- `dotnet test VideoGameManager.sln` runs both on a Windows machine, which is what a
  developer here uses. On Linux it runs the portable project alone.
- The Windows CI job is no longer only a build: it is a test gate, and its failures matter
  the same way the Linux job's do.
- The desktop test project has no integration tests and should not grow any. Anything that
  needs a database belongs in the portable project, where the container fixtures live.
