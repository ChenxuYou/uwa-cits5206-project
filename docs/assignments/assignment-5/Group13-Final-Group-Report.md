<!--
Final Group Report (Assignment 5): source for the PDF.

Brief and rubric: reference/unit/assignment-5-final-group-report.md
Due Tuesday 13 October 2026, 11:59 pm (UTC+8); one member submits.
No more than 5 pages in all, cover and contents included. Page 1 is the cover and contents,
so the body has pages 2 to 5. The page budget under each heading adds up to 4.

Links: write every link to a file or folder as a full URL on main, inside <...> so it shows in
full (F1): the repository URL, then /blob/main/<path> for a file or /tree/main/<path> for a
folder. Do not hand-write a SHA. After the last merge, pin them with
pin_links.py, which also fills in the SHA on the cover; then build with build.py (D5) and
open every link signed out (F3). Issue, pull request, milestone and release links stay live.

These comments do not appear in the PDF.
-->

<div class="cover compact">
<p class="kicker">CITS5206 Information Technology Capstone Project · Semester 2, 2026</p>
<h1 class="title">Final Group Report</h1>
<p class="subtitle">Assessment 5 · Group 13</p>

<table class="meta">
<tr><th>Project</th><td>Research Infrastructure Costing &amp; Pricing Tool</td></tr>
<tr><th>Client</th><td>UWA Research Infrastructure: Erika Slavin and Mathew Hall</td></tr>
<tr><th>Group</th><td>13: Chenxu You, Yichen Zhao, Wenmin Luo, Dai Lam La La, Jaswanth Vericherla</td></tr>
<tr><th>Release</th><td><code>v1.0.0</code>, handed over on 12 October 2026</td></tr>
</table>

<div class="linkbox">
<p class="lb-label">Repository</p>
<p><a href="https://github.com/ChenxuYou/uwa-cits5206-project">https://github.com/ChenxuYou/uwa-cits5206-project</a></p>
<p class="lb-label">Release</p>
<p><a href="https://github.com/ChenxuYou/uwa-cits5206-project/releases/tag/v1.0.0">https://github.com/ChenxuYou/uwa-cits5206-project/releases/tag/v1.0.0</a></p>
<p class="lb-label">Open issues: known bugs, gaps and future work</p>
<p><a href="https://github.com/ChenxuYou/uwa-cits5206-project/issues?q=is%3Aissue+is%3Aopen">https://github.com/ChenxuYou/uwa-cits5206-project/issues?q=is%3Aissue+is%3Aopen</a></p>
</div>

<p class="note"><strong>About the links.</strong> Every link is written out in full. The repository, issues, pull requests and CI runs are public, so no invitation is needed. Links to files are pinned to commit <code>{{PINNED_SHA}}</code>, the state of <code>main</code> when this report was submitted.</p>

<table class="handles">
<tr><th>Member</th><th>GitHub account</th><th>Layer</th></tr>
<tr><td>Chenxu You</td><td><code>ChenxuYou</code></td><td>General backend, documentation</td></tr>
<tr><td>Yichen Zhao</td><td><code>itsEvanZHAO</code></td><td>Front end, client liaison</td></tr>
<tr><td>Wenmin Luo</td><td><code>onikirinana</code></td><td>Calculation engine</td></tr>
<tr><td>Dai Lam La La</td><td><code>ladailam382</code></td><td>Deployment</td></tr>
<tr><td>Jaswanth Vericherla</td><td><code>jaswanth-kumar24</code></td><td>Authentication and accounts</td></tr>
</table>
</div>

<div class="toc-page compact">
<h2 class="toc-title">Contents</h2>
<!--TOC-->
</div>

## 1. Requirements and solution

<!--
Rubric 3.1, Delivered Project. About 0.75 page.
- The problem: UWA RI's costing workbook; the custodian must defend each rate ("why does it cost $50 an hour?").
- The agreed MVP: all 18 Must stories (110 points), signed by Mathew Hall on 20 August 2026.
- What was delivered: all 18 Must stories plus US-20, approval by the delegated authority. The
  six-step workflow, three rates, submit, return, approve and seal, sealed records with PDF.
- Partly delivered: US-24 benchmarking notes, US-22 comparison. Not built: US-05, US-21, US-23, US-25.
- The solution in one paragraph: ASP.NET Core and SQLite, the engine in its own project (ADR-001).
-->

*To be written.*

<div class="ev" markdown="1">
- Signed MVP agreement
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/client/mvp-agreement.md>
- Requirements and user stories
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/spec/requirements.md>
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/spec/user-stories.md>
- Architecture and the technology decision
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/spec/architecture.md>
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/decisions/adr-001-technology-stack.md>
</div>

## 2. Verification and testing

<!--
Rubric 3.1 (quality assurance) and 3.2 (client acceptance testing). About 0.75 page.
- 249 automated tests: engine 36, PDF 31, web 182. The client's worked example to the cent.
- CI on every push and pull request: build, tests, formatting, dependency scan, migration check.
  Last run on main at fb0321b, green; later merges were docs-only and skipped by paths-ignore,
  so say "green on the last code change".
- Code review: nothing merges on its author's approval.
- Deployment rehearsal, 25 Sep: release, HTTPS, forced password change, backup, restore, rollback.
- Client acceptance testing, 9 Oct: the client used staging in all three roles; six requests,
  five done, the sixth (salary fill) partly done.
- Regression pass on staging, 12 Oct, all three roles (#101). Fresh-clone run, 12 Oct, by a
  member who did not write the setup steps.
-->

*To be written.*

<div class="ev" markdown="1">
- Evidence table in the README
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/README.md#how-we-know-it-works>
- Test projects and CI
  <https://github.com/ChenxuYou/uwa-cits5206-project/tree/main/tests>
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/.github/workflows/ci.yml>
- Client testing record, 9 October
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/client/communication-history/2026-10-09-client-testing-notes/README.md>
- Regression pass on staging, 12 October
  <https://github.com/ChenxuYou/uwa-cits5206-project/issues/101>
</div>

## 3. User guides and deployment

<!--
Rubric 3.2, Deployment and Documentation. About 0.5 page.
- How to run it: the README's "Run it on your machine"; the marker follows it.
- User manual for custodians, approvers and administrators, with 15 screenshots.
- Deployment runbook: Ubuntu, Caddy, SQLite; daily backups, restore and rollback.
- Staging: live since 30 Sep, runs v1.0.0; reached by IP, so the address and accounts went to
  the client by email and are not in the public repository. Production is UWA's decision (handover §6).
-->

*To be written.*

<div class="ev" markdown="1">
- Run it on your machine
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/README.md#run-it-on-your-machine>
- User manual
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/user-manual.md>
- Deployment runbook
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/deploy/README.md>
</div>

## 4. Client acceptance and handover

<!--
Rubric 3.4, Client Feedback. About 0.5 page.
- Handover document and one-page summary sent to the client on 12 October, with a reply to their
  testing notes. Sign-off not yet received when this report was submitted: say so plainly.
  If it arrives before the links are pinned, file the signed copy, merge, and change this section.
- What UWA has to decide (summary §3) and the method questions (Q11–Q16).
-->

*To be written.*

<div class="ev" markdown="1">
- Handover document and one-page summary
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/handover.md>
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/handover-summary.md>
- Handover email, 12 October
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/client/communication-history/2026-10-12-handover-for-sign-off/covering-email.md>
- Client meetings
  <https://github.com/ChenxuYou/uwa-cits5206-project/tree/main/docs/meetings/client>
</div>

## 5. Discussion and future work

<!--
Rubric 3.1, limitations (future work). About 0.5 page.
- What went well and what did not: e.g. milestones met late (M3, M5), staging first deployed from
  a branch, not main. Brief; the detail is in plan.md and the risk register.
- Limitations and next work: handover §8, Known gaps in src/README, the 14 open issues. Salary
  pre-fill from pay scales (US-05) is the first recommended step.
-->

*To be written.*

<div class="ev" markdown="1">
- Known limitations and recommended next work
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/handover.md#8-known-limitations-and-recommended-next-work>
- Known gaps, each linked to its issue
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/src/README.md#known-gaps>
- Project plan and risk register
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/project/plan.md>
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/project/risks.md>
</div>

## 6. Responsible use of AI

<!--
Rubric 3.3. About 0.6 page. Proficient needs evidence that AI output was critically evaluated,
adapted and integrated, not only acknowledged.
- Start from the acknowledgement statement in ai-use.md §9.
- Two or three concrete cases of output checked, corrected or rejected, and how: tests, review,
  the client's worked example. Take them from ai-use.md §4 and §6.
- Check the LMS first for a set AI-statement format (brief O5).
-->

*To be written.*

<div class="ev" markdown="1">
- AI-use statement and record
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/project/ai-use.md>
- The acknowledgement for this report
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/project/ai-use.md#9-acknowledgement-statement-for-the-report>
</div>

## 7. References

<!-- About 0.3 page. Hanging-indent entries, as in D3. -->

<div class="refs" markdown="1">

*To be written.*

</div>
