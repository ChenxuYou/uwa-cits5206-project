# Project Plan — 24 August to 13 October 2026

**Version:** 1.5 — 16 September 2026
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
| M2 | ✅ **Guided flow, validated server-side** | **11 Sep 2026** | **Met.** Costs, income, capacity and forecast utilisation are captured and validated server-side across `Start → Costs → Capacity → Rates → Review`, and every step loads through `RicPageModel`, so no step can forget an `Include` or an ownership filter |
| M3 | Rates, proposed rates and balance | **18 Sep 2026** | Three rates per capability with the figures behind each; proposed rates and the resulting surplus or deficit. **On track** — the screens exist; what S4 adds is US-10, US-12 and US-13 |
| M4 | **Vertical slice complete** | **25 Sep 2026** | Sign in → create cycle → enter inputs → see rates → propose → justify → seal → export PDF → reopen. **The export was the one unbuilt link and is now spiked** — [ADR-002](../decisions/adr-002-pdf-generation.md), `src/CostingTool.Pdf` |
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
agreed at the [team meeting of 15 September](../meetings/2026-09-15-team-meeting.md) and set out
in §3: general backend, calculation, deployment, front end and authentication. S1–S3 below are
left as they happened; S4 onwards is written against the layers. What does not change is the
review rule: the person who writes a story is never the person who verifies it.

| Sprint | Week commencing | Goal | Stories | Pts | Build | Verify |
| --- | --- | --- | --- | --- | --- | --- |
| S1 | 24 Aug | Backlog, ADR, engine extracted from the page models | US-18 (partial), engine refactor | 8 | Wenmin Luo | Jaswanth Vericherla |
| S2 | 31 Aug | **Engine provably right** — golden file, decimal, versioned config | US-09, US-18 | 16 | Wenmin Luo, Chenxu You | Jaswanth Vericherla |
| S3 ✅ | 7 Sep | Costs, income, capacity, forecast utilisation | US-03, US-04, US-06, US-07, US-08 | 26 | Wenmin Luo, Chenxu You (US-03, US-07) · Dai Lam La La (US-04) · Jaswanth Vericherla (US-06) · Yichen Zhao (US-08) | Jaswanth Vericherla — except US-06, verified by Chenxu You |
| S4 ◀ | 14 Sep | Rates, proposed rates, balance, justification | US-09, US-10, US-11, US-12, US-13 | 24 | Wenmin Luo (calculation) · Chenxu You (page models, persistence) · Yichen Zhao (screens) · Dai Lam La La (US-13, carried from before the split) | Jaswanth Vericherla — except US-13's screens, verified by Chenxu You |
| S5 | 21 Sep | Seal, PDF with workings, retrieval, supersession | US-14, US-15, US-16, US-17, US-01, US-02 | 26 | Chenxu You (seal, retrieval, supersession, US-01, US-02) · Wenmin Luo (PDF workings, US-17) · Yichen Zhao (review and approver screens) · Jaswanth Vericherla (who-sealed identity on the record) | Jaswanth Vericherla — except his own piece, verified by Chenxu You |
| S6 | 28 Sep | Identity hardening and deploy to staging | US-19, deployment | 10 | Jaswanth Vericherla (US-19, staging accounts) · Dai Lam La La (server, CD, TLS) | Chenxu You (US-19) · Jaswanth Vericherla (deployment) |
| S7 | 5 Oct | Stabilise — critical fixes only | — | — | Whoever owns the fix | Dai Lam La La |
| S8 | 12 Oct | Final release and handover | — | — | Dai Lam La La (release), Chenxu You (handover notes) | Whole team |

**S3 closed on 13 September**; M2 was met on the 11th. **S4 is the sprint in flight**, re-mapped to the layers on 15 September. US-16's renderer was brought forward out of S5 into a spike this week, because it was the only link in the M4 chain that nobody had built or costed — see [ADR-002](../decisions/adr-002-pdf-generation.md). What stays in S5 is wiring it to the approver's side, the supersession watermark question, and review.

**Story points are re-estimated at each Saturday meeting.** The table above is the plan of
record; the [board](https://github.com/users/ChenxuYou/projects/2) is the live state, and the
Build column is written to match the assignees on it.

---

## 3. Responsibilities

**From 15 September 2026 each member owns one technical layer** ([minutes](../meetings/2026-09-15-team-meeting.md)).
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
| CI extended to CD, with a documented rollback | Dai Lam La La | 25 Sep |
| DNS, reverse proxy, TLS | Dai Lam La La | 30 Sep |
| Staging accounts provisioned; no demo account reachable | Jaswanth Vericherla | 30 Sep |
| EF Core migrations in place — the schema can change without losing entered data | Chenxu You | 30 Sep |
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
| — | **Write up the 24 July and 5 August meetings.** Carried out of Assignment 1; the minutes rule applies from here on, and the 24 July record is a raw transcript, so what goes in `docs/meetings/` is written minutes | Jaswanth Vericherla | ⚠️ **Missed 29 Aug — re-dated to 5 Sep** |
| — | ~~Add Option F to [`architecture.md` §8](../spec/architecture.md#8-options-assessed) and re-run the weighted comparison~~ | Chenxu You | ✅ Done 24 Aug |
| — | ~~Stop tracking `src/bin/` and `src/obj/`~~ | Wenmin Luo | ✅ Done 24 Aug |
| — | **Replace `EnsureCreated()` with EF Core migrations.** The schema cannot currently evolve, so a model change costs the local database. Harmless now; data loss once the client is entering figures on staging, which makes it a gate on M5 rather than a tidy-up | Chenxu You (from 15 Sep; was Wenmin Luo) | 30 Sep — a gate on **M5** |
| — | **Confirm two modelling decisions that carry no source marker** — whether a multi-year cost profile is averaged into one annual figure, and whether the indirect-cost uplift is retained by the platform in the revenue projection. Both surfaced on 2 Sep while the engine was extracted; both are commented in the code as ours rather than the client's | Wenmin Luo (from 15 Sep; was Dai Lam La La), put to the client by Yichen Zhao | With the next question batch |
| — | **Commit the regenerated lockfiles.** `src/CostingTool.Pdf` adds a package reference, so `dotnet restore --force-evaluate` must be run once and both `packages.lock.json` files committed — CI restores against them | Chenxu You | 15 Sep |
| — | **Move the web application to `src/CostingTool.Web/`.** `src/CostingTool.csproj` globs `**/*.cs` from its own directory, so every sibling project underneath it needs four `Remove` lines to avoid CS0436. The comment there said this was worth doing "before a third project is added"; `CostingTool.Pdf` is the third project. It is a folder move plus three path edits, and it is cheapest now, before the deploy sprint | Chenxu You (from 15 Sep; was Wenmin Luo) | 26 Sep |
| — | **Amend the ADR-001 stack table's *PDF export* row**, which still says "server-side HTML → PDF". [ADR-002](../decisions/adr-002-pdf-generation.md) decided otherwise and says why; two decision records that contradict each other in a reader's hands are worse than one | Chenxu You | 15 Sep |
| — | **Minutes have not been committed since 20 August.** §4 promises a Saturday review with minutes inside 24 hours; four Saturdays have passed without one. The cadence is either kept or the plan stops claiming it — this row exists so the choice is made deliberately at the next meeting | Jaswanth Vericherla | 19 Sep |
| — | **Re-map the board to the five layers.** Reassign open issues to their layer's owner and check the Build column in §2 against the assignees, per the [15 September minutes](../meetings/2026-09-15-team-meeting.md) | Chenxu You | 19 Sep |
| — | **A test that a custodian cannot open another platform's record.** Access control is enforced in `Program.cs` policies and `RicPageModel`, but nothing automated proves it ([`risks.md`](risks.md) R12) | Jaswanth Vericherla | 2 Oct (**M5**) |
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
| 1.5 | 16 Sep 2026 | **Technical ownership split into five layers**, agreed at the [team meeting of 15 September](../meetings/2026-09-15-team-meeting.md): Chenxu You general backend, Wenmin Luo calculation, Dai Lam La La deployment, Yichen Zhao front end, Jaswanth Vericherla authentication. §3 rewritten around the layers with standing responsibilities kept beside them. §2's S4–S8 Build and Verify columns re-mapped; S1–S3 left as they happened. §5's server, CD, DNS and TLS rows move to Dai Lam La La, and two gates that were implicit get rows of their own — staging accounts (Jaswanth Vericherla) and migrations (Chenxu You). §6: migrations and the `CostingTool.Web` move to Chenxu You, A15 and the modelling decisions to Wenmin Luo; two rows added, for re-mapping the board and for an access-control test |
| 1.4 | 13 Sep 2026 | **M2 met**, S3 closed, S4 marked as the sprint in flight. US-16's renderer brought forward out of S5 as a spike, with [ADR-002](../decisions/adr-002-pdf-generation.md) recording why MigraDoc and why the document is built from the sealed snapshot rather than from the live rows. Four §6 rows added — the regenerated lockfiles, the `src/CostingTool.Web/` move that the third project now forces, the contradicting *PDF export* row in ADR-001, and the fact that **no minutes have been committed since 20 August** although §4 promises them weekly. The board row and Q8 are re-dated rather than quietly carried: the board is half-automated and half deliberately manual, and Q8 is unblocked because [`NOTICE`](../../NOTICE) now records the client's written confirmation instead of still waiting for it. §5's hosting decision is marked as missed, because every deployment row below it depends on an answer this repository does not hold |
| 1.3 | 2 Sep 2026 | Engine extracted and provably correct; M1 met two days early |
