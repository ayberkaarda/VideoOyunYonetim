# ADR 0008 - Presenters and views move into their own project

Status: accepted

## Context

The presentation layer was built as Model-View-Presenter (ADR 0003): every screen has an
`I…View` interface it implements and a presenter that holds the logic. The presenters were
written so that they mention no WinForms type at all - no `Form`, no `Control`, no
`MessageBox` - precisely so that they could be tested without a window.

They could not be. Both files lived inside `VideoGameManager`, which targets
`net10.0-windows` because it hosts the forms. `VideoGameManager.Tests` targets `net10.0`,
and a `net10.0` project cannot reference a `net10.0-windows` one. So the layer that holds
the screen logic - what happens when a filter changes, which message a failed save shows,
whether deleting the last row of a page steps back a page - was the only layer in the
solution with no tests at all, while the layers around it sat at 100%, 100% and 87% line
coverage.

Two ways out were considered:

1. **A second test project targeting `net10.0-windows`.** It would reference the desktop
   project directly and could test the presenters where they are. But then part of the
   suite only runs on Windows, and the Linux job that runs everything today would stop
   being able to say the suite passed.
2. **Move the presenters and view interfaces into a `net10.0` project of their own.** The
   desktop project references it, the test project references it, and both can.

## Decision

`VideoGameManager.Presentation`, targeting `net10.0`, holding `Presenters/` and `Views/`.
It references Domain and Services. `VideoGameManager` and `VideoGameManager.Tests` both
reference it.

`RootNamespace` stays `VideoGameManager`, so the namespaces do not move: the types are
still `VideoGameManager.Presenters.*` and `VideoGameManager.Views.*`. The forms go on
constructing `Presenters.AddGamePresenter` exactly as before, and not one form file had to
change.

The dependency direction is unchanged:

```
WinForms  ->  Presentation  ->  Services  ->  Data  ->  Domain
```

Presentation reaches `VideoGameManager.Data` types - `GameFilter`, `GameSortField`,
`PagedResult<T>`, `CatalogueStatistics` - through Services, which exposes them in its own
signatures. That is a transitive reference in the same direction as every other arrow, not
a new edge. It is worth naming rather than leaving to be discovered: those types are part
of the contract the service layer offers, so they are effectively shared vocabulary, and
moving them would be a larger decision than this one.

## Rationale

**The tests are the point.** A presenter is where a screen's behaviour lives, and it is the
part most likely to be changed by someone who is not thinking about every case. It is also
the cheapest thing in the codebase to test: the view is an interface, the services are
interfaces, and a substitute for each is two lines.

**Option 1 costs the property that makes the suite trustworthy.** Today one command on a
Linux agent runs everything, integration tests included, and a green result means the whole
suite is green. Splitting the suite across two operating systems means every future reading
of "tests passed" has to be qualified with "which ones".

**The move was almost free because the design was already right.** The presenters held no
WinForms type before this change, which is the only reason a `net10.0` project could
compile them unchanged. The work was mechanical: the screen names used in log scopes had
referred to form types, which no longer exist in this assembly, and public members that had
been internal to the desktop project now needed the XML documentation the other class
libraries produce.

**Keeping the namespaces is what kept the diff small.** Renaming them would have touched
every form and every `using`, turning a project move into a rename that reviewers would have
to read line by line to confirm nothing else changed.

## Consequences

- The presenters have unit tests, and they run on any operating system.
- `VideoGameManager` shrinks to what it should be: forms, the shared control library, the
  composition root, and nothing that could have been tested without a window.
- The screen name recorded in log scopes changed from the form's name to the presenter's -
  `BrowseGamesForm` became `BrowseGamesPresenter`. Log files written before and after this
  change use different names for the same screen.
- `VideoGameManager.Presentation` produces XML documentation, so every public member in it
  must be documented or the build fails. That is a deliberate ratchet: this is now a library
  with two consumers rather than folders inside an executable.
- A future UI - a different desktop toolkit, or a web front end - would consume this project
  rather than reimplement it. That was already true in principle under ADR 0003; it is now
  true in the project graph.
