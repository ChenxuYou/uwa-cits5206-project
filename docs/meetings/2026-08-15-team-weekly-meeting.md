# Team Weekly Meeting — Assignment 1, the MVP and the Work Split

**Date:** Saturday 15 August 2026, evening
**Duration:** approximately 36 minutes
**Format:** Microsoft Teams
**Present:** Chenxu You (chair), Yichen Zhao, Wenmin Luo, Dai Lam La La
**Apologies:** Jaswanth Vericherla — briefed afterwards in the Teams group chat
**Purpose:** restart the weekly cadence, agree how Assignment 1 gets finished, and split the
work five ways.

> **About this record.** These minutes are written from the meeting recording. The recording and
> its raw transcript are **not committed** — see [`.gitignore`](../../.gitignore) §3 — so this
> file is the project's record of the meeting. Members are named because the transcript carries
> speaker labels.

---

## 1. Repository walkthrough

Chenxu walked the team through the repository so that everyone can find things without asking.

- `docs/` holds the specification, the client correspondence, the meeting records and the project
  administration. `docs/decisions/` is deliberately empty — no architecture decision has been
  taken yet.
- `docs/spec/architecture.md` records the candidate solutions for the client's requirements;
  [`docs/project/team.md`](../project/team.md) is the only home of the roster.
- `presentations/` holds every deck. Decks are **HTML, not PowerPoint**, so that each change
  diffs in GitHub like any other artefact.
- `reference/` holds the client's own material and the unit's material.

The point of putting everything in one tree is **context**: whoever picks up a task can reach
every document it depends on without chasing anyone. Chenxu noted for the record that this is
about convenience for the team, and is **not** a licence to feed client material to AI tools.

### The repository licence

Chenxu raised the licence as unfinished business. An MIT licence lets anyone modify the software
and **sell** it, which sits badly against the client's position that the costing logic is UWA's
and the overarching IP is joint ([`NOTICE`](../../NOTICE) §2). A `NOTICE` file has been added as
an interim answer.

Dai Lam pointed out the countervailing interest: members want to show this work to recruiters
later, which needs the repository to be public. **[ASK]** The team will put the licence question
to the lab facilitator rather than settle it alone.

## 2. Assignment 1 — status against the four criteria

The submission deadline has moved by **one week**, confirmed by the unit coordinator: **ten days
from this meeting**. Yichen noted the detailed requirements are in the LMS rubric, under the
submission area.

| Criterion | Where we stand |
|---|---|
| Problem statement | Strongest. A solid write-up of the client's requirements exists |
| Client communication & MVP | **Most urgent.** Nothing sent, no client agreement, no evidence trail |
| Project management | **Weakest.** Task-level accountability not yet documented |
| Risk & technology | Not started. Risks not yet registered |

## 3. The MVP and the client's sign-off

The requirement is not just that an MVP exists. The client has to **agree to it in writing**, and
that exchange has to be visible on the LMS as evidence of client interaction. A first draft that
the client disagrees with is a workable outcome — the interaction is the artefact — but silence is
not.

Agreed sequence:

1. Draft complete by **Sunday night 16 August**.
2. Sent to the client **Monday 17 August**, so they have time to read it before any meeting.
3. Meeting booked by **Tuesday 18 August**, for **Wednesday or Thursday** — the client's stated
   preference ([kickoff §10](2026-07-29-client-kickoff.md)) — with a backup plan if they are busy.

On what the MVP should be, Dai Lam argued for a working prototype built from what the team
already understands of the spreadsheet, so there is something concrete to show and agree on. The
exact logic can be corrected in a later sprint; the priority is agreement on shape. Chenxu
described the same idea as the standard cycle — ship something plain that works, ask for
correction, revise — rather than one long build.

Yichen's caution: the client asked for a calculator in very concrete terms, so the MVP still has
to name the variables it takes in, which is why the conflicts in §4 matter now rather than later.

## 4. The conflicts in the client's material

The client's guide and their calculator do not agree with each other on how the budget is worked
out, and parts of the workbook appear internally contradictory. Chenxu was explicit that this is
**not yet fully understood**, and that it needs to be articulated clearly and put to the client at
the next meeting.

Two points shaped the decision:

- **Not everyone needs to understand it.** One or two people own the issue, understand it
  properly, re-express it in their own words and take it to the client. The rest of the team works
  in parallel.
- **The client should be asked to be specific.** Yichen's suggestion: ask them to point at the
  row, the column and the formula, so the answer is checkable rather than a matter of opinion.

## 5. Weekly cadence and minutes

The unit requires a weekly online meeting written up as minutes. The team has missed one or two
weeks and is restarting.

**Agreed: Saturday evening, weekly, minuted every time.** A short 20-minute catch-up was added for
**Sunday 16 August, 7pm**, to confirm everyone is on the same page before the draft goes out.

## 6. Outstanding question to the client on AI use

The team's Q6 — whether AI tools may be used on client material — has had no reply. Agreed to
**leave it rather than chase it**: the tool is not commercial, and the exposure is limited if AI
is used to help write code rather than to process client data. It does not block anything.

## 7. Deployment — the biggest unanswered question

The team's thinking is ahead of the client's on scope, which is comfortable. The one substantial
thing nobody has an answer to is **how the software gets deployed**. The client wants a system
that replaces their Excel and web form, but not where it would run.

Possibilities raised: the client lends server capacity; the team uses the **UWA domain** the
client already shared. Dai Lam recalled a requirement that staff log in with their own accounts,
which would point the same way. **Not urgent for Assignment 1**, but it goes to the client.

## 8. Work division

Dai Lam proposed a structure by role rather than by document, and the team filled it in:

| Area | Owner |
|---|---|
| Client communication and meeting scheduling | Yichen Zhao |
| Application build | Wenmin Luo, Chenxu You |
| Costing logic and the conflicts in the client's material | Dai Lam La La |
| Risk & technology, and the problem statement | Dai Lam La La |
| Project management section | Whole team |

Yichen volunteered for communication and asked to stay involved in the MVP work as well. Dai Lam
will contact Jaswanth to agree his part; if he is not available, Dai Lam carries it.

Assignment 1 also requires a **per-member accountability record** — each person's tasks written
down, not a general statement that the team collaborated. Yichen reminded everyone the individual
accountability form is due **Sunday**, including the backlog from previous weeks.

---

## Decisions

| # | Decision |
|---|---|
| D7 | Weekly team meeting on **Saturday evening**, minuted every week, restarting from this meeting |
| D8 | MVP statement drafted by Sunday 16 August, sent Monday 17 August, client meeting booked by Tuesday 18 August for Wednesday or Thursday |
| D9 | The MVP is deliberately simple and need not reproduce the client's spreadsheet logic exactly; fidelity is a later sprint |
| D10 | The conflicts in the client's material are owned by one member, not understood by all five in parallel |
| D11 | Q6 (AI use on client material) is not chased; it blocks nothing |
| D12 | Work split as recorded in §8; each member's tasks documented for the Assignment 1 accountability record |
| D13 | The repository licence is put to the lab facilitator before it is settled |

Decision numbering continues from the [client kickoff](2026-07-29-client-kickoff.md), which ended
at D6.

## Actions

| # | Action | Owner | By |
|---|---|---|---|
| A5 | Finish the MVP statement and question list | Chenxu You | Sun 16 Aug |
| A6 | Send the statement to the client and book the meeting | Yichen Zhao | Mon–Tue 17–18 Aug |
| A7 | Build a demo from the MVP draft to show the client | Wenmin Luo | Wed 19 Aug |
| A8 | Write up the conflicts in the client's material and put them to the client | Dai Lam La La | Next client meeting |
| A9 | Draft the problem statement and the risk assessment | Dai Lam La La | — |
| A10 | Post the detailed work division in the group chat so Jaswanth can pick up his part | Yichen Zhao | Sun 16 Aug |
| A11 | Submit the individual accountability form, including the outstanding weeks | All | Sun 16 Aug |
| A12 | Raise the repository licence with the lab facilitator | Chenxu You | Wed 19 Aug |
| A13 | Short catch-up to confirm everyone is aligned before the draft goes out | All | Sun 16 Aug, 7pm |

## Open questions

- **Deployment.** Where does the tool run — client servers, the UWA domain, or something else? Does staff sign-in have to use UWA accounts?
- **The conflicts.** Which governs where the guide and the calculator disagree? To be put to the client with the specific row, column and formula.
- **Licence.** What licence does a public capstone repository carry when the overarching IP is joint? For the lab facilitator.
- **Jaswanth's availability**, and which part of the work he takes.

---

*Written from the meeting recording of 15 August 2026. The recording and its transcript are held
in the team's Teams area and are deliberately excluded from this repository.*
