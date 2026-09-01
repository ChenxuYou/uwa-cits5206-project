# Assignment 1 — Readiness

**Deliverable:** project specification and plan, a single PDF, submitted by one member
**Due:** Tuesday 25 August 2026, 11:59 pm — **submitted 25 August 2026**
**Worth:** 15 points across four criteria
**Brief and rubric:** [`reference/unit/`](../../reference/unit/)
**Version:** 2.2 — 25 August 2026

> ### 🔒 Closed — Assignment 1 was submitted on 25 August 2026
>
> One PDF, `Group13-Project Spec and Plans.pdf`, uploaded by one member and filed in
> [`docs/assignments/assignment-1/`](../assignments/assignment-1/) beside the markdown it was
> assembled from. **Nothing below is live work.** This document is kept because it records what
> the assessment against the rubric actually said at the time, including where we were weak; it
> is not edited to look better after the fact.
>
> The two items it left open are carried, not dropped: the **GitHub Projects board** and the
> **24 July and 5 August minutes** are now tracked in [`plan.md`](plan.md). The work that
> follows is M1 — the engine provably correct by **4 September 2026**.

> ### ✅ Superseded in substance on 24–25 August 2026 — kept as the record of the assessment
>
> This document is a **rubric assessment written on 22 August**, and its value is that it says
> honestly where we were weak. Most of what it lists as missing now exists, and the gaps it
> names have been closed rather than argued away:
>
> | What §4–§5 said was missing | Where it is now |
> | --- | --- |
> | No risk register anywhere in the repository | [`risks.md`](risks.md) — 21 rows with likelihood, impact, mitigation, **trigger** and owner, cybersecurity included, plus four risks already realised and closed |
> | The skills audit has not happened | [`skills-audit.md`](skills-audit.md), run 24 August. Ratings are provisional until each member confirms their own row |
> | No milestone schedule, no named owners against work | [`plan.md`](plan.md) — M0–M7 with dates, S1–S8 with the 110 points assigned, and standing responsibilities per member |
> | Nothing under `.github/` — no CI | [`.github/workflows/ci.yml`](../../.github/workflows/ci.yml) — build, test and dependency scan on every push and pull request |
> | The technology decision is not written down; the code contradicts the assessment | [ADR-001](../decisions/adr-001-technology-stack.md), and [`architecture.md` §8–§9](../spec/architecture.md#8-options-assessed) now carries Option F and records how the gate closed |
> | *"We have no code"* (§7) | Not true when it was written, and less so now: [`src/`](../../src/) holds a working ASP.NET Core application. §7 is left unedited and this row is the correction |
>
> **Two items in this document are deliberately no longer live.** §2 argues for publishing a
> catalogue of formula defects found in the client's workbook. We have decided not to: a
> line-by-line reconciliation of the calculator is **deferred to a later cycle**, once the engine
> exists to compare against, and the problem statement makes the structural case instead. The
> client's written rule — **the guide governs**, and divergences are flagged to them — is what
> stands. §6 item 7 and §5's related lines should be read with that in mind.
>
> **The GitHub Projects board is the one artefact still to be created by hand.** Everything else
> above opens.

**Version:** 2.0 — 22 August 2026

> ### ✅ The client signed on 20 August 2026
>
> The gap this document was written around is closed. Criterion 2's Exceptional band requires
> that the client *has approved* the MVP, and they have: both confirmations ticked, signed by
> Mathew Hall, Strategic Development Coordinator, dated 20/8/2026 — plus written answers to all
> five open questions. Filed in
> [`docs/client/communication-history/2026-08-20-client-meeting/`](../client/communication-history/2026-08-20-client-meeting/)
> and [minuted](../meetings/2026-08-20-client-meeting.md).
>
> **That was the whole point of the extension**, and it worked. §3 below is rewritten; §6's
> client items are struck through as done. What remains is criterion 3 — the artefacts — and
> the day-by-day plan for finishing them is in
> [`assignment-1-completion-plan.md`](assignment-1-completion-plan.md).

> **The deadline moved.** We asked the unit coordinator for an extension and he granted it by
> email on **14 August 2026**: the due date is now **Tuesday 25 August**, one week later than
> the 18 August in the brief. The email is the only record of that, so it belongs in the team's
> Teams area. The brief itself is [annotated](../../reference/unit/assignment-1-instructions.md)
> rather than edited, because it is a transcription of the unit's document and not ours to
> rewrite.
>
> **What the extra week is for, and what it is not for.** It is for the one thing four days
> could not buy: **a written answer from the client**. Criterion 2's Exceptional band requires
> that the client *has approved* the MVP, and a request sent on Friday had no realistic chance
> of coming back before Tuesday. Now it does. The extra week is not a licence to start later —
> every task below still lands on the same weekday it would have, one week on, and anything
> finished early is slack we will want at the end.

> **What this document is.** An honest read of where we stand against the marking rubric: the
> argument we are in a position to make, and the evidence we do not yet have. Every gap carries
> an owner and a date, because criterion 3 marks precisely that.

---

## 1. The submission, in one page

Four sections, mapped to the rubric's four criteria and their weights:

| # | Section | Pts | Our position |
| --- | --- | --- | --- |
| 1 | Problem statement — what we are building and why | 4 | **Strong.** We have something most teams will not: named, reproducible defects in the client's live tool. One correction outstanding (§2) |
| 2 | Client communication and MVP agreement | 4 | **Strong, as of 20 August.** The agreement is in writing, signed, with a four-step paper trail behind it |
| 3 | Project management and plans | 4 | **Now the weakest.** The rubric asks for artefacts — board, plan, CI, risk register — and most still do not exist |
| 4 | Risk and technology assessment | 3 | **Mixed.** Technology assessment is strong; the risk register and the skills audit are still not written down |

Mechanics, from the brief:

- One PDF, uploaded by **one** member only.
- Must contain links to the GitHub repository **and** the MS Teams area, including meeting notes and plans.
- **The facilitator must have access to every linked resource.** Confirm this before submitting, not after.
- Peer review (Feedback Fruits) weights the group mark 50%; client feedback 25%; facilitator 25%. Group Member Evaluation is due in week 12, not now.

---

## 2. Criterion 1 — Problem statement · 4 pts

**Exceptional (3–4) requires:** a clear statement of the problem, why the client wants this
software, and the key MVP deliverables.

### The argument we make

Not "spreadsheets are fragile." That is the generic version and it marks as generic. Ours is:

> The client's own costing workbook contains silent formula defects that produce wrong numbers
> today. We can name the cells. A guided web application with a server-side calculation engine
> and a fixed aggregate contract makes each of those defects structurally impossible to
> reproduce — not less likely, impossible.

That sentence is the whole submission's spine. It converts an abstract complaint the client
made — *"it's functional, but nobody can really use it because it's easy to break"* — into
evidence, and it makes the MVP's design decisions follow from the evidence rather than from
taste.

### What we already have

- The client's problem statement in their own words, with the failure modes elaborated ([`requirements.md` §2](../spec/requirements.md#2-the-problem)).
- Documented defects in the live workbook, with cell references and dollar effects: a **$2,079.08** platform "surplus" that is purely an artefact, and platform revenue understated by **$4,874.74**.
- The MVP defined twice over and reconciled: [`requirements.md` §7](../spec/requirements.md#7-scope) as the scope baseline, [`user-stories.md` §5](../spec/user-stories.md#5-mvp-definition) as eighteen Must stories at 110 points, and a stated demonstration that proves it works.
- The client's success sentence — *"why does it cost $50 an hour?"* — which gives the reader the product's purpose in twenty seconds.

### What is missing

- **The defect evidence needs correcting before it is published.** [`audit-2026-08-14.md`](../internal/audit-2026-08-14.md) §B found that several cell citations in §2 are wrong or incomplete: `I29` is misclassified, `I41`/`I42` carry the same defect and are undocumented, `C24` is not `SUM(D:K)`, "one column too far" describes three different faults as one, and there are at least six defects rather than three. **A wrong cell reference in a document whose entire premise is source fidelity is the single most damaging thing we could submit.** Fix §2 before it goes in the PDF.
- **Defect 4 should be promoted into the submission.** The indirect-cost total omits the Office floor-area row entirely — a cost the user is asked to enter and the spreadsheet then never counts. That is a better story than a mis-typed reference, and it is the cleanest possible illustration of why N14 exists.
- The workbook has **eight** sheets, not three. "Three process sheets over a background data layer" is both accurate and more useful, because that data layer is an input source the MVP has no story for.

---

## 3. Criterion 2 — Client communication and MVP agreement · 4 pts

**Exceptional (3–4) requires:** evidence of good communication with the client and other
stakeholders, **and** that the client *has approved* the MVP and other deliverables to date.

### What we already have

- A recorded 34-minute client walkthrough, written up as [minutes](../meetings/2026-07-29-client-kickoff.md) with decisions D1–D6 and actions A1–A4.
- A working relationship with agreed terms: Wednesdays, a shared Teams chat, batched questions, support tapering (kickoff §10).
- Requirements rewritten **against the client's own documents**, with a stated source-precedence order and every claim marked **[G]**, **[W]** or **[K]**.
- Open questions each carrying a proposed default so a non-answer does not block us ([`requirements.md` §9](../spec/requirements.md#9-open-questions)). **Five went to the client** in [round 1](../client/questions-round-1.md) and **all five came back answered**; **one remains open** — Q8, the repository licence, which is ours and closes at handover.
- Two client-facing documents drafted and reconciled against the spec: the [scope statement and question list](../client/2026-08-15-scope-and-questions.md) the client received, and the [reasoning behind it](../client/mvp-agreement.md) traced promise by promise to a numbered requirement.
- A second client meeting, [minuted](../meetings/2026-08-20-client-meeting.md) with decisions D14–D19 and actions A14–A19.

### The thing that separated 2 from 4 — now closed

**The client has approved the MVP, in writing.** This section previously said the opposite, at
length, and the argument it made was correct: the rubric's Exceptional band names client approval
in as many words, and until 20 August we did not have it.

The trail, and it is the whole answer to criterion 2:

| Date | What happened | Evidence |
| --- | --- | --- |
| Mon 17 Aug | Scope statement and five questions emailed by Yichen Zhao | [`2026-08-17-email-scope-and-questions/`](../client/communication-history/2026-08-17-email-scope-and-questions/) — the email and both attachments |
| Tue 18 Aug | Client replied, confirming a time to meet | Email thread. **The reply beat the deadline we asked for**, so the chase planned for the 19th was never sent |
| Thu 20 Aug | In-person meeting; all five questions walked through | [Minutes](../meetings/2026-08-20-client-meeting.md) · [notes taken in the room](../client/communication-history/2026-08-20-client-meeting/dai-lam-meeting-notes.md) |
| Thu 20 Aug | **Signed scope statement returned**, both confirmations ticked, plus written answers to all five questions | [Signed PDF](../client/communication-history/2026-08-20-client-meeting/project-scope-summary-signed.pdf) · [answers](../client/communication-history/2026-08-20-client-meeting/client-answers-to-the-five-questions.md) |

**What to put in the PDF, and in what order.** The signature is the headline, but on its own it
proves only that someone signed something. Three things make it evidence of *good* communication
rather than of one lucky email:

1. **The questions were answered, not defaulted.** Every one of the five carried a default we
   would have used in the absence of a reply. None was needed. That is the difference between a
   team that asked and a team that was answered.
2. **One answer changed what we promised, and we said so.** The client asked that the sealed PDF
   carry the calculator's **workings**, not just its outputs. Our scope statement had not promised
   that. It is recorded as new work with an owner ([minutes §5](../meetings/2026-08-20-client-meeting.md)),
   not folded in silently — and a team that can point at the thing the client added to its own
   scope is demonstrating exactly what this criterion measures.
3. **One request was declined, in writing, with a reason.** The client raised linking staff roles
   to a UWA HR system. That is the class of integration the signed scope defers. It is recorded
   as *raised, not accepted*, and it needs a trade. Saying no to a client and writing down why is
   stronger evidence of a working relationship than saying yes to everything.

**Do not overstate it either.** The client answered *who* approves a record — the administrator —
which is not the same as saying that approval must be a routed step inside the tool. The signed
scope keeps in-tool approval as a stretch item and the client signed it that way. We are going
back to confirm (action A14) rather than reading the answer to suit us.

### The evidence gap that will be noticed

`README.md` states that **minutes are committed within 24 hours**. `docs/meetings/` now contains
**three** records — 29 July, 15 August and 20 August — which is better than the one it held a
week ago but still not what the sentence claims. The gap is **24 July** and **5 August**, and the
cadence between 29 July and 15 August implies a stand-up or two with nothing written down.

This is the cheapest high-value work available to us. Writing up 24 July and 5 August costs an
hour each and repairs criterion 2 *and* criterion 3 at once. The 5 August record is doubly
load-bearing: the checkpoint deck's own header carries a `TODO` that depends on it, and
[`STYLE-GUIDE.md`](../../presentations/STYLE-GUIDE.md) §9 has a checklist item that cannot be
ticked without it.

---

## 4. Criterion 3 — Project management and plans · 4 pts

**Exceptional (3–4) requires:** a realistic plan with enough detail for *each member* to work
on the next deliverable; responsibilities and deadlines documented; project tools set up, with
evidence of effective planning for group workflow **and software deployment**.

This is our weakest criterion, and it is weak in a way that is fixable in a weekend, because
what is missing is artefacts rather than thinking.

### What we already have

- A [team roster](team.md) with a single home, and a documented member departure with the work redistributed.
- A [build order](../spec/architecture.md#10-delivery-approach) — engine first, then identity, then forms, then rates, then seal and export — with a stated rationale for the order.
- A [definition of done](../spec/architecture.md#10-delivery-approach) per story.
- A [decision gate](../spec/architecture.md#9-decision-gate) with a named fallback trigger at end of week 8.
- Eighteen estimated stories, 110 points, ready to be sequenced.

### What does not exist

| Missing | Why the rubric cares |
| --- | --- |
| **A milestone schedule with dates** | "Sufficient detail for each team member to work on the next deliverable" |
| **Named owners against work** | "Responsibilities and deadlines are documented" — we have owners for *presentation sections* and for nothing else |
| **A visible GitHub Projects board** | "Project tools are set up and show evidence of effective planning" |
| **Anything under `.github/`** | No CI, no issue templates, no PR template — `architecture.md` §10 promises "CI from the first commit of code" |
| **A deployment plan** | The criterion names software deployment explicitly; AQ2 is still open |
| **Minutes since 29 July** | The tool we claim to use for briefing absent members |

`README.md` currently states *"Every task is a GitHub issue with one named owner and a
deadline."* Nothing in the repository shows this. If the board exists, **link it**; if it does
not, the sentence is a plan and must read as one. A facilitator who clicks and finds nothing
has learned something about the whole submission.

### Minimum credible fix, in priority order

1. `docs/project/plan.md` — a milestone table from now to end of semester, with dates, and each of the 110 points assigned to a named member for the next two sprints.
2. GitHub Projects board, populated from the eighteen Must stories, one issue each, owner and due date on every one. Link it from `README.md`.
3. `.github/workflows/ci.yml` — even a lint-and-test stub against an empty `src/`. It makes "CI from the first commit of code" a fact rather than an intention, and it is fifteen minutes' work.
4. Minutes for 24 July and 5 August.

---

## 5. Criterion 4 — Risk and technology assessment · 3 pts

**Exceptional (3) requires:** a realistic assessment of skills, resources and risks; **skills
gaps identified and addressed**; technology choices considered and justified; **relevant risks,
including cybersecurity, identified and planned for.**

### Technology — strong, with one thing to say better

[`architecture.md` §8](../spec/architecture.md#8-options-assessed) assesses five options against six
weighted drivers and reports the scores plainly. It is the best-evidenced part of the project,
and the v1.1 change-log entry — recommendation withdrawn because the document's own table
contradicted its text — is itself creditable.

One correction to make before submitting. §8 already scores **"Fit to team's current skills"**
at weight 4, while §9 says the decision waits on a skills audit that has not been run. Decompose
C's nine-point lead over B:

| Criterion | Weight | C − B | Weighted |
| --- | --- | --- | --- |
| Probability of shipping complete | 5 | +2 | **+10** |
| Fit to team's current skills | 4 | +1 | **+4** |
| Interaction quality | 3 | −1 | −3 |
| Learning value | 2 | −1 | −2 |
| | | | **+9** |

The whole lead rests on two judgement calls, one of them the very thing the audit exists to
measure. A one-point reversal on skills fit gives **B 142, C 139** and flips the ranking. §8's
conclusion — nine points is inside the noise — is right; publishing *this table* is what turns
it from an assertion into an argument, and it pre-empts the obvious question about why we
scored a skills criterion before running the skills audit.

### Skills — the audit has not happened

§9 makes the skills audit the first piece of evidence in the decision gate: every member
self-rates on JavaScript/React, Python, SQL, HTML/CSS, Git and testing. The rubric asks for
gaps **identified and addressed**.

**Run it this weekend.** It is a five-row table and a ten-minute conversation, it produces a
concrete artefact for the PDF, and it unblocks the technology decision we are otherwise
deferring past the submission.

### Risk — there is no risk register anywhere in the repository

This is the clearest single gap in the whole submission. The criterion names cybersecurity
explicitly. What we have is scattered: `architecture.md` §6 is a competent control table,
`requirements.md` §10 lists constraints, and the fallback trigger in §9 is risk management
without the label. None of it is a register, and a marker looking for one will not find one.

`docs/project/risks.md`, one table, likelihood × impact × mitigation × owner × trigger. The material
already exists — it needs collecting, not inventing:

| Risk | Where it already lives |
| --- | --- |
| Client answer latency blocks work (two-week wait on a blocking question) | `requirements.md` §10 |
| MVP scope not signed off before build starts | This document, §3 |
| Technology choice made late, or made wrong | `architecture.md` §9, with the week-8 fallback trigger as the mitigation |
| Team JavaScript depth unproven | `architecture.md` §8, unmeasured until the skills audit |
| Team of five with one member already withdrawn; bus factor on the engine | `team.md` |
| **Cybersecurity:** credential leakage into the repository | `.gitignore` §4 — mitigation exists, risk unstated |
| **Cybersecurity:** UWA-internal, FOI-subject data; injection, XSS, CSRF, mass assignment | `architecture.md` §6 — controls exist, risk unstated |
| **Cybersecurity:** an incorrect or undetectably altered rate — the threat that actually matters here | `architecture.md` §6, already identified as the real threat |
| IP: an MIT grant published in an early commit | Resolved; worth stating *as a risk we found and closed*, because that is exactly the evidence the criterion wants |
| IP: a sign-off document that asked the client to confirm the **wrong** ownership position | Resolved before sending — [`NOTICE`](../../NOTICE) §2, [`mvp-agreement.md`](../client/mvp-agreement.md) change log 1.1 |
| Client material committed without the client's agreement | `reference/client/README.md`, `.gitignore` §1 — mitigation exists, risk unstated |

**Those two IP rows are worth dwelling on.** A risk identified, assessed and closed by rewriting
history is stronger evidence of risk management than any hypothetical, and we have the commit
trail to prove it. The second is better still, because it was caught *in review of our own
outbound document*: the ownership confirmation asked the client to agree that the overarching IP
was UWA's alone, when our own `NOTICE` holds it jointly — and `NOTICE` §1 makes the client's written
confirmation the very thing that closes the licensing question. One sentence in a document we
were about to send would have signed away the position it was written to protect. It never left
the building.

---

## 6. What to do, in order, before Tuesday

Ordered by marks-per-hour. Items 1–4 are the difference between a competent submission and an
exceptional one.

**Two items keep their original dates, or as near as they can.** The client request (1) has
already slipped a day, from Friday 14 to Saturday 15 August, and does not slip again — the whole
value of the extension is the time it buys for a reply. The skills audit (3) does not move,
because today's meeting decides to run it *in the room* — deferring it to 22 August would leave
the technology decision open for another week for no reason. Everything else shifts one week,
landing on the same weekday.

> **Items 1–4 are done.** They are left in the table rather than deleted, because this table goes
> in the submission and a plan whose completed rows have been quietly removed is not evidence of
> anything. The live plan for the remaining days is
> [`assignment-1-completion-plan.md`](assignment-1-completion-plan.md).

| # | Task | Fixes | Owner | By |
| --- | --- | --- | --- | --- |
| ~~1~~ | ~~Send the client [`2026-08-15-scope-and-questions.md`](../client/2026-08-15-scope-and-questions.md) — scope statement, two confirmations, five questions~~ | Crit 2 | Yichen Zhao | ✅ **Sent Mon 17 Aug** — two days later than this row planned |
| ~~2~~ | ~~Assign every owner in this table~~ | All | Whole team | ✅ Sat 15 Aug — [minutes §8](../meetings/2026-08-15-team-weekly-meeting.md) |
| 3 | Skills audit — five rows, six skills, gaps and how each is addressed | Crit 4 | Chenxu You | ⚠️ **Not done. Was due Sat 15 Aug and did not happen** — carried into the completion plan |
| — | *(Date we asked the client to reply by)* | Crit 2 | — | *Tue 18 Aug — **client replied on time*** |
| ~~4~~ | ~~Chase the client if there is no reply~~ | Crit 2 | Yichen Zhao | ✅ **Not needed.** The reply arrived first; met in person **Thu 20 Aug** and signed |
| 5 | `docs/project/risks.md` — risk register, cybersecurity rows included | Crit 4 | | Sat 22 Aug |
| 6 | `docs/project/plan.md` — milestones with dates; 110 points assigned to named members | Crit 3 | | Sat 22 Aug |
| 7 | Correct `requirements.md` §2 cell evidence per [audit §B](../internal/audit-2026-08-14.md); promote defect 4 | Crit 1 | | Sun 23 Aug |
| 8 | Minutes for 24 July and 5 August | Crit 2, 3 | | Sun 23 Aug |
| 9 | GitHub Projects board from the eighteen Must stories; link from `README.md` | Crit 3 | | Sun 23 Aug |
| 10 | `.github/workflows/ci.yml` stub | Crit 3 | | Sun 23 Aug |
| 11 | Add the sensitivity decomposition to `architecture.md` §8 | Crit 4 | | Sun 23 Aug |
| 12 | Annotate or update the [5 August deck](../../presentations/2026-08-05-facilitator-checkpoint.html) — it still teaches the superseded two-income-line model and cites a transcript we do not commit | Crit 1, 2 | | Mon 24 Aug |
| 13 | Narrow the kickoff annotation to what it can support ([audit §A2](../internal/audit-2026-08-14.md)) | Crit 1 | | Mon 24 Aug |
| 14 | Write the PDF | All | | Mon 24 Aug |
| 15 | **Confirm the facilitator has access to the repository and the Teams area.** Then submit | Mechanics | | Tue 25 Aug |

Owners are deliberately blank. Fill them in at today's meeting — an unassigned task list is
the exact failure criterion 3 marks down, and this table is going in the submission.

**One trap worth naming.** An extension usually gets spent, not saved. Items 5–11 were sized for
a weekend and they still are; putting them on 22–23 August rather than 15–16 August is a
deliberate choice to leave the intervening week clear for the client's answer and for the
correction work it might trigger — not permission to start on the 22nd. Anything finished early
is slack we will want on the 24th.

---

## 7. The honest summary

**What we are genuinely good at, and should lead with:** we treat our own documents as things
that can be wrong. Requirements v2.0 withdrew a set of figures that had been transcribed from a
recording and were heading for a test fixture, because a source-precedence rule caught them. The
architecture document withdrew its own recommendation when its table contradicted its text. Two
internal audits are on file. An MIT licence published by accident was found and rewritten out.
The client sign-off document was reviewed before sending and four faults came out of it, one of
them a sentence that would have had the client confirm the wrong owner of the project's IP.
None of that is decoration on a capstone about *defensible numbers* — it is the same discipline
the product exists to enforce, applied to ourselves, and it is the most persuasive thing we can
put in front of a facilitator.

**What we should not overstate:** we have no code, no risk register, no board, no CI, no skills
audit and three committed meeting records where the README claims a weekly cadence. The thinking
is still well ahead of the artefacts. **Criterion 3 marks artefacts**, and it is now the weakest
of the four rather than the second weakest.

**The extension bought exactly what it was asked to buy.** Criterion 2's top band turns on a
single sentence — *the client has approved the MVP* — and on 15 August that was out of reach.
The document went out on the 17th, the client replied on the 18th, we met on the 20th, and it
came back signed. That is the one thing four days could not have delivered, and it is delivered.

**The warning in this section a week ago was that an extension usually gets spent, not saved.**
Half of that came true. The client work landed; the weekend items — the risk register, the plan,
the skills audit — did not move, and three days remain. Everything left is hours of work and
most of it is collecting material that already exists in this repository rather than inventing
it. The day-by-day allocation is in
[`assignment-1-completion-plan.md`](assignment-1-completion-plan.md), and unlike the table above
it has a name against every line.

---

## Change log

| Version | Date | Change |
| --- | --- | --- |
| 2.1 | 1 Sep 2026 | **The board this document called "the one artefact still to be created by hand" now exists** — created 1 September, public, linked to the repository, 25 story issues on it. The body is a rubric assessment of the repository as it stood on 22 August and is not edited; this row is the only correction, so that a reader who reaches §43 or §380 and finds *"we have no board"* can see when that stopped being true. Criterion 3's evidence is now in place. |
| 2.0 | 22 Aug 2026 | **Criterion 2 is closed. The client signed on 20 August 2026** — both confirmations, plus written answers to all five open questions ([minutes](../meetings/2026-08-20-client-meeting.md), [evidence folder](../client/communication-history/2026-08-20-client-meeting/)). §3 is rewritten from *"the client has not approved the MVP"* to the trail that shows they have, and now says what to put in the report and in what order: the questions were **answered, not defaulted**; one answer **added** to our scope and we recorded it as new work; one request was **declined in writing with a reason**. §1's position column is re-read against all four criteria — **criterion 3 is now the weakest**, not the second weakest. §6's items 1, 2 and 4 are struck through as done, with what actually happened against each rather than what was planned; **item 3, the skills audit, is marked as missed** — it was due 15 August, did not happen, and saying so is the only honest version. §7 is rewritten. The remaining work moves to a new document, [`assignment-1-completion-plan.md`](assignment-1-completion-plan.md), because a readiness assessment and a delivery plan are two different things and this file had been doing both. |
| 1.0 | 14 Aug 2026 | First version, written against the rubric in [`reference/unit/assignment-1-rubric.md`](../../reference/unit/assignment-1-rubric.md) and the repository as it stands on 14 August. |
| 1.1 | 14 Aug 2026 | **Deadline extended to Tuesday 25 August 2026**, requested by us and granted by the unit coordinator by email on 14 August. §6 rescheduled: the client request (item 1) and the skills audit (item 3) keep their original dates, a client chase on 19 August is added, and everything else shifts one week onto the same weekday. §3 rewritten — written client sign-off moves from unrealistic to achievable, which is where the extension earns its keep. The unit brief is annotated rather than edited. |
| 1.3 | 15 Aug 2026 | **Aligned with the reduced question set.** The batch put to the client is **five questions**, not six, and our own open list is **six**, not seven, after the question about our working method was withdrawn — it settled nothing in the product. Q-numbers follow [`requirements.md` §9](../spec/requirements.md#9-open-questions) v2.2, in which old Q7–Q11 became Q6–Q10, so the repository licence question is now **Q8**. The risk table's client-material row now states the risk it always meant: client material committed without the client's agreement, mitigated by [`.gitignore`](../../.gitignore) §1 and [`reference/client/README.md`](../../reference/client/README.md). |
| 1.2 | 15 Aug 2026 | **Re-dated to 15 August and aligned with the client documents.** Item 1 did not go out on the 14th: the slip is recorded in §3 and §6 rather than absorbed, because this table is going in the submission. The client timeline is now three dates, not two — sent **15 Aug**, reply asked for **Tue 18 Aug**, chase **Wed 19 Aug** — matching [`2026-08-15-scope-and-questions.md`](../client/2026-08-15-scope-and-questions.md) and [`mvp-agreement.md`](../client/mvp-agreement.md) §C; the chase previously fell on the deadline itself. §3 now names the outbound document and says six of the seven open questions travel to the client. Requirements reference updated to v2.1. §5 gains a second IP risk — the sign-off document's ownership sentence, found in review and corrected before sending — and §7 leads with it, because a fault caught in our own outbound document is exactly the evidence criterion 4 asks for. No change to any deadline, owner or deliverable. |
