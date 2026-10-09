# Research Infrastructure Costing & Pricing Tool

**CITS5206 Professional Computing — capstone project, The University of Western Australia · Group 13**
**Client:** UWA Research Infrastructure

A guided web application that takes a research-infrastructure platform custodian through
UWA's costing method, computes the platform's hourly (or per-unit) rates, has them approved by
the delegated authority, and seals the result as a record that can be filed and defended
years later. It replaces an Excel calculator that, in the client's words, is hard to use
because it is easy to break.

---

## Contents

1. [Status](#status)
2. [Start here](#start-here)
3. [Run it on your machine](#run-it-on-your-machine)
4. [The problem](#the-problem)
5. [Who uses it](#who-uses-it)
6. [The calculation](#the-calculation)
7. [What was delivered](#what-was-delivered)
8. [How we know it works](#how-we-know-it-works)
9. [Deployment](#deployment)
10. [Known gaps and open questions](#known-gaps-and-open-questions)
11. [Client](#client)
12. [Team and ways of working](#team-and-ways-of-working)
13. [Use of generative AI](#use-of-generative-ai)
14. [Technology](#technology)
15. [Repository layout](#repository-layout)
16. [Conventions and confidential material](#conventions-and-confidential-material)
17. [Ownership](#ownership)
18. [Deliverables](#deliverables)

---

## Status

**As of 9 October 2026.** The plan of record is [`docs/project/plan.md`](docs/project/plan.md).

| | |
| --- | --- |
| **Scope** | All 18 Must stories (110 points) and US-20, approval by the delegated authority, are built and closed. Signed by the client on 20 August 2026 |
| **Tests** | 240 automated tests across the engine, the sealed-record PDF and the web workflow, run by [CI](.github/workflows/ci.yml) on every push. The client's worked example is asserted to the cent |
| **Client testing** | 9 October 2026: the client used staging in all three roles and sent written notes ([record](docs/client/communication-history/2026-10-09-client-testing-notes/README.md)). Five of their six requests are done; the sixth is salary pre-fill (US-05, [#42](https://github.com/ChenxuYou/uwa-cits5206-project/issues/42)) |
| **Staging** | Live since 30 September 2026, reached by IP address; the address and one account per role were sent to the client by email. It still runs an earlier branch; redeploying it from `main` is [#101](https://github.com/ChenxuYou/uwa-cits5206-project/issues/101) |
| **Release** | `v1.0.0`, to be tagged on `main` once staging runs it |
| **Handover** | Expected Monday 12 October 2026 — [handover document](docs/handover.md) |
| **Final report** | Due Tuesday 13 October 2026, 11:59 pm (UTC+8) |

## Start here

| You want to… | Read |
| --- | --- |
| Run the tool on your own machine | [Run it on your machine](#run-it-on-your-machine), then [`src/README.md`](src/README.md) |
| Use the tool — as a custodian, an approver or an administrator | [User manual](docs/user-manual.md) |
| Deploy it to a server | [`deploy/README.md`](deploy/README.md) |
| Take it over | [Handover document](docs/handover.md) |
| Know what is not done | [Known gaps](src/README.md#known-gaps) and the [open issues](https://github.com/ChenxuYou/uwa-cits5206-project/issues) |
| Check the method and the scope | [Requirements](docs/spec/requirements.md), [user stories](docs/spec/user-stories.md), [architecture](docs/spec/architecture.md) |
| See how AI was used | [AI statement](docs/project/ai-use.md) |
| See how the project was run | [Plan](docs/project/plan.md), [risks](docs/project/risks.md), [meeting minutes](docs/meetings/README.md) |

## Run it on your machine

**You need** the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (`dotnet --info`
must list a `10.0.x` SDK) and Git. Nothing else: the database is a local SQLite file that the
application creates on first run.

```bash
git clone https://github.com/ChenxuYou/uwa-cits5206-project.git
cd uwa-cits5206-project
dotnet run --project src/CostingTool.csproj
```

Open **<https://localhost:7267>**. The first run shows a certificate warning for the local
development certificate; `dotnet dev-certs https --trust` clears it for good.

Sign in with a demo account:

| Role | Username | Password |
| --- | --- | --- |
| Platform custodian | `entry` | `Entry123!` |
| Delegated approver | `approver` | `Approve123!` |
| Administrator | `admin` | `Admin123!` |

These accounts are created **in the Development environment only**, which `dotnet run` uses.
A deployed server starts with one administrator and no demo accounts
([`deploy/README.md`](deploy/README.md)).

**A five-minute tour.** As `entry`, select **Start new costing cycle** and work through the six
steps — Platform, Costs, Funding, Capacity, Rates, Review — then submit. Sign out, sign in as
`approver`, open the submission and **Approve & seal** it. Back as `entry`, open the sealed
record and select **Export PDF**. The [user manual](docs/user-manual.md) explains each screen.

**Run the tests:**

```bash
dotnet test CostingTool.sln
```

In VS Code, open the repository folder (the one containing `CostingTool.sln`), install the
recommended **C# Dev Kit** extension and press <kbd>F5</kbd>. Editor tasks, troubleshooting and
how to change the data model are in [`src/README.md`](src/README.md).

---

## The problem

UWA runs research infrastructure — electron microscopes, a human MRI, radio telescopes,
phenotyping drones — that is expensive to buy and to operate. Part of that cost is passed on to
the researchers who buy time on it, and because UWA is publicly funded, the way those prices are
set has to be **transparent, consistent across every platform, and defensible years after the
fact**. The aim is sustainability, not profit.

The client already had the method, in a guide and an Excel calculator. **The calculator's
fragility belongs to the medium, not to that file.** Sharing a workbook shares its formulas, and
nothing marks a cell to fill apart from a cell that computes, so mistakes are silent: a cleared
formula, a range dragged one column too far or an amount typed with an extra zero all still
return a plausible number, and nothing records how the number was reached. Where the guide and
the calculator disagree, **the guide governs**, as the client confirmed in writing on 20 August
2026. See [`requirements.md` §2](docs/spec/requirements.md#2-the-problem).

What success looks like, in the client's words:

> If someone comes to us and says "why does it cost $50 an hour for me to use?", we want to
> be able to say: well, it costs $100,000 a year to run, this is how many hours a year it's
> going to be used, we divide one by the other — $50 an hour. That's the reason we charge
> that price.
>
> — UWA Research Infrastructure, client walkthrough, 29 July 2026

## Who uses it

| Role | Who they are | What they do in the tool |
| --- | --- | --- |
| **Platform custodian** | Academic or professional staff who run a platform — the client's own word for them | Enter the platform's costs, funding and forecast use; propose rates and explain them; submit. They do this roughly **once every three to five years**, so the tool has to explain itself |
| **Delegated approver** | Typically the head of the business unit that carries the platform's costs | Review a submission, then return it with comments or approve and seal it |
| **Administrator** | Whoever UWA names to manage access | Create and manage accounts; view every cycle, read-only |

## The calculation

Total operating cost, less non-variable income, divided by **forecast** use. Three rates come
out, one per user category, for **each capability** of a platform:

```
R_uwa        = (C − I_total)   / U
R_apfr       = ((C − I_nonuwa) / U) × k
R_commercial = (C / U)               × k
```

| Symbol | Meaning |
| --- | --- |
| `C` | Total annual operating cost for the capability |
| `I_total` | All non-variable income: UWA GP/in-kind + State + Federal (incl. NCRIS) + Other |
| `I_nonuwa` | The same, less the UWA portion |
| `U` | **Forecast** annual utilisation — not capacity |
| `k` | `1.35`, UWA's standard indirect-cost recovery for any external party |

The client's worked example, reproduced to the cent by a golden-file test in
[`tests/CostingTool.Engine.Tests`](tests/CostingTool.Engine.Tests/):

| | |
| --- | --- |
| Operating costs · UWA in-kind · WA Government support | $150,000 · $20,000 · $30,000 |
| Forecast utilisation | 1,000 hours |
| → UWA researcher · APFR · Commercial | **$100.00** · **$162.00** · **$202.50** per hour |

Two things are easy to get wrong, and the tool guards both. The divisor is forecast use, not
capacity: a capability with 1,882.5 hours of machine availability may see far less real use.
And costs are entered both per capability and per platform, with platform costs split evenly
across capabilities and the split shown on screen.

## What was delivered

A guided six-step workflow — **Platform → Costs → Funding → Capacity → Rates → Review** — followed
by approval and a sealed record.

- **Nothing is anonymous.** Every user signs in, and each record keeps who created, submitted, returned and approved it.
- **The calculation stays on the server**, out of the user's reach, and every input is validated there, so `$20,000` cannot become `$200,000` unnoticed.
- **Totals cannot drift.** Every total is summed over the same set of capabilities as the figures it is compared with — the calculator's own failure mode, made structurally impossible.
- **Every number is explained.** Each rate is shown beside the figures that produce it, and the custodian records the reasoning behind costs, utilisation, benchmarking and proposed rates.
- **Approval seals the record.** Its inputs, workings and decision are frozen as a snapshot that cannot be edited or deleted, and it exports as a PDF for UWA's Content Manager (TRIM). A sealed record can be superseded by a new cycle, never changed.

| Priority | Stories | State |
| --- | --- | --- |
| Must | All 18: US-01 to US-04, US-06 to US-19 | **Delivered** |
| Should | US-20 approval by the delegated authority | **Delivered** |
| Should | US-24 record the benchmarking | Partly: a free-text field on the rates step, carried into the sealed record and its PDF; the guide's structured prompts are not built |
| Could | US-22 compare with the last cycle | Partly: a new cycle shows the previous sealed record's key figures beside it |
| Should / Could | US-05 salary pre-fill, US-21 method configuration screen, US-23 replacement reserve, US-25 price-change communication | Not built — open issues |

Stories and acceptance criteria: [`docs/spec/user-stories.md`](docs/spec/user-stories.md).

## How we know it works

| Evidence | What it shows | Where |
| --- | --- | --- |
| **Engine tests** — 36 | The client's worked example to the cent; decimal arithmetic, with rounding once at presentation; zero or negative inputs refused; the capacity baselines and staff caps | [`tests/CostingTool.Engine.Tests`](tests/CostingTool.Engine.Tests/) |
| **PDF tests** — 31 | The worked example reaches the PDF to the cent; every rate is printed beside its arithmetic, with the method version and integrity hash; unsealed or damaged records are refused; older records still render as sealed | [`tests/CostingTool.Pdf.Tests`](tests/CostingTool.Pdf.Tests/) |
| **Web tests** — 173 | Validation on every step, editing lines, rates and variances, sign-in, lockout and record ownership, approval and sealing under concurrent requests, supersession, migrations, and the staff fields added after client testing | [`tests/CostingTool.Web.Tests`](tests/CostingTool.Web.Tests/) |
| **Continuous integration** | Every push and pull request is built and tested, formatting is checked, dependencies are scanned for known vulnerabilities, and a model change without a database migration fails the build | [`.github/workflows/ci.yml`](.github/workflows/ci.yml) |
| **Code review** | Nothing merges on its author's approval, and nobody approves a pull request in their own layer | [`plan.md` §3](docs/project/plan.md#3-responsibilities) |
| **Deployment rehearsal** — 25 Sep | First release, HTTPS, forced password change, backup, restore and automatic rollback of a deliberately broken release | [`deploy/README.md`](deploy/README.md) |
| **Client acceptance testing** — 9 Oct | The client used the deployed tool in all three roles and sent written notes | [Testing record](docs/client/communication-history/2026-10-09-client-testing-notes/README.md) |

## Deployment

The tool is a standalone web application: ASP.NET Core behind Caddy, which provides HTTPS, on
one Ubuntu 24.04 server, with the SQLite database, daily backups and the cookie keys kept outside
the application release. Releasing, restoring and rolling back are one script each.
**Runbook: [`deploy/README.md`](deploy/README.md).**

- **Staging** has been live since 30 September 2026. It is reached by IP address, so browsers
  show a certificate warning; its address and accounts were sent to the client by email and are
  not recorded in this public repository. It was deployed from the branch
  `feat/deployment_docker` (Docker Compose and PostgreSQL) and is being redeployed from `main`
  with the runbook ([#101](https://github.com/ChenxuYou/uwa-cits5206-project/issues/101)).
- **Production** does not exist yet. Who runs it, where, and on which database are UWA's
  decisions, set out in [handover §6](docs/handover.md#6-decisions-uwa-needs-to-make).

## Known gaps and open questions

**Every known gap is a GitHub issue**, listed with its reason in
[`src/README.md` — Known gaps](src/README.md#known-gaps). The
[open issues](https://github.com/ChenxuYou/uwa-cits5206-project/issues) also hold the stretch
stories not built, and [handover §8](docs/handover.md#8-known-limitations-and-recommended-next-work)
puts them in the order we would do them.

**Seven questions are open**, each with a working answer already in the tool, so a different
answer from UWA is a contained change. Full list, with reasons and sources:
[`requirements.md` §9](docs/spec/requirements.md#9-open-questions).

| # | Question | Issue |
| --- | --- | --- |
| Q8 | What licence does this repository carry, given jointly owned IP? | — see [Ownership](#ownership) |
| Q11 | Does the approver's approval seal the record, or the custodian's confirmation? | [#105](https://github.com/ChenxuYou/uwa-cits5206-project/issues/105) |
| Q12 | Is a multi-year cost profile averaged into one annual figure? | [#103](https://github.com/ChenxuYou/uwa-cits5206-project/issues/103) |
| Q13 | Does the platform keep the indirect-cost uplift in its revenue projection? | [#104](https://github.com/ChenxuYou/uwa-cits5206-project/issues/104) |
| Q14 | Who runs the tool after handover, where, and on which database? | [Handover §6](docs/handover.md#6-decisions-uwa-needs-to-make) |
| Q15 | Should a superseded record's PDF say that it is superseded? | [#106](https://github.com/ChenxuYou/uwa-cits5206-project/issues/106) |
| Q16 | What do "LG funded" and "GP funded" stand for, and should either change the costing? | [Handover §7](docs/handover.md#7-open-questions-on-the-method) |

## Client

**UWA Research Infrastructure** — Erika Slavin, Manager (Research Infrastructure & Partnerships)
/ Business Development Coordinator, and Mathew Hall, Strategic Development Coordinator
([contacts](docs/client/contacts.md)).

| Date | What happened | Record |
| --- | --- | --- |
| 29 Jul 2026 | Kick-off walkthrough of the method, the guide and the calculator | [Minutes](docs/meetings/client/2026-07-29-client-meeting.md) |
| 20 Aug 2026 | **Scope and ownership signed** by Mathew Hall; five written answers to our questions. The PDF must show the calculator's workings, which became tracked work | [Minutes](docs/meetings/client/2026-08-20-client-meeting.md), [signed scope](docs/client/communication-history/2026-08-20-client-meeting/project-scope-summary-signed.pdf), [MVP agreement](docs/client/mvp-agreement.md) |
| 22 Sep 2026 | First look at the working tool; agreement that the team may host staging until UWA IT provides resources | [Minutes](docs/meetings/client/2026-09-22-client-meeting.md) |
| 30 Sep 2026 | Staging address and one account per role sent to the client | [#60](https://github.com/ChenxuYou/uwa-cits5206-project/issues/60) |
| 9 Oct 2026 | Client testing in all three roles; six change requests, five done the same day | [Testing record](docs/client/communication-history/2026-10-09-client-testing-notes/README.md) |

Everything that crossed to the client is filed in
[`docs/client/communication-history/`](docs/client/communication-history/).

## Team and ways of working

| Member | Technical layer | GitHub |
| --- | --- | --- |
| Chenxu You | General backend, repository and documentation | [ChenxuYou](https://github.com/ChenxuYou) |
| Yichen Zhao | Front end; client liaison | [itsEvanZHAO](https://github.com/itsEvanZHAO) |
| Wenmin Luo | Calculation engine | [onikirinana](https://github.com/onikirinana) |
| Dai Lam La La | Deployment | [ladailam382](https://github.com/ladailam382) |
| Jaswanth Vericherla | Authentication and accounts | [jaswanth-kumar24](https://github.com/jaswanth-kumar24) |

The roster's home is [`docs/project/team.md`](docs/project/team.md); the layers were agreed on
15 September 2026 ([minutes](docs/meetings/team/2026-09-15-team-meeting.md)).

- **Planning.** One-week sprints against seven milestones, M1 to M7, in [`plan.md`](docs/project/plan.md); risks in [`risks.md`](docs/project/risks.md).
- **Tracking.** Every task is a GitHub issue with an owner and a milestone, on the [Projects board](https://github.com/users/ChenxuYou/projects/2).
- **Meetings.** A weekly online stand-up, fortnightly on campus with the facilitator, and the client on Wednesdays as needed plus a shared Teams chat. Minutes are in [`docs/meetings/`](docs/meetings/README.md).
- **Review.** Every change arrives by pull request and is reviewed by a second member; CI must pass before merge.

## Use of generative AI

AI was used as **a tool that drafts, never as an author that decides**. It drafted code, tests
and documents, reviewed pull requests and helped the team learn ASP.NET Core and EF Core. Every
output was checked by a person against something other than the AI — the client's worked
example, the test suite, the code, or the record on GitHub — and the person who committed it
owns it. No client data, personal data or third-party IP went into a public AI tool.

The [AI statement](docs/project/ai-use.md) sets out the team's rules, the tools and what each was
used for, how output was checked, how this maps to UWA and ACS requirements, each member's own
account with examples of output that was corrected or rejected, and what we learned.

## Technology

**ASP.NET Core Razor Pages with Entity Framework Core on .NET 10, and SQLite** — recorded in
[ADR-001](docs/decisions/adr-001-technology-stack.md). Six options were assessed
([`architecture.md` §8](docs/spec/architecture.md#8-options-assessed)); a server-rendered
monolith came out ahead because it keeps the calculation on the server and gives one codebase
with framework-provided sign-in and validation. A timeboxed spike then showed the team could
build it fastest in C#, whose native `decimal` type also suits money arithmetic that has to be
exact. The PDF is drawn with MigraDoc from the sealed snapshot
([ADR-002](docs/decisions/adr-002-pdf-generation.md)).

The calculation engine is its own project with no database, UI or package dependency, and its
method configuration is versioned, so a record sealed in 2026 reproduces its figures under the
method it was sealed with. Schema changes go through EF Core migrations.

## Repository layout

```
├── src/                         The application — how to run and change it: src/README.md
│   ├── CostingTool.Engine/        The calculation, with no dependencies at all
│   ├── CostingTool.Pdf/           The sealed-record PDF
│   └── CostingTool.csproj         The web application (Razor Pages, EF Core, SQLite)
├── tests/                       Engine, PDF and web test projects
├── deploy/                      Production runbook: systemd, Caddy, release, backup, restore
├── docs/
│   ├── user-manual.md             For custodians, approvers and administrators
│   ├── handover.md                What UWA receives, and what it has to decide
│   ├── spec/                      Requirements, user stories, architecture
│   ├── decisions/                 ADR-001 technology stack, ADR-002 PDF generation
│   ├── project/                   Plan, risks, team, skills audit, AI statement
│   ├── client/                    Contacts, MVP agreement, and everything that crossed to the client
│   ├── meetings/                  Minutes — client, facilitator and team
│   ├── assignments/               What was submitted to the unit
│   └── internal/                  Our own review notes — not committed
├── reference/unit/              Assignment briefs, rubrics and unit material
├── presentations/               Self-contained HTML decks and their style guide
├── scripts/                     One-off repository tooling, not application code
├── .github/workflows/ci.yml     Build, test, format check and dependency scan
├── .vscode/                     Shared editor setup — F5 runs the app
├── CostingTool.sln              All projects, so one command builds and tests them
└── NOTICE                       Ownership, and UWA's permission to use the tool
```

## Conventions and confidential material

**Sources.** The client's guide, calculator and walkthrough do not always agree, so
[`requirements.md`](docs/spec/requirements.md) ranks them — guide **[G]**, then calculator
**[W]**, then our minutes of the walkthrough **[K]** — and marks every statement with its
source.

**Naming.** Paths are lowercase-kebab-case, and anything tied to a date starts `YYYY-MM-DD-`.
Two exceptions: `src/` follows .NET's PascalCase, and files received from the client or
submitted to the unit keep the name they arrived or left under.

**Formats.** Documentation is Markdown and presentations are single HTML files, so both can be
reviewed like code. PDFs are kept only for submitted or signed material.

**Not committed, on purpose** ([`.gitignore`](.gitignore) gives the reasons):

| Not committed | Why |
| --- | --- |
| The client's guide, calculator and documents (`reference/client/`) | The client's material, not ours to publish. Our own summaries are committed with their source named |
| Meeting recordings and transcripts | Identifiable voices and unreviewed speech; written minutes are the record |
| Internal review notes (`docs/internal/`) | Working critique of our own documents, not a deliverable |
| Credentials, keys, `.env` files, local databases, API state dumps | The obvious reasons, and stale snapshots invite misplaced trust |

Two files committed before these rules settled — an early meeting transcript and an MIT
`LICENSE` — were rewritten out of the history; neither appears anywhere in it.

## Ownership

UWA owns the costing method; the team owns the code and may show it in portfolios; the
overarching IP is held jointly, and the tool is not to be sold onward. Agreed on 29 July 2026
and **signed by the client on 20 August 2026**.

The repository carries **no licence: all rights reserved**, and [`NOTICE`](NOTICE) grants UWA a
perpetual, royalty-free permission to use, run, copy, modify, host and internally distribute the
tool. A licence granted by one joint owner alone may not be effective, so the choice of licence
is left to be agreed with UWA at handover ([Q8](docs/spec/requirements.md#9-open-questions)). A
noncommercial licence such as PolyForm is the likely answer; an open-source licence would not
fit, because MIT, Apache-2.0, GPL and AGPL all permit sale.

## Deliverables

| Assessment | Type | State |
| --- | --- | --- |
| Assignment 1 — project specification and plan | Group | **Submitted** 25 Aug 2026 — [PDF and source](docs/assignments/assignment-1/) |
| Assignment 2 — software feature report | Individual | Each member's own; due 29 Sep 2026 |
| Assignment 3 — professional reflection | Individual | Each member's own; due 29 Sep 2026 |
| Assignment 4 — pitch video | Individual | Each member's own |
| Assignment 5 — final group report | Group | Due Tue 13 Oct 2026, 11:59 pm (UTC+8) |
| Group Member Evaluation | Individual, on Feedback Fruits | Due Mon 19 Oct 2026, 11:59 pm |

Briefs and rubrics: [`reference/unit/`](reference/unit/).
