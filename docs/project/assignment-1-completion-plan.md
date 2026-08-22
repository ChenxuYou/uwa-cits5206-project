# Assignment 1 — Completion Plan

**Deliverable:** project specification and plan. **One PDF**, uploaded by **one** member.
**Due:** Tuesday 25 August 2026, 11:59 pm
**Written:** Saturday 22 August 2026 — **three days and a deadline evening left**
**Companion:** [`assignment-1-readiness.md`](assignment-1-readiness.md) says where we stand
against the rubric. This says who does what, by when, and what "done" means.

> **What changed on 20 August.** The client signed the scope statement and answered all five
> open questions ([minutes](../meetings/2026-08-20-client-meeting.md)). Criterion 2 — worth 4 of
> 15 points and the one we could not close by working harder — is closed. **Everything left is
> ours alone**, which means nothing on this plan can be blocked by someone else's inbox.

---

## 1. The shape of the submission

Four sections, one per rubric criterion, in the brief's order. Two exist in draft, two do not.

| § | Section | Pts | Where it stands | Lives in |
| --- | --- | --- | --- | --- |
| 1 | Problem statement | 4 | **Drafted.** Needs the corrected defect evidence folded in | [`submission-draft.md`](../assignments/assignment-1/submission-draft.md) |
| 2 | Client communication and MVP agreement | 4 | **TBD — but the material is now all in hand** | — |
| 3 | Project management and plans | 4 | **TBD, and the artefacts it cites do not exist yet** | — |
| 4 | Risk and technology assessment | 3 | **Drafted**, including a risk register | [`submission-draft.md`](../assignments/assignment-1/submission-draft.md) |

**The single biggest risk to the mark is §3**, and it is not a writing problem. Criterion 3 says
*"project tools are set up and show evidence of effective planning for group workflow and
software deployment."* A facilitator will click the links. If the board is empty or the CI file
is absent, no amount of §3 prose recovers it. **Build the artefacts first, write §3 last.**

## 2. The rule this plan runs on

**Artefact before prose.** Every one of the three days below builds things the report can point
at, and only then writes the report. A section describing a plan that does not exist reads
exactly like what it is.

**Nothing in the report is invented.** All four sections cite documents in this repository. If a
claim has no document behind it, either the document gets written or the claim comes out.

**Submit Tuesday evening, not Tuesday midnight.** The deadline is 11:59 pm; we aim to upload by
**8:00 pm**. The last four hours are for the upload failing, not for writing.

---

## 3. Day by day

### Saturday 22 August — build the missing artefacts

Five people, five parallel tracks. Nothing here depends on anything else here.

| # | Task | Owner | Done when |
| --- | --- | --- | --- |
| 1 | **`docs/project/risks.md`** — promote the risk register already drafted in [`submission-draft.md`](../assignments/assignment-1/submission-draft.md) into its own document. Add the rows [`assignment-1-readiness.md` §5](assignment-1-readiness.md#5-criterion-4--risk-and-technology-assessment--3-pts) lists, each with likelihood × impact × mitigation × **owner** × trigger | Dai Lam La La | Every row has a named owner and a trigger. **Cybersecurity rows are present** — the criterion names them. The two IP risks we found and closed are in it, as closed |
| 2 | **Skills audit** — five members × six skills (JavaScript/React, Python, SQL, HTML/CSS, Git, testing), self-rated, with the gap and how it is addressed. It was due **15 August and did not happen** | Chenxu You | A table in `docs/project/skills-audit.md`. Ten minutes of conversation, and it unblocks [`architecture.md` §9](../spec/architecture.md#9-decision-gate) |
| 3 | **`docs/project/plan.md`** — milestones from now to end of semester with dates, and the 110 story points assigned to named members for the next two sprints | Chenxu You | Each of the eighteen Must stories has an owner and a sprint. Deployment appears as a milestone, not as a wish |
| 4 | **GitHub Projects board**, populated from the eighteen Must stories — one issue each, owner and due date on every one. Link it from [`README.md`](../../README.md) | Wenmin Luo | The link opens, and what it opens is not empty. `README.md`'s claim that *"every task is a GitHub issue with one named owner and a deadline"* becomes true |
| 5 | **`.github/workflows/ci.yml`** — a lint-and-test stub against an empty `src/`. Fifteen minutes | Wenmin Luo | A green check on a commit. Turns [`architecture.md` §10](../spec/architecture.md#10-delivery-approach)'s *"CI from the first commit of code"* from intention into fact |
| 6 | **Minutes for 24 July and 5 August** — the two meetings the repository claims a cadence for and does not have | Jaswanth Vericherla | Two files in `docs/meetings/`. The 5 August one also clears the `TODO` in the checkpoint deck header |
| 7 | **A18** — close Q3, Q4, Q5, Q9, Q10 in [`requirements.md` §9](../spec/requirements.md#9-open-questions) against the client's written answers | Chenxu You | ✅ **Done 22 Aug** — v2.4 |

### Two things found on 22 August that are not on any day above

A repository tidy-up on 22 August turned up two problems that are bigger than the tidy-up. Both
are **recorded here and deliberately not fixed in that pass**, because each needs a decision the
team takes together rather than a change one person makes quietly.

| # | Finding | Why it matters | Owner | By |
| --- | --- | --- | --- | --- |
| **T1** | **The technology decision is not written down anywhere, and the code contradicts the assessment.** [`src/`](../../src/) holds a working ASP.NET Core Razor Pages application (EF Core, SQLite, sign-in, approvals, notifications, sealed snapshots). [`architecture.md` §8](../spec/architecture.md#8-options-assessed) assesses five options and **none of them is .NET**; the nearest, Option C, is Django + HTMX + PostgreSQL. `docs/decisions/` is empty | **Criterion 4, directly.** The rubric marks whether technology choices were *"carefully considered and clearly justified"*. A marker opening both files finds a contradiction, not a justification — and the strongest asset we have (a working app) currently reads as evidence against us | Whole team — decide; Chenxu You — write the ADR | **Sun 23 Aug** |
| **T2** | **`src/bin/` and `src/obj/` are committed** — 76 files, 38 MB of build output including DLLs and per-platform native binaries. `src/.gitignore` already lists `bin/` and `obj/`, but the files were committed before that rule existed and **gitignore does not apply to files already tracked** | Repository hygiene, and it is the exact trap the root [`.gitignore`](../../.gitignore) header warns about in its own words. Low marking risk, real reviewer-impression risk — and it makes every clone 38 MB heavier than it needs to be | Wenmin Luo | Before the next code commit |

**T1 is the one that matters.** The honest framing is available and costs nothing: the team built
a prototype in the stack it could actually move fastest in, that prototype validated the shape
Option C recommended, and the decision record was written after the fact rather than before. That
is a normal thing to have happened and a defensible thing to write down. What is not defensible
is leaving `README.md` and `architecture.md` claiming no decision has been taken while a
finished application sits in `src/`.

**T2 is `git rm -r --cached src/bin src/obj` plus a commit** — the files stay on disk, they
simply stop being tracked. It does not rewrite history, so it needs no force-push and no
re-clone. Clearing the 38 MB out of history as well would, and that is a separate decision the
team should take on its merits after the submission, not the night before it.

### Sunday 23 August — correct what is wrong, then draft §2 and §3

| # | Task | Owner | Done when |
| --- | --- | --- | --- |
| 8 | **Correct [`requirements.md` §2](../spec/requirements.md#2-the-problem)** — the cell evidence, per [audit §B](../internal/audit-2026-08-14.md). `I29` misclassified, `I41`/`I42` undocumented, `C24` is not `SUM(D:K)`, and there are at least **six** defects, not three. **Promote defect 4** — the indirect-cost total omits the Office floor-area row | Wenmin Luo | Every cell reference re-checked against the workbook. **This is not optional.** §1 of the report argues from source fidelity; a wrong cell reference in it is the most damaging single thing we could submit |
| 9 | **A17** — give "the sealed PDF shows the calculator's workings" a requirement ID and a story estimate. It is the one thing the client added to our scope on 20 August | Wenmin Luo | An F-number in [`requirements.md`](../spec/requirements.md), a story in [`user-stories.md`](../spec/user-stories.md), and the MVP total re-stated if it moved |
| 10 | **Deployment plan** — a section or a short document answering the question the [15 August minutes §7](../meetings/2026-08-15-team-weekly-meeting.md) call the biggest one open: where does this run? Client servers, the UWA domain, or a platform-as-a-service. Options, a recommendation, and what has to be true to decide | Wenmin Luo | Criterion 3 names *software deployment* explicitly. A named recommendation with a decision date beats a survey |
| 11 | **Draft report §2 — client communication and MVP agreement** | Yichen Zhao | Built on the four-date trail in [`assignment-1-readiness.md` §3](assignment-1-readiness.md). Leads with the signature, then the three things that make it evidence of *good* communication: the questions were answered rather than defaulted, one answer added to our scope and we recorded it, one request was declined in writing with a reason |
| 12 | **Draft report §3 — project management and plans** | Chenxu You | Written **after** items 1–5 exist, and citing them by link. Names, dates, board, CI, deployment |
| 13 | **A15** — send the client the guide-vs-calculator discrepancy list they asked for | Dai Lam La La | Sent, and filed in [`communication-history/`](../client/communication-history/). Also usable as evidence in report §2 |
| 14 | **A16** — [`NOTICE`](../../NOTICE) and [`README`](../../README.md) cite the signed ownership confirmation; state what still has to happen before Q8 closes | Chenxu You | ✅ **README done 22 Aug.** `NOTICE` outstanding |
| 15 | **A14** — ask the client to confirm whether in-tool approval routing is needed in the core | Yichen Zhao | Sent. **Not a blocker**: the signed scope keeps it a stretch item, so silence changes nothing |

### Monday 24 August — assemble

| # | Task | Owner | Done when |
| --- | --- | --- | --- |
| 16 | **Fold the corrected defect evidence into report §1** and finish §4 from the existing draft | Dai Lam La La | §1 and §4 final. §4 cites `risks.md` and the skills audit rather than repeating them |
| 17 | **Annotate the [5 August deck](../../presentations/2026-08-05-facilitator-checkpoint.html)** — it still teaches the superseded two-income-line model and cites a transcript we do not commit | Jaswanth Vericherla | Annotated, not silently edited. A superseded document marked as superseded is a record; one quietly fixed is not |
| 18 | **Assemble the single PDF** — four sections, consistent voice, with the GitHub and Teams links in it | Chenxu You | One file. Links are absolute URLs, not repository-relative paths — **relative links do not work in a PDF** |
| 19 | **Full read-through by someone who did not write it** | Dai Lam La La | Every claim traced to a document; every date consistent with every other date; the roster uses the enrolment names, not Oliver and Evan ([`team.md`](team.md)) |

### Tuesday 25 August — check access, then submit

| # | Task | Owner | Done when |
| --- | --- | --- | --- |
| 20 | **Confirm the facilitator can open every linked resource** — the GitHub repository and the MS Teams area, including meeting notes and plans | Yichen Zhao | Confirmed by the facilitator opening them, or by checking the permissions directly. **The brief names this and it is the cheapest possible way to lose marks** |
| 21 | **Click every link in the PDF** | Jaswanth Vericherla | All resolve. A dead link in the submission is worse than no link |
| 22 | **One member uploads.** Not two | Chenxu You | Uploaded by **8:00 pm**, not 11:59 |
| 23 | **Individual accountability records up to date** for every member | All | Separate from this submission, but due, and the same evidence supports both |

---

## 4. Who owns what, at a glance

Derived from the work split agreed at the [15 August meeting §8](../meetings/2026-08-15-team-weekly-meeting.md).

| Member | Owns | Report section |
| --- | --- | --- |
| **Chenxu You** | Plan, skills audit, PDF assembly, `NOTICE`, spec updates | §3 |
| **Yichen Zhao** | Client contact (A14, A15 delivery), Teams area, facilitator access | §2 |
| **Wenmin Luo** | Board, CI, deployment plan, `requirements.md` corrections, A17 | Technical detail feeding §1 and §3 |
| **Dai Lam La La** | Risk register, discrepancy list, final review | §1, §4 |
| **Jaswanth Vericherla** | Minutes backfill, deck annotation, link checking | Verification pass |

**Jaswanth attended the 20 August client meeting**, which settles the availability question left
open at the 15 August meeting. His items are self-contained and can be re-owned by Dai Lam on
Sunday morning if anything changes — but they should not need to be.

## 5. What we cut if we run out of time

Stated now, while it is a decision rather than a panic. In the order we drop them:

1. **The 5 August deck annotation** (item 17). It improves an existing artefact; it does not create a missing one.
2. **The deployment plan as a separate document** (item 10) — but *not* deployment as a topic. Criterion 3 names it, so it stays in §3 as a paragraph with a decision date even if it never becomes its own file.
3. **The architecture §8 sensitivity table.** [`assignment-1-readiness.md` §5](assignment-1-readiness.md) makes a good case for it, and it is a refinement of a section that already scores well.

**Nothing on the Saturday list gets cut.** Items 1–6 are what criterion 3 is marked on, and each
is between fifteen minutes and two hours. If Saturday slips, Sunday absorbs it and Monday's
assembly moves to Monday evening — the buffer is deliberate.

## 6. What "finished" looks like

- [ ] One PDF, four sections, uploaded by one member before 8:00 pm on **Tuesday 25 August**
- [ ] Every claim in it traceable to a document in this repository
- [ ] Links to **GitHub** and the **MS Teams** area, both absolute URLs, both opening for the facilitator
- [ ] The signed scope statement quoted in §2 and the evidence folder linked
- [ ] `risks.md`, `plan.md`, `skills-audit.md`, the Projects board and `ci.yml` all existing and all linked
- [ ] No cell reference, date or name in the report contradicting any other document

---

## Change log

| Version | Date | Change |
| --- | --- | --- |
| 1.1 | 22 Aug 2026 | **Two findings from the repository tidy-up added as T1 and T2**, above the Sunday list because neither belongs to a single day. **T1** — the built ASP.NET Core application in `src/` matches none of the five options assessed in [`architecture.md` §8](../spec/architecture.md#8-options-assessed), and `docs/decisions/` is empty, so the repository argues for one stack and ships another. It goes to criterion 4 and needs an ADR, not a rewrite. **T2** — `src/bin/` and `src/obj/` are tracked in git, 76 files and 38 MB, because they were committed before `src/.gitignore` existed. Both are recorded rather than fixed: T1 is a team decision, T2 is a commit someone should make deliberately. The same pass renamed folders and files to one convention and rewrote every cross-reference — see [README § Naming](../../README.md#naming). |
| 1.0 | 22 Aug 2026 | First version, written the day after the client signed. Splits the delivery plan out of [`assignment-1-readiness.md`](assignment-1-readiness.md), which had been carrying both a rubric assessment and a task list. Carries forward the items that document listed as outstanding, adds the six actions from the [20 August client meeting](../meetings/2026-08-20-client-meeting.md), and records the skills audit as **missed on 15 August** rather than quietly re-dating it. |
