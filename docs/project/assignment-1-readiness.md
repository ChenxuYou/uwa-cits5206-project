# Assignment 1 — Readiness

**Deliverable:** project specification and plan, a single PDF, submitted by one member
**Due:** Tuesday 25 August 2026, 11:59 pm — **eleven days from today (Friday 14 August)**
**Worth:** 15 points across four criteria
**Brief and rubric:** [`reference/unit/`](../reference/unit/)

> **The deadline moved.** We asked the unit coordinator for an extension and he granted it by
> email on **14 August 2026**: the due date is now **Tuesday 25 August**, one week later than
> the 18 August in the brief. The email is the only record of that, so it belongs in the team's
> Teams area. The brief itself is [annotated](../reference/unit/assignment1-instructions.md)
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
| 1 | Problem statement — what we are building and why | 4 | **Strong.** We have something most teams will not: named, reproducible defects in the client's live tool |
| 2 | Client communication and MVP agreement | 4 | **At risk.** The evidence of communication is good; the *agreement* is not in writing |
| 3 | Project management and plans | 4 | **Weakest.** The rubric asks for artefacts we have not built yet |
| 4 | Risk and technology assessment | 3 | **Mixed.** Technology assessment is strong; risk and skills are not written down anywhere |

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

- The client's problem statement in their own words, with the failure modes elaborated ([`requirements.md` §2](requirements.md#2-the-problem)).
- Documented defects in the live workbook, with cell references and dollar effects: a **$2,079.08** platform "surplus" that is purely an artefact, and platform revenue understated by **$4,874.74**.
- The MVP defined twice over and reconciled: [`requirements.md` §7](requirements.md#7-scope) as the scope baseline, [`user-stories.md` §5](user-stories.md#5-mvp-definition) as eighteen Must stories at 110 points, and a stated demonstration that proves it works.
- The client's success sentence — *"why does it cost $50 an hour?"* — which gives the reader the product's purpose in twenty seconds.

### What is missing

- **The defect evidence needs correcting before it is published.** [`audit-2026-08-14.md`](audit-2026-08-14.md) §B found that several cell citations in §2 are wrong or incomplete: `I29` is misclassified, `I41`/`I42` carry the same defect and are undocumented, `C24` is not `SUM(D:K)`, "one column too far" describes three different faults as one, and there are at least six defects rather than three. **A wrong cell reference in a document whose entire premise is source fidelity is the single most damaging thing we could submit.** Fix §2 before it goes in the PDF.
- **Defect 4 should be promoted into the submission.** The indirect-cost total omits the Office floor-area row entirely — a cost the user is asked to enter and the spreadsheet then never counts. That is a better story than a mis-typed reference, and it is the cleanest possible illustration of why N14 exists.
- The workbook has **eight** sheets, not three. "Three process sheets over a background data layer" is both accurate and more useful, because that data layer is an input source the MVP has no story for.

---

## 3. Criterion 2 — Client communication and MVP agreement · 4 pts

**Exceptional (3–4) requires:** evidence of good communication with the client and other
stakeholders, **and** that the client *has approved* the MVP and other deliverables to date.

### What we already have

- A recorded 34-minute client walkthrough, written up as [minutes](meetings/2026-07-29-client-kickoff.md) with decisions D1–D6 and actions A1–A4.
- A working relationship with agreed terms: Wednesdays, a shared Teams chat, batched questions, support tapering (kickoff §10).
- Requirements rewritten **against the client's own documents**, with a stated source-precedence order and every claim marked **[G]**, **[W]** or **[K]**.
- Seven open questions, each with a proposed default so a non-answer does not block us ([`requirements.md` §9](requirements.md#9-open-questions)).

### The one thing that separates 2 from 4

**The client has not approved the MVP.** The rubric's Exceptional band says so in as many words.
Requirements v2.0 is marked *"not yet reviewed by the client"* on its own front page.

**The extension changes this from a lost cause to a winnable one.** Under the old deadline a
request sent on Friday would almost certainly not have come back by Tuesday, and the honest
plan was to submit with sign-off outstanding. With eleven days, a written answer is realistic —
which is why the client step is the **one item below that does not move**.

1. **Send the consolidated question list and a one-page MVP statement to the client today, Friday 14 August**, asking for a written yes/no on the MVP scope. Sending it a week later would spend the extension on nothing.
2. **Follow up on Wednesday 19 August** if there is no reply. Wednesdays are the client's stated preference, and it leaves six days to absorb whatever comes back.
3. **If it arrives, say so and quote it.** If it does not, **say plainly in the PDF that sign-off was requested on 14 August, chased on 19 August, and is outstanding.** A team that knows exactly what it is waiting for reads better than one that blurs it. The 5 August deck already takes this line — *"The client's sign-off on the MVP is outstanding, not assumed"* — and we should not retreat from it.

### The evidence gap that will be noticed

`README.md` states that **minutes are committed within 24 hours**. `docs/meetings/` contains
**one** committed meeting record. Between 29 July and today the stated cadence implies roughly
two stand-ups plus the 5 August facilitator checkpoint, and none of them has minutes.

This is the cheapest high-value work available to us. Writing up 24 July and 5 August costs an
hour each and repairs criterion 2 *and* criterion 3 at once. The 5 August record is doubly
load-bearing: the checkpoint deck's own header carries a `TODO` that depends on it, and
[`STYLE-GUIDE.md`](../presentations/STYLE-GUIDE.md) §9 has a checklist item that cannot be
ticked without it.

### One more, quietly important

**Q6 — may AI tools be used on client material? — was asked on 29 July and is still
unanswered.** Our own default is *no*, recorded in
[`reference/client/README.md`](../reference/client/README.md). Requirements v2.0 was rewritten
against the client's documents in the meantime. Whatever the answer, this question should be at
the front of the batched list, and we should be able to say cleanly how that work was done.

---

## 4. Criterion 3 — Project management and plans · 4 pts

**Exceptional (3–4) requires:** a realistic plan with enough detail for *each member* to work
on the next deliverable; responsibilities and deadlines documented; project tools set up, with
evidence of effective planning for group workflow **and software deployment**.

This is our weakest criterion, and it is weak in a way that is fixable in a weekend, because
what is missing is artefacts rather than thinking.

### What we already have

- A [team roster](team.md) with a single home, and a documented member departure with the work redistributed.
- A [build order](architecture.md#10-delivery-approach) — engine first, then identity, then forms, then rates, then seal and export — with a stated rationale for the order.
- A [definition of done](architecture.md#10-delivery-approach) per story.
- A [decision gate](architecture.md#9-decision-gate) with a named fallback trigger at end of week 8.
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

1. `docs/plan.md` — a milestone table from now to end of semester, with dates, and each of the 110 points assigned to a named member for the next two sprints.
2. GitHub Projects board, populated from the eighteen Must stories, one issue each, owner and due date on every one. Link it from `README.md`.
3. `.github/workflows/ci.yml` — even a lint-and-test stub against an empty `src/`. It makes "CI from the first commit of code" a fact rather than an intention, and it is fifteen minutes' work.
4. Minutes for 24 July and 5 August.

---

## 5. Criterion 4 — Risk and technology assessment · 3 pts

**Exceptional (3) requires:** a realistic assessment of skills, resources and risks; **skills
gaps identified and addressed**; technology choices considered and justified; **relevant risks,
including cybersecurity, identified and planned for.**

### Technology — strong, with one thing to say better

[`architecture.md` §8](architecture.md#8-options-assessed) assesses five options against six
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

`docs/risks.md`, one table, likelihood × impact × mitigation × owner × trigger. The material
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
| Client material handling and the unanswered AI question | `reference/client/README.md`, Q6 |

That last-but-one row is worth dwelling on. A risk identified, assessed, and closed by rewriting
history is stronger evidence of risk management than any hypothetical, and we have the commit
trail to prove it.

---

## 6. What to do, in order, before Tuesday

Ordered by marks-per-hour. Items 1–4 are the difference between a competent submission and an
exceptional one.

**Two items keep their original dates.** The client request (1) does not move, because the whole
value of the extension is the time it buys for a reply. The skills audit (3) does not move,
because tomorrow's meeting decides to run it *in the room* — deferring it to 22 August would
leave the technology decision open for another week for no reason. Everything else shifts one
week, landing on the same weekday.

| # | Task | Fixes | Owner | By |
| --- | --- | --- | --- | --- |
| 1 | Send the client the consolidated question list **and a one-page MVP statement, requesting written sign-off**. Put Q6 first | Crit 2 | | **Fri 14 Aug — unmoved** |
| 2 | Assign every owner in this table; agree the §4.2 deviation on the planning deck | All | | Sat 15 Aug |
| 3 | Skills audit — five rows, six skills, gaps and how each is addressed. Run it in the meeting | Crit 4 | | **Sat 15 Aug — unmoved** |
| 4 | Chase the client if there is no reply — Wednesday is their stated preference | Crit 2 | | Wed 19 Aug |
| 5 | `docs/risks.md` — risk register, cybersecurity rows included | Crit 4 | | Sat 22 Aug |
| 6 | `docs/plan.md` — milestones with dates; 110 points assigned to named members | Crit 3 | | Sat 22 Aug |
| 7 | Correct `requirements.md` §2 cell evidence per [audit §B](audit-2026-08-14.md); promote defect 4 | Crit 1 | | Sun 23 Aug |
| 8 | Minutes for 24 July and 5 August | Crit 2, 3 | | Sun 23 Aug |
| 9 | GitHub Projects board from the eighteen Must stories; link from `README.md` | Crit 3 | | Sun 23 Aug |
| 10 | `.github/workflows/ci.yml` stub | Crit 3 | | Sun 23 Aug |
| 11 | Add the sensitivity decomposition to `architecture.md` §8 | Crit 4 | | Sun 23 Aug |
| 12 | Annotate or update the [5 August deck](../presentations/2026-08-05-facilitator-checkpoint.html) — it still teaches the superseded two-income-line model and cites a transcript we do not commit | Crit 1, 2 | | Mon 24 Aug |
| 13 | Narrow the kickoff annotation to what it can support ([audit §A2](audit-2026-08-14.md)) | Crit 1 | | Mon 24 Aug |
| 14 | Write the PDF | All | | Mon 24 Aug |
| 15 | **Confirm the facilitator has access to the repository and the Teams area.** Then submit | Mechanics | | Tue 25 Aug |

Owners are deliberately blank. Fill them in at tomorrow's meeting — an unassigned task list is
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
None of that is decoration on a capstone about *defensible numbers* — it is the same discipline
the product exists to enforce, applied to ourselves, and it is the most persuasive thing we can
put in front of a facilitator.

**What we should not overstate:** we have no client sign-off, no code, no risk register, no
board, no CI and one committed meeting record. The thinking is well ahead of the artefacts.
Criterion 3 marks artefacts.

**The eleven days are more than enough** — every gap above is hours of work, not days, and most
of the content already exists somewhere in this repository and needs collecting rather than
inventing. What it needs is the owners filled in tomorrow.

**And the extension buys one thing four days could not.** Criterion 2's top band is worth two
marks and turns on a single sentence: *the client has approved the MVP*. That was out of reach
on Friday and is in reach now — but only if the request goes out today and gets chased on
Wednesday. If we let the extra week drift, we will arrive at 25 August in exactly the position
we were in on 18 August, having gained nothing but a later date.

---

## Change log

| Version | Date | Change |
| --- | --- | --- |
| 1.0 | 14 Aug 2026 | First version, written against the rubric in [`reference/unit/assignment1-rubric.md`](../reference/unit/assignment1-rubric.md) and the repository as it stands on 14 August. |
| 1.1 | 14 Aug 2026 | **Deadline extended to Tuesday 25 August 2026**, requested by us and granted by the unit coordinator by email on 14 August. §6 rescheduled: the client request (item 1) and the skills audit (item 3) keep their original dates, a client chase on 19 August is added, and everything else shifts one week onto the same weekday. §3 rewritten — written client sign-off moves from unrealistic to achievable, which is where the extension earns its keep. The unit brief is annotated rather than edited. |
