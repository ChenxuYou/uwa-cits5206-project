# Assignment 1 — Marker Feedback and What It Cost

**Deliverable:** `Group13-Project Spec and Plans.pdf`, submitted 25 August 2026
**Result:** **12 / 15**
**Recorded:** 13 September 2026
**Brief and rubric:** [`reference/unit/assignment-1-instructions.md`](../../../reference/unit/assignment-1-instructions.md) ·
[`assignment-1-rubric.md`](../../../reference/unit/assignment-1-rubric.md)

> **The two marks we lost were not lost on the work. They were lost on links that would not
> open** — and §4 below establishes exactly why they would not open, from the submitted PDF
> itself. The fix is already scheduled into
> [`docs/project/assignment-2-completion-plan.md`](../../project/assignment-2-completion-plan.md),
> which is due 29 September under an identical linking requirement.

---

## 1. The result

| Criterion | Band awarded | Score | Possible |
| --- | --- | --- | --- |
| 1 · Problem Statement — what are you going to build and why | Exceptional | **3** | 4 |
| 2 · Client communication and MVP agreement | **Competent** | **2** | 4 |
| 3 · Project Management and Plans | Exceptional | **4** | 4 |
| 4 · Risk and Technology Assessments | Exceptional | **3** | 3 |
| | | **12** | **15** |

Three of the four criteria were marked *Exceptional*. **Every mark we dropped is in criterion 1's
presentation and criterion 2's evidence** — neither of which is about the quality of the thinking
in the report.

---

## 2. The feedback, verbatim

### Criterion 1 — Problem Statement · 3 / 4 · Exceptional

> No cover page or table of contents; the descrription of the project motivation, objectives and
> values, and MVP deliverables are detailed

### Criterion 2 — Client communication and MVP agreement · 2 / 4 · Competent

> Most links cannot be opened and are just blue underlined text. Except for the initial GitHub
> repo link, the other evidence, such as communication history, user stories, and meeting notes,
> all return "404 - page not found. The main branch of uwa-cits5206-project does not contain the
> path doc."

### Criterion 3 — Project Management and Plans · 4 / 4 · Exceptional

No criterion feedback recorded. Band description awarded in full:

> Project plan is realistic and contains sufficient detail for each team member to work on the
> next deliverable. Responsibilities and deadlines are documented. Project tools are set up and
> show evidence of effective planning for group workflow and software deployment.

### Criterion 4 — Risk and Technology Assessments · 3 / 3 · Exceptional

No criterion feedback recorded. Band description awarded in full:

> The team has made a realistic assessment of their skills, resources and risks for this project.
> Skills gaps have been identified and addressed. The team has carefully considered different
> choices of technology for the project and clearly justified the decisions made. Relevant risks,
> including cybersecurity, have been identified and planned for.

---

## 3. What criterion 2 actually says

The band we were given reads *"Some communication with client; MVP not clearly articulated or
agreed; lacks evidence of good outcomes from this."*

**None of that describes what happened.** The client signed the scope statement on 20 August and
answered all five open questions in writing the same day; the signed PDF, their annotated answers
and our notes from the room are all in
[`docs/client/communication-history/2026-08-20-client-meeting/`](../../client/communication-history/2026-08-20-client-meeting/).
The *Exceptional* band — *"client has approved MVP and any other deliverables to date"* — is a
factual description of where the project stood on submission day.

**The marker could not open any of it.** An evidence link that does not resolve is not weaker
evidence; from the other side of the submission it is no evidence at all. This is worth stating
plainly rather than filing as bad luck, because the same thing will happen again on 29 September
unless the mechanism in §4 is fixed.

---

## 4. Why the links failed — established from the submitted PDF

The submitted PDF was re-examined on 13 September. It carries **28 link annotations across 19
pages, resolving to 19 distinct URLs**. Every one of them is absolute, correctly spelled, and
points at a path that exists: the repository is public, its default branch is `main`, and
`docs/meetings/`, `docs/client/communication-history/`, `docs/spec/` and `.github/workflows/ci.yml`
all resolve unauthenticated today.

**So the URLs were right. Two separate defects stopped the marker reaching them.**

### 4.1 The long URLs wrapped mid-path, and broke after `doc`

Page 1 lists four resource URLs as visible text. Each is too long for the measure and wraps. The
PDF's own text layer shows where:

```
https://github.com/ChenxuYou/uwa-cits5206-project/tree/main/doc
s/meetings
```

The same break occurs on all four: `…/tree/main/doc` + `s/spec`, `s/project`,
`s/client/communication-history`.

**A reader copying the first line gets `https://github.com/ChenxuYou/uwa-cits5206-project/tree/main/doc`,
and GitHub answers that with "main does not contain the path doc".** That is, word for word, the
error quoted in the criterion 2 feedback. The failure is reproduced and explained; there is
nothing left to speculate about.

### 4.2 Page 10 hid ten URLs behind the word "link"

Page 10 is the project-resources table — issues, the Projects board, pull requests, CI, meeting
notes, the plan, the risk register, the skills audit, the decision records, the client
communication history. It is the single densest page of criterion 2 and criterion 3 evidence in
the report.

**Its entire "Where" column is the word `link`, ten times.** The URLs exist only as annotations
behind that word. When the annotation is not passed through to the reader — which is precisely
what the unit warns about — the page renders as ten rows of blue underlined "link" with no
address anywhere on it. The marker's first sentence, *"Most links cannot be opened and are just
blue underlined text"*, is describing this page.

### 4.3 What this rules out

| Suspected cause | Verdict |
| --- | --- |
| A typo in the paths (`doc` for `docs`) written into the report | **No.** All 19 URLs are correctly spelled in the PDF's annotations |
| The repository or the paths were private at marking time | **No.** The root link worked for the marker, and the paths resolve unauthenticated now |
| The work had not been pushed to `main` | **Not supported.** The paths resolve on `main` today and the tree was reorganised before submission |
| **Long URLs wrapping mid-path, and URLs hidden behind anchor words** | **Confirmed** — §4.1 and §4.2 |

---

## 5. What changes, and where it is written down

Nothing here is a lesson worth learning twice. Each row below is carried into a document that
someone will actually open before the next deadline.

| # | Change | Where it now lives |
| --- | --- | --- |
| F1 | **Never hide a URL behind an anchor word.** No `link`, no `here`, no document name standing in for an address. Every reference is the full `https://…` as visible text | [`assignment-2-completion-plan.md` §7](../../project/assignment-2-completion-plan.md) |
| F2 | **No URL may wrap mid-path.** Set link text so it breaks only after a `/`, or shorten the path, or put the URL on its own line at a size that fits. Verified by copying each line out of the built PDF | [`assignment-2-completion-plan.md` §7](../../project/assignment-2-completion-plan.md) |
| F3 | **Check the built PDF, not the markdown.** Copy every URL out of the PDF by hand and open it signed out. This is the check that would have caught both defects | [`assignment-2-completion-plan.md` §6](../../project/assignment-2-completion-plan.md), 28 Sep |
| F4 | **Every submission gets a cover page and a table of contents.** One point, and the cheapest point on the rubric | This file; applies to Assignment 2 and the final report |
| F5 | **A link table is a liability, not a convenience.** If the evidence matters, name it in the sentence that claims it, with its URL inline | [`assignment-2-completion-plan.md` §5](../../project/assignment-2-completion-plan.md) |

**The unit's own Assignment 2 brief says the same thing** — *"Please give full URLs for your
links. It seems that pdf links don't work from LMS marking system."* We now have our own evidence
of what that costs: **two marks, on a criterion where the underlying work was at the top band.**

---

## 6. Related files

- The submitted PDF — `Group13-Project Spec and Plans.pdf`, in this folder
- The markdown it was assembled from — [`submission-draft.md`](submission-draft.md)
- What we planned to deliver, and what landed — [`assignment-1-completion-plan.md`](../../project/assignment-1-completion-plan.md)
- The rubric assessment written before submission — [`assignment-1-readiness.md`](../../project/assignment-1-readiness.md)
- The next deliverable, under the same linking requirement — [`assignment-2-completion-plan.md`](../../project/assignment-2-completion-plan.md)
