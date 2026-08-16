# Assignment 1

## Problem Statement

UWA Research Infrastructure currently calculates platform charge-out rates in a spreadsheet-based workbook. The workbook is functional, but it is **fragile**: the client has already identified that it is easy to break, hard to use, and too exposed to silent formula errors. Our own review of the workbook confirmed that problem. The copy we hold contains multiple formula defects that produce incorrect rates and totals without warning, which means the current process cannot be trusted as a stable basis for pricing.

The client wants software because this is a high-stakes process that repeats only every three to five years, so custodians cannot rely on memory or manual spreadsheet skill. They need a guided tool that makes the method easier to follow, prevents accidental formula damage, keeps the calculation logic hidden from users, and leaves behind a defensible record for audit, approval, and future reference. The software must also make it possible for a future custodian or administrator to reopen a sealed record and explain how the rate was set.


## Client Communication and MVP

TBD

## Project Management and Plans

TBD

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