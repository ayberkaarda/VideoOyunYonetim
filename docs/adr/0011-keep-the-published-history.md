# ADR 0011 - Keep the published history as it is

Status: accepted. Records a deliberate deviation from the modernisation brief.

## Context

The brief's first phase asked for two things to be removed from the repository: the Visual
Studio cache directory `.vs/` and the 4.7 MB SQL Server backup. It asked for them to go
from the history as well as from the working tree, by rewriting the history and
force-pushing the result.

The working tree half was done at the time. Commit `64e7ea9` untracked both, and they stay
on the developer's disk. The history half was not, and the question of whether to do it has
come back at the close of every round since. This record closes it.

What was measured before deciding:

- The whole of `.git` is 7.0 MB. Removing the two paths from every commit would save a few
  megabytes from a clone nobody has found slow.
- A secrets scan over every commit in the history finds nothing. The backup holds fifteen
  catalogue rows and two reviews, and the same fifteen rows, with the reviews translated,
  are checked in as `db/seed.sql`. Retrieving the backup from an old commit reveals nothing
  the repository does not already publish in a more readable form.
- The repository is public. A rewritten history cannot be recalled: the original commits
  stay reachable by hash for as long as the host keeps them, and every fork, mirror and
  cache keeps them regardless. The rewrite would not achieve what it was asked for.
- The release tags name commits in this history, and so do the commit hashes cited
  throughout the changelog and these records. A rewrite invalidates all of them at once,
  and each one would have to be re-cut or re-pointed by hand.

## Decision

The history is not rewritten. `.vs/` and the backup remain in the commits that once tracked
them, and the published history stays exactly as it was published.

## Consequences

- A clone stays at its current size, and every commit hash cited in the documentation stays
  valid.
- Anyone can retrieve the original backup from an old commit. That is harmless here: its
  data is already in the repository in a readable form, and the file holds no credentials.
- The brief's request is closed as "will not do" rather than "not yet", so it stops
  reappearing as an open item.
- Should a secret ever be committed in future, the answer would still not be a rewrite. A
  leaked credential is rotated, because rewriting the history does not un-leak it.
