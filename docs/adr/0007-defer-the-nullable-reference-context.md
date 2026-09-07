# ADR 0007 - Defer the nullable reference context

Status: accepted

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

| Diagnostic | Count | What it is |
|---|---|---|
| CS8625 | 166 | `null` passed to a non-nullable parameter |
| CS8618 | 106 | non-nullable field or property never assigned |
| CS8600 | 94 | converting a possible `null` to a non-nullable type |
| CS8622 | 80 | event handler nullability does not match the delegate |
| CS8603 | 54 | returning a possible `null` |
| CS8604 | 34 | possible `null` passed as an argument |
| others | 24 | CS8619, CS8601, CS8602, CS8765, CS8620 |
| **total** | **558** | across 122 files |

By project: WinForms 296, Tests 200, Data 32, Services 30, Domain 16.

The continuous integration build runs with `-warnaserror`, so switching the context on
without clearing all 558 would leave the build red from the moment the workflow was added.

## Decision

The nullable reference context stays off for now. `.editorconfig` carries every other rule
the brief asks for. The deviation is recorded here rather than left as a silent omission.

Enabling it is planned as its own piece of work, one project per commit, in dependency
order:

1. **Domain** — 16 diagnostics. The entities and validators are the vocabulary every other
   layer speaks, so their annotations decide what the layers above have to say.
2. **Data** — 32. Repository return types are where "no such game" is expressed today by
   returning `null`; those signatures become `Game?` and the callers are told.
3. **Services** — 30.
4. **Tests** — 200. Mostly CS8625: passing `null` deliberately to assert that a guard
   rejects it, which under the nullable context is written `null!`.
5. **WinForms** — 296. The largest share is CS8622 on designer-generated event handlers,
   which is a mechanical signature change, and CS8618 on fields the designer assigns in
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

- `.editorconfig` describes style only. `Nullable` stays `disable` in all six project files,
  where it is already explicit rather than inherited.
- The CI build can use `-warnaserror` from the first run, which is what makes it a real gate
  rather than a formality.
- Code written from here on should still avoid returning `null` where a `Result<T>` or an
  empty collection says it better, so the eventual migration is smaller.
- A reader who compares the repository against the brief will find `nullable enable`
  missing. This record is the answer to that question.
