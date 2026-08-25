# Project Plan — 24 August to 13 October 2026

**Version:** 1.0 — 24 August 2026
**Owner:** Chenxu You
**Reviewed:** every Saturday team meeting
**Companions:** [`risks.md`](risks.md) · [`skills-audit.md`](skills-audit.md) ·
[`team.md`](team.md) · [ADR-001](../decisions/adr-001-technology-stack.md)

> **The scope this plan delivers was signed by the client on 20 August 2026.** Nothing below is
> a proposal to the client; it is the sequence in which we build what they approved.

---

## 1. Milestones

| # | Milestone | Date | Done when |
| --- | --- | --- | --- |
| M0 | Assignment 1 submitted | **25 Aug 2026** | One PDF uploaded by one member, every linked resource open to the facilitator |
| M1 | **Engine provably correct** | **4 Sep 2026** | The client's worked example reproduces to the cent, as a CI merge gate. Written before any screen |
| M2 | Guided flow, validated server-side | **11 Sep 2026** | Costs, income, capacity and mandatory forecast utilisation captured and validated |
| M3 | Rates, proposed rates and balance | **18 Sep 2026** | Three rates per capability with the figures behind each; proposed rates and the resulting surplus or deficit |
| M4 | **Vertical slice complete** | **25 Sep 2026** | Sign in → create cycle → enter inputs → see rates → propose → justify → seal → export PDF → reopen |
| M5 | Staging live, client using it | **2 Oct 2026** | Deployed over HTTPS, seeded credentials replaced, the client reaches it unaccompanied |
| M6 | Release candidate, feature freeze | **9 Oct 2026** | Critical fixes only; full regression pass; evidence pack assembled |
| M7 | **Final release and handover** | **13 Oct 2026** | Tagged release deployed, handover notes written so UWA can rehost, final report submitted |

**Fallback trigger.** If M4 has not been met by the end of week 8, we cut stretch scope — we do
not change stack. The cut order is fixed in advance: dashboard → price-change communication →
benchmarking record → replacement reserve → salary pre-fill → in-tool approval routing.

---

## 2. Sprints and story assignment

One-week sprints, Saturday to Saturday. The MVP is **eighteen Must stories, 110 points**
([`user-stories.md` §5](../spec/user-stories.md#5-mvp-definition)). Development capacity is two
members; verification and research are the other three, so points below are assigned against the
developers with a named verifier on each.

| Sprint | Week commencing | Goal | Stories | Pts | Build | Verify |
| --- | --- | --- | --- | --- | --- | --- |
| S1 | 24 Aug | Backlog, ADR, engine extracted from the page models | US-18 (partial), engine refactor | 8 | Wenmin Luo | Jaswanth Vericherla |
| S2 | 31 Aug | **Engine provably right** — golden file, decimal, versioned config | US-09, US-18 | 16 | Wenmin Luo, Chenxu You | Jaswanth Vericherla |
| S3 | 7 Sep | Costs, income, capacity, forecast utilisation | US-03, US-04, US-06, US-07, US-08 | 26 | Chenxu You (screens), Wenmin Luo (validation) | Jaswanth Vericherla |
| S4 | 14 Sep | Rates, proposed rates, balance, justification | US-09, US-10, US-11, US-12, US-13 | 24 | Wenmin Luo (calc), Chenxu You (screens) | Dai Lam La La |
| S5 | 21 Sep | Seal, PDF with workings, retrieval, supersession | US-14, US-15, US-16, US-17, US-01, US-02 | 26 | Chenxu You (seal), Wenmin Luo (PDF) | Jaswanth Vericherla |
| S6 | 28 Sep | Identity hardening and deploy to staging | US-19, deployment | 10 | Chenxu You (CD), Wenmin Luo (server) | Jaswanth Vericherla |
| S7 | 5 Oct | Stabilise — critical fixes only | — | — | Both | Dai Lam La La |
| S8 | 12 Oct | Final release and handover | — | — | Both | Whole team |

**Story points are re-estimated at each Saturday meeting.** The table above is the plan of
record; the board is the live state.

---

## 3. Responsibilities

| Member | Area | Standing responsibilities |
| --- | --- | --- |
| **Yichen Zhao** | Client liaison, sprint coordination | Single point of client contact; books meetings; weekly demo summary; feedback into issues; decision log; facilitator access to every linked resource |
| **Chenxu You** | Application, workflow, release | Razor Pages workflow, authentication and approvals; CI/CD; release management; ADRs; PDF assembly for deliverables |
| **Wenmin Luo** | Engine, data, infrastructure | Calculation engine; data model and migrations; server provisioning, DNS and reverse proxy; dependency scanning |
| **Dai Lam La La** | Costing logic, risk | Reading of the client's guide and the assumptions log; [`risks.md`](risks.md); anything raised with the client where their material needs interpretation; final read-through of anything submitted |
| **Jaswanth Vericherla** | Verification, documentation | Test scenarios and golden-file fixtures; verification of each increment; meeting minutes; link and access checking |

**Nothing merges on its author's approval.** Every pull request is reviewed by a second member,
and the person who writes a calculation does not sign off its arithmetic.

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
| Hosting decision with the client — UWA VM, the UWA domain already shared with us, or team-provisioned; who administers it; whether sign-in must use UWA accounts | Yichen Zhao | 9 Sep |
| Provision and access | Wenmin Luo | 18 Sep |
| CI extended to CD, with a documented rollback | Chenxu You | 25 Sep |
| DNS, reverse proxy, TLS | Wenmin Luo, Chenxu You | 30 Sep |
| Deployment testing — build, release, rollback, reachability; seeded credentials replaced | Jaswanth Vericherla | 2 Oct |
| Staging sign-off | Whole team | 5 Oct |
| Final release and handover pack | Chenxu You, Wenmin Luo | 13 Oct |

---

## 6. Open items carried into this plan

| # | Item | Owner | By |
| --- | --- | --- | --- |
| A14 | Confirm in writing whether in-tool approval routing is required in the core, or whether recording the approver is enough | Yichen Zhao | 26 Aug |
| A15 | Report guide-vs-calculator divergences to the client as they surface. The commercial-rate divergence is already answered; a **line-by-line reconciliation of the calculator is deferred to the next cycle**, once the engine exists to compare against | Dai Lam La La | Rolling; first pass after **M1**, 4 Sep |
| A17 | Give "the sealed PDF shows the calculator's workings" a requirement ID and a story estimate | Wenmin Luo | 26 Aug |
| — | Add Option F to [`architecture.md` §8](../spec/architecture.md#8-options-assessed) and re-run the weighted comparison | Chenxu You | 26 Aug |
| — | Stop tracking `src/bin/` and `src/obj/` | Wenmin Luo | Before the next code commit |
| Q8 | Repository licence — unblocked by the client's written ownership confirmation; closes at handover | Chenxu You | 13 Oct |

---

## 7. Deliberately next cycle, not this one

Recorded so that neither is quietly forgotten and neither quietly becomes this semester's work.

| # | Item | Why it waits |
| --- | --- | --- |
| 1 | **Line-by-line reconciliation of the client's calculator** against the engine, with divergences reported to the client as they asked on 20 August | The engine is what you reconcile *against*. After M1 it is a matter of running both over the same inputs; before M1 it is hand work that would have to be redone |
| 2 | **UWA single sign-on** | Treated as a system integration, which the signed scope defers. Local sign-in sits behind an SSO-shaped seam so it can be swapped |
| 3 | **HR-system integration for staff roles** — raised by the client on 20 August | Raised, not accepted. It is the class of integration the signed scope defers and would need something traded out |
| 4 | **Writing records directly into Content Manager (TRIM)** | Out of scope as stated: the custodian downloads the PDF and files it. Only becomes work if the client asks the tool to write to TRIM |
