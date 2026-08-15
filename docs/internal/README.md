# Internal working notes — not committed

Our own critique of our own documents: audits, review passes, half-finished lists of
everything currently wrong with the project. **Nothing in this folder is committed** apart
from this file — the root [`.gitignore`](../../.gitignore) §3a excludes `docs/internal/*`.

They are how we find our own mistakes, and they are useful precisely because they are blunt.
That is also why they are not a deliverable: a working list of defects, read cold by a client
or a facilitator, describes a project in worse shape than the one it is describing.

## The rule

- **Findings do not stay here.** Anything an audit turns up is fixed in the document that owns it, and the audit entry is then closed. A finding that only ever lives in this folder has not been actioned.
- **Nothing here is cited in anything we hand over.** If a fact from an audit belongs in the report, it belongs in `docs/spec/` or `docs/project/` first, and the report cites that.
- Committed documents may link here for our own convenience. Those links will not resolve on GitHub, which is expected and is not a broken link to be fixed.

## What is here

| File | What it is |
| --- | --- |
| `audit-2026-08-14.md` | First review pass over requirements v2.0, the kickoff minutes and the README |
| `audit-2026-08-14-round2.md` | Second pass, over the corrections made after the first |
