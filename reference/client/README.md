# Client material — not committed

Everything the client gives us lands in this folder locally, and **nothing in it is
committed**. The root [`.gitignore`](../../.gitignore) §1 excludes `reference/client/*`
apart from this file.

The client described their material as relatively sensitive: not commercially
confidential, since UWA is a publicly funded institution, but not something they want
promoted while it is still being worked on. Treat it accordingly.

## Working with it

- Keep the originals in the team's Teams area. This folder is a local convenience, not a store.
- Do not paste client material into a public AI tool. The client has been asked whether AI tools may be used on their material and, as of 14 August 2026, has not answered.
- Anything we need in the repository — a summary, an extracted formula, a golden-file test case — is rewritten by us in markdown or code and committed as our own work, with the source named in the text.
- If you need to commit a client file, get the client's agreement first, then `git add -f <path>`.
