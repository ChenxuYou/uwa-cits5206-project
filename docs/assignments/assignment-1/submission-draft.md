# Assignment 1

## Problem Statement

UWA Research Infrastructure currently calculates platform charge-out rates in a spreadsheet-based workbook. The workbook is functional, but it is **fragile**: the client has already identified that it is easy to break, hard to use, and too exposed to silent formula errors. Our own review of the workbook confirmed that problem. The copy we hold contains multiple formula defects that produce incorrect rates and totals without warning, which means the current process cannot be trusted as a stable basis for pricing.

The client wants software because this is a high-stakes process that repeats only every three to five years, so custodians cannot rely on memory or manual spreadsheet skill. They need a guided tool that makes the method easier to follow, prevents accidental formula damage, keeps the calculation logic hidden from users, and leaves behind a defensible record for audit, approval, and future reference. The software must also make it possible for a future custodian or administrator to reopen a sealed record and explain how the rate was set.

From these pain points and the client requirements, we are deciding to build **a web application** that replaces the fragile workbook with a guided, server-side costing tool. It will let custodians enter the required inputs, calculate the rates behind the scenes, validate mistakes before they cause silent errors, and save a sealed record that can be reviewed or retrieved later.


## Client Communication and MVP

TBD

## Project Management and Plans

### Agile Plan: Next Week to 13 October 2026

#### Team Split

| Role | Person | Responsibility |
| --- | --- | --- |
| Client liaison and sprint coordination | Yichen Zhao | Own client communication, book meetings, send weekly demo summaries, track feedback, and update decisions |
| Development | Chenxu You | Implement core code changes, merge feedback-driven fixes, and prepare deployment |
| Development | Wenmin Luo | Implement core code changes, add regression fixes, and prepare deployment |
| Research, testing, documentation | Dai Lam La La | Trace spreadsheet logic, prepare test scenarios, document assumptions, and support validation |
| Research, testing, documentation | Jaswanth Vericherla | Trace spreadsheet logic, prepare test scenarios, document assumptions, and support validation |
| Deployment support | You | Help with server access, deployment setup, DNS or Nginx, and deployment testing |

#### Weekly Cadence

| Cadence | What happens |
| --- | --- |
| Every week | Reprioritise the backlog, assign owners, and confirm deadlines |
| Every week | Develop and test in parallel so changes are verified before the client demo |
| Every week | Demo the current increment to the client and capture feedback for the next sprint |
| Every week | Turn research findings into test cases, notes, and implementation guidance |
| Starting 28 September | Begin drafting the final report while keeping it aligned with the latest project state |

#### Project Tools and Evidence

| Tool / artefact | Purpose | Evidence we will keep |
| --- | --- | --- |
| GitHub Issues | Track every task with one owner and a due date | Issue list linked to sprint goals |
| GitHub Projects board | Show sprint planning and work in progress | Board columns for Backlog, In Progress, Review, Done |
| Meeting notes in the repo | Record decisions and client feedback | Weekly minutes linked from the plan |
| Pull requests | Show code review and ownership | PRs with review comments and merged changes |
| Test notes and scenarios | Prove validation of changes | Short test log for each sprint |
| Deployment notes | Show readiness for staging and handover | Checklist for server access, CI/CD, DNS, and deployment |

#### Deployment Plan

| Stage | What we do | Owner |
| --- | --- | --- |
| Access and planning | Discuss access to the new server, confirm who can administer it, and decide the deployment approach | Yichen Zhao, You |
| Server setup | Log into the server, create the repository, and set up CI/CD for automated deployment | Chenxu You, Wenmin Luo |
| DNS and reverse proxy | Configure DNS, and if needed use Nginx as the reverse proxy / entry point | You, Yichen Zhao |
| Deployment testing | Test deployment end to end, check build, release, and rollback behaviour, and verify the site is reachable | You, Dai Lam La La |
| Staging sign-off | Confirm the deployed instance is stable enough for client review and final report evidence | All |
| Final release | Lock the version, deploy the final release, and package handover notes | Chenxu You, Wenmin Luo, You |

#### Week-by-Week Plan

| Week commencing | Sprint goal | Yichen Zhao | Chenxu You | Wenmin Luo | Dai Lam La La | Jaswanth Vericherla | You | Output |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 24 Aug 2026 | Stabilise after MVP approval and turn feedback into a backlog | Own the client feedback list, summarise the MVP approval outcomes, and book the next client touchpoint | Review recent feedback and identify the first code changes | Review recent feedback and identify the first code changes | Start tracing spreadsheet logic for risky or unclear calculations | Start tracing spreadsheet logic for risky or unclear calculations | Support backlog triage and note deployment questions | Updated backlog, clear ownership, research notes |
| 31 Aug 2026 | Deliver the first code update cycle | Prepare the weekly client summary and confirm priorities for the sprint | Implement the highest-priority code fixes and keep the core flow moving | Implement the highest-priority code fixes and keep the core flow moving | Write test cases for the updated code paths and compare outputs with spreadsheet values | Write test cases for the updated code paths and compare outputs with spreadsheet values | Help validate the updated flow and record any deployment implications | Revised code slice, test notes, client demo notes |
| 7 Sep 2026 | Improve reliability and validate outputs | Run the client demo, capture feedback, and update the issue list | Fix bugs found in testing and tighten the main flow | Fix bugs found in testing and tighten the main flow | Expand scenario testing and document assumptions behind spreadsheet logic | Expand scenario testing and document assumptions behind spreadsheet logic | Support validation and note any hosting or deployment issues | More stable MVP, validation evidence, second feedback loop |
| 14 Sep 2026 | Prepare deployment and integration | Confirm server and hosting questions with the client and clarify access needs | Prepare the deployment approach and identify setup tasks | Prepare the deployment approach and identify setup tasks | Document integration assumptions and risks for the hosting setup | Document integration assumptions and risks for the hosting setup | Review deployment options and prepare for server access work | Deployment notes, integration risks, staging checklist |
| 21 Sep 2026 | Start deployment work | Coordinate the server access discussion and confirm who can administer the environment | Log into the new server, create the repository, and begin CI/CD configuration | Support server setup and test the deployment pipeline | Prepare deployment test cases and check DNS and hosting assumptions | Prepare deployment test cases and check DNS and hosting assumptions | Help with deployment setup and verify early server changes | Server access decision, repo on server, CI/CD draft |
| 28 Sep 2026 | Finish initial deployment and harden the app | Coordinate DNS and Nginx discussion, confirm the deployment path, and prepare the next client update | Configure DNS or Nginx, then test the deployed site end to end | Support deployment testing and fix issues from server setup | Test the deployed site and document the result | Test the deployed site and document the result | Help test the deployed site and confirm access from the team side | Working staging instance, deployment test log |
| 5 Oct 2026 | Start the final report and stabilise release | Draft report sections on client communication, deployment, and project planning | Fix only critical issues and keep the release stable | Fix only critical issues and keep the release stable | Gather evidence for the report and document testing outcomes | Gather evidence for the report and document testing outcomes | Confirm deployment evidence and support final reporting | Report draft, evidence pack, release candidate |
| 12 Oct 2026 | Final release and submission week | Finalise the report, confirm handover details, and package submission evidence | Lock the final release and deploy it | Lock the final release and deploy it | Help verify the final build and archive test evidence | Help verify the final build and archive test evidence | Support final deployment checks and handover documentation | Final release, final report, handover notes |

## Risk Assessment and Technology Assessment

### Technology Assessment

The team has five members with a split that is realistic for the semester: one person is responsible for client communication, two are responsible for coding, and two are focused on research, including checking other platforms, investigating the Excel workbook, and asking follow-up questions when the logic is unclear. That division reflects the current workload and also protects the most communication-heavy work from being lost inside implementation tasks.

The main skills gaps are in the areas that normally create project failure: turning a spreadsheet business process into a reliable software workflow, building a secure server-side calculation path, and designing a database-backed record system that preserves history. These gaps are being addressed by keeping the calculation engine pure and server-side, by limiting the MVP to one platform and one vertical slice, and by using the research role to verify the workbook logic, the client terminology, and the workflow assumptions before coding expands.

We have considered the main technology choices rather than assuming one stack by default. The two viable live options are a Single-page Application (SPA) implementing REST API with PostgreSQL. For this project, the server-rendered monolith is the stronger default because it keeps the interface simpler for a small team, reduces front-end complexity, and still supports the key requirement that users must not be able to alter the calculation logic. The SPA approach remains a fallback if the team’s skills audit shows much stronger JavaScript and React capability than Python and server-side web work. The technology decision is therefore based on fit to skills, delivery risk, and the need to ship a complete working slice rather than an over-ambitious system.

### Risk Assessment

The risk profile is dominated by three areas: scope, correctness, and security. Scope risk comes from trying to build too much beyond the MVP; the mitigation is to keep the build to the vertical slice and defer benchmarking, approval workflow depth, communication automation, and cross-cycle analytics unless time remains. Correctness risk comes from the fact that the current spreadsheet already contains silent defects; the mitigation is server-side validation, fixed aggregate contracts, and automated tests against the client’s worked example and a corrected workbook fixture. Security risk is mostly internal rather than public-facing, but it still matters because the application handles UWA-internal pricing records and authenticated access. That means transport security, authenticated sessions, role-based access, input validation, protection against injection and XSS, and an append-only audit trail are all part of the plan.

### Risk Register

| Risk | Likelihood | Impact | Mitigation | Owner |
| --- | --- | --- | --- | --- |
| Client response is slow or ambiguous | Medium | High | Batch questions, assign one client liaison, and keep open questions in a single tracked list | Client liaison |
| Spreadsheet logic is misunderstood | Medium | High | Two researchers cross-check the workbook, meeting notes, and client documents before implementation | Research leads |
| MVP scope grows beyond the semester | Medium | High | Keep the MVP limited to the vertical slice from cycle creation to sealed export | Whole team |
| Calculation defects reappear in code | Low-Medium | High | Pure server-side engine, automated tests, and fixed aggregate rules | Coding leads |
| Skills gap in the chosen stack | Medium | Medium-High | Run a skills audit early, choose the stack that best fits the team, and pair code on the first implementation tasks | Whole team |
| Schedule slips because tasks are not owned | Medium | Medium-High | Every task has a named owner and deadline, with the communicator tracking client dependencies | Team lead / communicator |
| Data is exposed through injection, XSS, or mass assignment | Low-Medium | High | Parameterised queries, escaped rendering, validation schemas, CSRF protection, and least-privilege access | Coding leads |
| No auditable record of how rates were set | Medium | High | Seal records as immutable snapshots with versioned method configuration and exportable PDFs | Coding leads |