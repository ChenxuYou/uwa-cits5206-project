# ADR-001 — Technology stack: ASP.NET Core Razor Pages with EF Core

**Status:** Accepted
**Date:** 24 August 2026
**Deciders:** Chenxu You, Yichen Zhao, Wenmin Luo, Dai Lam La La, Jaswanth Vericherla
**Supersedes:** the open decision gate in [`architecture.md` §9](../spec/architecture.md#9-how-the-decision-was-taken)
**Related:** [`architecture.md` §8](../spec/architecture.md#8-options-assessed) (options A–E),
[`skills-audit.md`](../project/skills-audit.md), [`risks.md`](../project/risks.md)

---

## Context

[`architecture.md`](../spec/architecture.md) assessed five options at the facilitator checkpoint
of 5 August 2026 and deliberately stopped short of a recommendation, deferring the choice to a
skills audit and a spike. Two options were carried forward as live:

- **B** — SPA (React/Vue) + REST API + PostgreSQL
- **C** — server-rendered monolith, Django + HTMX + PostgreSQL

The weighted comparison put C ahead of B by nine points on a 155-point scale — inside the noise
of our own scoring, which is why §8 said the table *narrows the field* rather than chooses.

**Two things then happened that the assessment did not anticipate.**

1. The team's two developers built a working end-to-end application in **ASP.NET Core Razor
   Pages**, not in either candidate language, and had it running in time for the client meeting
   of 20 August 2026. It covers sign-in and roles, the guided RIC workflow, cost and income
   capture, capacity and utilisation, rate calculation, an approval queue, notifications, and
   sealed snapshots with a SHA-256 integrity hash.
2. The skills audit, when it was finally run, confirmed what the spike had already demonstrated:
   the team's depth is in C# and server-side web work, and is thin in JavaScript frameworks.

So the repository was in the position of **arguing for one stack and shipping another**, with
`docs/decisions/` empty. This ADR closes that gap.

## Decision

**We will build the Research Infrastructure Costing & Pricing Tool as an ASP.NET Core Razor
Pages application with Entity Framework Core**, targeting .NET 10.

This is **Option C's architecture in a different language** — a server-rendered monolith, one
codebase, one deployment, framework-provided authentication, ORM, antiforgery and validation. It
is recorded as **Option F** and added to the comparison in `architecture.md` §8.

| Layer | Choice |
| --- | --- |
| Runtime | .NET 10 (SDK version pinned in CI) |
| Web | ASP.NET Core Razor Pages |
| Data access | Entity Framework Core 10 |
| Store | SQLite in development; the production store is decided with the hosting decision (9 Sep 2026) |
| Authentication | Cookie authentication with ASP.NET Core Identity password hashing; role-based page policies |
| Calculation engine | A plain C# class library with **no ASP.NET or EF dependency** |
| PDF export | Server-side HTML → PDF, sharing one template with the on-screen record |
| CI | GitHub Actions — restore, build, test; engine tests are the merge gate |

## Rationale

### The architecture was decided on the drivers, not on taste

The hardest requirement — *the user must not be able to reach the calculation* — is the defect we
were engaged to remove. A server-rendered application makes it **structural rather than
disciplinary**: the browser receives rendered HTML, so there is no formula to reach and no
carelessness later can reintroduce one. Both C and F have this property; B has it only if
maintained; A does not have it at all.

### The language was decided by evidence, not by argument

Rather than debate C# against Python, we built a timeboxed spike and measured what came out.
A complete vertical slice, working, within a sprint, by the two members who would own it, is the
strongest possible answer to the "fit to team's current skills" criterion — which
`architecture.md` §8 weights at 4 and which the readiness review correctly noted we had been
scoring before we had measured it.

### Four properties map onto the drivers unusually directly

| Driver | What ASP.NET Core gives us |
| --- | --- |
| **Exact money arithmetic** (`R2`, `N5`) | `decimal` is a native 128-bit base-10 type in C#, not a library opt-in. JavaScript has no decimal type; Python requires `Decimal` discipline at every boundary. Here it is the default and the compiler enforces the type |
| **Logic out of reach** (`N1`) | Razor Pages render server-side; there is no client bundle for the calculation to be compiled into |
| **Aggregates that cannot span the wrong set** (`R8`, `N14`) | LINQ over a strongly-typed capability collection. There is no cell range to mis-type; a mismatched aggregate does not compile |
| **Baseline security** (`§6`) | Antiforgery tokens by default, Razor auto-escaping, EF Core parameterising every query, Identity password hashing, authorisation as page conventions enforced server-side |

### Why not the alternatives

| Option | Why not |
| --- | --- |
| **A** — client-side SPA | Violates `N1` outright. Useful only as a throwaway flow prototype; never the product |
| **B** — SPA + REST + PostgreSQL | Highest ceiling and highest plumbing cost. Two codebases, two build chains, auth across the boundary — and it leans hardest on the skill the audit shows we are thinnest in. The ceiling is above what the MVP requires |
| **C** — Django + HTMX | The right shape, and the closest option to what we chose. Rejected on language fit only: nobody on the team has shipped a Django application, and one has now been shipped in .NET |
| **D** — Power Platform | Replaces a spreadsheet-adjacent low-code tool with another one. Golden-file testing is awkward, logic sits in a proprietary environment, and licensing and tenancy access are outside our control |
| **E** — inside UWA systems | Deferred by the client on 29 July 2026: *"integration would be ideal, but I think that might be a bit challenging"* — a functional standalone website first |

## Honesty note — the order in which this happened

**The spike preceded this record.** We built to learn, and this ADR documents what we learned;
presenting it as a decision taken in advance and then executed would be a tidier story and a
false one. Two things follow, and we do both rather than either:

1. The record is written, dated, and reconciled with `architecture.md` §8, so the repository no
   longer argues for one stack while shipping another.
2. The spike is treated as **evidence, not as a commitment**. Nothing in `src/` is exempt from
   the same review, testing and validation standard as code written after this date — and the
   first task under this ADR is to extract the calculation out of the page models into the
   framework-independent engine library this decision requires.

## Consequences

**Positive**

- One codebase, one language, one deployment. Auth, ORM, migrations, antiforgery and validation arrive on day one.
- `N1` is satisfied structurally rather than by discipline.
- A working vertical slice already exists, which converts the largest delivery risk into a known quantity.
- Decimal arithmetic is the default, not something to remember.

**Negative, and what we do about them**

| Consequence | Response |
| --- | --- |
| .NET 10 is very new; fewer worked examples, some libraries lag | Pin the SDK in CI; keep the dependency surface minimal (one NuGet package today); commit the lockfile; avoid preview-only features |
| Less interactive polish than a SPA | Acceptable — the client asked for prompts and boxes. Richer interaction is added as progressive enhancement over working pages |
| Production store still undecided | EF Core makes the provider a one-line change and no raw SQL is written. Decided with hosting on 9 Sep 2026 |
| Framework knowledge concentrated in two members | Engine kept framework-independent; both developers pair on it; every PR reviewed by a second member |

**Follow-on work created by this decision**

| # | Action | Owner | By | Status |
| --- | --- | --- | --- | --- |
| 1 | Extract the calculation into a pure engine with no EF or ASP.NET dependency | Wenmin Luo | 30 Aug 2026 | ✅ **Done 25 Aug** — `RateEngine` is a static, pure function; `MethodConfigProvider` does the database work; `RicCalculationService` is the thin wrapper page models depend on |
| 2 | Replace the hard-coded `1.35` with versioned method configuration (`R5`) | Wenmin Luo | 30 Aug 2026 | ✅ **Done 25 Aug** — `MethodConfigs` table seeded as version `2026.1`; `k` arrives as configuration and a cycle stamps its `MethodVersion` when sealed (`R6`) |
| 3 | Add `architecture.md` §8 Option F and re-run the weighted comparison including it | Chenxu You | 26 Aug 2026 | ✅ **Done 25 Aug** — F leads at 151; §9 rewritten from a gate into a record of how it closed |
| 4 | Golden-file test against the client's worked example, wired as a CI merge gate | Jaswanth Vericherla | 4 Sep 2026 (**M1**) | ✅ **Done 2 Sep** — `tests/CostingTool.Engine.Tests` asserts $100.00 / $162.00 / $202.50 to the cent, plus the boundaries. The engine moved into `src/CostingTool.Engine`, a project with no package references at all, so the test suite reaches the arithmetic without touching EF or ASP.NET — R7 enforced by the compiler. `ci.yml` no longer warns that the gate is empty; `dotnet test` runs the solution and the formatting check is no longer `continue-on-error` |
| 5 | Stop tracking `src/bin/` and `src/obj/` (`git rm -r --cached`) | Wenmin Luo | Before the next code commit | ✅ **Done 25 Aug** — 76 files untracked, files kept on disk, no history rewrite |
| 6 | Replace the seeded demo credentials before anything is deployed to staging | Chenxu You | 2 Oct 2026 (**M5**) | ⚠️ Outstanding — a gate on the staging deployment (`risks.md` R14) |
| 7 | Move pay scales, capacity baselines and the cost/income categories into `MethodConfig` alongside `k` | Wenmin Luo | 11 Sep 2026 (**M2**) | ⚠️ Outstanding, part-done and re-dated. `k`, the decimal places and the half-cent rule are configuration, and the categories are now named constants in one place (`Models/RicCostEntry`) rather than string literals repeated across the engine, the page models, the validation and the dropdown's JavaScript — which closes the drift risk even though it is not yet a database row. Pay scales and capacity baselines remain hard-coded |
| 8 | **New.** Replace `EnsureCreated()` with EF Core migrations | Wenmin Luo | 2 Oct 2026 (**M5**) | ⚠️ Outstanding. `EnsureCreated` cannot evolve a schema, so every model change costs the local database. Locally that is a nuisance; once the client has entered data on staging it is data loss, which makes this a gate on the deployment rather than a tidy-up |
| 9 | **New.** Confirm two modelling decisions with the client that carry no source marker: whether a multi-year cost profile should be averaged into one annual figure, and whether the indirect-cost uplift is retained by the platform in the revenue projection | Dai Lam La La | With the next question batch | ⚠️ Outstanding. Both surfaced while extracting the engine; both are commented in the code as ours rather than theirs |

## Fallback trigger

If a working end-to-end slice — sign in → create cycle → enter inputs → see rates → seal →
export — is not running by the **end of week 8**, we cut stretch scope. We do **not** change
stack. The engine, data model and tests are isolated so that a stack change could be survived,
but making one at that point would be the wrong answer, and saying so in advance stops it being
an option we reach for under pressure.
