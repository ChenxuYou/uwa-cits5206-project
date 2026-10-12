<!--
Final Group Report (Assignment 5): source for the PDF.

Brief and rubric: reference/unit/assignment-5-final-group-report.md
Due Tuesday 13 October 2026, 11:59 pm (UTC+8); one member submits.
No more than 5 pages in all, cover and contents included. Page 1 is the cover and contents,
so the body has pages 2 to 5.

Links: write every link to a file or folder as a full URL on main, inside <...> so it shows in
full (F1): the repository URL, then /blob/main/<path> for a file or /tree/main/<path> for a
folder. Do not hand-write a SHA. After the last merge, pin them with
pin_links.py, which also fills in the SHA on the cover; then build with build.py (D5) and
open every link signed out (F3). Issue, pull request, milestone and release links stay live.

Facts in this report are as of Monday 12 October 2026. If the client signs the handover before
the links are pinned, file the signed copy, merge, and change §4 and the AI note on page 1.

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

<p class="ack"><strong>Acknowledgement of generative AI use.</strong> Between August and October 2026 the team used Claude (Anthropic, 2026; Claude Code and the Claude app) and ChatGPT (OpenAI, 2026), every member using both, and GitHub Copilot (GitHub, 2026), used by one member. They drafted and reviewed code and tests, explained unfamiliar framework behaviour, reviewed pull requests, and drafted and edited documentation, including this report. No client data, personal data or third-party intellectual property was entered into a public AI tool. Every output was checked by a team member against the client's worked example, the test suite, the source code or the project record, and the member who committed each change is responsible for it. Section 6 gives the detail.</p>
</div>

## 1. Requirements and solution

**The problem.** UWA Research Infrastructure (RI) runs shared instruments, from electron microscopes to a human MRI, and recovers part of their running cost from the researchers who use them. Because UWA is publicly funded, every price must be transparent, consistent and defensible years later. RI's method lived in a guide and an Excel calculator (UWA Research Infrastructure, 2026), where mistakes are silent: a cleared formula or a mistyped amount still gives a plausible number, with no record of how. The test of success was to answer a researcher who asks *"why does it cost $50 an hour?"* from the record alone.

**The agreed MVP** is eighteen Must user stories (110 points): a platform and its capabilities, one costing cycle, and the whole path from signing in to retrieving a sealed record. Mathew Hall signed the scope and the ownership terms on 20 August 2026, and confirmed in writing that where the guide and the calculator disagree, the guide governs.

**The solution** is a guided web application. A platform custodian, who may do this only once every three to five years, works through six explained steps: Platform, Costs, Funding, Capacity, Rates and Review. The server computes three rates per capability: for UWA researchers (*C* − *I*<sub>total</sub>) / *U*; for other publicly funded researchers ((*C* − *I*<sub>non-UWA</sub>) / *U*) × 1.35; and for commercial users (*C* / *U*) × 1.35, where *C* is operating cost, *I* is non-variable income and *U* is forecast use. The custodian proposes rates, sees their effect on the platform's balance, justifies any variance and submits. The delegated approver returns the cycle with comments or approves it, and **approval seals the record**: its inputs, workings and decision are frozen in a snapshot with a SHA-256 hash that can be superseded by a new cycle but never edited, and it exports as a PDF for UWA's records system (Content Manager).

The stack, ASP.NET Core Razor Pages with EF Core and SQLite on .NET 10 (Microsoft, 2025), was chosen from six assessed options (ADR-001) because it keeps the calculation on the server and C#'s `decimal` type suits money. The engine is a separate project with no dependencies and a versioned method configuration, so a record sealed in 2026 reproduces its figures under the method it was sealed with.

| Scope | Stories | State on 12 October 2026 |
| --- | --- | --- |
| MVP (Must) | All 18: US-01 to US-04, US-06 to US-19 | **Delivered** and closed against their acceptance criteria |
| Beyond the MVP | US-20 approval by the delegated authority | **Delivered** |
| Partly delivered | US-24 benchmarking notes; US-22 comparison with the last cycle | Free-text benchmarking notes, sealed into the PDF; the last record's key figures beside a new cycle |
| Not built | US-05 salary pre-fill, US-21 method configuration screen, US-23 replacement reserve, US-25 price-change communication | Open issues, ordered in §5 |

<div class="ev" markdown="1">
- Signed MVP agreement, and the user stories with their acceptance criteria
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/client/mvp-agreement.md>
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/spec/user-stories.md>
- Architecture, the technology decision, and how the repository is organised
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/spec/architecture.md>
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/decisions/adr-001-technology-stack.md>
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/README.md#repository-layout>
</div>

## 2. Verification and testing

Quality assurance was built around one fixed point, the client's worked example: costs of $150,000, UWA in-kind support of $20,000 and State support of $30,000 over 1,000 forecast hours must give $100.00, $162.00 and $202.50 an hour. It is a merge gate in CI, and every layer is checked against it.

| Activity | What it showed |
| --- | --- |
| **249 automated tests**: engine 36, PDF 31, web 182 | The worked example to the cent in the engine and again in the printed PDF; rounding once, at presentation; validation on every step; sign-in, lockout and record ownership; approval and sealing under concurrent requests; supersession; schema migrations |
| **Continuous integration** on every push and pull request | Build, tests, format check and dependency scan; a model change without a database migration fails the build. Green on the last code change to `main` (`fb0321b`); later merges changed documentation only |
| **Deployment rehearsal**, 25 September | First release, HTTPS, forced password change, backup, restore, and automatic rollback of a deliberately broken release |
| **Client acceptance testing**, 9 October | The client used staging unaccompanied, in all three roles and with their own test data, and sent written notes (§4) |
| **Regression pass on staging**, 12 October | Every Must story and US-20 in all three roles, the client's requests, sign-in over HTTPS and no demo account, on the release handed over: all passed |
| **Fresh-clone run**, 12 October | A member who did not write the setup steps cloned the repository on a clean machine and ran the tool from the README as written |

**Limits.** The client tested an older build, so two of their six requests were already fixed, and their notes did not reach funding, proposed rates, sealing or the PDF; the regression pass covered those. The folder access rules work, but no test would fail if one were removed (#86). Not every pull request had a second member's review (§5).

<div class="ev" markdown="1">
- Evidence table in the README, the test projects, and the CI workflow
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/README.md#how-we-know-it-works>
  <https://github.com/ChenxuYou/uwa-cits5206-project/tree/main/tests>
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/.github/workflows/ci.yml>
- Regression pass on staging, 12 October
  <https://github.com/ChenxuYou/uwa-cits5206-project/issues/101>
</div>

## 3. User guides and deployment

**Running it.** The README's *Run it on your machine* needs only the .NET 10 SDK and Git. Three commands clone and start the tool, which creates its own database and, in development only, one demo account per role; a five-minute tour takes a cycle from draft to sealed PDF. The fresh-clone run followed exactly this path.

**Using it.** The user manual is written for people meeting the tool cold: task-based chapters for the custodian, the approver and the administrator, fifteen screenshots from staging, a troubleshooting table and a summary of the workflow. The screens also carry the guide's explanations, which the client singled out as helpful.

**Deploying it.** The runbook puts the tool on one Ubuntu 24.04 server: the application under systemd behind Caddy for HTTPS, the database outside the release, daily checked backups, and one script each to release, restore and roll back. A new server has no demo accounts, only an administrator who must change the password at first sign-in. **Staging** has run since 30 September and now serves `v1.0.0`; its address and one account per role went to the client by email, not into this public repository. Production hosting is UWA's decision (§4).

<div class="ev" markdown="1">
- Run it on your machine
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/README.md#run-it-on-your-machine>
- User manual, for custodians, approvers and administrators
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/user-manual.md>
- Deployment runbook, and the developer guide to the code
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/deploy/README.md>
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/src/README.md>
</div>

## 4. Client acceptance and handover

**What the client has said.** At her first look at the working tool on 22 September, Erika Slavin found the business process clear, the content comprehensive and the interface good. On 9 October Mathew Hall tested staging himself in all three roles and sent written notes. He found the capacity step clear and intuitive, the rates clear with the figures behind each, the review and approval pages clear, the return-with-comments protocol working, and the explanatory text useful because it is visible rather than hidden. He asked for six changes to the custodian's platform and cost steps. Two were already on `main`; three were made and merged, with tests, the same day (#110). The sixth, filling in each year's salary, is partly done: every year fills from a base salary the custodian enters, but filling it from the staff level and step needs UWA's pay scales (US-05, #42).

**The handover** went to the client on 12 October: a handover document and a one-page summary for signature. They set out what is handed over, how to run it, seven decisions that need a UWA owner (from who runs and hosts the tool to the licence and when the team's staging server closes), and five questions on how the tool reads the method, each with its current answer. **When this report was submitted, the client had not yet signed.** The testing notes are a positive assessment with change requests, not a formal acceptance, so the team will keep following up the signature.

<div class="ev" markdown="1">
- Client testing record, 9 October
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/client/communication-history/2026-10-09-client-testing-notes/README.md>
- Handover document and one-page summary for sign-off
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/handover.md>
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/handover-summary.md>
- Handover email of 12 October, and the client meeting minutes
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/client/communication-history/2026-10-12-handover-for-sign-off/covering-email.md>
  <https://github.com/ChenxuYou/uwa-cits5206-project/tree/main/docs/meetings/client>
</div>

## 5. Discussion and future work

**What worked.** Writing the client's worked example as a test first: the engine milestone was met two days early, and the arithmetic never regressed. Asking the client early and in writing: five questions sent on 17 August were all answered on 20 August, and a written order of sources (the client's answers, then the guide, the calculator and our minutes) settled disagreements without guesswork; assumptions no source supported went to the client as questions, not silently into code. One owner per technical layer, and one client contact, kept work and messages from overlapping.

**What did not.** Two milestones slipped: rates and balance by five days, and client use of staging by a week. Staging was first deployed from a branch, not `main`, so the client tested a build two weeks old and reported two problems already fixed; next time we would deploy from `main` from day one. And our process claims ran ahead of practice: the plan required a second review on every pull request, yet 22 of the 37 merged by 3 October have no approving review on GitHub, and team minutes stopped after 15 September. The repository records both rather than glossing over them.

**Future work.** The 14 open issues hold every known gap. In the order the handover recommends:

1. **Salary pre-fill from UWA's pay scales** (US-05, #42), the client's one outstanding request.
2. **Reconcile the client's calculator line by line** against the engine; where they differ, the guide governs.
3. **A routing-level test for the folder access rules** (#86), and **a PDF link on the approval page** with a recorded A4 print check (#102).
4. **Deleting draft cycles and withdrawing submissions** (#107), and **email notifications** (#108).
5. The remaining stretch stories, and UWA single sign-on, which the signed scope deferred.

<div class="ev" markdown="1">
- Known limitations and recommended next work, and the known gaps, each linked to its issue
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/handover.md#8-known-limitations-and-recommended-next-work>
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/src/README.md#known-gaps>
- Project plan and risk register
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/project/plan.md>
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/project/risks.md>
</div>

## 6. Responsible use of AI

**How we used it.** On 5 August the team set its rules from UWA's guide for students (The University of Western Australia, n.d.) and the ACS Code of Professional Ethics (Australian Computer Society, 2023), and presented them to the facilitator: AI drafts and people decide; whoever commits an output checks it and answers for it; and no client data, personal data or third-party IP goes into a public AI tool. AI drafted code, tests and documents, gave pull requests a first review, explained unfamiliar parts of ASP.NET Core and EF Core, and polished emails. It made no decisions (the stack, scope, layers and cuts were settled in minuted meetings), and it never wrote a test's expected values, so it could not pass its own exam.

**How its output was checked.** The checks scale with consequence. Wording is read; the calculation, the seal and sign-in must pass tests that fail if the work is wrong. Five of the cases those checks caught:

| What the AI produced | How it was caught | What changed |
| --- | --- | --- |
| An approval handler in which a double-click could seal a record twice, rewriting its hash | The pre-staging audit of 25 September | A concurrency stamp on every cycle, with tests that stage the race on a real database (#90) |
| Start-up code using `EnsureCreated`, which cannot change a schema: the first release with a model change would strand the client's data | The same audit | EF Core migrations, and a CI check that fails a model change without one (#90) |
| A cost form that echoed user text through `@Html.Raw`, a cross-site scripting hole | Typing a `<script>` element into the field | Encoded output; hostile input now tried on every generated view that shows input back |
| A refactor that rounded at intermediate steps and came out one cent wrong on a boundary input | The engine's boundary tests | Rounding once, at presentation |
| Documents saying staging was not deployed and every pull request was reviewed | Checking against GitHub and the team | Corrected to what the record shows (§5) |

**What we learned.** The AI's errors were plausible, not random: each looked like correct work and passed a shallow check. The defence is a check that does not depend on the output looking right: a known answer, hostile input, the code, the live record. The gains were real, from fast first drafts to an attacker's eye on demand, but responsibility stayed with whoever committed the work. This report was drafted with Claude from the repository record, acknowledged as UWA Library (n.d.) advises, and every figure in it was checked against the repository and GitHub.

<div class="ev" markdown="1">
- AI statement: rules, tools and checks; each member's account of where AI was wrong; the fix for the first two cases
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/project/ai-use.md>
  <https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/project/ai-use.md#6-individual-accounts>
  <https://github.com/ChenxuYou/uwa-cits5206-project/pull/90>
</div>

## 7. References

<div class="refs" markdown="1">

Anthropic. (2026). *Claude* [Large language model]. <https://claude.ai>

Australian Computer Society. (2023). *ACS code of professional ethics*.

GitHub. (2026). *GitHub Copilot* [AI coding assistant]. <https://github.com/features/copilot>

Microsoft. (2025). *ASP.NET Core documentation*. <https://learn.microsoft.com/aspnet/core>

OpenAI. (2026). *ChatGPT* [Large language model]. <https://chatgpt.com>

The University of Western Australia. (n.d.). *Using artificial intelligence tools at UWA: A guide for students*. <https://teaching.csse.uwa.edu.au/units/CITS2003/Using_AI_Tools_at_UWA.pdf>

UWA Library. (n.d.). *Acknowledging and referencing AI*. <https://guides.library.uwa.edu.au/artificial_intelligence/acknowledging_and_referencing_AI>

UWA Research Infrastructure. (2026). *Costing and pricing research infrastructure* [Unpublished guide and cost calculator supplied to Group 13].

</div>
