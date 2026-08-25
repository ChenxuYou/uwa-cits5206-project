# Skills Audit

**Run:** 24 August 2026
**Method:** each member self-rates 1–5 against the six competencies this project actually needs,
then the team agrees the gaps and what is done about each
**Feeds:** [ADR-001](../decisions/adr-001-technology-stack.md) — the technology decision — and
[`risks.md`](risks.md) rows R1, R4 and R6

> ### ⚠ Ratings below are **provisional** and must be confirmed by each member
>
> They were drafted from contribution evidence in this repository — who wrote which pull
> request, who owns which document — so that the shape of the audit exists and the gaps can be
> discussed. **Each member should overwrite their own row with their own honest self-rating
> before this document is cited in a submission.** A self-assessment somebody else filled in is
> not a self-assessment.

**Scale.** 1 — never used it · 2 — tutorial level · 3 — can build with it, slowly, with
reference · 4 — comfortable and productive · 5 — could teach it.

---

## 1. The grid

| Member | C# / .NET | SQL & ORM | HTML / CSS | JavaScript / SPA frameworks | Git & PR workflow | Automated testing |
| --- | --- | --- | --- | --- | --- | --- |
| Chenxu You | 4 | 3 | 3 | 2 | 4 | 2 |
| Wenmin Luo | 4 | 3 | 3 | 2 | 4 | 2 |
| Yichen Zhao | 2 | 2 | 3 | 2 | 3 | 2 |
| Dai Lam La La | 2 | 2 | 2 | 2 | 3 | 2 |
| Jaswanth Vericherla | 2 | 2 | 2 | 2 | 3 | 2 |
| **Team ceiling** | **4** | **3** | **3** | **2** | **4** | **2** |
| **Team depth** (members at 3+) | **2** | **2** | **3** | **0** | **5** | **0** |

**Two readings come straight off this table.**

- **Depth is in C# and server-side web work; JavaScript depth is zero at 3+.** That is the single
  largest input to the technology decision, and it is why a SPA architecture was not chosen.
- **Automated testing has no member at 3 or above.** On a project whose entire value proposition
  is *the arithmetic can be trusted*, that is the most dangerous cell in the grid.

---

## 2. Gaps, and what is done about each

| # | Gap | Why it threatens *this* project | How it is addressed | Owner |
| --- | --- | --- | --- | --- |
| G1 | **Automated testing discipline** — nobody at 3+ | The product's claim is that its numbers can be defended in an FOI response. Untested arithmetic makes that claim empty | The golden-file test against the client's worked example is written **before** any screen and wired as a **CI merge gate**. Test ownership sits with a member who does not write the engine, so nobody signs off their own arithmetic. Property tests cover the boundaries: `U = 0`, negative inputs, income exceeding cost, rounding at the half-cent | Jaswanth Vericherla |
| G2 | **Deployment and operations** — no member has taken an application to a running server with TLS, DNS and a rollback path | A `localhost` demo is not a handover. Criterion 3 names software deployment explicitly | Deployment is a milestone with owners and dates, not an end-of-semester task. Hosting decided 9 Sep; staging live 28 Sep; the client uses it unaccompanied for two weeks before the final release | Chenxu You |
| G3 | **Translating a business method into a workflow** | This is where projects of this shape fail: a tool that is confidently wrong | Written source-precedence rule; every requirement carries a `[C]`/`[G]`/`[W]`/`[K]` marker; one owner for the guide-vs-calculator reconciliation instead of five parallel opinions | Dai Lam La La |
| G4 | **Costing/finance domain knowledge** — none of us is an accountant | Indirect cost recovery, non-variable income and FEC are not intuitive | The client stated we do not need to master the reasoning, only implement it faithfully. We ask rather than infer: five questions sent 17 Aug, all five answered in writing 20 Aug | Yichen Zhao |
| G5 | **JavaScript / SPA frameworks** — team depth zero at 3+ | Would be fatal under Option B | Designed around rather than closed: [ADR-001](../decisions/adr-001-technology-stack.md) chooses a server-rendered architecture. Any interactivity is progressive enhancement over working pages | Whole team |
| G6 | **Bus factor** — two of five write production code, and the team is already down one member | One person unavailable in week 9 stalls the build | Two owners for every critical area; the calculation engine is a plain library any member can read; every PR reviewed by a second member; no merge on the author's own approval | Whole team |
| G7 | **SQL / data modelling at 3, not 4** | Immutability and history are structural requirements, not policies | EF Core migrations reviewed by both developers; the sealed record stores full input and output **as JSON**, not as foreign keys to live rows, so a category renamed in 2028 cannot rewrite a 2026 record | Wenmin Luo |

---

## 3. What we are *not* claiming

The team is not strong at automated testing today, and this document does not pretend otherwise.
What it claims is narrower and checkable: **the one test that matters most is written first and
cannot be bypassed.** The client's worked example — $150,000 operating costs, $20,000 UWA
in-kind, $30,000 State support, 1,000 forecast hours → $100.00 / $162.00 / $202.50 per hour —
must reproduce to the cent, or the build does not merge.

Skills grow over eleven weeks. The mitigation that does not depend on that happening is the one
we rely on.

---

## Change log

| Version | Date | Change |
| --- | --- | --- |
| 1.0 | 24 Aug 2026 | First version. Overdue: this was scheduled for 15 August and did not happen; it is dated when it was actually run rather than when it was planned. Ratings drafted from contribution evidence and flagged as provisional pending each member's own self-rating |
