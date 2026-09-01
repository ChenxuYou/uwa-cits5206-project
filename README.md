# Research Infrastructure Costing & Pricing Tool

**CITS5206 Professional Computing — Capstone Project, The University of Western Australia**
**Client:** UWA Research Infrastructure
**Status:** **Assignment 1 submitted on 25 August 2026** — one PDF, [`Group13-Project Spec and Plans.pdf`](docs/assignments/assignment-1/). Scope signed off by the client on 20 August 2026 — both confirmations, scope and ownership. Requirements written against the client's own documents. The technology decision is settled and recorded: **ASP.NET Core Razor Pages with EF Core** — see [ADR-001](docs/decisions/adr-001-technology-stack.md) and [Technology](#technology). Next: **M1, the engine provably correct, 4 September 2026** — [`docs/project/plan.md`](docs/project/plan.md).

---

## The problem

UWA runs research infrastructure — electron microscopes, a human MRI, radio telescopes,
phenotyping drones — that is expensive to buy and expensive to operate. Some of that cost is
passed on to the researchers who buy time on it, and because UWA is a publicly funded
institution, the way those prices are set has to be **transparent, consistent across every
platform, and defensible years after the fact**. The aim is sustainability, not profit.

The client already has the logic. It lives in a guide and an Excel workbook that is, in their
words, hard for anyone to actually use because it is easy to break. What they want is a guided
web application that asks a platform custodian for the inputs, keeps the calculation out of
reach behind the form, and produces a record that can be filed and retrieved.

**The fragility belongs to the medium, not to that particular file.** Sharing a workbook shares
its formulas: logic and data arrive in one object, with one set of permissions, and nothing marks
a cell the user should fill apart from a cell that computes. So mistakes are silent — a cleared
formula, a range dragged one column too far, or an amount typed with one extra zero all return a
plausible number — and nothing records how the number was reached. Where the client's guide and
their calculator disagree, **the guide governs**, confirmed in writing on 20 August 2026; a full
reconciliation of the calculator is work for a later cycle, once the engine exists to compare
against. See [`docs/spec/requirements.md` §2](docs/spec/requirements.md#2-the-problem).

The sentence the client used to describe what success looks like:

> If someone comes to us and says "why does it cost $50 an hour for me to use?", we want to
> be able to say: well, it costs $100,000 a year to run, this is how many hours a year it's
> going to be used, we divide one by the other — $50 an hour. That's the reason we charge
> that price.
>
> — UWA Research Infrastructure, client walkthrough, 29 July 2026

## Who uses it

**Platform custodians** — the academic or professional staff who run a platform, not the
researchers who buy time on it. (*Custodian* is the client's own word, used throughout their
guide.) They are technical people for whom this is administrative work, and they run the
exercise roughly **once every three to five years** to set rates for the period ahead. Low
frequency, high stakes: the tool has to be self-explanatory, because nobody will remember it
from last time.

Rates are then approved by a **delegated authority** — typically the head of the business unit
that carries the platform's operating costs.

## The calculation

Total operating cost, less non-variable income, divided by forecast utilisation. Three rates
come out, one per user category, and they are computed **per capability** — a platform holding
seven capabilities produces seven sets of three.

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
| `k` | `1.35`, UWA's standard indirect cost recovery applied to any external party |

The client's own worked example, which is also the first test we will write:

| | |
| --- | --- |
| Operating costs, UWA in-kind, WA Gov support | $150,000 · $20,000 · $30,000 |
| Forecast utilisation | 1,000 hours |
| → UWA Researcher · APFR · Commercial | **$100.00** · **$162.00** · **$202.50** per hour |

Two things are easy to get wrong. The divisor is *forecast* use, not capacity — a capability
with 1,882.5 hours of machine availability may see far less real use, and weekends,
maintenance windows, staff FTE and even weather all cut into it. And costs are captured both
per capability and per platform, with platform costs split evenly across capabilities.

The engine is validated against the client's worked example as a golden-file test before any
UI is written.

## What the MVP delivers

A guided web form in **three sequential sections**, mirroring the three sheets of the workbook:

1. **Costs** — staffing, consumables, maintenance contracts, utilities, other operating costs, plus the four lines of non-variable income that offset them.
2. **Capacity and utilisation** — billable unit (hours, days or samples), full capacity, forecast use, and the reasoning behind the forecast.
3. **Rates** — three calculated charge-out rates per capability, with room to adjust inputs and see the effect before committing.

Running through it:

- Users sign in; no record is created anonymously, and every record carries who made and sealed it.
- Calculation logic stays server-side, out of the user's reach.
- Mandatory fields and type validation, so `$20,000` cannot be entered as `$200,000` unnoticed.
- Every total is summed over the same set of capabilities as the figures it is compared against — the workbook's own failure mode, made structurally impossible.
- Free-text justification boxes throughout — the rate has to be defensible, not merely correct.
- On submit, the inputs and results are sealed into a record that can be exported and filed, and read back in three years' time.

Deployment is a standalone web application. Integration into existing UWA systems was
discussed and deferred; a working website comes first.

## Client sign-off

Our client is **UWA Research Infrastructure**: **Erika Slavin**, Manager (Research Infrastructure
& Partnerships) / Business Development Coordinator, and **Mathew Hall**, Strategic Development
Coordinator. Names, roles and how we contact them live in
[`docs/client/contacts.md`](docs/client/contacts.md).

**The client signed the scope statement on 20 August 2026.** Mathew Hall — both confirmations
ticked: the scope is right, and the ownership position is right.

The paper trail, in one place: the scope statement and five questions went by email on **17
August**; the client replied on **18 August** confirming a time; we met in person on **20
August**; the signed document and written answers to all five questions came back the same day.
Everything is filed in
[`docs/client/communication-history/`](docs/client/communication-history/) and the meeting is
[minuted](docs/meetings/2026-08-20-client-meeting.md).

The client's answers also changed one thing we had promised. The exported PDF must show **the
calculator's workings**, not only the inputs and the three rates, and records are filed into
UWA's Content Manager (TRIM). That is real additional work and it is tracked as such rather than
absorbed quietly — [minutes §5](docs/meetings/2026-08-20-client-meeting.md).

## Open questions

**One is open.** Five went to the client and all five came back answered — multi-year cycles,
access control, the record format, and both questions about whether the guide or the calculator
governs. Three earlier questions were **closed by reading the client's own documents** rather
than by asking. The full list is in
[`docs/spec/requirements.md`](docs/spec/requirements.md#9-open-questions).

The one that remains is ours and cannot be closed by us alone: **what licence this repository
carries** (Q8), because the IP is jointly held. The 20 August signature **unblocks** it — the
joint position is now confirmed in writing, which is what it was waiting for — but does not close
it, because confirming the position is not the same as choosing the licence that follows from it.
Until then the repository is all rights reserved and [`NOTICE`](NOTICE) carries the permissions.
See [Ownership](#ownership).

## Repository layout

`docs/` is grouped by who a document is for — the specification, how the team runs, and what
crosses to the client — because those three readerships want different things and were becoming
hard to tell apart in a single flat folder.

```
├── docs/
│   ├── spec/               What we are building
│   │   ├── requirements.md     What the client needs, and what is still open
│   │   ├── user-stories.md     Personas, epics, stories and acceptance criteria
│   │   └── architecture.md     System shape, options assessed, and the decision gate
│   ├── project/            How the team runs
│   │   ├── team.md             The roster — the only place it lives
│   │   ├── plan.md             Milestones, sprints and story assignment to 13 Oct
│   │   ├── risks.md            The risk register — likelihood, impact, mitigation, trigger, owner
│   │   ├── skills-audit.md     Where our gaps are, and what is done about each
│   │   ├── assignment-1-readiness.md   The 22 Aug rubric assessment — closed, kept as the record
│   │   └── assignment-1-completion-plan.md   Who did what, by when — closed, submitted 25 Aug
│   ├── assignments/        What was submitted, and the material it was built from
│   │   └── assignment-1/       Group13-Project Spec and Plans.pdf — submitted 25 Aug 2026,
│   │                           alongside submission-draft.md, the markdown it came from
│   ├── client/             Everything that crosses to the client
│   │   ├── contacts.md         Client names and roles — the only place they live
│   │   ├── 2026-08-15-scope-and-questions.md   The document they receive
│   │   ├── mvp-agreement.md    Why that scope, traced to requirement IDs — and the sign-off trail
│   │   ├── questions-round-1.md   Why those questions, and our defaults
│   │   └── communication-history/  What actually crossed, one folder per exchange
│   │       ├── 2026-08-17-email-scope-and-questions/       The outbound email and its two attachments
│   │       └── 2026-08-20-client-meeting/     The signed scope statement, the client's written
│   │                               answers, and our notes from the room
│   ├── meetings/           Minutes, one file per meeting
│   ├── decisions/          Architecture and process decision records
│   │   └── adr-001-technology-stack.md   Why ASP.NET Core Razor Pages
│   └── internal/           Our own review notes — not committed
├── .github/workflows/      CI — build, test and dependency scan on every push and PR
├── presentations/          Self-contained HTML decks, one file per deck
│   ├── README.md           How to build, present and export a deck
│   ├── STYLE-GUIDE.md      Binding style policy — read before building a deck
│   ├── template.html       Empty skeleton; copy it, never present from it
│   └── assets/
├── reference/
│   ├── client/             Client material — local only, not committed
│   └── unit/               Assignment briefs, rubric and unit resources
├── scripts/                One-off repository tooling, not application code
│   ├── seed-project-board.py   Milestones, labels, issues and the Projects board, built
│   │                           from docs/spec/user-stories.md and docs/project/plan.md
│   ├── dedupe-story-issues.py  One issue per story — keeps the first, deletes the rest
│   ├── backfill-issues.py      Assigns unowned Must stories; opens the deploy issue
│   └── export-issues.py        Dumps live issue state to issues.json (not committed)
├── src/                    Application code — ASP.NET Core Razor Pages (see the note below)
├── .gitattributes          Line endings — LF everywhere, so diffs stay readable
├── .gitignore              What never gets committed, and why
├── NOTICE                  Ownership, the grant to UWA, and portfolio use
└── README.md
```

Some working files live alongside these and are deliberately local — see
[Confidential material](#confidential-material).

Everything under `docs/` and `presentations/` is **markdown or HTML**, deliberately. Plain-text
artefacts diff in GitHub and review like code; Word documents and `.pptx` files do not.
Presentations are built as single self-contained HTML files for the same reason — see
[`presentations/STYLE-GUIDE.md`](presentations/STYLE-GUIDE.md) §8.

### Naming

One convention, so that a path can be guessed rather than looked up:

| Rule | Example |
| --- | --- |
| Folders and files are **lowercase-kebab-case** | `docs/client/communication-history/` |
| Anything tied to a date is prefixed **`YYYY-MM-DD-`** | `2026-08-20-client-meeting.md` |
| No spaces, no capitals, no camelCase in a path | `submission-draft.md`, not `submissionDrafts.md` |
| Repository meta-documents keep their conventional capitals | `README.md`, `NOTICE`, `STYLE-GUIDE.md` |
| The same thing is spelled the same way everywhere | `assignment-1-*`, never `assignment1-*` |

**`src/` is the deliberate exception.** C# and ASP.NET Core expect PascalCase files and folders
(`Pages/Ric/Rates.cshtml`, `Services/RicCalculationService.cs`), and fighting a framework's own
convention costs more than it buys. The rule inside `src/` is the .NET rule.

Client-supplied filenames in `reference/client/` are **left exactly as the client sent them**
— [`requirements.md`](docs/spec/requirements.md) cites them by name as sources, and a renamed
source is a broken citation.

**Submitted files keep the name they were submitted under.** `docs/assignments/assignment-1/Group13-Project Spec and Plans.pdf`
has capitals and spaces and stays that way: it is the file the unit received, and a submission
renamed after the fact no longer matches what was marked. The convention applies to everything
we author for ourselves; the moment a file crosses to the client or the unit, the name it
crossed under is the name it keeps.

A rename pass on 22 August 2026 brought the tree to this convention: a misspelled `assginment/`
folder, spaces in `Communication history/` and `Email content.md`, non-ISO dates in
`Email-17-Aug/`, and a camelCase `submissionDrafts.md` all went. Every cross-reference was
rewritten with them and all 274 internal links were checked afterwards.

## Where our facts come from

The client gave us a costing & pricing guide, a working calculator, and a recorded walkthrough.
They do not always agree, so [`docs/spec/requirements.md`](docs/spec/requirements.md) sets a precedence
order and every statement is marked with its source:

| Rank | Source | Marker |
| --- | --- | --- |
| 1 | The client's costing & pricing guide — their normative policy document | **[G]** |
| 2 | The client's calculator workbook — a reference implementation, and demonstrably buggy | **[W]** |
| 3 | Our minutes of the spoken walkthrough — good for intent, unreliable for figures | **[K]** |

This is not bureaucracy. An earlier draft quoted a set of demonstration figures transcribed
from the walkthrough that appear in no client document, and one of them was heading for a test
fixture. The precedence rule is what caught it.

## Confidential material

Some files referenced in this repository are **deliberately absent** from it:

| Not committed | Why |
| --- | --- |
| Client spreadsheets and documents (`reference/client/`) | The client's material, sensitive while in progress, and not ours to publish |
| Meeting audio and video (`.m4a`, `.mp4`, …) | Identifiable voices |
| Transcripts of any kind — subtitle formats (`.srt`, `.vtt`, …) and `*-transcript.md` | Verbatim, unreviewed speech, whatever the file extension. Written minutes are the record, and they go in `docs/meetings/` |
| Internal review notes (`docs/internal/`) | Our own working critique of our own documents. Useful to us; not a deliverable, and not something to hand anyone half-finished |
| API state dumps (`issues.json`, `projects.json`) | What `scripts/` reads to work out what already exists before it changes anything. A snapshot of live state, stale the moment anyone touches an issue, and committing it invites someone to trust it. Re-export it; never read it out of a commit |
| Credentials, `.env` files, keys, local databases | The obvious reasons |

**There is no exception for internal meetings.** An earlier version of these rules let a
raw transcript be committed when no external party was present. It has been withdrawn: a
transcript is unreviewed speech about identifiable people either way, and the minutes are the
artefact anyone actually needs. Everything the team is asked to read is written up in
`docs/meetings/`.

The rules and the reasoning are in [`.gitignore`](.gitignore). Check any single path with
`git check-ignore -v <path>`. Committing something excluded needs team agreement and
`git add -f`.

**Ignoring a file does not remove it from history.** Two files were committed before these
rules settled — the 24 July team transcript, and an earlier `LICENSE` — and adding them to
`.gitignore` did nothing to the commits that already held them. Both have since been rewritten
out and the result force-pushed. The current history begins at `84ab707` and neither file
appears anywhere in it.

That pass cost one rewrite and a re-clone for everyone, which is what it costs while the
history is short. The lesson is the rule at the top of this section: the check happens before
the commit, not after it.

## How the team works

- **Weekly** online stand-up — progress, blockers, next week's allocation.
- **Fortnightly** in person on campus, including the facilitator checkpoint.
- **Client on Wednesdays as needed**, plus a shared Teams chat for asynchronous questions. The client asked us to digest and come back with batched questions rather than hold a fixed weekly slot; support is heavier up front and eases off later (agreed 29 July 2026).
- Every MVP task is a GitHub issue with a named owner and a milestone date, tracked on the [Projects board](https://github.com/users/ChenxuYou/projects/2) and planned in [`docs/project/plan.md`](docs/project/plan.md). Stretch stories sit on the board unassigned, because assigning work nobody has agreed to do is how a plan starts lying.
- Minutes are committed within 24 hours, so members who missed a meeting can be briefed from the repository. Two meetings from before this rule settled — 24 July and 5 August — are still to be written up.
- CI runs on every push and pull request: [`.github/workflows/ci.yml`](.github/workflows/ci.yml).

Team roster: [`docs/project/team.md`](docs/project/team.md).

## Technology

**Decided: ASP.NET Core Razor Pages with Entity Framework Core, targeting .NET 10.** Recorded in
[ADR-001](docs/decisions/adr-001-technology-stack.md), 24 August 2026.

Six options were considered. Five went to the facilitator on 5 August 2026 — a client-side SPA
with no backend (prototype only); an SPA with a REST API and PostgreSQL; a Django + HTMX
monolith; Microsoft Power Platform (rejected); and building inside existing UWA systems
(deferred at the client's request). The weighted comparison in
[`docs/spec/architecture.md` §8](docs/spec/architecture.md#8-options-assessed) put the
**server-rendered monolith** ahead, on criteria that deliberately weight delivery risk: one
codebase, framework-provided auth and validation, and nowhere for calculation logic to leak to.

The sixth is what we build: **the same architecture in the language the team can actually move
fastest in.** That was settled by evidence rather than argument — a timeboxed spike produced a
working end-to-end application in .NET, running in time for the client meeting of 20 August, and
the skills audit confirmed the team's depth is in C# and server-side web work rather than in
JavaScript frameworks. `decimal` being a native base-10 type in C# also maps directly onto the
requirement that money arithmetic be exact and defensible.

**The decision record was written after the spike, not before it**, and
[ADR-001](docs/decisions/adr-001-technology-stack.md) says so. We built to learn; the ADR
documents what we learned, reconciles it with the five options already assessed, and lists the
follow-on work the choice creates.

Settled regardless of stack: the calculation engine is a pure, versioned, unit-tested module with
no database or UI dependency — decimal arithmetic, a divide-by-zero guard, aggregates that
iterate rather than index, and method configuration versioned so that a record created in 2026
still reproduces its figures in 2030. SQLite is the development store; the production store is
decided together with hosting on 9 September 2026, and EF Core makes the provider a one-line
change.

## Deliverables

**Assignment 1 — project specification and plan: submitted.** One PDF,
`Group13-Project Spec and Plans.pdf`, uploaded by one member on **Tuesday 25 August 2026**,
against a deadline extended by one week from 18 August at our request and granted by the unit
coordinator by email on 14 August 2026. Four sections: problem statement, client communication
and MVP agreement, project management and plans, risk and technology assessment. The submitted
PDF and the markdown it was assembled from are in
[`docs/assignments/assignment-1/`](docs/assignments/assignment-1/); both link to this
repository, which the facilitator can open.

Brief and rubric: [`reference/unit/`](reference/unit/).

**Next: M1 — the engine provably correct, 4 September 2026.** The client's worked example has to
reproduce to the cent as a CI merge gate, written before any screen. Milestones M0–M7 and the
sprint plan to 13 October are in [`docs/project/plan.md`](docs/project/plan.md).

## Ownership

The client owns the costing logic. The team owns the code and may use the project in
portfolios; the client raised no objection to us sharing what we build. Selling the tool
onward would not be appropriate, as the overarching IP is joint. Agreed with the client on
29 July 2026 — see the [kickoff minutes](docs/meetings/2026-07-29-client-kickoff.md) §9 — and
**confirmed in writing on 20 August 2026**, signed, as confirmation 2 of the
[scope statement](docs/client/communication-history/2026-08-20-client-meeting/project-scope-summary-signed.pdf).

**No licence: all rights reserved, with permissions set out in [`NOTICE`](NOTICE).** That file
records who owns what, grants UWA a perpetual permission to use, modify and host the tool for
its own purposes, and reserves portfolio use for the authors.

This is an interim position, held deliberately. The IP is **jointly** held, and a licence
granted by one joint owner alone may not be effective — so the team does not purport to grant
one. That is [Q8](docs/spec/requirements.md#9-open-questions), and it closes at handover, not
before.

**The condition on it has now been met.** The position held until the ownership was confirmed in
writing; the client signed that confirmation on 20 August 2026. What remains is choosing the
licence, which is a decision for handover and not one to take in the week before an assignment
is due.

Why an open-source licence would not do the job in the meantime: MIT and Apache-2.0 permit
sale; **GPL and AGPL also permit sale** — copyleft requires source disclosure, it does not
restrict commerce; Creative Commons advises against CC licences for software. A noncommercial
licence such as PolyForm would fit the client's position, and remains the likely answer at
handover — but any licence binds only the people who receive it, not the copyright holders,
so it is not what stops the tool being sold. The joint-IP position is.

Reserving all rights costs us nothing here: `NOTICE` already gives UWA everything it needs,
and GitHub's terms already let any user view and fork a public repository.

An early commit carried a `LICENSE` file — MIT, copyright "RTMart" — which would have granted
the public exactly the right to sell that the joint-IP position rules out. Deleting the file
would not have retracted a grant already published, so the history was rewritten to remove it,
force-pushed, and checked for forks and clones predating the rewrite. Nothing in the current
history grants a licence to anyone.
