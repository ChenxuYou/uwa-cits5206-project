# Requirements — Our Understanding of What the Client Needs

**Project:** Research Infrastructure Costing & Pricing Tool
**Client:** UWA Research Infrastructure
**Unit:** CITS5206 Professional Computing, The University of Western Australia
**Status:** v2.4 — 22 August 2026. **The scope this document describes was signed off by the
client on 20 August 2026**; the document itself has not been reviewed line by line by them
**Primary sources:** the client's own documents (see *Source precedence* below), supported by the
[client kickoff minutes, 29 July 2026](../meetings/2026-07-29-client-kickoff.md)

> **How to read this document.** It records *our* understanding, not the client's words.
> Companion documents: [user stories](user-stories.md) and [architecture](architecture.md).

### Source precedence

Three kinds of source describe the costing method, and they do not always agree. When they
conflict, the order below decides, and the disagreement is recorded as an open question rather
than silently resolved.

| Rank | Source | Marked | Why it ranks here |
| --- | --- | --- | --- |
| 1 | **The client's written answers to our questions** — [20 August 2026](../client/communication-history/2026-08-20-client-meeting/client-answers-to-the-five-questions.md), and the [signed scope statement](../client/communication-history/2026-08-20-client-meeting/project-scope-summary-signed.pdf) | **[C]** with the date | The client answering *our* question, in their own words, in writing, about this project. It outranks the guide because the guide is a general policy and this is a decision about how the policy applies here |
| 2 | **`01-context-costing-pricing-research-infrastructure.docx`** — the client's costing & pricing guide | **[G]** with the step number | The client's normative policy document. It states its own scope: "a consistent methodology for developing, reviewing and approving sustainable charge-out rates **across the University**" |
| 3 | **`02-template-ric-cost-calculator.xlsx`** — the working calculator | **[W]** with the sheet and cell | A reference *implementation* of the guide. Authoritative for structure and data shape; **not** authoritative where its arithmetic departs from the guide, because it contains demonstrable formula defects (§2) |
| 4 | [Kickoff minutes, 29 July 2026](../meetings/2026-07-29-client-kickoff.md) · [client meeting, 20 August 2026](../meetings/2026-08-20-client-meeting.md) | **[K]** with the section | Our own record of what was said in a room. Useful for intent and priority, unreliable for figures — and where our minutes and a **[C]** answer differ, **[C]** wins |

Client files live in `reference/client/` and are **not committed** — see
[`reference/client/README.md`](../../reference/client/README.md). Everything quoted from them here
is transcribed by us, with the source named.

**Anything not carrying a [C], [G], [W] or [K] marker is an inference of ours** and is listed in
§8 as an assumption to be confirmed.

**What the client has signed off, and what they have not.** On 20 August 2026 the client signed
the [scope statement](../client/communication-history/2026-08-20-client-meeting/project-scope-summary-signed.pdf)
— two confirmations, scope and ownership — and answered all five open questions in writing. That
is a sign-off on **the scope this document describes**, not a line-by-line review of the document
itself. Statements marked **[C]** are the client's; the rest still needs reading with the
precedence rule above.

---

## 1. Context

UWA operates research infrastructure — electron microscopes, a human MRI, radio astronomy
telescopes, drones used for plant phenotyping — that is expensive to buy and expensive to
run. Part of that cost is recovered from the researchers who buy time on it, who in turn
budget for it in their grant applications. **[K §1]**

The client's guide states the purpose directly:

> By understanding the full cost of operating a capability, estimating realistic utilisation,
> and considering market conditions, staff responsible for the management of UWA research
> infrastructure can establish pricing that supports long-term sustainability while maintaining
> access to world-class research infrastructure for the research community. **[G, Purpose]**

Two constraints shape everything:

- **Public funding demands transparency.** Much of the infrastructure is paid for with state and federal money, so how a price was arrived at must be explainable and defensible. **[K §1]**
- **The method must be identical across the institution.** The guide "applies to all UWA research infrastructure capabilities that charge for access to equipment, facilities, technical services or specialist expertise" and exists to provide one consistent methodology. **[G, Purpose]**

The objective is **sustainability, not profit** — enough per unit of time that the capability
remains fundable and continues to operate **[K §1]**. Note the refinement the guide adds:
costing "should focus on operating costs rather than historical capital expenditure. **However,
custodians should consider future replacement requirements**" **[G, Step 1]**. Recovering
*historical* capital is out; a *forward* replacement reserve is explicitly in — see §4 Step 1.

The client noted that the Australian research funding environment has shifted, which is why
every university is currently uplifting its research infrastructure frameworks. **[K §1]**

## 2. The problem

The costing logic already exists and works. It lives in an Excel workbook, and the client's
own assessment of it is the problem statement:

> Our problem with this is it's functional, but nobody can really use it because it's easy to
> break.
>
> — UWA Research Infrastructure, 29 July 2026 **[K §3]**

Elaborated over the meeting **[K §3]**:

| Failure | Consequence |
| --- | --- |
| It is complex, and does not look as complex as it is | Users underestimate it and make errors they do not notice |
| Sharing the file hands over control of the formulas | A recipient can delete a formula and silently break the calculation |
| It is not foolproof | Results cannot be trusted without re-checking the workbook itself |
| It is not user friendly | Busy custodians avoid or postpone the exercise |
| The calculation is visible and editable | It should happen behind the scenes, out of reach |

### The failure is not hypothetical

Reading the workbook confirms the client's assessment. Three defects are present in the copy
we hold, all of them the signature of a formula dragged one column too far, and all of them
silent:

| # | Defect | Effect |
| --- | --- | --- |
| 1 | `'3. ReCharge Rate Calculations'!I10` reads `'2. Capacity'!G17` — Capability 3's capacity — where it should read `J17` **[W]** | Capability 6 is priced against the wrong capability's capacity |
| 2 | `I29`, `I31` and `I32` read `'1. Forecast Operational Costs'!F43` / `F57` — Capability 3's costs — where they should read `I43` / `I57` **[W]** | Capability 6's revenue and balance are computed against another capability's cost base |
| 3 | The platform totals `C29`, `C30`, `C31`, `C32`, `C39`–`C42` are `SUM(D:I)`, stopping at Capability 6, while the cost total `C24` is `SUM(D:K)` **[W]** | Revenue and cost are summed over different column ranges; Capability 7 and the Analysis/Consulting line are counted as cost but not as income |

Together these produce the workbook's only non-zero platform balance — a **$2,079.08 surplus**
that is entirely an artefact of defect 2, and a platform revenue total understated by
**$4,874.74** by defect 3. Nothing in the spreadsheet flags any of this.

**This is the argument for the project, and it should be in the Assignment 1 problem
statement**: not "spreadsheets are fragile" in the abstract, but three specific silent errors
in the live tool, each of which a server-side calculation with a fixed column contract makes
structurally impossible.

The workbook is *already* structured as three sheets, with **yellow cells the user fills in
and blue cells holding the logic**. The client was explicit that only the yellow cells are
being asked of the user. **That structure is the shape of the product we are being asked to
build.** **[K §3]**

**What we are building, in one sentence:** a guided web application that collects the yellow
cells, keeps the blue cells server-side where nobody can break them, and produces a sealed,
retrievable record explaining why a given rate was set.

## 3. Stakeholders

| Stakeholder | Role in the system | What they need |
| --- | --- | --- |
| **Platform custodian** (primary user) | Enters costs, capacity, utilisation, benchmarking and justifications; proposes final rates | A self-explanatory process they can complete without training or finance expertise |
| **Delegated authority** (approver) | Approves rates before they take effect — "typically the head of the BU responsible for the operating costs of the infrastructure" **[G, Step 5]** | A complete, documented submission they can approve or return |
| **UWA Research Infrastructure** (client / process owner) | Owns the costing method; needs consistency across platforms and a defensible audit trail | Every platform priced by the same protocol; records retrievable years later |
| **Researchers and external users** (indirect) | Pay the rates | A credible answer to "why does it cost this much?" |
| **Auditors, FOI requesters, government funders** (indirect) | May scrutinise pricing | "Supporting documentation … retained for audit and review purposes" **[G, Step 5]** |
| **CITS5206 team** | Build and hand over the software | Clear scope, agreed MVP, decidable open questions |
| **Unit facilitator** | Assesses process and product | Visible planning, evidence of client engagement |

### Terminology — settled against the client's guide

Earlier drafts recorded our word choices as open. The client's guide settles most of them, so
these are now **the** terms, in the software and in these documents:

| Term we use | Status | Source |
| --- | --- | --- |
| **custodian** | The client's own word — "This guide provides Research Infrastructure **custodians** a practical approach…" | **[G, Purpose; Step 4; Step 5]** |
| **capability** | The client's unit of costing. Not "instrument" — the workbook's columns are `Capability 1`…`Capability 7` | **[G, Purpose]**, **[W, sheet 1 row 8]** |
| **billable unit** | The client's term for the unit rates are quoted in. Not "charging unit" | **[G, Step 2]** |
| **APFR** — Australian Publicly Funded Researcher | The guide's name for the middle user category. The workbook's sheet 3 calls it `PFRI (Publicly Funded Research Institutes)`; the guide governs | **[G, Step 3]** |
| **minimum sustainable charge-out rate** | The guide's name for the calculated (as opposed to proposed) rate | **[G, Step 3]** |

The client said "platform leaders" in the walkthrough **[K §4]**; "custodian" is their written
term and is used throughout. No confirmation needed.

### The primary user, in detail **[K §4]**

- Academic or professional staff who manage a platform or a capability — bookings, maintenance, training. All UWA staff.
- Best placed to forecast utilisation, because they know how popular their equipment is.
- Busy and technical, but not in this domain: *"I do microscopes, I'm really good at microscopes."* This is administrative work they tolerate rather than enjoy.
- They run the exercise on a **three-to-five year cycle**, then review and repeat.

The last point is the strongest single design constraint. **Low frequency, high stakes.**
Nobody will remember how the tool worked last time, so no step may rely on prior familiarity,
and no error may be recoverable only by someone who already knows the method.

## 4. The calculation

The client's guide sets out a **six-step process** **[G]**. The workbook implements steps 1–3
as its three sheets; steps 4–6 are currently done outside it.

| Step | Outcome | Where it lives today |
| --- | --- | --- |
| 1 Understand full operating costs | The true annual cost of operating the capability | Workbook sheet 1 |
| 2 Determine capacity and utilisation | Realistic annual billable use | Workbook sheet 2 |
| 3 Calculate minimum sustainable charge-out rates | Indicative rates per user category | Workbook sheet 3 |
| 4 Benchmark and understand user value | Comparable pricing and value delivered | Outside the workbook |
| 5 Seek approval | Delegated authority approval, documentation retained | Outside the workbook |
| 6 Communicate changes | Users and stakeholders informed | Outside the workbook |

The client was explicit that the team does **not** need to master the reasoning behind the
method — the tool takes the inputs and applies it. **[K §2]**

**Rates are produced per capability, not per platform.** The workbook computes an independent
set of three rates for every capability column, each against that capability's own costs and
its own capacity **[W, sheet 3 rows 25–27]**. A platform-level roll-up exists, but the rate a
user is charged is a capability's rate.

### Step 1 — Total operating costs

The guide asks the custodian to include "all costs required to provide the service, regardless
of whether they are cash-funded or provided in-kind" **[G, Step 1]**. The workbook's categories
**[W, sheet 1 rows 11–23, 27–36, 40–41]**:

**Directly incurred, per capability** — employee base salary (platform leader; research
officer), employee benefits and on-costs (same two), materials and supplies, non-capital
equipment purchases, other expenses, rental/hiring/leasing fees, repairs and maintenance,
maintenance contracts, R&M assumption threshold, decommissioning costs, utilities and rates.

**Directly allocated, entered once at platform level and apportioned** — materials and
supplies, other expenses, repairs and maintenance, cleaning and waste disposal, IT costs,
rental/hiring/leasing fees, anticipated R&M cost buffer, administration costs, platform leader
salary and on-costs.

**Indirect** — floor area, laboratory and office, at a rate per m² per annum.

**Cost allocation is an even split, not pro-rata.** Directly allocated costs are divided by the
number of capability columns: `=$C$27/COUNTA($D$8:$J$8)` **[W]**. Note that administration and
platform-leader rows use `COUNTA($D$8:$K$8)` instead, including the Analysis/Consulting column
in the divisor — nine categories split seven ways, two split eight ways. This answers what was
previously open question Q1.

**Replacement reserve.** The guide's worked example **[G, Step 1]**:

> Equipment replacement value: $500,000 · Strategic objective: recover 25% of replacement value
> over 5 years · Required reserve target: $500,000 × 25% = $125,000 · Annual contribution
> required: $125,000 ÷ 5 = $25,000 per year

"This amount should be incorporated into your annual target for recovery **where
appropriate**." Optional, custodian-determined, and it enters the cost total. Earlier drafts
of this document said the method excludes replacement cost; that was drawn from the walkthrough
and is wrong. What is excluded is *historical capital expenditure*, not a forward reserve.

### Step 2 — Capacity, then utilisation

**Billable unit.** "Use a unit that reflects how users access the service: **Hours; Days;
Samples**. The same unit should be used consistently throughout capacity, utilisation and
pricing calculations." **[G, Step 2]** Earlier drafts said "hours, days or weeks" — weeks is
not one of the client's units, and samples was missing. This answers what was previously
open question Q6.

**Capacity baselines.** The guide gives the standard UWA working year as **230 working days ×
7.5 hours = approximately 1,725 hours per annum** before operational constraints **[G, Step
2]**. The workbook carries two distinct baselines **[W, sheet 2 rows 3–4]**:

| Baseline | Derivation | Hours |
| --- | --- | --- |
| Machine availability | 365 − 104 weekends − 10 WA public holidays = 251 days × 7.5 h | **1,882.5** |
| Staff availability | 230 working days per EBA × 7.5 h | **1,725** |

Capacity is then reduced by whatever constrains that capability: maintenance, technical staff
capacity, downtime, compliance requirements, staff availability, setup and pack-down time,
planned outages **[G, Step 2]**. Where a capability needs a person physically present, the
workbook caps its capacity at the staff FTE allocated to it — the rule written in the sheet is
"**if reliant on staff, row 17 = row 12**" **[W, sheet 2 row 18]**. In the copy we hold,
Capability 7 and the Analysis/Consulting line are staff-capped at 86.25 h (0.05 FTE × 1,725),
while Capabilities 1–6 use the full 1,882.5 h machine baseline.

**Forecast utilisation.** "Estimate the annual utilisation that is realistically expected over
the coming pricing period", informed by historical usage and anticipated change — new research
centres, major grants, strategic appointments, industry demand, competing facilities,
regulatory change **[G, Step 2]**. The guide is explicit about its weight:

> This figure will be used in the calculation of the capability's minimum sustainable
> charge-out rates and is **one of the most significant assumptions underpinning the pricing
> model**. **[G, Step 2]**

> While the capacity might be a thousand hours per year, in practice it might only be used
> 500 hours a year. **[K §2 Step 3]**

**Forecast utilisation, not capacity, is the divisor.** This is the single most
misunderstandable number in the system and the tool must make the distinction impossible to
miss.

**A note on how utilisation is entered.** The guide asks for an absolute figure in the billable
unit. The workbook instead stores a utilisation *rate* per user type and derives the divisor as
`capacity × total rate` **[W, sheet 3 rows 19–22, 25]**. The two are equivalent; the guide
governs, so the tool captures the absolute figure and displays the implied percentage of
capacity alongside it.

### Step 3 — The three minimum sustainable charge-out rates

Three user categories, each charged differently **[G, Step 3]**:

| User category | Applies to | Formula |
| --- | --- | --- |
| **UWA Researcher** | UWA staff and HDR students, for projects meeting the HERDC definition of research | `(C − I_total) / U` |
| **APFR** — Australian Publicly Funded Researcher | Non-UWA university and government researchers, where use supports the publicly funded entity's objectives | `((C − I_nonuwa) / U) × k` |
| **Commercial** | All other non-research activity: industry access, facility services, consultancy for external entities | `(C / U) × k` |

| Symbol | Meaning |
| --- | --- |
| `C` | Total annual operating cost for the capability |
| `I_total` | All non-variable operating income |
| `I_nonuwa` | Non-variable operating income from sources other than UWA |
| `U` | **Forecast** annual utilisation, in the billable unit |
| `k` | `1.35` — UWA's standard indirect cost recovery, per the University Indirect Cost Recovery Policy |

`k` is applied whenever an external party engages UWA services: a 0.35 uplift covering
insurance, legal, finance, library, buildings and IT infrastructure — but **not** the equipment
or the capability itself **[K §2 Step 4]**. For commercial work, "operator time should be
charged at UWA consultancy rates or similar" **[G, Step 3]**.

The guide also records an obligation the tool should surface: custodians "should note the
University's obligations under **competitive neutrality**" when varying commercial rates
**[G, Step 3]**.

### Non-variable operating income

The guide asks for "recurrent or standard ongoing income that subsidises the operation of the
capability": university in-kind commitments, University General Purpose (GP) funding, NCRIS
operational funding, and other recurrent government support **[G, Step 1]**. The workbook
carries **four** lines **[W, sheet 1 rows 47–50]**:

| Line | Counts toward |
| --- | --- |
| UWA GP (platform leader) | `I_total` only — it is UWA money, so the APFR rate does not deduct it |
| State | `I_total` and `I_nonuwa` |
| Federal (incl. NCRIS) | `I_total` and `I_nonuwa` |
| Other (e.g. philanthropic) | `I_total` and `I_nonuwa` |

Earlier drafts modelled income as two figures, university and government. It is four, and the
split that matters to the formulas is **UWA versus non-UWA**, not university versus government.

> **N.B. user fees of any description should not be included here.** **[G, Step 1]**

Some platforms receive no non-variable income at all; entering nothing is valid. **[K §2]**

### The worked example — the golden-file fixture

This is the client's own example, taken from the guide, and it is the fixture the calculation
engine is verified against **[N13]**. It exercises all three formulas from one set of inputs.

| Input | Value |
| --- | --- |
| Operating costs `C` | $150,000 |
| University in-kind contribution | $20,000 |
| WA Gov annual income for technical staff support | $30,000 |
| → `I_total` | $50,000 |
| → `I_nonuwa` | $30,000 |
| Forecast annual utilisation `U` | 1,000 hours |

| Rate | Calculation | Result |
| --- | --- | --- |
| UWA Researcher | `($150,000 − $50,000) ÷ 1,000` | **$100.00 / hour** |
| APFR | `(($150,000 − $30,000) ÷ 1,000) × 1.35` | **$162.00 / hour** |
| Commercial | `($150,000 ÷ 1,000) × 1.35` | **$202.50 / hour** |

All three reconcile exactly. **[G, Step 3]**

> **Superseded figures.** Drafts up to v1.1 of this document quoted a demonstration platform at
> $380,000 total cost, $230,000 non-variable income, $150,000 to recover, a $3,291 calculated
> rate and a $15,000 deficit, all transcribed from the walkthrough. **None of those figures
> appears in any client document**, and the $3,291 rate was internally inconsistent with the
> "nobody charges five cents on top of $42" remark recorded beside it. They have been withdrawn
> and replaced by the guide's own example above. Nothing in the repository should cite them.

### Step 4 — Benchmark and understand user value

Once the calculated rates exist, the custodian benchmarks them: "While cost recovery provides
the foundation for pricing, final charge-out rates should also consider the value delivered to
users and the availability of alternative providers." **[G, Step 4]**

Named benchmark sources: NCRIS facilities, other Australian universities, commercial
laboratories, and international facilities where relevant. When interpreting them, allow for
differences in geographic location, technical capability, user support, accreditation and
compliance, data quality, turnaround time and access arrangements. **[G, Step 4]**

The guide then poses the questions the custodian must answer, and the tool should ask them in
these words **[G, Step 4]**:

- *If calculated rates are higher than market* — are we offering additional or different value? Can operating costs be reduced? Can we grow utilisation; is utilisation being underestimated?
- *If calculated rates are lower than market* — is there an opportunity to strengthen replacement reserves? What are our obligations under competitive neutrality? Could matching higher rates support long-term sustainability, or complementary capabilities?

### Step 5 — Proposed rates, approval, and the resulting balance

The calculated rates are **reference figures, not prices** — "a starting point for pricing
decisions before considering market conditions or strategic adjustments" **[G, Step 3]**. The
custodian is expected to step back and **propose their own rates** **[K §2 Step 5]**. The
workbook's proposed rates are $50 / $100 / $100 against calculated rates such as $36.64 —
round numbers, chosen for legibility **[W, sheet 3 rows 35–37]**.

The tool must then show what those proposed rates do to the forecast balance. The workbook
computes, per capability and per user type, the forecast revenue, the university overheads
recovered, the recovery of full economic cost, and the end-of-year balance **[W, sheet 3 rows
39–42]** — and it shows deficits: in the copy we hold, Capability 3 forecasts **−$21,372** and
Capability 4 **−$16,370** at the proposed rates. Surfacing that *before* a decision is made is
exactly what the client wants. **[K §2 Step 5]**

Before rates take effect, the guide requires **[G, Step 5]**:

> Document costing assumptions · Document utilisation assumptions · Record market benchmarking
> undertaken · Obtain approval from the appropriate delegated authority, typically the head of
> the BU responsible for the operating costs of the infrastructure. Supporting documentation
> should be retained for audit and review purposes.

**Design consequence.** The system stores two distinct sets of numbers — *calculated* and
*proposed* — plus the variance between them, the benchmarking that informed the proposal, and
the custodian's justification. A tool that only shows the calculated rate does not meet the
requirement.

### Step 6 — Communicate price changes

Approved rates are communicated to users, explaining why prices changed, the methodology used,
and the importance of sustainable infrastructure funding, "well in advance of actual roll out
to allow researchers to plan for grant and project budgets" **[G, Step 6]**.

### GST

Rates are **GST-exclusive**. "All costing, pricing and financial planning should be undertaken
on a GST-exclusive basis, with GST added separately when invoicing external users." The guide's
example: a $200/hour commercial rate is invoiced at $220 including 10% GST, and the $20 is not
available to support facility operations. **[G, Step 3, A Note on GST]**

This was assumption A3 in earlier drafts. It is now established fact and the tool must label
rates as GST-exclusive wherever they appear.

## 5. The record — the artefact the client actually wants

This came up repeatedly and is the heart of the requirement. All inputs, the calculation and
the justifications must be saved somewhere visible, formally approved, and put on file
**[K §6]** — which the guide restates as an obligation: "Supporting documentation should be
retained for audit and review purposes" **[G, Step 5]**.

> If someone comes to us and says "why does it cost $50 an hour for me to use?", we want to be
> able to say: well, it costs $100,000 a year to run, this is how many hours a year it's going
> to be used, we divide one by the other — $50 an hour. That's the reason we charge that price.

And why it must survive:

> We can't say "we don't know, it's $50 an hour because we just picked that number". That
> doesn't sound very good and that doesn't inspire confidence in the people that are using our
> services.

Records are kept across cycles. In three years the client wants to open the old record, see
what the pricing model was and why, and set the new one against it. The client suggested a
printout or a generated email; **the format is open**. **[K §6]**

**Design consequence.** The record is not a report generated on demand from live data — it is
a **frozen artefact**. Rates, inputs, benchmarking, justifications and the version of the
method used are captured at seal time and never recomputed. A 2026 record must still reproduce
its own figures in 2030 even if `k` has changed or the calculation has been revised.

**The lifecycle that follows from it.** A record is editable in exactly one state and in no
other, and the three stages are worth stating plainly because the rest of this document only
implies them:

1. **Draft.** Every input can be changed and the figures recompute as it changes **[F8, F10]**.
2. **Sealed.** Nothing can be changed, by anyone, through the application **[F11]**.
3. **Superseded.** A later cycle replaces this one by reference; this record stays readable
   exactly as it was sealed **[F22]**.

Costs move during a cycle — a staff departure, a renegotiated service contract, a utility rise —
and none of that reaches back into a sealed record. The sealed figures are the evidence for a
rate that was actually charged, and revising them to match today's costs would destroy the
chain the client asked for: the record would no longer explain the price anyone was billed.
A material change is answered by a **new cycle that supersedes the old one** **[F22, A5]**, not
by editing.

Two boundaries around that, both deliberate. **What counts as material** — and therefore when a
new cycle is opened — is the client's operating judgement; the tool sets no threshold and issues
no prompt **[A11]**. And **whether a sealed record ever needs amending inside its own cycle**, as
opposed to being replaced by the next one, is open and asked of the client at
[Q3](#9-open-questions); the answer changes how records are stored, which is why it is asked
before the engine is written.

## 6. Requirements

Requirement IDs are referenced by the [user stories](user-stories.md) and the
[architecture](architecture.md). **IDs are stable, not sequential** — F19–F21 were added in
v2.0, and F22 in v2.3, taking the next free number rather than renumbering everything that
cites them. The table is ordered by priority, so a late Must sits with the other Musts.

### 6.1 Functional

| ID | Requirement | Source | Priority |
| --- | --- | --- | --- |
| **F1** | Guided, sequential web form in three sections — costs → capacity & utilisation → rates — mirroring the workbook's three sheets | K §5, W | Must |
| **F2** | Capture operating costs at both capability and platform level, across the client's categories: directly incurred, directly allocated, and indirect | G Step 1, W sheet 1 | Must |
| **F3** | Capture non-variable income on four lines — UWA GP/in-kind, State, Federal (incl. NCRIS), Other — and distinguish UWA from non-UWA sources, because the three formulas deduct them differently | G Step 1, W sheet 1 rows 47–50 | Must |
| **F4** | Capture billable unit (hours, days or samples), annual capacity from the stated baselines, and the deductions that reduce it — including the staff-FTE cap where a capability needs a person present | G Step 2, W sheet 2 | Must |
| **F5** | Capture forecast utilisation as a mandatory input, visibly distinct from capacity, with the implied percentage of capacity shown alongside | G Step 2 | Must |
| **F6** | Compute the three minimum sustainable charge-out rates **per capability**, server-side, and display them with the figures behind each | G Step 3, W sheet 3 | Must |
| **F7** | Accept custodian-proposed rates alongside the calculated ones, per capability, and show the resulting forecast surplus or deficit | G Step 3 & 5, K §2 Step 5 | Must |
| **F8** | Allow the user to go back, change any input, and see the effect on the numbers before committing | K §5 | Must |
| **F9** | Provide free-text justification fields throughout, for anomalies and for how a position was reached | K §5 | Must |
| **F10** | Save a live draft that can be left and resumed | K §5 | Must |
| **F11** | On submit, **seal** the record: inputs, calculated rates, proposed rates, benchmarking and justifications become immutable | K §5, §6 | Must |
| **F12** | Export the sealed record to a portable file that can be filed and read years later | K §6, G Step 5 | Must |
| **F13** | List and retrieve past sealed records for a platform, so a new cycle can be set against the last one | K §6 | Must |
| **F15** | Authenticate users; every record carries the identity of who created, submitted and sealed it, and access is restricted to identified UWA staff | K §7 | **Must** |
| **F22** | Create a new cycle that **supersedes** a previous sealed cycle by reference, leaving the superseded record readable and unchanged — the only route by which a change in costs reaches the rates | K §6, [A5](#8-assumptions) | Must |
| **F14** | Pre-fill what can be pre-filled — select "academic, level X" or "professional staff, level 8" and the salary is filled in, so it cannot be forgotten | K §5 | Should |
| **F16** | Formal approval by a delegated authority, distinct from the custodian's submission | G Step 5 | Should |
| **F19** | Capture an optional replacement reserve — replacement value, recovery percentage, recovery period — and carry the annual contribution into the cost total | G Step 1 | Should |
| **F20** | Record the benchmarking undertaken: sources consulted, comparable rates, and the custodian's answer to the guide's Step 4 questions | G Step 4 & 5 | Should |
| **F17** | Dashboard comparing costs across years, categories and suppliers | K §8 — team proposal; client: "if you want to give that a crack, even better" | Could |
| **F21** | Generate the Step 6 price-change communication — what changed, the methodology, and the notice period | G Step 6 | Could |
| **F18** | Integration with existing UWA systems | K §8 — explicitly deferred | Won't (this project) |

### 6.2 Non-functional

| ID | Requirement | Rationale | Priority |
| --- | --- | --- | --- |
| **N1** | The calculation executes server-side; no formula is exposed to or modifiable by the user | "They can't break it, they can't mess it up" **[K §5]** — the core defect of the spreadsheet | Must |
| **N2** | Type and range validation on every numeric field. The named failure to prevent: a maintenance contract entered as $200,000 instead of $20,000 because of one extra zero | K §5 | Must |
| **N3** | Mandatory fields enforced — utilisation in particular; the user cannot proceed without it | K §5 | Must |
| **N4** | Usable without training or documentation by a technical non-specialist returning after 3–5 years | K §4 | Must |
| **N5** | Monetary arithmetic is exact — decimal, not binary floating point — and rounding is applied once, at presentation, by a stated rule. Rates are labelled GST-exclusive | Publicly defensible figures must reconcile to the cent; **[G, GST]** | Must |
| **N6** | The calculation is deterministic and reproducible: a sealed record regenerates its own figures from its own stored inputs and method version | K §6 — "in three years, open the old record" | Must |
| **N7** | The method version, `k`, the capacity baselines and the cost/income category lists are configuration, versioned, not constants in code | `k` and the method will change across a 3–5 year cycle | Must |
| **N8** | Divide-by-zero guarded — `U = 0` is a validation failure with a plain-language message, never a crash or an infinity | Zero forecast utilisation is a plausible user error | Must |
| **N9** | Transport encrypted (HTTPS); credentials and secrets never committed to the repository | Baseline security | Must |
| **N11** | Data is UWA-internal and subject to FOI; not commercially confidential, but not for public promotion, especially while in progress | K §7 | Must |
| **N13** | The engine is verified against the client's own worked example (§4) as a golden-file test before any UI is written, and against the workbook's structure for the per-capability roll-up | Correctness must be demonstrated, not asserted | Must |
| **N14** | Column and category contracts are fixed in code, so a total can never be summed over a different range from the figures it is compared against | Defect 3 in §2 — the workbook's own failure mode | Must |
| **N10** | Every state change on a record is attributable — who, what, when — and the audit log is append-only | The record must be defensible under audit or FOI | Should |
| **N12** | Accessible to WCAG 2.1 AA — a UWA public-sector obligation | Institutional requirement | Should |

### 6.3 Acceptance — how we will know the MVP is right

1. The engine reproduces the client's worked example in §4 — $100.00, $162.00 and $202.50 per hour from $150,000 / $20,000 / $30,000 / 1,000 hours — to the cent, as an automated test. **[N13]**
2. The engine reproduces the workbook's per-capability rates for a transcribed capability, to the cent, **with the workbook's three defects corrected**, and the corrections are documented in the test. **[N13, N14]**
3. A platform custodian who has never seen the tool completes a full cycle — create → enter → review → benchmark → propose → seal → export — without assistance.
4. A sealed record, reopened, shows every input, both rate sets, the variance and every justification, and recomputes its own figures unchanged.
5. Deliberate bad input — a word in a number field, an empty utilisation, a $200,000 typo, a zero divisor — is caught with a message the user can act on.
6. No calculation formula is present in anything sent to the browser.
7. Every record shows who created and sealed it, and no record can be created anonymously. **[F15]**

## 7. Scope

**This section is the scope baseline.** Where the [user stories](user-stories.md) or the
[architecture](architecture.md) disagree with this section about whether something is in the
MVP, this section wins and the other document is corrected.

**In scope (MVP).** F1–F13, F15 and F22, and N1–N9, N11, N13, N14 — one platform with its
capabilities, one costing cycle, the full vertical slice from creating a cycle to exporting a
sealed record, with authenticated users throughout. Every **Must** in §6.1 and §6.2, and
nothing else.

**In scope if time allows.** F14 (salary pre-fill), F16 (delegated approval), F19 (replacement
reserve), F20 (benchmarking record), F17 (dashboard), F21 (price-change communication), N10,
N12 — everything marked **Should** or **Could** in §6.1 and §6.2.

**Out of scope.** Integration with UWA finance, HR or booking systems **[K §8]**; billing or
invoicing of actual usage; a researcher-facing price lookup; migration of historical
spreadsheets; multi-institution or multi-tenant operation.

A vertical slice is preferred to a broader but incomplete build: a narrow end-to-end path that
works beats three-quarters of every feature. See
[`reference/unit/wipro-mvp-summary.md`](../../reference/unit/wipro-mvp-summary.md).

## 8. Assumptions

Recorded so that they can be confirmed or corrected. Each is ours, not the client's.

| # | Assumption | If wrong |
| --- | --- | --- |
| A1 | A costing cycle covers one platform and produces **three rates per capability**, plus a platform-level roll-up. *(Established from [W, sheet 3]; retained here because we have not yet had it confirmed in writing.)* | Reverting to one rate set per platform would simplify the model, but nothing in the client's material suggests it |
| A2 | A single forecast `U` per capability is the divisor for all three of that capability's rates; the per-user-type split is used only for the revenue and balance projection, as the workbook does **[W, sheet 3 rows 19–22 vs 29]** | If each user type needs its own divisor, the formulas take three — a change to the engine contract |
| A3 | Cost figures are annual and in AUD. *(GST-exclusive is no longer an assumption — see §4.)* | Period handling changes the arithmetic and the labels |
| A4 | `k = 1.35` applies to APFR and commercial users only, and is uniform UWA-wide per the University Indirect Cost Recovery Policy | Per-platform or per-category `k` values require the config to be keyed differently |
| A5 | A sealed record is superseded by a new one, never edited or deleted; corrections create a new version that references the old. **F22 carries this**, and [architecture §4](architecture.md#4-data-model) builds it | Amendment-in-place would break the audit trail — needs a client decision, and it is the second half of [Q3](#9-open-questions) |
| A6 | A custodian may see their own platform's records; an administrator sees all; the delegated authority sees what is submitted to them | Access control design depends on the answer — see Q4 |
| A7 | The MVP's export format is PDF, being the most durable and the closest to the "printout" the client suggested | Client may prefer generated email, CSV or a system of record — see Q5 |
| A8 | Salary pre-fill uses a published UWA pay scale table, versioned by year, maintained as configuration | If rates are not published in a usable form, F14 becomes manual entry |
| A9 | Fewer than 100 platforms and a handful of concurrent users — this is a low-traffic internal tool | Nothing in the design assumes scale; if wrong, little changes |
| A10 | The Analysis/Consulting line is a chargeable column that is not a capability: it takes costs and produces rates, but has no machine capacity and is always staff-capped **[W]** | If it is a capability like any other, the capacity model simplifies |
| A11 | Cost and income figures are the **budgeted** annual amounts for the period the rates will cover — not expenditure actually incurred to date (refining A3, which fixes only the period and the currency). It follows that mid-cycle variance does not alter a sealed record, and that **the tool sets no threshold for when a new cycle should be opened**: what counts as a material change is the client's operating judgement, made outside the tool | If the client expects actual expenditure to drive rates within a cycle, the record stops being a frozen artefact, F11 changes shape and rates become a monitoring problem rather than a periodic one. This is the assumption most likely to be tested the first time a real cost moves, which is why it is written down rather than left to be inferred from §5 |

## 9. Open questions

**One question is open.** **Q1, Q2 and Q6** were closed by the client's own documents and their
answers are in §4. **Q3, Q4, Q5, Q9 and Q10** were put to the client on 17 August 2026 and
**answered by the client on 20 August 2026** — in the meeting and again in writing. **Q7** is
answered, but recorded because the answer is ours rather than the client's. **Q8**, the
repository licence, is the one that remains, and it closes at handover.

Any count of open questions quoted elsewhere in the repository refers to this table. If a
question is added, it is added here first.

> **No default was used.** Every question in the batch carried a proposed default so that a
> non-answer would not block us. All five came back answered, four of them matching the default.
> The fifth — Q5, the sealed record — went further than our default and is the only place where
> the client's answer adds work: see the row itself.
>
> **Client answers:** [`client-answers-to-the-five-questions.md`](../client/communication-history/2026-08-20-client-meeting/client-answers-to-the-five-questions.md)
> · **Meeting:** [minutes of 20 August 2026](../meetings/2026-08-20-client-meeting.md) §3

| # | Question | Status / proposed default |
| --- | --- | --- |
| ~~Q1~~ | How are costs allocated between the per-capability and per-platform levels? | **Closed.** Even split across capability columns **[W]** — see §4 Step 1 |
| ~~Q2~~ | How is utilisation split across the three user types? | **Closed.** A single `U` per capability drives the rates; the per-type split drives the balance projection **[W]** — see §4 Step 2 |
| ~~Q3~~ | How are multi-year cycles handled, and how is a sealed record superseded rather than overwritten? **And does a sealed record ever need amending *within* its own cycle**? | **Closed 20 Aug 2026 by the client** — and our proposed default confirmed in full. *"Retain all records, most recent approved record is current and supersedes prior records… aiming for a validity period of 3-5 years (capability dependent)… don't expect to review records/pricing annually."* **[C]** Supersession only (A5, **F22**); nothing overwritten; **no in-cycle amendment**, so F11 and US-15 do not grow and the data model keeps a single frozen version. Validity period is now a stated range rather than a guess |
| ~~Q4~~ | What access control is needed beyond authentication — who may see, and who is the delegated authority in the system? | **Closed 20 Aug 2026 by the client:** *"Agreed that administrator is approver of the record."* **[C]** The three roles stand — custodian, delegated authority, administrator — with authentication local to the app for the MVP. **The approval *action* remains F16 (Should, not MVP):** the client answered who holds the authority, not whether the tool must route a submission to them, and signed the scope statement with that flag in it unchanged. Confirmation is action A14. Separately, the client raised **linking staff roles to a UWA HR system** — out of the agreed scope, recorded as raised in the [minutes](../meetings/2026-08-20-client-meeting.md) §5 |
| ~~Q5~~ | What format does the sealed record take — printout, generated email, or something else? | **Closed 20 Aug 2026 by the client**, and this is the one answer that **adds work**: *"Confirm that .pdf is appropriate format. No such template exists currently – ideally the .pdf record would include workings for the calculator (for transparency and traceability) and outputs. Records to be stored on UWA records management system: Content Manager (TRIM)."* **[C]** PDF confirmed, server-side, plus the stable in-app URL. **New:** the export must show the *workings*, not only the inputs and the three rates — needs a requirement ID and a story estimate (action A17). TRIM is a filing destination the custodian uses, **not** an integration the tool performs |
| ~~Q6~~ | What is the billable unit set? | **Closed.** Hours, days or samples **[G, Step 2]** |
| ~~Q7~~ | Is the calculated-vs-proposed variance ever required to be zero? | **Answered by us, not the client.** Always permissible, always surfaced, justification mandatory when non-zero. The workbook itself carries large deficits at proposed rates, so a non-zero variance is clearly normal **[W]** |
| **Q8** | What licence does this repository carry, given jointly owned IP? | **Open — the only one.** All rights reserved, with UWA's permissions granted directly in [`NOTICE`](../../NOTICE). A licence from one joint owner alone may not be effective, so we do not purport to grant one. **Unblocked 20 Aug 2026:** the client signed confirmation 2, so the joint ownership position is now confirmed in writing — which is what this question was waiting on. It still does not close today, because the signature confirms the *position*, not the licence that follows from it. Closes at handover — see [README §Ownership](../../README.md#ownership) and action A16 |
| ~~Q9~~ | The guide's commercial formula deducts no income; the workbook's commercial row deducts federal and other income **[W, sheet 1 row 59]**. Which is correct? | **Closed 20 Aug 2026 by the client:** *"Guide governs. Where a discrepancy occurs, we'd appreciate if these can be flagged to us for our knowledge and guidance."* **[C]** `R_commercial = (C / U) × k`, no deduction. The workbook's commercial row is confirmed as a defect. The second sentence is an obligation on us, not a preference — discrepancies go back to the client as we find them (action A15) |
| ~~Q10~~ | Should the tool reproduce the workbook's three formula defects for backward comparability, or correct them? | **Closed 20 Aug 2026 by the client:** *"Tool should follow the guide."* **[C]** Correct them, and show the corrected figure beside the workbook's where a historical comparison is made (N14). A tool that faithfully reproduces a known error is not an improvement, and the client agrees |

## 10. Constraints

- **Academic calendar.** The project ends with the semester; the technology decision has a go/no-go at the mid-semester checkpoint. Delivery risk, not client risk, drives the schedule.
- **Client availability.** The client asked us to digest, then meet when we have questions — no fixed weekly slot. Wednesdays preferred, Teams chat for async, support tapering over the project. **[K §10]** Question batching matters; a two-week wait on a blocking answer is a real risk.
- **Client material handling.** Client documents stay local and uncommitted, and **nothing the client gave us enters the repository without their agreement**; anything the repository needs is rewritten by us and attributed. Enforced by [`.gitignore`](../../.gitignore) §1 and set out in [`reference/client/README.md`](../../reference/client/README.md). **[K §7]**
- **IP.** The team owns the code and may use it in portfolios. The costing logic is UWA's, and overarching IP is **joint** — selling the tool onward would not be appropriate. **[K §9]** The kickoff's decision D5 phrases this as "the costing logic and overarching IP remain UWA's, jointly", which reads two ways; *jointly* governs the overarching IP, not the costing logic. That is the reading [`NOTICE`](../../NOTICE) §2 records and the only one consistent with the client's own words in [kickoff §9](../meetings/2026-07-29-client-kickoff.md#9-intellectual-property-and-portfolio-use). Anything we put in front of the client states it in full rather than quoting D5.
- **Data.** Relatively sensitive, subject to FOI, not commercially confidential, not for wide promotion while in progress. **[K §7]**

---

## Change log

| Version | Date | Change |
| --- | --- | --- |
| 2.4 | 22 Aug 2026 | **The client answered. Five of the six open questions close, and one source outranks the guide.** On **20 August 2026** the client signed the scope statement (both confirmations) and answered all five questions put to them on 17 August, in the meeting and again in writing — [answers](../client/communication-history/2026-08-20-client-meeting/client-answers-to-the-five-questions.md), [minutes](../meetings/2026-08-20-client-meeting.md). **§9 goes from six open questions to one.** Q3 closes with our default confirmed and the validity period stated as **3–5 years, capability dependent**; Q4 closes on *"administrator is approver of the record"*, which settles **who** approves and leaves F16 (routed in-tool approval) a Should, as the signed scope has it; Q5 closes on PDF, **with the calculator's workings included** and filing into Content Manager (TRIM); Q9 and Q10 close on *"guide governs"* and *"tool should follow the guide"*. **Q8 alone remains open**, and it is now unblocked rather than blocked: confirmation 2 puts the joint ownership position in writing, which is what it was waiting for. **A new source rank, [C], is added above the guide** — the client answering our question in writing about this project outranks their general policy document, and the precedence table now says so and says that **[C]** beats our own minutes. **No requirement, story or MoSCoW priority changes in this version.** Two things the client's answers create are recorded as actions rather than folded in silently: the sealed PDF must show the workings, which is new work needing a requirement ID and an estimate (A17), and the client asks that guide-vs-calculator discrepancies be flagged to them as we find them (A15). Both are in the [minutes](../meetings/2026-08-20-client-meeting.md) §5, along with a third — HR-system integration for staff roles — which is **raised, not accepted**, because it is the class of work the signed scope defers. |
| 1.0 | 14 Aug 2026 | First draft, written from the 29 July kickoff minutes. Not yet reviewed by the client. |
| 1.1 | 14 Aug 2026 | §7 named as the scope baseline that the user stories must follow, and tied to the MoSCoW priorities in §6. §9 now says which questions came from the client and which are ours. |
| 2.0 | 14 Aug 2026 | **Rewritten against the client's own documents.** Source precedence established: the costing guide governs, the workbook is a reference implementation, the minutes rank last. The demonstration figures transcribed from the walkthrough ($380,000 / $230,000 / $3,291 / $15,000) are **withdrawn** — none appears in any client document — and replaced by the guide's worked example ($150,000 / $20,000 / $30,000 / 1,000 h → $100 / $162 / $202.50), which is now the golden-file fixture. Rates are **per capability**, not per platform (A1). Income is **four lines**, split UWA vs non-UWA, not two. Billable units are **hours, days, samples**, not weeks. Capacity baselines, the staff-FTE cap and the even-split allocation rule are stated from the workbook. Replacement reserve added (F19) — the earlier "not replacement cost" line was wrong. Benchmarking (F20) and price-change communication (F21) added from guide steps 4 and 6. GST-exclusive promoted from assumption to fact. Terminology settled from the guide: custodian, capability, billable unit, APFR. **F15 raised to Must and moved into the MVP.** N14 added against the workbook's column-range defect. Q1, Q2 and Q6 closed by the client's documents; Q9 and Q10 opened by them. §2 now documents three specific formula defects in the live workbook as evidence for the problem statement. |
| 2.1 | 15 Aug 2026 | **Q4's proposed default corrected.** It committed the delegated authority to *approving* submissions, which contradicted F16 (Should, not MVP) and the scope baseline in §7 — the core role is a **viewing** role, per A6, and the approval action remains stretch. The guide's own answer to "who holds the delegated authority" — "typically the head of the BU responsible for the operating costs" **[G, Step 5]** — is now quoted in the default, so what we are actually asking the client is the narrower question of whether *typically* holds for these platforms. Both corrections found while reviewing [`2026-08-15-scope-and-questions.md`](../client/2026-08-15-scope-and-questions.md), whose Question 2 carried the same conflict; that document is corrected to match. §10's IP constraint now flags that kickoff decision D5 is ambiguously worded and states which reading governs, after a draft of the client sign-off document took the wrong one — [`NOTICE`](../../NOTICE) §2 carries the full note. No requirement, story or MoSCoW priority changes. |
| 2.3 | 15 Aug 2026 | **The record's lifecycle is stated rather than left to be assembled.** A reader asking the obvious question — *costs change during a cycle, so can a cost be edited?* — could previously find F8 (drafts are editable), F11 (sealed records are not) and A5 (supersession) in three places and no sentence joining them. §5 now carries the three states, says why a sealed record is not revised to match today's costs, and names both boundaries around that. Four changes follow from it. **(1) F22 is new**: creating a cycle that supersedes a previous sealed one by reference. This was asserted in [A5](#8-assumptions) and already built in [architecture §4](architecture.md#4-data-model), but carried no requirement ID and therefore no story — the architecture implemented something no requirement asked for. It is a **Must**, so §7's MVP list gains it, and it is owned by US-01 rather than a new story, which leaves the MVP at eighteen stories and 110 points. **(2) A11 is new**: cost and income figures are **budgeted** annual amounts, not expenditure to date — undefined until now, and the distinction that decides the whole question, since actuals would make the record a monitoring problem rather than a periodic one. A11 also records that the tool sets no threshold for when a new cycle is opened. **(3) Q3 gains the half it was missing**: whether a sealed record needs amending *within* its own cycle. That half already travels to the client as question 2 of [`2026-08-15-scope-and-questions.md`](../client/2026-08-15-scope-and-questions.md) and was added there in v1.4 of [`mvp-agreement.md`](../client/mvp-agreement.md), but was never folded back into this table, which the same document names as the authoritative source — so the rule that questions are added here first had been broken in one place. **(4) A5** now points at F22 and at Q3. No MoSCoW priority changes, no story is added or re-estimated, and nothing in the outbound client document is touched. |
| 2.2 | 15 Aug 2026 | **§9 reduced to the questions that decide what we build.** The question about the team's own working method was withdrawn — it settled nothing in the product and does not belong in a list whose purpose is to unblock the build — and the remaining questions are renumbered without a gap: old **Q7–Q11 become Q6–Q10**. Any Q-number quoted elsewhere in the repository follows this table and has been updated with it. §10's client-material constraint now states the rule it always meant: **nothing the client gave us enters the repository without their agreement**, enforced by [`.gitignore`](../../.gitignore) §1 and set out in [`reference/client/README.md`](../../reference/client/README.md). No requirement, story or MoSCoW priority changes. |
