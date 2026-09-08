# ADR 0010 - English as the primary language, with a Turkish translation of the README

Status: accepted. Records a deliberate deviation from the modernisation brief.

## Context

The brief asked for two things about language. The final phase was to produce "a Turkish
main README plus an English `README.en.md`", and an earlier phase was to keep the Turkish
strings already in the user interface, moving new ones into resource files so that
localisation stayed possible later.

Both were reasonable when written. Neither survived contact with what the project became.

The user interface was translated to English early, at the project owner's instruction, and
the translation was total: every form caption, every message, every column header, the
sample data, the database itself. The screenshots in the README were retaken against the
English screens. From that point a Turkish README described an application whose every
label was English, using names that no longer appeared anywhere on screen.

The code had already gone the same way. Identifiers, comments, XML documentation, SQL,
log messages and commit subjects are English throughout, and the entities the brief itself
named - `Game`, `Review`, `Genre`, `Platform` - were English from the first phase.

So the question was never "which language is this project in". It was only which file gets
the name `README.md`, because that is the one GitHub renders on the landing page.

## Decision

English is the primary language of the repository. `README.md` is English. A faithful
Turkish translation lives beside it as `README.tr.md`, and the two change together.

Turkish appears in exactly one tracked file: `README.tr.md`. Nowhere else - not in code,
comments, SQL, resource values, screenshot filenames or commit messages.

New user-facing strings still go into the resource files rather than being written inline,
as the brief asked, even though only one language ships.

## Rationale

**The primary README should be in the language of the thing it documents.** The
application, the code and the screenshots are English. A Turkish landing page would be a
translation layer over an English artefact, and the first place it would mislead is the
part a newcomer needs most: the walkthrough of screens whose buttons it would have to name
twice.

**`README.tr.md` rather than `README.en.md` is the same decision seen from the other side.**
Only one of the two files can be the default, and the default should match the code.

**The translation has a real maintenance cost, and it has already been paid once.** The
Turkish README was deleted outright in `12227bb`: it had fallen behind its English
counterpart and five of its image references were broken. It was reinstated in `23538a3`,
in full, because being able to read the project in Turkish was worth the upkeep. What made
it rot was drift, not translation - so the rule attached to it is that a change to
`README.md` is incomplete until the same change is in `README.tr.md`.

**Keeping the resource files in the loop costs nothing now and keeps the option open.**
Strings live in `.resx` because that is where a second shipped language would come from.
That the project ships one language today does not argue for hard-coding.

## Consequences

- Any edit to `README.md` lands in `README.tr.md` in the same change. A pull request that
  touches one and not the other is incomplete.
- A reader comparing the repository against the brief finds the two README languages
  swapped and no `README.en.md`. That is this record's reason for existing.
- The user interface has no Turkish strings and will not regain any. Restoring them would
  mean retaking every screenshot and retranslating the sample data, which is a larger
  change than it looks.
- Adding a third language is a new decision, not an extension of this one. Two translations
  drift faster than one, and the mechanism here - discipline, not tooling - does not scale
  to three.
