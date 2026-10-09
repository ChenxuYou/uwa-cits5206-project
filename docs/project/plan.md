# Project Plan — 24 August to 13 October 2026

**Version:** 1.15 — 9 October 2026
**Owner:** Chenxu You
**Reviewed:** every Saturday team meeting
**Companions:** [`risks.md`](risks.md) · [`skills-audit.md`](skills-audit.md) ·
[`team.md`](team.md) · [ADR-001](../decisions/adr-001-technology-stack.md) ·
[ADR-002](../decisions/adr-002-pdf-generation.md)

> **The scope this plan delivers was signed by the client on 20 August 2026.** Nothing below is
> a proposal to the client; it is the sequence in which we build what they approved.

**Where we are, 9 October 2026.** M1–M6 are met. All 18 Must stories and US-20 are built,
verified and closed, the client has tested staging in all three roles, and five of their six
requests are on `main`, three of them merged in #110. What is left is M7: redeploy staging from
`main`, run and record the regression pass, tag `v1.0.0`, hand over on Monday 12 October and
submit the final report on Tuesday 13 October. The steps, owners and dates are in
[§5](#5-deployment-and-release).

---

## 1. Milestones

| # | Milestone | Date | Done when |
| --- | --- | --- | --- |
| M0 | ✅ Assignment 1 submitted | **25 Aug 2026** | **Met.** `Group13-Project Spec and Plans.pdf` uploaded by one member on 25 Aug, every linked resource open to the facilitator |
| M1 | ✅ **Engine provably correct** | **4 Sep 2026** | **Met 2 Sep, two days early.** The client's worked example reproduces to the cent in `tests/CostingTool.Engine.Tests`, and `dotnet test` is a merge gate rather than a warning. The engine now sits in its own project with no package references, so the tests reach the arithmetic without EF or ASP.NET behind it |
| M2 | ✅ **Guided flow, validated server-side** | **11 Sep 2026** | **Met.** Costs, funding, capacity and forecast utilisation are captured and validated server-side across `Platform → Costs → Funding → Capacity → Rates → Review`, and every step loads through `RicPageModel`, so no step can forget an `Include` or an ownership filter. **Criteria completed 21 Sep:** an audit against [`user-stories.md`](../spec/user-stories.md) found US-03, US-04, US-07 and US-08 (#20, #22, #24, #25) still short of their criteria — the client's cost categories, platform floor area, a capacity built from a baseline, and a forecast above capacity warned about rather than blocked. See change log 1.6 |
| M3 | ✅ Rates, proposed rates and balance | **18 Sep 2026** | **Met 23 Sep, five days late.** Three rates per capability with the figures behind each; proposed rates and the resulting surplus or deficit. US-09 to US-13 (#26–#30) verified and closed, and the milestone closed on GitHub |
| M4 | ✅ **Vertical slice complete** | **25 Sep 2026** | **Met 24 Sep, a day early.** Sign in → create cycle → enter inputs → see rates → propose → justify → seal → export PDF → reopen, driven end to end in a browser. US-01, US-02 and US-14 to US-17 (#18, #19, #31–#34) built in #83 and #84, reviewed, verified and closed. One criterion is met differently from its wording: US-15 has the custodian confirm and the record sealed, while the tool has the custodian submit and the approver's approval seal it, as in US-20. This is recorded on #32 for the client to confirm |
| M5 | ✅ Staging live, client using it | **2 Oct 2026** | Deployment rehearsal completed 25 Sep: first release, HTTPS by IP, forced password change, backup, restore and automatic rollback of a deliberately broken release. **Staging live since 30 Sep**, two days early: deployed by Dai Lam La La from `feat/deployment_docker`, reached by IP address over HTTPS, and the link sent to the client with one account per role the same day ([#60](https://github.com/ChenxuYou/uwa-cits5206-project/issues/60)). **Client use recorded 9 Oct:** the client tested staging in all three roles and sent written notes, summarised in [the testing record](../client/communication-history/2026-10-09-client-testing-notes/README.md) ([#99](https://github.com/ChenxuYou/uwa-cits5206-project/issues/99)). **Status: met late, 9 Oct.** |
| M6 | ✅ Release candidate, feature freeze | **9 Oct 2026** | Critical fixes only; full regression pass; evidence pack assembled. **Met 9 Oct, on the day**, and closed on GitHub: the freeze holds, and the release candidate is `main` at #110, with CI green. The client's testing notes of 9 Oct were taken in as the one exception to the freeze — three small staff-field changes, with tests. **The regression pass is carried into S8**, because it means something only on the server the client will use: it is run on staging once staging runs `main` (§5) |
| M7 | **Final release and handover** | **13 Oct 2026** | Tagged release deployed, [handover document](../handover.md) signed, final report submitted. **Handover expected Monday 12 Oct; the report is due Tuesday 13 Oct, 11:59 pm (UTC+8), and is no more than 5 pages in all** ([brief](../../reference/unit/assignment-5-final-group-report.md)). Closed on GitHub by mistake with M6 on 9 Oct and reopened the same day |

**Fallback trigger.** If M4 has not been met by the end of week 8, we cut stretch scope — we do
not change stack. The cut order is fixed in advance: dashboard → price-change communication →
benchmarking record → replacement reserve → salary pre-fill → in-tool approval routing.

---

## 2. Sprints and story assignment

One-week sprints, Saturday to Saturday. The MVP is **eighteen Must stories, 110 points**
([`user-stories.md` §5](../spec/user-stories.md#5-mvp-definition)).

**Everyone on this team writes code.** Up to S3, Wenmin Luo and Chenxu You carried most of it,
and the first MVP increment — the ASP.NET Core application already in [`src/`](../../src/) — was
**mainly Wenmin Luo's work**. **From S4 the build is split into five layers with one owner each**,
agreed at the [team meeting of 15 September](../meetings/team/2026-09-15-team-meeting.md) and set out
in §3: general backend, calculation, deployment, front end and authentication. S1–S3 below are
left as they happened; S4 onwards is written against the layers. What does not change is the
review rule: the person who writes a story is never the person who verifies it.

| Sprint | Week commencing | Goal | Stories | Pts | Build | Verify |
| --- | --- | --- | --- | --- | --- | --- |
| S1 ✅ | 24 Aug | Backlog, ADR, engine extracted from the page models | US-18 (partial), engine refactor | 8 | Wenmin Luo | Jaswanth Vericherla |
| S2 ✅ | 31 Aug | **Engine provably right** — golden file, decimal, versioned config | US-09, US-18 | 16 | Wenmin Luo, Chenxu You | Jaswanth Vericherla |
| S3 ✅ | 7 Sep | Costs, income, capacity, forecast utilisation | US-03, US-04, US-06, US-07, US-08 | 26 | Wenmin Luo, Chenxu You (US-03, US-07) · Dai Lam La La (US-04) · Jaswanth Vericherla (US-06) · Yichen Zhao (US-08) | Jaswanth Vericherla — except US-06, verified by Chenxu You |
| S4 ✅ | 14 Sep | Rates, proposed rates, balance, justification | US-09, US-10, US-11, US-12, US-13 | 24 | Wenmin Luo (calculation) · Chenxu You (page models, persistence) · Yichen Zhao (screens) · Dai Lam La La (US-13, carried from before the split) | Jaswanth Vericherla — except US-13's screens, verified by Chenxu You |
| S5 ✅ | 21 Sep | Seal, PDF with workings, retrieval, supersession | US-14, US-15, US-16, US-17, US-01, US-02 | 26 | Chenxu You (seal, retrieval, supersession, US-01, US-02) · Wenmin Luo (PDF workings, US-17) · Yichen Zhao (review and approver screens) · Jaswanth Vericherla (who-sealed identity on the record) | Jaswanth Vericherla — except his own piece, verified by Chenxu You |
| S6 ✅ | 28 Sep | Identity hardening and deploy to staging | US-19, deployment | 10 | Jaswanth Vericherla (US-19, staging accounts) · Dai Lam La La (server, CD, TLS) | Chenxu You (US-19) · Jaswanth Vericherla (deployment) |
| S7 ✅ | 5 Oct | Stabilise — critical fixes only; the client's testing notes | Client feedback items 3–5 | — | Chenxu You (#110) | CI only — #110 was merged without a second review |
| S8 ◀ | 10 Oct | Redeploy, regression pass, release, handover and final report — §5 | — | — | Dai Lam La La (release), Chenxu You (handover, report) | Whole team |

**Every Must story was closed by 27 September**, and M4 was met on 24 September, a day early.
The S6 deployment procedure was rehearsed on 25 September, and staging went live on 30 September,
two days before M5. S7 took in the client's testing notes of 9 October and met M6 the same day.
**S8 is the last sprint**: it starts a weekend early, on 10 October, because handover is on
Monday 12 October. US-16's renderer was brought forward out of S5 into a spike in S4 — see
[ADR-002](../decisions/adr-002-pdf-generation.md).

**Story points are re-estimated at each Saturday meeting.** The table above is the plan of
record; the [board](https://github.com/users/ChenxuYou/projects/2) is the live state, and the
Build column is written to match the assignees on it.

---

## 3. Responsibilities

**From 15 September 2026 each member owns one technical layer** ([minutes](../meetings/team/2026-09-15-team-meeting.md)).
A layer is what a person is accountable for building, not the only thing they may touch; the
standing responsibilities alongside it carry over from v1.4 unless noted. The layers do not
overlap, which is also what lets five Assignment 2 reports each lead with their own work.

| Member | Layer | Builds and owns | Standing responsibilities |
| --- | --- | --- | --- |
| **Chenxu You** | **General backend** | Page models and `RicPageModel`; services other than the calculation; data model and **EF Core migrations**; seal, retrieval and supersession; CI; repository structure | ADRs; release management with Dai Lam La La; PDF assembly for deliverables |
| **Yichen Zhao** | **Front end** | Razor views and `site.css` for the guided flow, the review page and the approver's side; accessibility and layout; the screens a client demo is judged on | Single point of client contact; books meetings; decision log; facilitator access to every linked resource |
| **Wenmin Luo** | **Backend — calculation** | `CostingTool.Engine`, `MethodConfig` and `RicCalculationService`; the workings in the sealed PDF; the two unconfirmed modelling decisions; the guide-vs-calculator reconciliation | Calculation fixtures kept in step with the client's guide |
| **Dai Lam La La** | **Backend — deployment** | Server provisioning; CI extended to CD with a documented rollback; DNS, reverse proxy and TLS; staging; the tagged release; dependency scanning | [`risks.md`](risks.md); final read-through of anything submitted |
| **Jaswanth Vericherla** | **Authentication** | Sign-in, roles and authorisation policies; ownership checks and the tests that prove them; account provisioning for staging in place of the demo accounts; US-19 and the SSO-ready seam | Verification of increments outside his layer; meeting minutes; link and access checking |

**Nothing merges on its author's approval.** Every pull request is reviewed by a second member,
nobody approves a pull request in their own layer, and the person who writes a calculation does
not sign off its arithmetic.

---

## 4. Cadence and tools

| When | What | Where |
| --- | --- | --- |
| Saturday evening | Sprint review and planning; risk register reviewed; minutes committed within 24 hours | `docs/meetings/` |
| Monday–Thursday | Build and test in parallel | GitHub, feature branches |
| Wednesday | Client touchpoint — demo and feedback (the client's stated preferred day) | Teams / in person |
| Thursday | Feedback becomes issues with owners and dates | GitHub Issues / Projects board |
| Every push and PR | Restore, build, test. Engine tests are the merge gate | [`.github/workflows/ci.yml`](../../.github/workflows/ci.yml) |

---

## 5. Deployment and release

The production procedure is a single Ubuntu 24.04 server with systemd, Caddy and SQLite,
rehearsed end to end on 25 September ([`deploy/README.md`](../../deploy/README.md)). Staging has
been live since 30 September but was deployed from `feat/deployment_docker`, with Docker Compose
and PostgreSQL ([#60](https://github.com/ChenxuYou/uwa-cits5206-project/issues/60)), so it does
not run `main`. Who runs the tool after handover, where, and on which database is UWA's decision,
put to the client as Q14 in [handover §6](../handover.md#6-decisions-uwa-needs-to-make).

### Deployment gates

| Stage | Owner | By |
| --- | --- | --- |
| Hosting decision and service owner — client-managed UWA host or agreed interim server; DNS and client identity requirements | Yichen Zhao, with Dai Lam La La on technical options | ↪ **Handed to UWA.** The client agreed on 22 Sep that the team may host staging until UWA IT provides resources; the rest is Q14 in [handover §6](../handover.md#6-decisions-uwa-needs-to-make), sent with the reply of 9 Oct. Target 9 Sep missed |
| Provisioning, release procedure, backup/restore and rollback rehearsal | Dai Lam La La | ✅ Rehearsed 25 Sep on Ubuntu 24.04 with a stand-in for `systemctl`; see [`deploy/README.md`](../../deploy/README.md) |
| HTTPS and reverse proxy | Dai Lam La La | ✅ Caddy configurations support HTTPS by IP for testing and domain/Let's Encrypt when DNS resolves. Client-facing hostname and certificate trust still require confirmation |
| Production/staging accounts; no demo account reachable | Jaswanth Vericherla | ✅ Staging live since 30 Sep with one account per role, sent to the client by email ([#60](https://github.com/ChenxuYou/uwa-cits5206-project/issues/60)). That no demo account is reachable is checked in the regression pass below |
| EF Core migrations in place — the schema can change without losing entered data | Chenxu You | ✅ **Done 25 Sep.** A baseline migration replaces `EnsureCreated`; CI fails when the model changes without one ([`src/README.md`](../../src/README.md#changing-the-data-model)) |
| M5 client-use acceptance | Whole team | ✅ **Recorded 9 Oct**, a week late: the client's testing notes, [summarised here](../client/communication-history/2026-10-09-client-testing-notes/README.md). Formal acceptance is the signature on the [handover document](../handover.md) |
| M6 release candidate | Chenxu You | ✅ **9 Oct.** `main` at #110, CI green. The regression pass moves to the table below |

### Run-in to handover — S8, 10 to 13 October

| Step | Owner | By |
| --- | --- | --- |
| Note the figures on the current staging server, then **redeploy it from `main`** with [`deploy/README.md`](../../deploy/README.md). Its PostgreSQL data does not carry over to SQLite; the figures are test data, and the client re-enters what they need ([#101](https://github.com/ChenxuYou/uwa-cits5206-project/issues/101)) | Dai Lam La La | Sun 11 Oct |
| **Regression pass on the redeployed staging**: every Must story and US-20 in all three roles, the client's six requests, sign-in over HTTPS, no demo account reachable. Record the result when closing #101 | Dai Lam La La, Jaswanth Vericherla | Sun 11 Oct |
| **Screenshots in the [user manual](../user-manual.md)**, taken from the redeployed staging | Yichen Zhao | Sun 11 Oct |
| **Tag `v1.0.0`** on the commit staging runs, and publish it as a GitHub release | Dai Lam La La | Sun 11 Oct |
| **Handover meeting** with the client: walk through the [handover document](../handover.md), its decisions (§6) and open questions (§7), and get §11 signed | Yichen Zhao, Chenxu You | Mon 12 Oct |
| **Final report** submitted by one member; links pinned to `v1.0.0` | Chenxu You, with the whole team | Tue 13 Oct, 11:59 pm |
| **Group Member Evaluation** on Feedback Fruits — each member, individually | Everyone | Mon 19 Oct, 11:59 pm |

---

## 6. Open items carried into this plan

**Closed out on 9 October 2026**, checked against GitHub and the repository. Every row now
says whether it was done, superseded, handed to UWA or dropped, so nothing here is still open
by default. The two rows that could not be verified say so.

| # | Item | Owner | By |
| --- | --- | --- | --- |
| A14 | Confirm in writing whether in-tool approval routing is required in the core, or whether recording the approver is enough | Yichen Zhao | ✅ **Superseded.** In-tool approval was built as US-20 ([#56](https://github.com/ChenxuYou/uwa-cits5206-project/issues/56), closed 27 Sep), so the question no longer needs an answer. Missed 26 Aug and 5 Sep before that |
| A15 | Report guide-vs-calculator divergences to the client as they surface. The commercial-rate divergence is already answered; a **line-by-line reconciliation of the calculator is deferred to the next cycle** | Wenmin Luo (from 15 Sep; was Dai Lam La La) | ↪ **Next cycle** — §7 item 1, and [handover §8](../handover.md#8-known-limitations-and-recommended-next-work) item 3 |
| A17 | Give "the sealed PDF shows the calculator's workings" a requirement ID and a story estimate | Wenmin Luo | ✅ **Done 25 Aug** — [#10](https://github.com/ChenxuYou/uwa-cits5206-project/issues/10) closed; the workings are in the sealed PDF (US-16) |
| — | ~~**Create the GitHub Projects board** — populated from the eighteen Must stories. Carried out of Assignment 1 as the one artefact that has to be made by hand~~ | Wenmin Luo, Chenxu You | ✅ Done 1 Sep. Board #2, public and linked to the repository; 25 story issues carried their points, priority and sprint across. Built by [`scripts/seed-project-board.py`](../../scripts/seed-project-board.py), so it can be rebuilt from `user-stories.md` rather than by hand |
| — | **Finish the board by hand** — rename Status `Todo` to `Backlog` and add `Review`; add a board view grouped by Status; add issues #10, #21 and #60, which are not stories and so are not in `user-stories.md`; enable the three Workflows that move cards without anyone dragging them | Chenxu You | ⚠️ **Missed 5 Sep — re-dated to 19 Sep.** Half of it is now automated: [`scripts/finish-project-board.py`](../../scripts/finish-project-board.py) adds the three issues and then audits the board against this row, printing what is still outstanding. The Status rename stays by hand **deliberately** — the GraphQL mutation that edits single-select options replaces the whole option list and clears every card's Status, so the two-minute job in the web UI is the safe one. **Not verified at close-out.** Issues and milestones on GitHub, not the board's columns, are the record of what was done |
| — | **Write up the 24 July and 5 August meetings.** Carried out of Assignment 1; the minutes rule applies from here on, and the 24 July record is a raw transcript, so what goes in `docs/meetings/` is written minutes | Jaswanth Vericherla | 5 August ✅ written up 24 Sep ([minutes](../meetings/facilitator/2026-08-05-facilitator-meeting.md)). **24 July not written up** — closed at project end; that record stays a transcript in the team's Teams area ([`team.md`](team.md)) |
| — | ~~Add Option F to [`architecture.md` §8](../spec/architecture.md#8-options-assessed) and re-run the weighted comparison~~ | Chenxu You | ✅ Done 24 Aug |
| — | ~~Stop tracking `src/bin/` and `src/obj/`~~ | Wenmin Luo | ✅ Done 24 Aug |
| — | ~~**Replace `EnsureCreated()` with EF Core migrations.**~~ | Chenxu You | ✅ **Done 25 Sep.** Baseline migration checked against the model by `MigrationTests`; a database made by `EnsureCreated` is refused at start-up with the reason |
| — | **Confirm two modelling decisions that carry no source marker** — whether a multi-year cost profile is averaged into one annual figure, and whether the indirect-cost uplift is retained by the platform in the revenue projection. Both surfaced on 2 Sep while the engine was extracted; both are commented in the code as ours rather than the client's | Wenmin Luo (from 15 Sep; was Dai Lam La La), put to the client by Yichen Zhao | ↪ **Handed to UWA** as Q12 and Q13 ([#103](https://github.com/ChenxuYou/uwa-cits5206-project/issues/103), [#104](https://github.com/ChenxuYou/uwa-cits5206-project/issues/104)), [handover §7](../handover.md#7-open-questions-on-the-method) |
| — | **Commit the regenerated lockfiles.** `src/CostingTool.Pdf` adds a package reference, so `dotnet restore --force-evaluate` must be run once and both `packages.lock.json` files committed — CI restores against them | Chenxu You | ✅ Done — both lockfiles are committed |
| — | **Move the web application to `src/CostingTool.Web/`.** `src/CostingTool.csproj` globs `**/*.cs` from its own directory, so every sibling project underneath it needs four `Remove` lines to avoid CS0436. The comment there said this was worth doing "before a third project is added"; `CostingTool.Pdf` is the third project. It is a folder move plus three path edits, and it is cheapest now, before the deploy sprint | Chenxu You (from 15 Sep; was Wenmin Luo) | ✖ **Dropped.** Not done by 26 Sep, and a folder move during the freeze would put the release at risk for no change a user sees. The `Remove` lines work; the move is left to whoever next restructures `src/` |
| — | **Amend the ADR-001 stack table's *PDF export* row**, which still says "server-side HTML → PDF". [ADR-002](../decisions/adr-002-pdf-generation.md) decided otherwise and says why; two decision records that contradict each other in a reader's hands are worse than one | Chenxu You | ✅ **Done 8 Oct**, late — ADR-001's row now points to ADR-002 |
| — | **Minutes have not been committed since 20 August.** §4 promises a Saturday review with minutes inside 24 hours; four Saturdays have passed without one. The cadence is either kept or the plan stops claiming it — this row exists so the choice is made deliberately at the next meeting | Jaswanth Vericherla | ✅ Gap closed 24 Sep: six records written up (5 Aug, 19 Aug, 22 Aug, 16 Sep, 22 Sep, 23 Sep), indexed in [`docs/meetings/`](../meetings/README.md). No team-meeting minutes were committed after 15 September; client and facilitator meetings went on being minuted |
| — | **Re-map the board to the five layers.** Reassign open issues to their layer's owner and check the Build column in §2 against the assignees, per the [15 September minutes](../meetings/team/2026-09-15-team-meeting.md) | Chenxu You | **Not verified at close-out**; every story issue is closed, so the board's Build column no longer matters |
| — | ~~**A test that a custodian cannot open another platform's record.**~~ | Jaswanth Vericherla | ✅ **Already covered** — `SignInTests.ACustodianCannotOpenAnotherCustodiansCycle` and `AMissingCycleAndSomeoneElsesCycleAnswerTheSameWay`, and `SealedRecordsTests` for the register. Found stale in the 25 Sep audit |
| Q8 | **Repository licence.** Unblocked by the client's written ownership confirmation of 20 August; [`NOTICE`](../../NOTICE) §1 and §2 now record that confirmation rather than still waiting for it (they were three weeks stale, and §1 still pointed at `docs/requirements.md`). What is left is agreeing the licence text with UWA — a licence granted by one joint owner alone may not be effective, so the team does not write one unilaterally | Chenxu You | ↪ **Handed to UWA** — [handover §6](../handover.md#6-decisions-uwa-needs-to-make), Mon 12 Oct |

---

## 7. Deliberately next cycle, not this one

Recorded so that neither is quietly forgotten and neither quietly becomes this semester's work.

| # | Item | Why it waits |
| --- | --- | --- |
| 1 | **Line-by-line reconciliation of the client's calculator** against the engine, with divergences reported to the client as they asked on 20 August | The engine is what you reconcile *against*. After M1 it is a matter of running both over the same inputs; before M1 it is hand work that would have to be redone |
| 2 | **UWA single sign-on** | Treated as a system integration, which the signed scope defers. Local sign-in sits behind an SSO-shaped seam so it can be swapped |
| 3 | **HR-system integration for staff roles** — raised by the client on 20 August | Raised, not accepted. It is the class of integration the signed scope defers and would need something traded out |
| 4 | **Writing records directly into Content Manager (TRIM)** | Out of scope as stated: the custodian downloads the PDF and files it. Only becomes work if the client asks the tool to write to TRIM |
| 5 | **Salary pre-fill from UWA's pay scales** (US-05, [#42](https://github.com/ChenxuYou/uwa-cits5206-project/issues/42)) — the client's sixth request of 9 October | Needs UWA's current academic and professional pay scales, which the team does not hold. First in line after handover ([handover §8](../handover.md#8-known-limitations-and-recommended-next-work)) |

---

## 8. Change log

| Version | Date | Change |
| --- | --- | --- |
| 1.15 | 9 Oct 2026 | **Brought up to date for the final week.** A status paragraph opens the plan. M5 ticked; M6's row says what was met — the freeze and the release candidate — and moves the regression pass into S8, where it is run on the redeployed staging; M7 records the handover expected on Monday 12 October and that its GitHub milestone was reopened. S1, S2, S4 and S7 ticked; S8 starts on 10 October. §5 becomes *Deployment and release*, with the hosting row handed to UWA as Q14 and a dated run-in to handover, including the Group Member Evaluation on 19 October. Every §6 row closed out as done, superseded, handed to UWA or dropped; two that could not be checked say so. Salary pre-fill added to §7 |
| 1.14 | 9 Oct 2026 | **Client use recorded; report deadline confirmed.** The client tested staging in all three roles and sent notes on 9 October, so M5 is marked met late and its §5 row closed. Three of their six requests are taken into M6 as the one exception to the freeze. The final report is due 13 October, 11:59 pm (UTC+8), and is no more than 5 pages in all, both confirmed on the LMS; M7's row now says so. **M6 met on the day** and closed on GitHub |
| 1.13 | 8 Oct 2026 | **Staging recorded as live.** It has run since 30 September, deployed from `feat/deployment_docker`, and the client has had the link and one account per role since that day (#60). M5's row, S6, the S6 note and §5 now say so instead of asking for confirmation. Client-use acceptance is still outstanding |
| 1.12 | 8 Oct 2026 | **Status review after the M5 target date.** M5's 2 Oct target passed; the repository contains a deployment rehearsal from 25 Sep but no client-use acceptance record, so confirmation is explicitly required rather than assumed. The plan now reflects S7 as the current stabilisation sprint, identifies M6 as due 9 Oct, updates the deployment method to the rehearsed systemd/Caddy/SQLite setup, corrects M2 to the six shipped workflow steps, and flags overdue §6 items for owner verification. |
| 1.11 | 25 Sep 2026 | **Audit of 25 September acted on, for M5.** Three critical findings fixed: the schema moves to **EF Core migrations** (C1), so a deployment can no longer lose the client's data or break on a new column; a **concurrency stamp** on each cycle stops two approval requests both sealing or returning it (C3); and a [`deploy/`](../../deploy/README.md) kit puts the application behind Caddy over HTTPS (C2) with a daily backup, a tested restore and a release script that rolls back. Also: data-protection keys kept beside the database, an absolute database path required outside Development, only a local proxy trusted, anti-framing headers, a sign-in rate limit per address, and a password an administrator set replaced at first sign-in. §5 rows re-dated accordingly; the access-control test row closed as already covered |
| 1.10 | 24 Sep 2026 | **M4 met, a day early**, and **M3 marked met** (23 Sep), which this plan still showed as on track. The vertical slice was driven end to end in a browser, and #18, #19 and #31–#34 were closed against their criteria. The one criterion met differently from its wording, who seals under US-15, is recorded on #32 for the client to confirm, along with the other decisions listed in #83 and #84. S5 marked done; S6, US-19 and the deploy to staging, is next |
| 1.9 | 23 Sep 2026 | **US-14, US-15 and US-16 completed, for M4.** **US-14:** the review page lists everything still missing when it opens — capacity, forecast, utilisation assumptions, proposed rates, a justification where rates differ or forecast a deficit — each linked to the step that supplies it, and submission is refused on that same list (`SubmissionChecks`, shared with the rates step). The page now shows every input by step, each with a link back, and both rate sets with the variance. **US-15:** the approver's confirmation names the consequence; a sealed record is refused any change or deletion at the database, not only by each page; and a sealed record's figures are read from its snapshot on every page, never recalculated — recalculation had fallen back to today's method when the sealed version was not a stored row. **US-16:** snapshot schema 1.4 adds who sealed the record and the capacity inputs; the PDF prints who sealed it, the variance beside each rate, the capacity build-up, every cost and income line, and the guide's retention requirement. Older snapshots still render |
| 1.8 | 23 Sep 2026 | **US-17 built, for M4.** A read-only **Sealed records** register lists every sealed record by platform and pricing period, with its sealed date, method version, who prepared and approved it, and whether it is current or superseded (F22). Opening a record shows the whole of it — the facts of approval, the method version, `k` and rounding, the platform's balance, each capability's minimum and proposed rates beside the arithmetic that produced them, the cost and income lines behind the totals, and the reasons given — **read from the sealed snapshot, not recalculated**, so a later method version, a new `k` or an edited row cannot change it [N6, N7]; the stored hash is re-checked on every view and a damaged snapshot is reported rather than rendered. Custodians see their own records; approvers and administrators see every one, and can download the sealed PDF from the record. The snapshot reader now reads the cost lines it already stored |
| 1.7 | 23 Sep 2026 | **US-01 and US-02 built, for M4.** **US-01:** a new cycle can be started from one of the custodian's sealed records. It carries over the platform, unit and capabilities, starts the period the year after the old one ended, shows the old record's key figures alongside on the platform, rates and review steps, and keeps a reference to the record it replaces (F22). The old record is never written to. It reads as superseded once the replacement is sealed, and the replacement's snapshot (schema 1.3) and PDF name it with its hash. A new cycle whose platform already has a sealed record is asked whether it replaces it, and a record can be replaced by only one cycle at a time. **US-02:** a draft reopens from the overview at the step it was left on, shows when it was last edited and by whom, and the capacity and rates steps save what was typed when the custodian leaves by any link, not only by their buttons. The local database moves to `ric-costing-v8.db` |
| 1.6 | 21 Sep 2026 | **M2's open criteria completed.** M2 was marked met on 11 Sep while four of its stories were still open on GitHub, and an audit against [`user-stories.md`](../spec/user-stories.md) showed why. **US-03 / US-04:** costs now take the workbook's categories — directly incurred per capability, directly allocated and indirect (floor area × rate per m²) at platform level — and the costs step shows a running total per capability, each capability's allocated share labelled as allocated, and the reconciliation to the platform total; the even-split rule is stated on the screen, including that the workbook splits two categories one more way than this tool does. **US-07:** capacity is built from the machine (1,882.5 h) or staff (1,725 h) baseline, both now method configuration under N7, less five kinds of deduction each with a note, capped at FTE × 1,725 h where a person must be present, and itemised. **US-08:** the guide's "most significant assumption" warning is on the screen, the guide's prompts sit beside the assumptions field, and a forecast above capacity now needs a reason instead of being refused — the old test asserted the opposite of the story. The local database moves to `ric-costing-v7.db`. Three decisions are ours and are listed in the pull request for the team to confirm |
| 1.5 | 16 Sep 2026 | **Technical ownership split into five layers**, agreed at the [team meeting of 15 September](../meetings/team/2026-09-15-team-meeting.md): Chenxu You general backend, Wenmin Luo calculation, Dai Lam La La deployment, Yichen Zhao front end, Jaswanth Vericherla authentication. §3 rewritten around the layers with standing responsibilities kept beside them. §2's S4–S8 Build and Verify columns re-mapped; S1–S3 left as they happened. §5's server, CD, DNS and TLS rows move to Dai Lam La La, and two gates that were implicit get rows of their own — staging accounts (Jaswanth Vericherla) and migrations (Chenxu You). §6: migrations and the `CostingTool.Web` move to Chenxu You, A15 and the modelling decisions to Wenmin Luo; two rows added, for re-mapping the board and for an access-control test |
| 1.4 | 13 Sep 2026 | **M2 met**, S3 closed, S4 marked as the sprint in flight. US-16's renderer brought forward out of S5 as a spike, with [ADR-002](../decisions/adr-002-pdf-generation.md) recording why MigraDoc and why the document is built from the sealed snapshot rather than from the live rows. Four §6 rows added — the regenerated lockfiles, the `src/CostingTool.Web/` move that the third project now forces, the contradicting *PDF export* row in ADR-001, and the fact that **no minutes have been committed since 20 August** although §4 promises them weekly. The board row and Q8 are re-dated rather than quietly carried: the board is half-automated and half deliberately manual, and Q8 is unblocked because [`NOTICE`](../../NOTICE) now records the client's written confirmation instead of still waiting for it. §5's hosting decision is marked as missed, because every deployment row below it depends on an answer this repository does not hold |
| 1.3 | 2 Sep 2026 | Engine extracted and provably correct; M1 met two days early |
