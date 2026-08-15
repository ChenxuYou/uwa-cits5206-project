# Client material — not committed

Everything the client gives us lands in this folder locally, and **nothing in it is
committed**. The root [`.gitignore`](../../.gitignore) §1 excludes `reference/client/*`
apart from this file.

The client described their material as relatively sensitive: not commercially
confidential, since UWA is a publicly funded institution, but not something they want
promoted while it is still being worked on. Treat it accordingly.

## Working with it

- Keep the originals in the team's Teams area. This folder is a local convenience, not a store.
- **Nothing the client gave us enters the repository without the client's agreement.** That is the one rule this folder exists to enforce, and [`.gitignore`](../../.gitignore) §1 enforces it mechanically.
- Anything we need in the repository — a summary, an extracted formula, a golden-file test case — is rewritten by us in markdown or code and committed as our own work, with the source named in the text. A rewrite of ours is not client material and needs no permission.
- If a client file itself has to be committed, get the client's agreement first, then `git add -f <path>`. Record where that agreement was given, so a later reader can check it.
- Ignoring a file does not remove it from history. If client material has already been committed without agreement, say so on the team channel and have it rewritten out before pushing further.
