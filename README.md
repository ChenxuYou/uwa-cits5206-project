# Research Infrastructure Costing & Pricing Tool

**CITS5206 Professional Computing — Capstone Project, The University of Western Australia**
**Client:** UWA Research Infrastructure
**Status:** Requirements agreed with the client; technology not yet committed. No application code yet.

---

## The problem

UWA runs research infrastructure — electron microscopes, a human MRI, radio telescopes,
phenotyping drones — that is expensive to buy and expensive to operate. Some of that cost is
passed on to the researchers who buy time on the instruments, and because UWA is a publicly
funded institution, the way those prices are set has to be **transparent, consistent across
every platform, and defensible years after the fact**. The aim is break-even, not profit.

The client already has the logic. It lives in an Excel workbook that is, in their words,
hard for anyone to actually use because it is easy to break. What they want is a guided web
application that asks a platform leader for the inputs, keeps the calculation out of reach
behind the form, and produces a record that can be filed and retrieved.

The sentence the client used to describe what success looks like:

> If someone comes to us and says "why does it cost $50 an hour for me to use?", we want to
> be able to say: well, it costs $100,000 a year to run, this is how many hours a year it's
> going to be used, we divide one by the other — $50 an hour. That's the reason we charge
> that price.
>
> — UWA Research Infrastructure, client walkthrough, 29 July 2026

## Who uses it

**Platform custodians** — the academic or professional staff who run a facility, not the
researchers who buy time on it. They are technical people for whom this is administrative
work, and they run the exercise roughly **once every three to five years** to set rates for
the period ahead. Low frequency, high stakes: the tool has to be self-explanatory, because
nobody will remember it from last time.

## The calculation

Total operating cost, less any non-variable subsidy, divided by forecast utilisation. Three
rates come out, one per user type:

```
R_internal   = (C − I_uwa − I_gov) / U
R_national   = ((C − I_gov) / U) × k
R_commercial = (C / U) × k
```

| Symbol | Meaning |
| --- | --- |
| `C` | Total annual operating cost |
| `I_uwa`, `I_gov` | Non-variable income from the university and from government |
| `U` | **Forecast** annual utilisation — not capacity |
| `k` | `1.35`, UWA's standard indirect cost recovery applied to any external party |

Two things are easy to get wrong. The divisor is *forecast* use, not capacity — an
instrument with 1,000 hours of capacity may see 500 hours of real use, and weekends,
maintenance windows, staff FTE and even weather all cut into it. And costs are captured both
per instrument and per platform; the facility the client demonstrated held seven instruments.

The engine will be validated against the client's own spreadsheet as a golden-file test
before any UI is written.

## What the MVP delivers

A guided web form in **three sequential sections**, mirroring the three parts of the
spreadsheet:

1. **Costs** — staffing, consumables, maintenance contracts, utilities, other operating costs, plus the non-variable income that offsets them.
2. **Capacity and utilisation** — full capacity, forecast use, and the reasoning behind the forecast.
3. **Rates** — the three calculated charge-out rates, with room to adjust inputs and see the effect before committing.

Running through it:

- Calculation logic stays server-side, out of the user's reach.
- Mandatory fields and type validation, so `$20,000` cannot be entered as `$200,000` unnoticed.
- Free-text justification boxes throughout — the rate has to be defensible, not merely correct.
- On submit, the inputs and results are sealed into a record that can be exported and filed, and read back in three years' time.

Deployment is a standalone web application. Integration into existing UWA systems was
discussed and deferred; a working website comes first.

## Open questions

Cost-allocation method, how utilisation is split across the three user types, multi-year
handling, access control, and the format of the sealed record are all unresolved. A
consolidated question list is being prepared for the client; the full list is in the
[kickoff minutes](docs/meetings/2026-07-29-client-kickoff.md#open-questions).

Two questions sit outside that list and are ours to close:

- **Whether AI tools may be used on client material.** Asked on 29 July, not yet answered. Until it is, client material stays out of public AI tools.
- **What licence this repository carries.** See [Ownership](#ownership) — currently none.

## Repository layout

```
├── docs/
│   ├── decisions/          Architecture and process decision records
│   ├── meetings/           Minutes and notes, one file per meeting
│   └── team.md             The roster — the only place it lives
├── presentations/          Self-contained HTML decks, one file per deck
│   ├── STYLE-GUIDE.md      Binding style policy — read before building a deck
│   ├── template.html       Empty skeleton; copy it, never present from it
│   └── assets/
├── reference/
│   ├── client/             Client material — local only, not committed
│   └── unit/               Assignment briefs, rubric and unit resources
├── src/                    Application code (empty pending the technology decision)
├── .gitignore
└── README.md
```

Everything is **markdown or HTML**, deliberately. Plain-text artefacts diff in GitHub and
review like code; Word documents and `.pptx` files do not. Presentations are built as
single self-contained HTML files for the same reason — see
[`presentations/STYLE-GUIDE.md`](presentations/STYLE-GUIDE.md) §8.

## Confidential material

Some files referenced in this repository are **deliberately absent** from it:

| Not committed | Why |
| --- | --- |
| Client spreadsheets and documents (`reference/client/`) | The client's material, sensitive while in progress, and not ours to publish |
| Meeting audio and video (`.m4a`, `.mp4`, …) | Identifiable voices |
| Subtitle-format transcripts (`.srt`, `.vtt`, …) | Verbatim, unreviewed speech; written minutes go in `docs/meetings/` instead |
| Credentials, `.env` files, keys, local databases | The obvious reasons |

The one exception is `docs/meetings/2026-07-24-team-meeting-transcript.md` — an internal team
meeting with no client present, committed as a markdown record of what was discussed.

The rules and the reasoning are in [`.gitignore`](.gitignore). Check any single path with
`git check-ignore -v <path>`. Committing something excluded needs team agreement and
`git add -f`.

## How the team works

- **Weekly** online stand-up — progress, blockers, next week's allocation.
- **Fortnightly** in person on campus, including the facilitator checkpoint.
- **Client on Wednesdays as needed**, plus a shared Teams chat for asynchronous questions. Cadence agreed with the client on 29 July 2026: heavier support up front, easing off later.
- Every task is a GitHub issue with one named owner and a deadline.
- Minutes are committed within 24 hours, so members who missed a meeting can be briefed from the repository.

Team roster: [`docs/team.md`](docs/team.md).

## Technology

**Not yet decided.** Five options were assessed at the facilitator checkpoint on 5 August 2026:
a client-side SPA with no backend (prototype only), an SPA with an API and PostgreSQL
(recommended), a Django + HTMX monolith (fallback, highest probability of shipping complete),
and Power Platform / UWA system integration (rejected and deferred respectively).

A **team skills audit** runs before the decision, with a go/no-go at the mid-semester
checkpoint. Whatever is chosen, the calculation engine is a pure, versioned, unit-tested
module with no database or UI dependency — decimal arithmetic, a divide-by-zero guard, and
configuration versioned so that a record created in 2026 still reproduces its figures in 2030.

## Next deliverable

**Assignment 1 — project specification and plan.** A single PDF, submitted by one member,
due **Tuesday 18 August 2026, 11:59 pm**. Four sections: problem statement, client
communication and MVP agreement, project management and plans, risk and technology
assessment. The submission links to this repository and the team's Teams area, and the
facilitator must have access to both.

Brief and rubric: [`reference/unit/`](reference/unit/).

## Ownership

The client owns the costing logic. The team owns the code and may use the project in
portfolios; the client raised no objection to us sharing what we build. Selling the tool
onward would not be appropriate, as the overarching IP is joint. Agreed with the client on
29 July 2026 — see the [kickoff minutes](docs/meetings/2026-07-29-client-kickoff.md) §9.

**This repository carries no licence.** A permissive licence such as MIT would grant everyone
the right to sell the software, which contradicts the joint-IP position above, so the licence
file has been removed until the position is settled in writing with the client. In the
meantime the default applies: no rights are granted, and anyone wanting to reuse this should
ask. This needs to be resolved with the client before handover.
