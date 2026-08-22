# Research Infrastructure Costing & Pricing Tool

**CITS5206 Professional Computing — Capstone Project, The University of Western Australia**
**Client:** UWA Research Infrastructure
**Status:** **Scope signed off by the client on 20 August 2026** — both confirmations, scope and ownership. Requirements written against the client's own documents. A working ASP.NET Core prototype exists in [`src/`](src/); **the written technology decision has not caught up with it** — see [Technology](#technology).

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

**The fragility is demonstrable, not rhetorical.** Reading the workbook turns up three silent
formula defects: one capability priced against another capability's capacity, the same
capability costed against another's cost base, and platform totals that sum revenue over six
columns while summing cost over eight. Together they produce a $2,079 "surplus" that is purely
an artefact and a revenue total understated by $4,875. Nothing in the spreadsheet flags any of
it. See [`docs/spec/requirements.md` §2](docs/spec/requirements.md#2-the-problem).

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

**The client signed the scope statement on 20 August 2026.** Mathew Hall, Strategic Development
Coordinator, UWA Research Infrastructure — both confirmations ticked: the scope is right, and the
ownership position is right.

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
│   │   ├── assignment-1-readiness.md   Where we stand against the marking rubric
│   │   └── assignment-1-completion-plan.md   Who does what, by when, before 25 Aug
│   ├── assignments/        Draft material for each submission
│   │   └── assignment-1/       submission-draft.md — the four sections in progress
│   ├── client/             Everything that crosses to the client
│   │   ├── 2026-08-15-scope-and-questions.md   The document they receive
│   │   ├── mvp-agreement.md    Why that scope, traced to requirement IDs — and the sign-off trail
│   │   ├── questions-round-1.md   Why those questions, and our defaults
│   │   └── communication-history/  What actually crossed, one folder per exchange
│   │       ├── 2026-08-17-email-scope-and-questions/       The outbound email and its two attachments
│   │       └── 2026-08-20-client-meeting/     The signed scope statement, the client's written
│   │                               answers, and our notes from the room
│   ├── meetings/           Minutes, one file per meeting
│   ├── decisions/          Architecture and process decision records
│   └── internal/           Our own review notes — not committed
├── presentations/          Self-contained HTML decks, one file per deck
│   ├── README.md           How to build, present and export a deck
│   ├── STYLE-GUIDE.md      Binding style policy — read before building a deck
│   ├── template.html       Empty skeleton; copy it, never present from it
│   └── assets/
├── reference/
│   ├── client/             Client material — local only, not committed
│   └── unit/               Assignment briefs, rubric and unit resources
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
- Every task is a GitHub issue with one named owner and a deadline.
- Minutes are committed within 24 hours, so members who missed a meeting can be briefed from the repository.

Team roster: [`docs/project/team.md`](docs/project/team.md).

## Technology

> ### ⚠ The code and the assessment do not match — unresolved as of 22 August 2026
>
> [`src/`](src/) contains a working **ASP.NET Core Razor Pages** application — Entity Framework
> Core over SQLite, sign-in, an approvals queue, notifications, and a sealed-record snapshot with
> a SHA-256 hash. It is real, and a good deal further along than this section describes.
>
> **None of the five options assessed below is ASP.NET Core.** Option C is a server-rendered
> monolith, which is the same *shape*, but the assessed stack was Django + HTMX + PostgreSQL,
> and what was built is .NET + SQLite. So the repository currently argues for one stack in
> `architecture.md`, records no decision in `docs/decisions/`, and ships another in `src/`.
>
> **This is flagged, not fixed.** Criterion 4 of the Assignment 1 rubric marks whether
> *"different choices of technology have been carefully considered and the decisions clearly
> justified"* — and right now a marker who opens both files finds a contradiction rather than a
> justification. The fix is not to rewrite history but to write the decision down: an ADR in
> [`docs/decisions/`](docs/decisions/) saying why .NET, how it maps to Option C, what the SQLite
> versus PostgreSQL position is, and when it was actually taken. Tracked in
> [`assignment-1-completion-plan.md`](docs/project/assignment-1-completion-plan.md).
>
> Everything below this box is the 5 August position and is left unedited, because it is what
> was presented to the facilitator on that date.

**Assessed, but not committed — and the team has not yet met to choose.** Five options were
put to the facilitator on 5 August 2026: a client-side SPA with no backend (prototype only);
an SPA with a REST API and PostgreSQL; a Django + HTMX monolith; Microsoft Power Platform
(rejected); and building inside existing UWA systems (deferred at the client's request).

The SPA and the monolith are both still live. On the weighted comparison in
[`docs/spec/architecture.md`](docs/spec/architecture.md#8-options-assessed) the monolith leads — 143 to
134 — on criteria that deliberately weight delivery risk; the SPA leads on interaction quality
and on what the team learns. Nine points apart is too close to settle on paper, so nothing has
been chosen.

A **team skills audit** and a one-week spike run before the decision, with a go/no-go at the
mid-semester checkpoint. Whatever is chosen, some things are already settled: the calculation
engine is a pure, versioned, unit-tested module with no database or UI dependency — decimal
arithmetic, a divide-by-zero guard, aggregates that iterate rather than index, and
configuration versioned so that a record created in 2026 still reproduces its figures in 2030.
PostgreSQL is the relational store either way.

## Next deliverable

**Assignment 1 — project specification and plan.** A single PDF, submitted by one member,
due **Tuesday 25 August 2026, 11:59 pm** — extended by one week from 18 August, requested by
us and granted by the unit coordinator by email on 14 August 2026. Four sections: problem statement, client
communication and MVP agreement, project management and plans, risk and technology
assessment. The submission links to this repository and the team's Teams area, and the
facilitator must have access to both.

Brief and rubric: [`reference/unit/`](reference/unit/).

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
