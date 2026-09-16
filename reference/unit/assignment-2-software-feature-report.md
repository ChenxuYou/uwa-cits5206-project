# Assignment 2 — Software Feature Report (Individual)

**Transcribed from the LMS on 13 September 2026** from *CITS5206_SEM-2_2026 → Assessments →
Assessment 2: Individual Submissions → Software Feature Report (Individual)*. This file is the
unit's words plus the rubric, kept **unedited** as the record of what we were set. What the team
does about it lives in
[`docs/project/assignment-2-completion-plan.md`](../../docs/project/assignment-2-completion-plan.md).

LMS item: https://lms.uwa.edu.au/ultra/courses/_113158_1/outline

---

## 1. At a glance

| | |
| --- | --- |
| **Type** | **Individual.** Every member submits their own report — this is not a group deliverable |
| **Due** | **Tuesday 29 September 2026, 11:59 pm (UTC+8)** |
| **Format** | A **single PDF**, uploaded through the *Upload Files* link |
| **Attempts** | Unlimited |
| **Marks** | **20 points**, across three sections (8 / 6 / 6) |
| **Period covered** | Individual contributions **up to week 9** |
| **Grading** | Rubric-graded — see §4 |

> **Contrast with Assignment 1.** Assignment 1 was one PDF uploaded by *one* member for the whole
> group. This one is five PDFs, one per member, each about that member alone. Nothing written for
> Assignment 1 can be submitted again as-is.

---

## 2. The brief, as set

> The goal of this individual deliverable is for each member of the group to take responsibility
> for some functionality of the project, and to integrate their work with others. Your group is
> required to prepare an agreed plan for this iteration of the project (up to week 9) as part of
> the project specification deliverable.
>
> Use the Upload Files link to upload a single pdf file that summaries your individual
> contributions (up to week 9) to delivering your team's project.
>
> Your submission should include links to your deliverable sources: code and tests, issues,
> milestones, pull reviews, documents.
>
> Ensure that your group facilitator has access to all linked resources in your report: github,
> your ms-teams area and any other project resources so they can mark your submission.
>
> 1. Describe the artifacts that you created for the project? (your technical contributions,
>    e.g., code, tests, software to manage and related github branches)
>
> 2. Explain how you have managed your work within the project? (commits, pull requests, issue
>    reporting and resolution, milestones, plans, code reviews, meetings)
>
> 3. Demonstrate how you have collaborated with others to integrate all contributions? (your pull
>    request reviews, any training or joint work, milestone management)
>
> See the marking rubric for guidance on how these three sections will be assessed. Please note
> you need to explain how do you use GenAI wherever you used it.
>
> Please give full URLs for your links. It seems that pdf links don't work from LMS marking
> system.

---

## 3. Three constraints that are easy to read past

Each of these is stated in §2 and each one is a way to lose marks without writing anything wrong.

| # | The constraint | What it means in practice |
| --- | --- | --- |
| **C1** | *"Please give full URLs for your links. It seems that pdf links don't work from LMS marking system."* | **This already cost this team two marks.** A hyperlink behind a word arrives at the marker as dead text, and a visible URL that wraps mid-path is copied out broken. Assignment 1 did both — see [`docs/assignments/assignment-1/feedback.md` §4](../../docs/assignments/assignment-1/feedback.md). Every link must be a complete visible `https://…` URL that survives being copied out of the PDF by hand |
| **C2** | *"Ensure that your group facilitator has access to all linked resources."* | A link that resolves for us and 404s for the facilitator scores as no link at all. Covers GitHub (repository, issues, PRs, board, milestones, Actions runs), the MS Teams area, and anything else cited |
| **C3** | *"You need to explain how do you use GenAI wherever you used it."* | A disclosure, placed where the use occurred, not a blanket sentence. It applies to code, tests, documents and the report itself |

**The rubric's own recurring theme is attribution.** All three criteria ask which parts are
*yours* — "Unclear which parts have been written by the student" is what separates *Competent*
from *Skilled* in §4.1. In a five-person repository, a claim that cannot be traced to a commit,
a PR, a review or an issue is not evidence.

---

## 4. Marking rubric

**Total: 20 points.** Levels as shown on the LMS rubric; an *Exceptional* band exists above
*Skilled* on the LMS rubric card but was not captured in the transcription — the bands below are
what was visible, and the Skilled band is what this plan aims at.

| Criterion | Points | Weight |
| --- | --- | --- |
| Software Functionality — the artifacts YOU created | 8 | 40% |
| Project Coordination — how YOU managed your work | 6 | 30% |
| Collaboration — how YOU integrated with others | 6 | 30% |

### 4.1 Software Functionality

*Describe the artifacts that YOU created for the project? (your technical contributions, e.g.,
code, tests, software to manage and related github branches)*

**8 possible points (40%)**

| Level | Score | Description |
| --- | --- | --- |
| Undeveloped | 0 – 1 | Missing or minimal running software product. Missing or minimal supporting documentation. Lacks evidence of individual's contributions to the project software. |
| Competent | 2 – 4 | Some software contributions described. Unclear which parts have been written by the student. Limited supporting documentation. Competent quality and volume of project software delivered in the time so far. |
| Skilled | 5 – 8 | Clear descriptions of the functionality for the project that has been written by you and delivered as quality code that runs, has been tested and is appropriately documented. Shows supporting documentation you developed such as in code comments, readme etc. Good quality and volume of project software delivered in the time so far. |

### 4.2 Project Coordination

*Explain how YOU have managed your work within the project? (commits, pull requests, issue
reporting and resolution, milestones, plans, code reviews, meetings)*

**6 possible points (30%)**

| Level | Score | Description |
| --- | --- | --- |
| Undeveloped | 0 – 1 | Little evidence of systematic management of individual contributions to the project software. Limited or ineffective reviews of software. Ineffective use of project management and planning tools. |
| Competent | 2 – 4 | Some evidence of systematic management of interactions with team members, the client and other stakeholders for integrating your components with the main project. Made some use of issue reporting and planning to manage your submissions. Some use of code reviews and software tests to ensure code quality. |
| Skilled | 5 – 6 | Evidence of effective interactions with team members, the client and other stakeholders for integrating your components with the main project. Made effective use of issue reporting and planning to manage your submissions. Made effective use of code reviews and software tests to ensure code quality. |

### 4.3 Collaboration

*Demonstrate how YOU have collaborated with others to integrate all contributions? (your pull
request reviews, any training or joint work, milestone management)*

**6 possible points (30%)**

| Level | Score | Description |
| --- | --- | --- |
| Undeveloped | 0 – 1 | Insufficient evidence of collaboration to deliver the project OR Insufficient outputs for this deliverable. |
| Competent | 2 – 4 | Limited evidence of effective use of project collaboration. Limited evidence of effective use of tools for managing project workflow and managing SW quality and integrations. |
| Skilled | 5 – 6 | Collaborated with other team members for smooth planning and integration of project parts. Professional conduct of code reviews, any joint activities, training, and managing project milestones. |

---

## 5. Open items on the brief itself

Recorded here rather than assumed, because each one changes what goes in the PDF.

| # | Item | Owner | By |
| --- | --- | --- | --- |
| B1 | **Confirm the calendar date on which "week 9" ends**, so the cut-off for evidence is a date and not a guess. Contributions after it are out of scope for this report | Yichen Zhao | 16 Sep |
| B2 | **Confirm the facilitator's GitHub account and MS Teams access**, and that the Projects board, milestones and Actions runs are reachable by them — C2 above | Yichen Zhao | 19 Sep |
| B3 | **Agree a file-naming convention for the five PDFs** so five submissions are not five different shapes. Once submitted, the name is fixed — [`README.md` §Naming](../../README.md#naming) | Chenxu You | 19 Sep |
| B4 | **Check whether the LMS rubric carries an *Exceptional* band above *Skilled***, and transcribe it here if so | Whoever opens the rubric next | 19 Sep |

---

## 6. Related files

- Completion plan and checklist — [`docs/project/assignment-2-completion-plan.md`](../../docs/project/assignment-2-completion-plan.md)
- **Assignment 1's marker feedback**, and why C1 above is written in bold — [`docs/assignments/assignment-1/feedback.md`](../../docs/assignments/assignment-1/feedback.md)
- Assignment 1 brief and rubric, for the format this file follows — [`assignment-1-instructions.md`](assignment-1-instructions.md), [`assignment-1-rubric.md`](assignment-1-rubric.md)
- Project plan, which supplies the milestones and story ownership this report cites — [`docs/project/plan.md`](../../docs/project/plan.md)
- Team roster, for the name each member submits under — [`docs/project/team.md`](../../docs/project/team.md)
