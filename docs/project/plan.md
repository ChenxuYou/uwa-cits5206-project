# Project Plan — 24 August to 13 October 2026

**Version:** 1.11 — 25 September 2026
**Owner:** Chenxu You
**Reviewed:** every Saturday team meeting
**Companions:** [`risks.md`](risks.md) · [`skills-audit.md`](skills-audit.md) ·
[`team.md`](team.md) · [ADR-001](../decisions/adr-001-technology-stack.md) ·
[ADR-002](../decisions/adr-002-pdf-generation.md)

> **The scope this plan delivers was signed by the client on 20 August 2026.** Nothing below is
> a proposal to the client; it is the sequence in which we build what they approved.

---

## 1. Milestones

| # | Milestone | Date | Done when |
| --- | --- | --- | --- |
| M0 | ✅ Assignment 1 submitted | **25 Aug 2026** | **Met.** `Group13-Project Spec and Plans.pdf` uploaded by one member on 25 Aug, every linked resource open to the facilitator |
| M1 | ✅ **Engine provably correct** | **4 Sep 2026** | **Met 2 Sep, two days early.** The client's worked example reproduces to the cent in `tests/CostingTool.Engine.Tests`, and `dotnet test` is a merge gate rather than a warning. The engine now sits in its own project with no package references, so the tests reach the arithmetic without EF or ASP.NET behind it |
| M2 | ✅ **Guided flow, validated server-side** | **11 Sep 2026** | **Met.** Costs, income, capacity and forecast utilisation are captured and validated server-side across `Start → Costs → Capacity → Rates → Review`, and every step loads through `RicPageModel`, so no step can forget an `Include` or an ownership filter. **Criteria completed 21 Sep:** an audit against [`user-stories.md`](../spec/user-stories.md) found US-03, US-04, US-07 and US-08 (#20, #22, #24, #25) still short of their criteria — the client's cost categories, platform floor area, a capacity built from a baseline, and a forecast above capacity warned about rather than blocked. See change log 1.6 |
| M3 | ✅ Rates, proposed rates and balance | **18 Sep 2026** | **Met 23 Sep, five days late.** Three rates per capability with the figures behind each; proposed rates and the resulting surplus or deficit. US-09 to US-13 (#26–#30) verified and closed, and the milestone closed on GitHub |
| M4 | ✅ **Vertical slice complete** | **25 Sep 2026** | **Met 24 Sep, a day early.** Sign in → create cycle → enter inputs → see rates → propose → justify → seal → export PDF → reopen, driven end to end in a browser. US-01, US-02 and US-14 to US-17 (#18, #19, #31–#34) built in #83 and #84, reviewed, verified and closed. One criterion is met differently from its wording: US-15 has the custodian confirm and the record sealed, while the tool has the custodian submit and the approver's approval seal it, as in US-20. This is recorded on #32 for the client to confirm |
| M5 | Staging live, client using it | **2 Oct 2026** | Deployed over HTTPS, seeded credentials replaced, the client reaches it unaccompanied |
| M6 | Release candidate, feature freeze | **9 Oct 2026** | Critical fixes only; full regression pass; evidence pack assembled |
| M7 | **Final release and handover** | **13 Oct 2026** | Tagged release deployed, handover notes written so UWA can rehost, final report submitted |

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
| S1 | 24 Aug | Backlog, ADR, engine extracted from the page models | US-18 (partial), engine refactor | 8 | Wenmin Luo | Jaswanth Vericherla |
| S2 | 31 Aug | **Engine provably right** — golden file, decimal, versioned config | US-09, US-18 | 16 | Wenmin Luo, Chenxu You | Jaswanth Vericherla |
| S3 ✅ | 7 Sep | Costs, income, capacity, forecast utilisation | US-03, US-04, US-06, US-07, US-08 | 26 | Wenmin Luo, Chenxu You (US-03, US-07) · Dai Lam La La (US-04) · Jaswanth Vericherla (US-06) · Yichen Zhao (US-08) | Jaswanth Vericherla — except US-06, verified by Chenxu You |
| S4 ◀ | 14 Sep | Rates, proposed rates, balance, justification | US-09, US-10, US-11, US-12, US-13 | 24 | Wenmin Luo (calculation) · Chenxu You (page models, persistence) · Yichen Zhao (screens) · Dai Lam La La (US-13, carried from before the split) | Jaswanth Vericherla — except US-13's screens, verified by Chenxu You |
| S5 ✅ | 21 Sep | Seal, PDF with workings, retrieval, supersession | US-14, US-15, US-16, US-17, US-01, US-02 | 26 | Chenxu You (seal, retrieval, supersession, US-01, US-02) · Wenmin Luo (PDF workings, US-17) · Yichen Zhao (review and approver screens) · Jaswanth Vericherla (who-sealed identity on the record) | Jaswanth Vericherla — except his own piece, verified by Chenxu You |
| S6 | 28 Sep | Identity hardening and deploy to staging | US-19, deployment | 10 | Jaswanth Vericherla (US-19, staging accounts) · Dai Lam La La (server, CD, TLS) | Chenxu You (US-19) · Jaswanth Vericherla (deployment) |
| S7 | 5 Oct | Stabilise — critical fixes only | — | — | Whoever owns the fix | Dai Lam La La |
| S8 | 12 Oct | Final release and handover | — | — | Dai Lam La La (release), Chenxu You (handover notes) | Whole team |

**S5's stories are done and M4 was met on 24 September**, a day early; M3 was met on the 23rd. **S6 is next**: US-19 and the deploy to staging (#15, #60), which gate M5 on 2 October. US-16's renderer was brought forward out of S5 into a spike in S4, because it was the only link in the M4 chain that nobody had built or costed — see [ADR-002](../decisions/adr-002-pdf-generation.md).

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
| **Chenxu You** | **General backend** | Page models and `RicPageModel`; services other than the calculation; data model and **EF Core migrations**; seal, retrieval and supersession; CI; repository structure, including the `src/CostingTool.Web/` move | ADRs; release management with Dai Lam La La; PDF assembly for deliverables |
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

## 5. Deployment

The largest question still open after the 15 August meeting, so it carries dates rather than
intentions. Detail in Assignment 1 §3.5.

| Stage | Owner | By |
| --- | --- | --- |
| Hosting decision with the client — UWA VM, the UWA domain already shared with us, or team-provisioned; who administers it; whether sign-in must use UWA accounts | Yichen Zhao, with Dai Lam La La on the technical options | ⚠️ **9 Sep — no answer recorded in this repository.** Every row below it depends on it, and M5 is 2 Oct. If it is not settled at the 19 Sep meeting it stops being a date and becomes a risk with a fallback: provision a team-held server ourselves and hand the client the migration path at handover |
| Provision and access | Dai Lam La La | 18 Sep |
| CI extended to CD, with a documented rollback | Dai Lam La La | ⚠️ **25 Sep — half done.** [`deploy/release.sh`](../../deploy/release.sh) releases a published build, backs the database up first and rolls back by itself when the new build does not answer; rehearsed on 25 Sep. Running it from CI is still to do |
| DNS, reverse proxy, TLS | Dai Lam La La | 30 Sep. Caddy configuration for both cases is in [`deploy/`](../../deploy/README.md): by IP address with Caddy's own certificate now, Let's Encrypt once a name resolves. **Plain `http://` cannot work** — the sign-in cookie is Secure outside Development |
| Staging accounts provisioned; no demo account reachable | Jaswanth Vericherla | 30 Sep |
| EF Core migrations in place — the schema can change without losing entered data | Chenxu You | ✅ **Done 25 Sep.** A baseline migration replaces `EnsureCreated`; CI fails when the model changes without one ([`src/README.md`](../../src/README.md#changing-the-data-model)) |
| Deployment testing — build, release, rollback, reachability | Jaswanth Vericherla, with Dai Lam La La | 2 Oct |
| Staging sign-off | Whole team | 5 Oct |
| Final release and handover pack | Dai Lam La La, Chenxu You | 13 Oct |

---

## 6. Open items carried into this plan

| # | Item | Owner | By |
| --- | --- | --- | --- |
| A14 | Confirm in writing whether in-tool approval routing is required in the core, or whether recording the approver is enough | Yichen Zhao | ⚠️ **Missed 26 Aug — re-dated to 5 Sep.** No issue was ever opened for it, which is why it passed unnoticed; open one first |
| A15 | Report guide-vs-calculator divergences to the client as they surface. The commercial-rate divergence is already answered; a **line-by-line reconciliation of the calculator is deferred to the next cycle**, once the engine exists to compare against | Wenmin Luo (from 15 Sep; was Dai Lam La La) | Rolling; first pass after **M1**, 4 Sep |
| A17 | Give "the sealed PDF shows the calculator's workings" a requirement ID and a story estimate | Wenmin Luo | ⚠️ **Missed 26 Aug — re-dated to 5 Sep.** Tracked as issue #10, still open. It gates a Must story's estimate, so it cannot slip past the S4 planning on 14 Sep |
| — | ~~**Create the GitHub Projects board** — populated from the eighteen Must stories. Carried out of Assignment 1 as the one artefact that has to be made by hand~~ | Wenmin Luo, Chenxu You | ✅ Done 1 Sep. Board #2, public and linked to the repository; 25 story issues carried their points, priority and sprint across. Built by [`scripts/seed-project-board.py`](../../scripts/seed-project-board.py), so it can be rebuilt from `user-stories.md` rather than by hand |
| — | **Finish the board by hand** — rename Status `Todo` to `Backlog` and add `Review`; add a board view grouped by Status; add issues #10, #21 and #60, which are not stories and so are not in `user-stories.md`; enable the three Workflows that move cards without anyone dragging them | Chenxu You | ⚠️ **Missed 5 Sep — re-dated to 19 Sep.** Half of it is now automated: [`scripts/finish-project-board.py`](../../scripts/finish-project-board.py) adds the three issues and then audits the board against this row, printing what is still outstanding. The Status rename stays by hand **deliberately** — the GraphQL mutation that edits single-select options replaces the whole option list and clears every card's Status, so the two-minute job in the web UI is the safe one |
| — | **Write up the 24 July and 5 August meetings.** Carried out of Assignment 1; the minutes rule applies from here on, and the 24 July record is a raw transcript, so what goes in `docs/meetings/` is written minutes | Jaswanth Vericherla | 5 August ✅ written up 24 Sep ([minutes](../meetings/facilitator/2026-08-05-facilitator-meeting.md)). ⚠️ 24 July still outstanding |
| — | ~~Add Option F to [`architecture.md` §8](../spec/architecture.md#8-options-assessed) and re-run the weighted comparison~~ | Chenxu You | ✅ Done 24 Aug |
| — | ~~Stop tracking `src/bin/` and `src/obj/`~~ | Wenmin Luo | ✅ Done 24 Aug |
| — | ~~**Replace `EnsureCreated()` with EF Core migrations.**~~ | Chenxu You | ✅ **Done 25 Sep.** Baseline migration checked against the model by `MigrationTests`; a database made by `EnsureCreated` is refused at start-up with the reason |
| — | **Confirm two modelling decisions that carry no source marker** — whether a multi-year cost profile is averaged into one annual figure, and whether the indirect-cost uplift is retained by the platform in the revenue projection. Both surfaced on 2 Sep while the engine was extracted; both are commented in the code as ours rather than the client's | Wenmin Luo (from 15 Sep; was Dai Lam La La), put to the client by Yichen Zhao | With the next question batch |
| — | **Commit the regenerated lockfiles.** `src/CostingTool.Pdf` adds a package reference, so `dotnet restore --force-evaluate` must be run once and both `packages.lock.json` files committed — CI restores against them | Chenxu You | 15 Sep |
| — | **Move the web application to `src/CostingTool.Web/`.** `src/CostingTool.csproj` globs `**/*.cs` from its own directory, so every sibling project underneath it needs four `Remove` lines to avoid CS0436. The comment there said this was worth doing "before a third project is added"; `CostingTool.Pdf` is the third project. It is a folder move plus three path edits, and it is cheapest now, before the deploy sprint | Chenxu You (from 15 Sep; was Wenmin Luo) | 26 Sep |
| — | **Amend the ADR-001 stack table's *PDF export* row**, which still says "server-side HTML → PDF". [ADR-002](../decisions/adr-002-pdf-generation.md) decided otherwise and says why; two decision records that contradict each other in a reader's hands are worse than one | Chenxu You | 15 Sep |
| — | **Minutes have not been committed since 20 August.** §4 promises a Saturday review with minutes inside 24 hours; four Saturdays have passed without one. The cadence is either kept or the plan stops claiming it — this row exists so the choice is made deliberately at the next meeting | Jaswanth Vericherla | ✅ Gap closed 24 Sep: six records written up (5 Aug, 19 Aug, 22 Aug, 16 Sep, 22 Sep, 23 Sep), indexed in [`docs/meetings/`](../meetings/README.md). Whether the Saturday cadence is kept is still for the next meeting |
| — | **Re-map the board to the five layers.** Reassign open issues to their layer's owner and check the Build column in §2 against the assignees, per the [15 September minutes](../meetings/team/2026-09-15-team-meeting.md) | Chenxu You | 19 Sep |
| — | ~~**A test that a custodian cannot open another platform's record.**~~ | Jaswanth Vericherla | ✅ **Already covered** — `SignInTests.ACustodianCannotOpenAnotherCustodiansCycle` and `AMissingCycleAndSomeoneElsesCycleAnswerTheSameWay`, and `SealedRecordsTests` for the register. Found stale in the 25 Sep audit |
| Q8 | **Repository licence.** Unblocked by the client's written ownership confirmation of 20 August; [`NOTICE`](../../NOTICE) §1 and §2 now record that confirmation rather than still waiting for it (they were three weeks stale, and §1 still pointed at `docs/requirements.md`). What is left is agreeing the licence text with UWA — a licence granted by one joint owner alone may not be effective, so the team does not write one unilaterally | Chenxu You | 13 Oct |

---

## 7. Deliberately next cycle, not this one

Recorded so that neither is quietly forgotten and neither quietly becomes this semester's work.

| # | Item | Why it waits |
| --- | --- | --- |
| 1 | **Line-by-line reconciliation of the client's calculator** against the engine, with divergences reported to the client as they asked on 20 August | The engine is what you reconcile *against*. After M1 it is a matter of running both over the same inputs; before M1 it is hand work that would have to be redone |
| 2 | **UWA single sign-on** | Treated as a system integration, which the signed scope defers. Local sign-in sits behind an SSO-shaped seam so it can be swapped |
| 3 | **HR-system integration for staff roles** — raised by the client on 20 August | Raised, not accepted. It is the class of integration the signed scope defers and would need something traded out |
| 4 | **Writing records directly into Content Manager (TRIM)** | Out of scope as stated: the custodian downloads the PDF and files it. Only becomes work if the client asks the tool to write to TRIM |

---

## 8. Change log

| Version | Date | Change |
| --- | --- | --- |
| 1.11 | 25 Sep 2026 | **Audit of 25 September acted on, for M5.** Three critical findings fixed: the schema moves to **EF Core migrations** (C1), so a deployment can no longer lose the client's data or break on a new column; a **concurrency stamp** on each cycle stops two approval requests both sealing or returning it (C3); and a [`deploy/`](../../deploy/README.md) kit puts the application behind Caddy over HTTPS (C2) with a daily backup, a tested restore and a release script that rolls back. Also: data-protection keys kept beside the database, an absolute database path required outside Development, only a local proxy trusted, anti-framing headers, a sign-in rate limit per address, and a password an administrator set replaced at first sign-in. §5 rows re-dated accordingly; the access-control test row closed as already covered |
| 1.10 | 24 Sep 2026 | **M4 met, a day early**, and **M3 marked met** (23 Sep), which this plan still showed as on track. The vertical slice was driven end to end in a browser, and #18, #19 and #31–#34 were closed against their criteria. The one criterion met differently from its wording, who seals under US-15, is recorded on #32 for the client to confirm, along with the other decisions listed in #83 and #84. S5 marked done; S6, US-19 and the deploy to staging, is next |
| 1.9 | 23 Sep 2026 | **US-14, US-15 and US-16 completed, for M4.** **US-14:** the review page lists everything still missing when it opens — capacity, forecast, utilisation assumptions, proposed rates, a justification where rates differ or forecast a deficit — each linked to the step that supplies it, and submission is refused on that same list (`SubmissionChecks`, shared with the rates step). The page now shows every input by step, each with a link back, and both rate sets with the variance. **US-15:** the approver's confirmation names the consequence; a sealed record is refused any change or deletion at the database, not only by each page; and a sealed record's figures are read from its snapshot on every page, never recalculated — recalculation had fallen back to today's method when the sealed version was not a stored row. **US-16:** snapshot schema 1.4 adds who sealed the record and the capacity inputs; the PDF prints who sealed it, the variance beside each rate, the capacity build-up, every cost and income line, and the guide's retention requirement. Older snapshots still render |
| 1.8 | 23 Sep 2026 | **US-17 built, for M4.** A read-only **Sealed records** register lists every sealed record by platform and pricing period, with its sealed date, method version, who prepared and approved it, and whether it is current or superseded (F22). Opening a record shows the whole of it — the facts of approval, the method version, `k` and rounding, the platform's balance, each capability's minimum and proposed rates beside the arithmetic that produced them, the cost and income lines behind the totals, and the reasons given — **read from the sealed snapshot, not recalculated**, so a later method version, a new `k` or an edited row cannot change it [N6, N7]; the stored hash is re-checked on every view and a damaged snapshot is reported rather than rendered. Custodians see their own records; approvers and administrators see every one, and can download the sealed PDF from the record. The snapshot reader now reads the cost lines it already stored |
| 1.7 | 23 Sep 2026 | **US-01 and US-02 built, for M4.** **US-01:** a new cycle can be started from one of the custodian's sealed records. It carries over the platform, unit and capabilities, starts the period the year after the old one ended, shows the old record's key figures alongside on the platform, rates and review steps, and keeps a reference to the record it replaces (F22). The old record is never written to. It reads as superseded once the replacement is sealed, and the replacement's snapshot (schema 1.3) and PDF name it with its hash. A new cycle whose platform already has a sealed record is asked whether it replaces it, and a record can be replaced by only one cycle at a time. **US-02:** a draft reopens from the overview at the step it was left on, shows when it was last edited and by whom, and the capacity and rates steps save what was typed when the custodian leaves by any link, not only by their buttons. The local database moves to `ric-costing-v8.db` |
| 1.6 | 21 Sep 2026 | **M2's open criteria completed.** M2 was marked met on 11 Sep while four of its stories were still open on GitHub, and an audit against [`user-stories.md`](../spec/user-stories.md) showed why. **US-03 / US-04:** costs now take the workbook's categories — directly incurred per capability, directly allocated and indirect (floor area × rate per m²) at platform level — and the costs step shows a running total per capability, each capability's allocated share labelled as allocated, and the reconciliation to the platform total; the even-split rule is stated on the screen, including that the workbook splits two categories one more way than this tool does. **US-07:** capacity is built from the machine (1,882.5 h) or staff (1,725 h) baseline, both now method configuration under N7, less five kinds of deduction each with a note, capped at FTE × 1,725 h where a person must be present, and itemised. **US-08:** the guide's "most significant assumption" warning is on the screen, the guide's prompts sit beside the assumptions field, and a forecast above capacity now needs a reason instead of being refused — the old test asserted the opposite of the story. The local database moves to `ric-costing-v7.db`. Three decisions are ours and are listed in the pull request for the team to confirm |
| 1.5 | 16 Sep 2026 | **Technical ownership split into five layers**, agreed at the [team meeting of 15 September](../meetings/team/2026-09-15-team-meeting.md): Chenxu You general backend, Wenmin Luo calculation, Dai Lam La La deployment, Yichen Zhao front end, Jaswanth Vericherla authentication. §3 rewritten around the layers with standing responsibilities kept beside them. §2's S4–S8 Build and Verify columns re-mapped; S1–S3 left as they happened. §5's server, CD, DNS and TLS rows move to Dai Lam La La, and two gates that were implicit get rows of their own — staging accounts (Jaswanth Vericherla) and migrations (Chenxu You). §6: migrations and the `CostingTool.Web` move to Chenxu You, A15 and the modelling decisions to Wenmin Luo; two rows added, for re-mapping the board and for an access-control test |
| 1.4 | 13 Sep 2026 | **M2 met**, S3 closed, S4 marked as the sprint in flight. US-16's renderer brought forward out of S5 as a spike, with [ADR-002](../decisions/adr-002-pdf-generation.md) recording why MigraDoc and why the document is built from the sealed snapshot rather than from the live rows. Four §6 rows added — the regenerated lockfiles, the `src/CostingTool.Web/` move that the third project now forces, the contradicting *PDF export* row in ADR-001, and the fact that **no minutes have been committed since 20 August** although §4 promises them weekly. The board row and Q8 are re-dated rather than quietly carried: the board is half-automated and half deliberately manual, and Q8 is unblocked because [`NOTICE`](../../NOTICE) now records the client's written confirmation instead of still waiting for it. §5's hosting decision is marked as missed, because every deployment row below it depends on an answer this repository does not hold |
| 1.3 | 2 Sep 2026 | Engine extracted and provably correct; M1 met two days early |
