# Architecture Vision

**Project:** Research Infrastructure Costing & Pricing Tool
**Status:** v2.2 — 25 August 2026. **Technology committed: ASP.NET Core Razor Pages with EF Core** — see §8 Option F, §9 and [ADR-001](../decisions/adr-001-technology-stack.md).
**Companion documents:** [requirements](requirements.md) · [user stories](user-stories.md)

> This document sets out the architecture we build, the options assessed for realising it, and
> how the stack was chosen. The **shape** of the system — §2 to §7 — holds regardless of which
> option is chosen, which is what made the choice reversible while it was open. The **stack** —
> §8 — is now **decided**: Option F, a server-rendered ASP.NET Core monolith. §9 records how.
>
> Options A–E were presented at the [facilitator checkpoint on 5 August 2026](../../presentations/2026-08-05-facilitator-checkpoint.html);
> **Option F was added on 25 August 2026** when the decision was written up in
> [ADR-001](../decisions/adr-001-technology-stack.md).
>
> Source markers follow [requirements, *Source precedence*](requirements.md): **[G]** the
> client's costing & pricing guide, **[W]** the client's calculator workbook, **[K]** the
> kickoff minutes.

---

## 1. What the architecture has to achieve

Six requirements drive every structural decision. Everything else is detail.

| Driver | Source | Structural consequence |
| --- | --- | --- |
| **The user must not be able to break the calculation** | Sharing a workbook shares its formulas **[N1]** | Calculation runs server-side, in a module with no path from the browser to its internals |
| **Totals must be summed over the same set as the figures they are compared against** | A hand-typed spreadsheet range can silently span the wrong columns **[N14, requirements §2]** | Capabilities are a first-class collection; every aggregate iterates it. There is no place to write a range |
| **A record must reproduce its own figures years later** | "Open the old record in three years" **[N6]** | Sealed records are immutable snapshots; method and constants are versioned configuration, not code |
| **Figures must be exact and defensible** | Public money, FOI, audit **[N5]** | Decimal arithmetic end to end; one rounding rule, applied once, at presentation |
| **A returning user must succeed without training** | Once every 3–5 years **[N4]** | Guided sequential flow with server-side validation at every step; state persisted continuously |
| **We must ship something complete** | One semester, five members, mixed skills | Prefer the boring, well-trodden stack; a working narrow path beats a broad unfinished one |

## 2. System shape

Three tiers, with the calculation isolated inside the middle one.

```
┌──────────────────────────────────────────────────────────────────────┐
│  BROWSER                                                             │
│  Guided wizard — validated form, review page, scenario modeller      │
│  Holds no formulas, no constants, no cost coefficients               │
└─────────────────────────────┬────────────────────────────────────────┘
                              │  HTTPS / JSON
┌─────────────────────────────▼────────────────────────────────────────┐
│  APPLICATION SERVER                                                  │
│                                                                      │
│  ┌────────────────┐   ┌──────────────────┐   ┌────────────────────┐  │
│  │  API layer     │   │ CALCULATION      │   │ Report generator   │  │
│  │  auth          │──▶│ ENGINE           │   │ sealed record      │  │
│  │  validation    │   │ pure · versioned │──▶│ → PDF              │  │
│  │  workflow      │   │ unit-tested      │   │                    │  │
│  └───────┬────────┘   └──────────────────┘   └─────────┬──────────┘  │
└──────────┼─────────────────────────────────────────────┼─────────────┘
           │                                             │
┌──────────▼─────────────────┐              ┌────────────▼─────────────┐
│  RELATIONAL DATABASE       │              │  FILE STORE              │
│  cycles · records · audit  │              │  sealed PDF records      │
└────────────────────────────┘              └──────────────────────────┘
```

**Why the engine is drawn as its own box.** It is not a layer, it is a boundary. The engine
takes a valid input structure and returns numbers. It does not read the database, render
anything, know about HTTP, or know what a user is. That isolation is what makes it testable
against the client's material **[N13]**, what makes a 2026 record reproducible in 2030
**[N6]**, and what makes it swappable if we change the surrounding stack.

**Auth is drawn inside the API layer, and it is in the MVP.** F15 is a **Must** in
[requirements §7](requirements.md#7-scope): every record carries the identity of who created,
submitted and sealed it, and US-02, US-15 and US-16 depend on that identity. Earlier versions
of this document said "session login for the MVP" while the requirements put F15 outside it;
the requirements have since been corrected and the two now agree.

## 3. The calculation engine

The most important 300 lines in the project. Written first, before any UI.

### Contract

```
calculate(inputs, method_version) → results

inputs           capabilities[], each with its own costs, allocated share, capacity,
                 deductions and forecast utilisation; platform-level costs and the
                 allocation rule; income by source (uwa_gp, state, federal, other);
                 billable unit; optional replacement reserve
method_version   identifies the formula set and the constants (k, rounding rule,
                 capacity baselines)
results          per capability: three calculated rates and the intermediate figures
                 behind each; where proposed rates are supplied, the forecast balance.
                 Plus the platform roll-up, summed over every capability
```

### Rules

| # | Rule | Why |
| --- | --- | --- |
| R1 | Pure function: same inputs and version, same outputs, always. No I/O, no clock, no randomness | Reproducibility **[N6]** and trivial testing |
| R2 | Decimal arithmetic throughout. Money never touches binary floating point | `0.1 + 0.2 ≠ 0.3` is not defensible in an FOI response **[N5]** |
| R3 | One rounding rule, stated, applied once at presentation. Stored values are unrounded | Figures reconcile; totals do not drift from their parts |
| R4 | `U = 0` or `U` absent raises a domain error with a plain-language message. Never infinity, never a crash | Zero utilisation is a plausible user error **[N8]** |
| R5 | `k`, pay scales, capacity baselines, cost and income categories and the formula set live in versioned configuration | `k` and the method will change within one 3–5 year cycle **[N7]** |
| R6 | Every result carries the inputs and version that produced it | The record must explain itself |
| R7 | The engine never sees a user, a session or a database row | Keeps the boundary honest |
| R8 | Aggregates iterate the capability collection. No aggregate may be expressed as a range, a slice or a hard-coded count | A total summed over a different set from the figures it is compared against becomes unrepresentable rather than merely unlikely **[N14]** |

### The formulas

Per capability, from the client's guide **[G, Step 3]**:

```
R_uwa        = (C − I_total)   / U
R_apfr       = ((C − I_nonuwa) / U) × k
R_commercial = (C / U)               × k

I_total   = uwa_gp + state + federal + other
I_nonuwa  =          state + federal + other
k         = 1.35        (versioned configuration, not a constant)
```

### Verification

The engine is validated as a **golden-file test** before any UI exists **[N13]**, against two
fixtures at two levels.

**Fixture 1 — the guide's worked example.** The client's own numbers, which exercise all three
formulas from one input set **[G, Step 3]**:

| Input | Value | | Expected output | |
| --- | --- | --- | --- | --- |
| `C` operating costs | $150,000 | | UWA Researcher | **$100.00 / hour** |
| UWA in-kind | $20,000 | | APFR | **$162.00 / hour** |
| WA Gov (technical staff) | $30,000 | | Commercial | **$202.50 / hour** |
| `U` forecast utilisation | 1,000 hours | | | |

This is the first test written on this project.

**Fixture 2 — a transcribed capability from the workbook**, to prove the per-capability
roll-up, the even-split allocation and the staff-FTE capacity cap against a real column
**[W, sheets 1–3]**.

> **The guide governs, and divergences are recorded rather than absorbed.** The client confirmed
> in writing on 20 August 2026 that where their guide and their calculator disagree, the tool
> follows the **guide** ([Q10](requirements.md#9-open-questions)). Fixture 2 therefore asserts the
> figure the guide's method produces, and where that differs from the workbook's the test records
> both and states which is asserted and why — so a future reader does not mistake a deliberate
> divergence for a bug of ours. A full line-by-line reconciliation of the calculator is out of
> scope for this stage and is scheduled for a later cycle, once the engine exists to compare
> against.

> **Withdrawn fixtures.** Versions up to 1.1 of this document named "$380,000 total, $230,000
> income, $150,000 to recover, the $3,291 calculated rate, the $15,000 deficit" as the first
> fixtures. Those figures were transcribed from the walkthrough and appear in **no client
> document**; the $3,291 rate was internally inconsistent with the remark recorded beside it.
> They are withdrawn. Nothing in the repository should cite them.

Property tests cover the boundaries: zero and negative inputs, `U = 0`, income exceeding cost,
very large amounts, rounding at the half-cent, a platform with one capability, and a platform
where one capability is staff-capped and another is not.

## 4. Data model

Sketch, not schema. It exists to show that the immutability and consistency requirements are
structural rather than policies we hope to remember.

```
Platform ──1:N── CostingCycle ──1:N── Capability
                      │                   │
                      │                   ├──1:1── CapacityProfile (baseline_ref, staff_capped,
                      │                   │                         deductions[], forecast_U, note)
                      │                   ├──1:N── CostLine        (category, amount, note)
                      │                   └──1:1── RateSet          (calculated[3], proposed[3],
                      │                                              variance[3], balance,
                      │                                              justification)
                      │
                      ├──1:N── CostLine        (platform level: category, amount, note,
                      │                         allocation_rule)
                      ├──1:N── IncomeLine      (source: uwa_gp | state | federal | other,
                      │                         amount, note)
                      ├──0:1── ReplacementReserve (value, recovery_pct, years, annual_amount)
                      ├──0:N── BenchmarkEntry  (source, rate, comparability_notes)
                      └──1:1── SealedRecord    (immutable snapshot: full input JSON,
                                                results JSON, method_version, sealed_by,
                                                sealed_at, export_uri, snapshot_hash)

AuditEntry   (append-only: actor, action, entity, timestamp, before → after)
MethodConfig (version, effective_from, k, rounding_rule, capacity_baselines[],
              cost_categories[], income_sources[], pay_scale_table)
User         (identity, role: custodian | delegated_authority | administrator, platform_scope)
```

**Design points**

- **`RateSet` hangs off `Capability`, not off the cycle.** The client's workbook produces an independent set of three rates for every capability column **[W, sheet 3 rows 25–27]**, so the rate set is per capability and the platform figure is a roll-up. This was assumption A1 in earlier drafts, which had it the other way round; [AQ1](#11-open-architectural-questions) is closed accordingly.
- **`CostLine` appears at two levels, as two relationships rather than one nullable key.** A capability-level line belongs to a capability; a platform-level line belongs to the cycle and carries its `allocation_rule`. Making them distinct relationships is what stops an aggregate from accidentally spanning both — the structural half of **[N14]**.
- **`IncomeLine.source` is a four-value enum, and the UWA/non-UWA split is derived from it**, never stored separately. The three formulas deduct different subsets **[requirements §4]**; deriving the subsets from one enum means they cannot drift apart.
- **`CapacityProfile.baseline_ref` points at a `MethodConfig` baseline**, not at a number. The client uses 1,882.5 h for machine availability and 1,725 h for staff **[W, sheet 2]**; both are configuration **[N7]**.
- **`SealedRecord` stores the full input and output as JSON**, not as foreign keys to live rows. If a cost category is renamed in 2028, the 2026 record still says what it said. This is the mechanism behind **[N6]** — normalised references would silently rewrite history.
- **A cycle is superseded, never edited.** A new cycle references the one it replaces ([A5](requirements.md#8-assumptions)).
- **`AuditEntry` is append-only** with no update or delete path in the application **[N10]**.
- **`User` is in the MVP.** F15 is a Must; `sealed_by` and the "last edited by" of US-02 resolve to a real identity rather than a free-text name.

## 5. The seal — the mechanism that makes the record trustworthy

The client's requirement is not "generate a report". It is "make a record that cannot change
and can be read years later" **[requirements §5]** — which the guide restates as an obligation:
"Supporting documentation should be retained for audit and review purposes" **[G, Step 5]**.
Three enforcement layers, because one is not enough:

1. **Application.** No route accepts a write to a sealed record. Sealing is a one-way state transition: `Draft → (Submitted → Approved) → Sealed`. The `Submitted → Approved` pair exists only when F16 is built; in the MVP the custodian's confirmation seals directly.
2. **Data.** The snapshot is written once and read thereafter. Recomputation on read is forbidden — the stored numbers *are* the record. A verification job may recompute and compare, but only to raise an alarm, never to correct.
3. **Artefact.** The exported PDF is generated at seal time, stored, and served from the store. Regenerating it later would risk producing a different document from a changed template.

**Integrity check.** A hash of the snapshot is stored alongside it, so tampering at the
database level is detectable. Cheap to build, and it is the difference between "we believe
this record is unchanged" and "we can show this record is unchanged" — which is the whole
point of the project.

## 6. Security

The data is UWA-internal, subject to FOI, not commercially confidential, and not for wide
promotion while in progress **[N11]**. That calls for competent baseline security, not a
high-security posture.

| Concern | Approach |
| --- | --- |
| Transport | HTTPS only, HSTS |
| Authentication | Session-based login, **in the MVP** — F15 is a Must **[requirements §7]**. UWA SSO is the right long-term answer but depends on institutional access we do not have, so the MVP authenticates locally behind an SSO-shaped seam ([AQ3](#11-open-architectural-questions)) |
| Authorisation | Custodian sees own platform; delegated authority sees what is submitted to them; administrator sees all. Checked server-side on every request, never inferred from the UI ([Q4](requirements.md#9-open-questions), [A6](requirements.md#8-assumptions)) |
| Injection | Parameterised queries via an ORM; no string-built SQL |
| XSS | Framework auto-escaping; justification free text escaped on render and on PDF generation |
| CSRF | Tokens on all state-changing requests |
| Mass assignment | Explicit input schemas; the API never binds a request body straight onto a model |
| Secrets | Environment variables, never committed. Enforced by [`.gitignore`](../../.gitignore) |
| Dependencies | Lockfiles committed; automated vulnerability scanning in CI |
| Audit | Append-only log of every state change **[N10]** |
| Client material | Nothing the client gave us is committed without their agreement; the repository carries our own rewrites, attributed. Enforced by [`.gitignore`](../../.gitignore) §1 |

**The threat that actually matters here** is not an external attacker — it is an incorrect or
undetectably altered rate. The controls that count are server-side validation **[N2, N3]**,
the consistent aggregate contract **[N14]**, the immutable snapshot, the integrity hash and
the audit log.

## 7. What we will not build

- **No microservices.** One application, one database. Five people, one semester, low traffic.
- **No custom authentication cryptography.** Framework-provided, always.
- **No calculation in the browser.** Not even a "preview" — it would leak the logic and reintroduce the spreadsheet's defect **[N1]**.
- **No premature multi-tenancy.** One institution ([A9](requirements.md#8-assumptions)).
- **No integration with UWA systems.** Explicitly deferred by the client **[requirements §7]**.
- **No reproduction of the calculator's arithmetic where it departs from the guide.** We implement the client's *method*, and show both figures wherever a new number is compared with an existing one ([Q10](requirements.md#9-open-questions)).

## 8. Options assessed

Presented to the facilitator on 5 August 2026.

| Option | Shape | Verdict |
| --- | --- | --- |
| **A** | Client-side SPA, no backend | **Prototype only** |
| **B** | SPA + REST API + PostgreSQL | **Considered, not chosen** |
| **C** | Server-rendered monolith (Django + HTMX) | **Considered, not chosen** |
| **D** | Microsoft Power Platform | **Rejected** |
| **E** | Build inside existing UWA systems | **Deferred** |
| **F** | **Server-rendered monolith — ASP.NET Core Razor Pages + EF Core** | ✅ **Chosen** |

### Option A — client-side SPA, no backend

Everything in the browser; records exported to a local file.

- **For:** fastest possible path to something clickable; excellent for validating the flow with the client in week 5.
- **Against:** violates **N1** outright — the logic would sit in JavaScript the user can read and modify, which is the defect we were hired to remove. No shared records, no audit, no retrieval **[F13]**. No authentication **[F15]**.
- **Verdict:** valuable as a **throwaway prototype** to confirm the wizard flow. Never the product. If built, it is deleted, not evolved.

### Option B — SPA + REST API + PostgreSQL *(considered, not chosen)*

React or Vue front end; Python (FastAPI or Django REST) or Node back end; PostgreSQL.

- **For:** meets every stated requirement. Clean engine isolation. The rich interaction of US-10 and US-12 — change a number, watch the rates and the balance move across every capability — is native to this shape. Standard, employable, well-documented.
- **Against:** two codebases, two build chains, CORS, auth across the boundary. The team's JavaScript depth is unproven. It is the option most likely to consume time on plumbing rather than on the client's problem.
- **Cost:** highest setup cost of the three shapes that could ship; highest ceiling.

### Option C — server-rendered monolith, Django + HTMX *(considered, not chosen)*

Django with HTMX for partial updates; PostgreSQL; server-side PDF generation.

- **For:** one codebase, one deployment, one language. Django gives auth, admin, ORM, migrations, CSRF and form validation on day one — a large fraction of §6 arrives for free, and F15 in particular is close to free. Server-rendered forms make **N1** structurally trivial: there is nowhere for logic to leak to. HTMX covers the live-update need without a SPA. **Highest probability of shipping something complete.**
- **Against:** less interactive polish; a genuinely rich scenario modeller would be harder. Less fashionable on a CV.
- **Cost:** lowest risk, lowest ceiling, and the ceiling is above what the MVP requires.
- **Not chosen** on language fit alone: nobody on the team has shipped a Django application, and by 20 August one had been shipped in .NET. Option F is this option in C#.

### Option D — Microsoft Power Platform *(rejected)*

- **For:** exists inside UWA's Microsoft tenancy; low-code; potentially fast.
- **Against:** it is a spreadsheet-adjacent low-code tool, and the failure we are replacing is a spreadsheet-adjacent low-code tool. Logic sits in a proprietary environment where the "user cannot break it" guarantee is weaker, versioning and golden-file testing **[N13]** are awkward, and licensing and tenancy access are outside our control. It also teaches the team little.
- **Verdict:** rejected.

### Option E — build inside existing UWA systems *(deferred)*

- The client's own words: *"would be ideal, but I think that might be a bit challenging"* — get a functional website working first **[K §8]**.
- Requires institutional system access, approvals and lead times a one-semester capstone cannot absorb.
- **Verdict:** deferred by the client. **Not scored below**, because the constraint that rules it out is availability, not merit — a score would imply we chose against it, and we did not. Out of scope; noted as the natural next step after handover.

### Option F — server-rendered monolith, ASP.NET Core Razor Pages + EF Core *(chosen)*

**Option C's architecture in a different language.** One codebase, one deployment, one language.

- **For:** everything C is for — server-rendered forms make **N1** structurally trivial, and framework-provided authentication, ORM, migrations, antiforgery and model validation deliver a large fraction of §6 on day one. Three things C does not have: **`decimal` is a native 128-bit base-10 type in C#**, so **R2** is the default rather than a discipline; **LINQ over a typed capability collection** means **R8** is enforced by the compiler rather than by review; and the team has *demonstrated* it can ship in this stack rather than asserting it.
- **Against:** .NET 10 is very new, so there are fewer worked examples and some libraries lag. Less interactive polish than a SPA, as with C.
- **Evidence:** a timeboxed spike produced a working end-to-end application — sign-in, roles, the guided RIC workflow, rate calculation, an approval queue, notifications, and sealed snapshots with a SHA-256 integrity hash — running in time for the client meeting of 20 August 2026.
- **Verdict:** **chosen.** [ADR-001](../decisions/adr-001-technology-stack.md).

### Comparison

Weighted against the drivers in §1. Scores 1–5, higher is better. Option E is excluded for the
reason given above.

| Criterion | Weight | A | B | C | D | **F** |
| --- | --- | --- | --- | --- | --- | --- |
| Meets N1 (logic out of reach) | 5 | 1 | 5 | 5 | 3 | **5** |
| Supports the sealed record (F11–F13) | 5 | 1 | 5 | 5 | 3 | **5** |
| Probability of shipping complete in one semester | 5 | 3 | 3 | 5 | 3 | **5** |
| Fit to team's current skills | 4 | 3 | 3 | 4 | 2 | **5** |
| Interaction quality (US-10, US-12) | 3 | 5 | 5 | 4 | 3 | **4** |
| Testability of the engine | 4 | 2 | 5 | 5 | 1 | **5** |
| Maintainability after handover | 3 | 1 | 4 | 4 | 3 | **4** |
| Learning value for the team | 2 | 3 | 5 | 4 | 1 | **4** |
| **Weighted total** | | **69** | **134** | **143** | **77** | **151** |

**F leads at 151.** It scores C's architecture identically — the two are the same shape — and
separates from C on one criterion only: **fit to the team's current skills, 5 against 4.** That
single point, at weight 4, is worth eight, and it is the one criterion we now have direct
evidence for rather than an estimate.

**Where the earlier scoring was weak, and how F fixes it.** C's nine-point lead over B rested
almost entirely on two judgement calls, one of them the very thing the skills audit existed to
measure:

| Criterion | Weight | C − B | Weighted |
| --- | --- | --- | --- |
| Probability of shipping complete | 5 | +2 | **+10** |
| Fit to team's current skills | 4 | +1 | **+4** |
| Interaction quality | 3 | −1 | −3 |
| Learning value | 2 | −1 | −2 |
| | | | **+9** |

Nine points on a 155-point scale was inside the noise of our own scoring, which is why v1.1
withdrew the recommendation and v2.1 said the table narrowed the field rather than choosing. The
[skills audit](../project/skills-audit.md) and the spike are what replaced the estimate with a
measurement — see §9.

## 9. How the decision was taken

**Closed 24 August 2026: Option F.** Recorded as
[ADR-001](../decisions/adr-001-technology-stack.md), which names the choice, the evidence and the
fallback trigger.

The gate this section set out required two pieces of evidence, and both were produced:

1. **Skills audit** — every member self-rates on JavaScript/React, C#/.NET, SQL, HTML/CSS, Git and testing. Result: depth is concentrated in C# and server-side web work; **no member rates 3 or above on JavaScript frameworks**. That rules out B on the criterion the audit existed to settle. [`skills-audit.md`](../project/skills-audit.md).
2. **Spike** — a timeboxed build of the engine plus the workflow, by the two members who would own it. It produced a working end-to-end application in ASP.NET Core, running before the client meeting of 20 August.

**One thing to state plainly, because the order matters.** The spike came *before* this record,
not after it. We built to learn, and the ADR documents what we learned; presenting it as a
decision taken in advance and then executed would be a tidier story and a false one. What the
ADR does instead is close the gap the repository had — arguing for one stack in this document
while shipping another in `src/` — and treat the spike as evidence rather than as a commitment
exempt from review.

**Fallback trigger, unchanged in substance:** if a working end-to-end slice — sign in → create
cycle → enter inputs → see rates → seal → export — is not running by the end of week 8, we cut
stretch scope. We do **not** change stack. The engine, the data model and the tests are isolated
so that a stack change *could* be survived, and that portability is the reason for the isolation
in §2 and §3 — but making one at that point would be the wrong answer, and saying so in advance
stops it being an option we reach for under pressure.

**What holds regardless of stack, and did throughout:**

- The engine is a pure, versioned, unit-tested module with no database or UI dependency.
- Decimal arithmetic, divide-by-zero guard, versioned method configuration, aggregates that iterate rather than index.
- Rates are computed per capability; the platform figure is a roll-up.
- Sealed records are immutable snapshots that store their own inputs and method version.
- Users authenticate; no record is created anonymously.
- Golden-file verification against the client's worked example before any UI is written.
- Server-side validation is authoritative.

**The relational store is the one open sub-decision.** SQLite is the development store today.
Whether production runs on SQLite or PostgreSQL is decided together with hosting on
**9 September 2026** ([AQ2](#11-open-architectural-questions)); EF Core makes the provider a
one-line change and no raw SQL is written anywhere, so deferring it costs nothing.

## 10. Delivery approach

**Build order** — the vertical slice, thinnest possible, then widen
(see [`reference/unit/wipro-mvp-summary.md`](../../reference/unit/wipro-mvp-summary.md)):

1. **Engine + golden-file test** — no UI, no database. Proves the arithmetic against the guide's worked example.
2. **Data model + persistence + identity.** Proves a cycle survives a restart and knows who made it. Authentication lands here, not at the end: `sealed_by` and "last edited by" are MVP acceptance criteria (US-02, US-15, US-16), and retrofitting identity through a data model is more expensive than starting with it.
3. **The three sections as forms**, with server-side validation. Proves the flow.
4. **Rates and balance, per capability.** Proves the client's question can be answered.
5. **Seal + PDF export + retrieval.** Proves the record.
6. **Widen:** salary pre-fill, replacement reserve, benchmarking record, delegated approval, polish. Only once the slice holds.

**Environments.** Local development → a deployed staging instance the client can click
through → a demonstration instance for handover. A client who can use it will tell us more in
ten minutes than a specification review will in an hour.

**CI from the first commit of code.** Tests, linting and dependency scanning on every pull
request. Engine tests are the gate: the build fails if the golden file drifts.

**Definition of done for a story.** Acceptance criteria met · server-side validation ·
unit tests for logic · reviewed by a second member · merged to main · deployed to staging.

## 11. Open architectural questions

| # | Question | Blocks | Current position |
| --- | --- | --- | --- |
| ~~AQ1~~ | Per-capability rates, or one rate set per platform? | Data model shape | **Closed.** Per capability — the client's workbook computes an independent rate set for every capability column **[W, sheet 3]**. §4 and [A1](requirements.md#8-assumptions) updated |
| AQ2 | Where will this be deployed after handover — UWA infrastructure, or team-provisioned? | Deployment, auth strategy | Team-provisioned for the MVP; documented so UWA can rehost |
| AQ3 | Is UWA SSO available to us within the semester? | US-19 | Assume not; local auth with an SSO-shaped seam. F15 is in the MVP either way |
| AQ4 | PDF generation approach | US-16 | Server-side HTML → PDF, so the export and the on-screen record share one template |
| AQ5 | Does the client need cost data imported from an existing system? | Scope | No — manual entry, per [requirements §7](requirements.md#7-scope) |
| AQ6 | How many capabilities must one screen handle before the per-capability rate table stops being readable? | UI shape of US-09, US-12 | The demonstration platform has seven **[W]**; design for a dozen, paginate beyond |

---

## Change log

| Version | Date | Change |
| --- | --- | --- |
| 1.0 | 14 Aug 2026 | First draft. Options A–E as assessed at the 5 August facilitator checkpoint; recommended B, with C as fallback; decision deferred to the skills audit. |
| 1.1 | 14 Aug 2026 | Recommendation withdrawn. v1.0 recommended B, but its own weighted table scored C higher without the text acknowledging it; §8 and §9 now report the scores plainly and carry B and C forward as equal candidates until the team has met on it. §9 fallback trigger reworded to cover either choice. |
| 2.0 | 14 Aug 2026 | **Realigned to requirements v2.0.** §3 golden-file fixtures replaced: the withdrawn walkthrough figures ($380,000 / $230,000 / $3,291 / $15,000, none of which appears in any client document) give way to the guide's worked example, $150,000 / $20,000 / $30,000 / 1,000 h → $100.00 / $162.00 / $202.50, plus a second fixture transcribed from a workbook capability. §4 data model reshaped: `RateSet` moves to `Capability`, income becomes a four-value enum with UWA/non-UWA derived, capacity references a configured baseline, and `ReplacementReserve`, `BenchmarkEntry` and `User` are added. New driver and engine rule **R8** against **N14**, the workbook's mismatched-column-range defect. §6 authentication contradiction resolved — F15 is a Must and the MVP authenticates. §10 build order moves identity into step 2 rather than step 6. **AQ1 closed** (per capability); AQ6 opened. §8 now says why Option E is unscored, and the v1.1 change-log entry is rewritten to say what it meant. |
| 2.2 | 25 Aug 2026 | **The stack is decided: Option F, ASP.NET Core Razor Pages + EF Core** ([ADR-001](../decisions/adr-001-technology-stack.md)). §8 gains Option F and re-runs the weighted comparison including it — F leads at 151, separating from C on fit-to-skills alone, the one criterion for which there is now direct evidence rather than an estimate. B and C move from *live candidate* to *considered, not chosen*; C is rejected on language fit only, since F is the same architecture in C#. §9 is rewritten from a gate that had not closed into a record of how it closed, including the plain statement that **the spike preceded the decision record**. The line "PostgreSQL as the relational store" is withdrawn from the settled list: SQLite is the development store and the production store is decided with hosting on 9 September (AQ2). §1 and §3 no longer characterise the client's workbook as defective — the client has confirmed that **the guide governs** where the two disagree, and a line-by-line reconciliation of the calculator is deferred to a later cycle. No structural decision in §2–§7 changes. |
| 2.1 | 15 Aug 2026 | **Synchronised with requirements v2.2.** §6's client-material control now states the rule directly — nothing the client gave us is committed without their agreement, enforced by [`.gitignore`](../../.gitignore) §1 — rather than deferring to an open question. Q-number references follow the renumbering in [requirements §9](requirements.md#9-open-questions), where old Q7–Q11 became Q6–Q10: the workbook-defect question is now **Q10**. No architectural decision changes. |
