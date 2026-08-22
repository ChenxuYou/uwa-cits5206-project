# Client Meeting — Scope Sign-Off and the Five Open Questions

**Date:** Thursday 20 August 2026
**Format:** in person, on campus
**Client:** Mathew Hall, Strategic Development Coordinator, UWA Research Infrastructure
**Present (team):** Wenmin Luo, Dai Lam La La, Jaswanth Vericherla
**Apologies:** Chenxu You, Yichen Zhao — briefed from this record and from the client's
written answers
**Purpose:** walk the client through the scope statement and the five open questions sent on
17 August, and obtain a written decision on both confirmations.

> **About this record.** Written from [Dai Lam's notes taken in the room](../client/communication-history/2026-08-20-client-meeting/dai-lam-meeting-notes.md)
> and from the client's own written answers, which arrived by email the same day. Where the two
> differ, **the client's written answers govern** and this record says so. Every artefact
> referred to here is filed in
> [`docs/client/communication-history/2026-08-20-client-meeting/`](../client/communication-history/2026-08-20-client-meeting/).

---

## 1. Outcome in one line

**The client signed the scope statement.** Both confirmations — scope, and the ownership
position — are ticked, signed by Mathew Hall as Strategic Development Coordinator and dated
**20/8/2026**. All five open questions were answered, in the meeting and again in writing.

This closes the single largest gap in Assignment 1: criterion 2's Exceptional band requires that
the client *has approved* the MVP, and until this meeting it had not.

## 2. How we got here

| Date | Event |
| --- | --- |
| Mon 17 Aug | Yichen Zhao emailed the client the scope statement and the five questions, proposing Wednesday 4:00–4:30pm |
| Tue 18 Aug | Client replied by email, confirming a time to meet |
| Thu 20 Aug | This meeting. The client walked through all five questions with the team |
| Thu 20 Aug | Signed scope statement and written answers returned by email |

The chase planned for Wednesday 19 August was **not needed** — the client had already replied.
[`mvp-agreement.md`](../client/mvp-agreement.md) Part C, the "if no answer comes" plan, never
became live.

## 3. The client's answers

### 3.1 Question 1 — who may see and approve a record

**Answered:** *"Agreed that administrator is approver of the record."*

In the room the client put it slightly more fully: each record needs a **nominated approver**,
and it is the administrator who grants that nomination. Both statements agree on where the
authority sits.

**What this does not say.** It settles *who* approves; it does not say that approval must be a
routed, in-tool step in the core version. Our scope statement flags in-tool approval as a
stretch item (F16, Should) and explicitly invites the client to move it into the core if it is
essential. The client signed the scope statement with that flag in it and did not ask for the
move. **F16 therefore stays a Should**, and the core still records who created, submitted and
sealed a record without routing it.

**Open, deliberately.** If the client's intent was in fact in-tool approval, we would rather
find that out now than after the engine is built. Action A14.

### 3.2 Question 2 — multi-year cycles

**Answered:** retain all records; the most recent approved record is current and supersedes
prior records; validity period **3–5 years, capability dependent**; annual review is rare and
would itself produce a superseding record.

Exactly our proposed default, with the validity period now a number rather than a guess. **No
mid-cycle amendment is required** — the sealed record stays frozen and is replaced, never
edited. Q3 closes and the data model keeps its single frozen version, with no amendment chain.

### 3.3 Question 3 — the sealed record and where it is filed

**Answered:** PDF is the right format; **no UWA template exists**; the PDF should ideally
include **the workings of the calculator as well as the outputs**, for transparency and
traceability; and records are filed in UWA's records management system, **Content Manager
(TRIM)**.

Two of the three are what we proposed. **The third is new work.** Our scope statement promised
a PDF carrying every input, both sets of rates, the variance and every justification — it did
not promise to show the calculation itself. Showing the workings is a real addition, and it is
tracked as one rather than absorbed. See §5.

TRIM is named for the first time here. It is a filing destination, not an integration: the
custodian downloads the PDF and files it. Nothing in the MVP changes unless the client later
asks the tool to write to TRIM directly, which would be system integration and out of the
agreed scope.

### 3.4 Question 4 — commercial rates, guide or calculator

**Answered:** *"Guide governs. Where a discrepancy occurs, we'd appreciate if these can be
flagged to us for our knowledge and guidance."*

Q9 closes in favour of the guide: `R_commercial = (C / U) × k`, with no income deducted. The
workbook's commercial row, which deducts federal and other income before the uplift, is
confirmed as a defect.

**The second sentence is an obligation on us, not a preference.** The client wants discrepancies
between the guide and the calculator reported to them as we find them. We have such a list
already — it is what §2 of [`requirements.md`](../spec/requirements.md#2-the-problem) is built
from — and it now has somewhere to go. Action A15.

### 3.5 Question 5 — guide or calculator, generally

**Answered:** *"Tool should follow the guide."*

Q10 closes. The tool implements the guide's method and corrects the workbook's defects rather
than reproducing them. Our own proposal — show both figures wherever a new figure is compared
with an existing one, so a difference is visible rather than silent — stands, and the client's
request to have discrepancies flagged is the same instinct from their side.

## 4. Sign-off

Both confirmations ticked on the returned document
([`project-scope-summary-signed.pdf`](../client/communication-history/2026-08-20-client-meeting/project-scope-summary-signed.pdf)):

1. **Scope** — what is in, what is out, what is stretch-only.
2. **Ownership** — the costing/pricing method is UWA's; the team writes and owns the source code
   and each member may show their contribution in a personal portfolio; overarching IP is held
   **jointly** by UWA and the team, and neither party sells it onward without the other.

**Confirmation 2 is what [Q8](../spec/requirements.md#9-open-questions) has been waiting for.**
The repository has been all-rights-reserved with permissions granted in [`NOTICE`](../../NOTICE)
precisely because a licence granted by one joint owner alone may not be effective, and the
ownership position was not confirmed in writing. It now is. Q8 does not close today — the
signature confirms the *position*, not the licence that follows from it — but the blocker that
kept it open has gone. Action A16.

The client also said the team's progress to date reads well and that the scope statement
matched their understanding of what they had asked for. Pleasant to hear; it is the two ticks
that go in the report.

## 5. What the client asked for that we had not scoped

Recorded here so that neither item quietly becomes a commitment.

| # | Raised | Status |
| --- | --- | --- |
| 1 | **The sealed PDF shows the calculator's workings**, not only inputs and outputs | **Accepted in principle.** It serves the project's whole purpose — answering *"why does it cost $50 an hour?"* — and the figures are already in the record. Needs a requirement ID and a story estimate before it is a promise |
| 2 | **Link to the HR system** so staff roles come from UWA records | **Raised, not accepted.** This is integration with a UWA HR system, which the signed scope statement explicitly defers. Not refused — but it would need something traded out, and it is not in the MVP |
| 3 | **Filing into Content Manager (TRIM)** | **Out of scope as stated.** The custodian files the exported PDF; the tool does not write to TRIM |

Item 2 came from the notes in the room, not from the written answers, which is the reason to
treat it as raised rather than agreed.

---

## Decisions

| # | Decision |
| --- | --- |
| D14 | **The client has approved the MVP scope and the ownership position in writing**, signed 20 August 2026. `mvp-agreement.md` moves to *Approved* |
| D15 | The **guide governs** wherever it and the calculator disagree, including the commercial rate. The tool corrects the workbook's defects and does not reproduce them |
| D16 | Sealed records are **superseded, never amended**. Validity period is 3–5 years, capability dependent, entered by the custodian |
| D17 | The sealed record is a **PDF including the workings**, filed by the custodian into Content Manager (TRIM). No UWA template exists to build to |
| D18 | The **administrator is the approver** of a record. In-tool routed approval stays a stretch item (F16, Should) until the client says otherwise |
| D19 | **HR-system integration for staff roles is not in the MVP.** It is recorded as raised and would need a scope trade |

Decision numbering continues from the [team meeting of 15 August](2026-08-15-team-weekly-meeting.md),
which ended at D13.

## Actions

| # | Action | Owner | By |
| --- | --- | --- | --- |
| A14 | Confirm in writing whether in-tool approval routing is required in the core, or whether recording the approver is enough | Yichen Zhao | Sun 24 Aug |
| A15 | Send the client the list of guide-vs-calculator discrepancies, as they asked | Dai Lam La La | Sun 24 Aug |
| A16 | Update [`NOTICE`](../../NOTICE) and [`README`](../../README.md) to cite the signed confirmation; state what still has to happen before Q8 closes | Chenxu You | Sun 23 Aug |
| A17 | Give "the PDF shows the workings" a requirement ID and a story estimate in [`requirements.md`](../spec/requirements.md) and [`user-stories.md`](../spec/user-stories.md) | Wenmin Luo | Sun 23 Aug |
| A18 | Close Q3, Q4, Q5, Q9 and Q10 in [`requirements.md` §9](../spec/requirements.md#9-open-questions) against the client's written answers | Chenxu You | Sat 22 Aug |
| A19 | Quote the sign-off in Assignment 1 §2 and link this folder from the submission | Dai Lam La La | Mon 24 Aug |

## Open questions after this meeting

- **In-tool approval routing** — core or stretch (A14). The signed scope says stretch; the client's answer is about authority, not mechanism.
- **Deployment.** Untouched by this meeting and still the largest unanswered question — see the [15 August minutes](2026-08-15-team-weekly-meeting.md) §7.
- **Q8, the repository licence.** Unblocked by confirmation 2, not yet closed.

---

*Written from notes taken in the room on 20 August 2026 and from the client's written answers
received the same day. No recording was made of this meeting.*
