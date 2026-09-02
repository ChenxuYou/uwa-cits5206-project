# Risk Register

**Owner of this document:** Dai Lam La La
**Reviewed:** every Saturday team meeting, alongside the sprint review
**Version:** 1.1 — 2 September 2026
**Feeds:** Assignment 1 §4.3 · [`skills-audit.md`](skills-audit.md) · [`plan.md`](plan.md)

> **Every row carries a trigger.** A mitigation without a trigger is a hope: it says what we
> would do without saying when we would know to do it. The trigger column names the observable
> event at which the mitigation stops being optional.

**Scale.** Likelihood and impact are Low / Medium / High; impact may also be **Critical**, which
is reserved for outcomes that damage the client rather than the project.

---

## 1. The risk that defines this project

Worth stating separately, because it shapes every other row.

**The threat here is not an attacker. It is an incorrect or undetectably altered rate** —
published for three to five years, embedded in grant budgets across the University, and then
defended in a Freedom of Information response. A spreadsheet cannot protect against it: a cleared
formula, a range dragged one column too far or an amount typed with one extra zero all return a
plausible number and flag nothing. That is the failure mode the client asked us to remove.

The controls that count against it are therefore not perimeter controls. They are: server-side
validation, aggregates that iterate a typed collection rather than a range, decimal arithmetic,
the immutable snapshot, the integrity hash, and the golden-file test as a merge gate.

---

## 2. Register

### Correctness

| # | Risk | L | I | Mitigation | Trigger | Owner |
| --- | --- | --- | --- | --- | --- | --- |
| R1 | A calculation defect reaches a published rate | Low | **Critical** | Pure server-side engine with no I/O; decimal arithmetic end to end; aggregates iterate a typed capability collection so no range can be mis-typed; golden-file test against the client's worked example as a CI merge gate; test ownership separated from engine authorship | Any engine test failing on `main`; any figure differing from the client's guide by more than one cent | Wenmin Luo |
| R2 | The client's method is misunderstood — we build the wrong thing, correctly | Medium | High | Written source-precedence rule (client's written answers → guide → calculator → our minutes); every requirement carries its source marker; one owner for the guide-vs-calculator reconciliation rather than five parallel opinions; questions batched and asked | Two documents disagreeing with no recorded decision; any requirement carrying no source marker | Dai Lam La La |
| R3 | The calculator's arithmetic is reproduced where it departs from the guide | Low | High | Client confirmed in writing on 20 Aug 2026 that **the guide governs**; fixtures assert the guide's figure and record both where they differ, so a future reader cannot mistake a deliberate divergence for a bug of ours. Divergences are reported to the client as we meet them | Any fixture asserting a figure taken from the calculator without a note | Dai Lam La La |
| R4 | Rounding drift — totals stop reconciling with their parts | Low | Medium | One rounding rule, stated once, applied once at presentation; stored values unrounded; property test at the half-cent | Any total differing from the sum of its displayed components | Wenmin Luo |

### Scope and delivery

| # | Risk | L | I | Mitigation | Trigger | Owner |
| --- | --- | --- | --- | --- | --- | --- |
| R5 | Scope grows past the semester | Medium | High | The vertical slice is the commitment; the stretch list is written to the client as "intentions, not commitments"; the cut order is stated in advance — dashboard, price-change communication, benchmarking, replacement reserve, salary pre-fill, in-tool approval | Any stretch item started before the slice is complete end to end | Whole team |
| R6 | Deployment is left to the end and does not happen | Medium | High | Deployment is a milestone with owners and dates, not an end-of-semester task: hosting decided 9 Sep, the deploy sprint runs the week of 28 Sep, staging live 2 Oct (**M5**), client uses it unaccompanied for the eleven days to handover | The 9 Sep hosting decision slipping by more than three days | Chenxu You |
| R7 | Schedule slips because a task has no owner | Medium | Medium | Every issue carries one owner and one date; the board is reviewed at the Saturday meeting; unassigned issues are not allowed into a sprint | Any issue in a sprint column with no assignee | Whole team |
| R8 | A key member becomes unavailable — five people, one already withdrawn | Medium | High | Two owners for every critical area; engine kept framework-independent so any member can read it; PR review by a second member; the redistribution after DongSheng Li's withdrawal is documented in [`team.md`](team.md) | Any area with a single owner for more than one sprint | Whole team |
| R9 | Client latency blocks a decision | Low | Medium | One named liaison so the client is never chased twice; questions batched; **every open question carries a default we will use if no answer comes**. All five sent 17 Aug came back answered — none defaulted | Any question open for more than seven days | Yichen Zhao |
| R10 | A late scope addition is absorbed silently and blows the plan | Medium | Medium | Anything the client raises outside the signed scope is recorded as *raised, accepted in principle,* or *declined with a reason* before it becomes work. Two items are already on that list: the PDF showing the workings (accepted, needs an ID and an estimate) and HR-system integration (declined, would need a trade) | Any new work in a sprint with no requirement ID | Yichen Zhao |

### Cybersecurity and data

The data is UWA-internal, subject to FOI, not commercially confidential, and *"not something to
promote widely while it is still being worked on."* That calls for competent baseline security,
not a high-security posture — but every row below is in the MVP, not deferred.

| # | Risk | L | I | Mitigation | Trigger | Owner |
| --- | --- | --- | --- | --- | --- | --- |
| R11 | Injection, XSS, CSRF or mass assignment | Low | High | EF Core parameterises every query, no string-built SQL anywhere; Razor auto-escaping, including justification free text on PDF render; antiforgery tokens on all state-changing requests; explicit view models — a request body is never bound onto an entity | Any raw SQL; any user-supplied text rendered unencoded; any entity bound directly from a form | Wenmin Luo |
| R12 | Broken access control — a custodian sees another platform's record | Medium | High | Authorisation enforced **server-side in page policies and POST handlers**; navigation visibility is explicitly *not* the security boundary; every handler re-checks ownership; roles are custodian / delegated authority / administrator | Any authorisation check found only in a view, or any handler that trusts a route parameter | Chenxu You |
| R13 | Credentials, client material or local data committed to a public repository | Low | High | `.gitignore` §1 (client material), §3 (recordings and transcripts), §4 (keys and secrets), §5 (local databases); secrets from environment variables only; dependency scanning in CI | Any secret, `.db` file, spreadsheet or recording appearing in a diff | Chenxu You |
| R14 | Seeded demo credentials reach a deployed instance | Medium | High | `entry`/`approver` demo accounts exist for local development only and are named as such in `src/README.md`. Replacing them is a **gate on the staging deployment**, not a later task | Any deployment attempted with the seeded accounts present | Chenxu You |
| R15 | A sealed record is altered at the database level | Low | High | One-way seal transition; no route accepts a write to a sealed record; immutable JSON snapshot with a **SHA-256 integrity hash**; recomputation on read forbidden — a verification job may compare and raise an alarm, never correct; append-only audit entries | Any hash mismatch on verification | Wenmin Luo |
| R16 | Transport or session weakness | Low | Medium | HTTPS only with HSTS; cookie authentication with an 8-hour expiry and sliding renewal; framework-provided password hashing — no custom cryptography | Any HTTP endpoint reachable in staging | Chenxu You |
| R17 | A dependency vulnerability | Low | Medium | Minimal dependency surface (one direct NuGet package today); automated vulnerability scanning on every push and pull request. High and Critical advisories fail the build unless assessed and recorded in [`.github/known-advisories.txt`](../../.github/known-advisories.txt) with a reason and a review date | Any scan finding a High or Critical advisory that is not already assessed | Wenmin Luo |
| R17a | **Live and accepted: CVE-2025-6965** (GHSA-2m69-gcr7-jv3q, High, CVSS 7.2). `SQLitePCLRaw.lib.e_sqlite3` ≤ 2.1.11, reached transitively through EF Core's SQLite provider, bundles SQLite < 3.50.2, where aggregate terms exceeding the available columns can corrupt memory | Low | Medium | **No patched version exists** — the advisory lists *Patched versions: None*, so this cannot be fixed by upgrading. Exploitation requires submitting crafted SQL; this application issues no raw SQL, every query goes through EF Core parameterised, and the database file is not network-reachable (R11). Recorded in `known-advisories.txt` so it does not mask a *new* advisory | A patched `SQLitePCLRaw` being released, or the hosting decision keeping SQLite in production | Wenmin Luo — **review 9 Sep 2026** with the hosting decision |

### Technology and legal

| # | Risk | L | I | Mitigation | Trigger | Owner |
| --- | --- | --- | --- | --- | --- | --- |
| R18 | .NET 10 immaturity — an SDK or library regression blocks the build | Low | Medium | SDK version pinned in CI; dependency surface kept minimal; no preview-only features; lockfile committed | A build failing on the toolchain rather than on our code | Wenmin Luo |
| R19 | The repository argues for one stack while shipping another | — | — | **Realised and closed** — see §3. [ADR-001](../decisions/adr-001-technology-stack.md) records the decision and reconciles it with `architecture.md` §8 | — | Chenxu You |
| R20 | IP or licensing dispute over a jointly-owned deliverable | Low | Medium | Ownership confirmed in writing by the client on 20 Aug 2026; [`NOTICE`](../../NOTICE) grants UWA perpetual internal-use permission and reserves each author's portfolio rights; **no open-source licence is granted until handover**, because a licence from one joint owner alone may not be effective | Any request to relicense or redistribute before handover | Chenxu You |
| R21 | Client material published by accident | Low | High | The client's guide, calculator and worked examples are **not committed**; only our own rewrites are, with the source named. Enforced by `.gitignore` §1 and `reference/client/README.md` | Any client-supplied file appearing in a diff | Dai Lam La La |

---

## 3. Risks already realised and closed

Kept in the register rather than deleted. A risk that fired, was caught and was closed is
stronger evidence of risk management than any hypothetical.

| # | What happened | How it was caught | Outcome |
| --- | --- | --- | --- |
| C1 | An **MIT licence** was published in an early commit, for a project whose overarching IP is jointly held with UWA | Repository review | Rewritten out of history. [`NOTICE`](../../NOTICE) now states the interim all-rights-reserved position and why one joint owner cannot grant a licence alone |
| C2 | A draft of the client sign-off document asked the client to confirm that the overarching IP was **UWA's alone** — the opposite of the position our own `NOTICE` holds, and the very confirmation that closes the licensing question | Review of our own outbound document, **before it was sent** | Corrected before it left the building. The signed version states joint ownership. One sentence would have signed away the position the document was written to protect |
| C3 | Figures transcribed from the kickoff **recording** — $380,000 / $230,000 / $3,291 / $15,000 — were heading into a test fixture, having appeared in no client document | The written source-precedence rule, which ranks spoken minutes below written client material | Withdrawn from all downstream use. Fixtures rebuilt from the client's guide: $150,000 / $20,000 / $30,000 / 1,000 h → $100.00 / $162.00 / $202.50 |
| C4 | The repository assessed five technology options, chose none of them, and shipped a sixth | The 22 August repository tidy-up, recorded as finding **T1** in [`assignment-1-completion-plan.md`](assignment-1-completion-plan.md) | [ADR-001](../decisions/adr-001-technology-stack.md), 24 August 2026 |

---

## Change log

| Version | Date | Change |
| --- | --- | --- |
| 1.1 | 2 Sep 2026 | **First review since the register was written, and it is late.** The header says this document is reviewed every Saturday; the 29 August meeting passed without it being opened, which is exactly the failure mode a register is supposed to catch in other people's work. Recorded here rather than backdated. Three corrections follow. **R6** said staging was live on 28 September and the client had it for two weeks; [`plan.md`](plan.md) has said **2 October** since it was written — 28 September is the week the deploy sprint *starts* (S6), not the date the milestone lands. R6 now names M5 and the eleven days that actually remain to handover, and the same correction was made in [`skills-audit.md`](skills-audit.md) G2 and [ADR-001](../decisions/adr-001-technology-stack.md) action 6, which had drifted the same way. **R7's trigger has fired and been cleared**: the Projects board was created on 1 September and the twenty-five story issues carry owners, so "any issue in a sprint column with no assignee" is now a check somebody can actually run. **R1 is the live risk this week** — M1 falls on 4 September and no test project exists yet, so the merge gate named in the mitigation is written into [`ci.yml`](../../.github/workflows/ci.yml) but not yet armed; ADR-001 action 4 carries it. No row is added, removed or re-scored. |
| 1.0 | 24 Aug 2026 | First version. Collects material previously scattered across `architecture.md` §6, `requirements.md` §10, `assignment-1-readiness.md` §5 and the Assignment 1 draft into a single register with owners and triggers. Adds the cybersecurity rows the rubric names explicitly, and §3, the risks already realised and closed |
