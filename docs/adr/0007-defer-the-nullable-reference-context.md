# ADR 0007 - Defer the nullable reference context

Status: accepted, and now carried out in full. Every project in the solution compiles with
the nullable context on. What follows is the reasoning for deferring it, kept because it
explains the order the work was done in; see "Progress" at the end for how it finished.

## Context

The brief for the final phase asks for an `.editorconfig` with "nullable enable" among its
rules, alongside `var` preferences and naming conventions. The other two are style
settings: a formatter applies them and the code keeps working. Nullable reference types
are not a style setting. Turning the context on changes what the compiler considers a
correct signature, and every place the codebase treats a reference as possibly absent has
to say so in the type.

The size of that change was measured rather than estimated:

```
dotnet build VideoGameManager.sln -p:Nullable=enable --no-incremental
```

**Counting these correctly takes one precaution.** MSBuild prints every diagnostic twice -
once where it occurs and once in the per-project summary - so counting the lines the build
emits reports exactly double the real number. The figures below are unique
`file(line,column): code` triples:

| Diagnostic | What it is |
|---|---|
| CS8625 | `null` passed to a non-nullable parameter |
| CS8618 | non-nullable field or property never assigned |
| CS8600 | converting a possible `null` to a non-nullable type |
| CS8622 | event handler nullability does not match the delegate |
| CS8603 | returning a possible `null` |
| CS8604 | possible `null` passed as an argument |

Roughly 280 distinct diagnostics across the solution when this was written, and the share
per project was lopsided: the desktop project and the test project between them accounted
for about four fifths of it, while Domain had 8 and Data 16.

The continuous integration build runs with `-warnaserror`, so switching the context on
everywhere at once, without clearing every one of them, would leave the build red from the
moment the workflow was added.

## Decision

The nullable reference context stays off for now. `.editorconfig` carries every other rule
the brief asks for. The deviation is recorded here rather than left as a silent omission.

Enabling it is planned as its own piece of work, one layer at a time, in dependency order:

1. **Domain** — the entities and validators are the vocabulary every other layer speaks, so
   their annotations decide what the layers above have to say.
2. **Data** — repository return types are where "no such game" is expressed today by
   returning `null`; those signatures become `Game?` and the callers are told.
3. **Services**.
4. **Presentation**.
5. **Tests** — mostly CS8625: passing `null` deliberately to assert that a guard rejects it,
   which under the nullable context is written `null!`. This is the one place where the
   null-forgiving operator is the right answer, because the test is asserting exactly what
   happens when a caller ignores the annotation.
6. **WinForms** — the largest share is CS8622 on designer-generated event handlers, which is
   a mechanical signature change, and CS8618 on fields the designer assigns in
   `InitializeComponent` after the constructor has run.

## Rationale

**The count is the argument.** 558 diagnostics over 122 files is not a formatting pass; it
is a change to public signatures across every layer. Folding it into a phase whose subject
is CI and documentation would mean one commit that both adds a build pipeline and rewrites
the API surface, and if something broke there would be no way to tell which half did it.

**Annotations are only worth having if they are true.** The fast way to clear 558
diagnostics is `!` and `#nullable disable`, which produces a codebase that claims to be
null-safe and is not. That is worse than an honest `disable`, because a reader would
believe it. Doing it properly means deciding, for each signature, whether absence is a
legitimate answer — which is design work, not cleanup.

**Nothing is lost by waiting.** The validation layer already rejects the cases that matter
at runtime: `GameValidator` and `ReviewValidator` return a `ValidationResult` rather than
letting an absent name reach the database, and `Result<T>` carries failure without a null
payload. The nullable context would document those guarantees in the type system; it would
not add them.

**The order is not arbitrary.** Annotating a layer before the one it depends on means
guessing what the lower layer promises and correcting it later. Domain first means each
subsequent layer is annotated against a settled vocabulary.

## Consequences

- `.editorconfig` describes style only. The setting lives in `Directory.Build.props`, which
  turns the context off for the solution, and a project turns it on by overriding that one
  line.
- The CI build can use `-warnaserror` from the first run, which is what makes it a real gate
  rather than a formality.
- Code written from here on should still avoid returning `null` where a `Result<T>` or an
  empty collection says it better, so the eventual migration is smaller.
- A reader who compares the repository against the brief will find `nullable enable`
  missing from half the solution. This record is the answer to that question.

## Progress

**2026-09-07 - Domain, Data and Services enabled.** The three lowest layers now carry
`<Nullable>enable</Nullable>` and build clean under `-warnaserror`. The whole suite - 471
tests, integration tests included - passed unchanged, and no null-forgiving operator, no
`required` modifier and no in-file pragma was used to get there.

What the annotations say, now that they say something:

- Absence is a real answer for `Game.Genre`, `Game.CoverUrl` and `Game.LatestReview`, for
  the three text fields of `GameFilter`, for `MigrationOutcome.Failure` and
  `DatabaseStatus.Failure`, and for every repository, service and strategy method that
  already returned `null` to mean "no such game". Those are `?` now.
- Absence is not a real answer for `Game.Name`, `Game.Platforms` and `Review.Body`. They
  start empty instead. Validation has always rejected blank and empty with the same
  message, so nothing a caller can observe changed - but `Game.Platforms` now genuinely is
  never `null`, which its documentation had been claiming for some time.
- `Game.Genre` became `string?` rather than taking an empty default, against the first
  guess. The reason is in the schema: `Game.GenreId` is nullable, the listing query reads
  the genre through a `LEFT JOIN`, and there is a test asserting that a game restored from
  the pre-normalisation schema comes back with no genre at all. `string` would have been a
  lie the database can disprove.
- `Result<T>` declares its invariant with `[MemberNotNullWhen(true, nameof(_value))]` on
  `IsSuccess` instead of suppressing the diagnostic where the value is returned. The
  compiler does not verify that attribute, so it is still an assertion by the author - but
  it is made once, in the type, and it pays the caller back: code that checks `IsSuccess`
  is then told by flow analysis that the value is there.

**2026-09-07, later the same day - the rest.** Presentation (29 diagnostics), the desktop
project (106) and the test project (177) followed, and the migrator turned out to have none
at all. The setting now lives in `Directory.Build.props` with no project overriding it: a
new project inherits it rather than having to remember it.

What the last three layers needed:

- **Presentation.** Two view methods were taking a parameter the presenter genuinely passes
  as `null` - `ShowDetails` when nothing is selected, `ShowGame` when no recommendation was
  found. Both are `Game?` now. Sixteen events became `EventHandler?`, which is what the BCL
  does with `INotifyPropertyChanged.PropertyChanged` and for the same reason.
- **The desktop project.** No `#nullable disable` was needed in the designer files: the
  compiler already treats them as generated code, and the designer wires handlers through
  an explicit `new EventHandler(...)`, which does not raise the conversion warning that an
  implicit method group would. So the count was real code - the cover image provider, which
  legitimately returns no image; `SearchBox.Text`, which needed `[AllowNull]` because
  `Control.Text` accepts null and the override was stricter than its base.
- **The tests.** `null!` here is the point rather than a workaround: a test asserting that a
  guard rejects `null` is asserting what happens when a caller ignores the annotation, so
  the annotation has to be overridden rather than the test weakened. Enabling the context
  also surfaced nine `xUnit1012` warnings - `[InlineData(null)]` against a non-nullable
  theory parameter - which are invisible while the context is off.

**One measurement worth keeping.** At every step, the layers already annotated produced no
diagnostics in the layers that were not. A compilation with the context off is oblivious: it
neither honours nor complains about the annotations in what it references. That is what made
this safe to do a layer at a time rather than merely convenient, and it is why the two halves
of the last round - the tests in one working tree, the presenters and forms in another -
could be written in parallel and still compile clean when they were put together.
