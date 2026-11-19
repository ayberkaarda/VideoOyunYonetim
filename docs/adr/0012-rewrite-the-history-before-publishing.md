# ADR 0012 - Rewrite the history before publishing

Status: accepted. Supersedes [ADR 0011](0011-keep-the-published-history.md).

## Context

ADR 0011 kept the history as published on the grounds that nothing in it was worth the cost
of a rewrite: the backup held only seed data, and the `.vs/` cache only slowed a clone
down. A closer look before the repository was presented as a portfolio piece found more
than that:

- Third-party artwork. A promotional background image from a console maker's anniversary
  campaign was committed as a resource and embedded, base64-encoded, in early form
  resource files. Screenshots of the browse and recommendation screens showed publishers'
  box art. The repository has no licence to redistribute any of it.
- Local machine details. The `.vs/` cache recorded absolute paths from a developer's
  machine, and the old connection strings named that machine's SQL Server instance.
- Personal addresses. Commits and tags carried personal e-mail addresses rather than the
  account's no-reply address.
- An unrelated screenshot. One early version of a README image was a capture of an
  unrelated terminal session rather than of the application.

## Decision

The history was rewritten once, before the project was presented:

- Build output, the `.vs/` cache, NuGet `packages/`, the database backup and user-specific
  project files were removed from every commit.
- The third-party background image, the form resources that embedded it, every superseded
  screenshot, and both screenshots that showed publishers' cover art were removed from
  every commit.
- Commit, tag and author identities were mapped to the account's no-reply address, and the
  machine's SQL Server instance name was replaced with `localhost`.
- The release tags `v1.0.0` and `v1.1.0` were not recreated. The repository publishes no tags; the versions are listed in the changelog only.

The repository was re-created from the rewritten history, so the old commits are no longer
served by the host.

## Consequences

- Every commit hash changed. The two hashes cited in ADR 0010 were updated; the commit that
  only untracked the build output became empty and no longer exists.
- Early commits no longer build exactly as they did, because files they referenced were
  removed. The current tree builds and its tests pass.
- The README shows no picture of the browse and recommendation screens until they can be
  captured with original artwork. The sample catalogue in `db/seed.sql` still points at
  publishers' cover images by URL; the application loads them at run time and the
  repository does not store them.
- Copies taken before the rewrite, such as forks or local clones, keep the old history. A
  rewrite cannot recall those; it only stops the host from serving them.
