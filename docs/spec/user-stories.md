# User Stories

**Project:** Research Infrastructure Costing & Pricing Tool
**Status:** Draft v2.0 — 14 August 2026, not yet reviewed by the client
**Companion documents:** [requirements](requirements.md) · [architecture](architecture.md)

> Stories trace to the requirement IDs in [requirements §6](requirements.md#6-requirements).
> Sources follow the precedence set out in [requirements, *Source precedence*](requirements.md):
> **[G]** the client's costing & pricing guide, **[W]** the client's calculator workbook,
> **[K]** the [kickoff minutes of 29 July 2026](../meetings/2026-07-29-client-kickoff.md).
> Anything not traceable to one of those is marked **(inferred)** and is a candidate for the
> next client conversation.

---

## 1. Personas

### Priya — platform custodian *(primary)*

Manages a microscopy platform with seven **capabilities**. Academic staff; runs bookings,
maintenance and training. Sets rates for the next three-to-five years, then does not touch
the process again until the next cycle.

> "I do microscopes, I'm really good at microscopes." **[K §4]**

- **Wants:** to get through this correctly, once, without needing to become a finance person.
- **Fears:** entering something wrong and only finding out when a researcher queries the price, or when the platform runs at a loss.
- **Behaviour:** does this so rarely that she remembers nothing from last time. Will not read a manual. Will abandon and come back days later.

### Mark — research infrastructure administrator *(client-side process owner)*

Sits in UWA Research Infrastructure. Owns the method, not any one platform. Answers the
question "why does it cost $50 an hour?" when it is asked — sometimes years after the
decision.

- **Wants:** every platform priced by the same protocol, and a filed record for each.
- **Fears:** having to say "we just picked that number." **[K §6]**
- **Behaviour:** reads records more often than he creates them; compares this cycle against the last.

### Dr Chen — delegated authority *(approver, secondary)*

Head of the business unit that carries the platform's operating costs. Approves rates before
they take effect and is the person the guide names: "Obtain approval from the appropriate
delegated authority, typically the head of the BU responsible for the operating costs of the
infrastructure." **[G, Step 5]**

- **Wants:** a complete submission — costing assumptions, utilisation assumptions, benchmarking, proposed rates and their effect on the balance — that can be approved or returned in one sitting.
- **Behaviour:** does not enter data and does not want to. Reads, questions, signs.

---

## 2. Story map

The MVP is the **vertical slice**: one narrow path through every layer, end to end. Read the
top row left to right — that is the journey. Everything below the line is depth added later.

```
 SIGN IN → START CYCLE → ENTER COSTS → CAPACITY & USE → SEE RATES → PROPOSE → SEAL → RETRIEVE
──────────────────────────────────────────────────────────────────────────────────────────────
MVP  identify   create      capability     billable unit   3 rates     own rates   submit  list past
     user       cycle       costs          capacity        per         entered     confirm records
                            platform       forecast use    capability  balance     export  reopen
                            costs          mandatory       breakdown   shown       PDF     sealed
                            income (×4)                    shown
──────────────────────────────────────────────────────────────────────────────────────────────
LATER salary     cost import  capacity      sensitivity    benchmark   delegated   compare
      pre-fill   from CSV     calculator    analysis       record      authority   cycles
      replacement                                          against     approval    dashboard
      reserve                                              peers       workflow    price-change
                                                                                   comms
```

## 3. Epics

| # | Epic | Requirements | MVP |
| --- | --- | --- | --- |
| E1 | Start and manage a costing cycle | F10, F13 | Yes |
| E2 | Capture operating costs and income | F2, F3 | Yes — F14, F19 stretch |
| E3 | Establish capacity and forecast utilisation | F4, F5 | Yes |
| E4 | Calculate and review charge-out rates | F6, F8 | Yes |
| E5 | Benchmark, propose rates and test the balance | F7, F9 | Yes — F20 stretch |
| E6 | Seal, export and retrieve the record | F11, F12, F13 | Yes |
| E7 | Identify the user and protect the calculation | F15, N1, N2, N3, N8, N14 | Yes |
| E8 | Approve, administer and communicate | F16, F21, N7, N10 | Stretch |
| E9 | Compare and analyse across cycles | F17 | Stretch |

An epic marked **Yes** is in the MVP, but only its **Must** stories are: E2 ships without the
salary pre-fill of US-05 and the replacement reserve of US-23, and E5 ships without the
benchmarking record of US-24, because [requirements §7](requirements.md#7-scope) puts F14, F19
and F20 outside the MVP. That section is the scope baseline; this file follows it.

**E7 now includes sign-in.** F15 was raised to **Must** in requirements v2.0, so US-19 is an
MVP story. This resolves the contradiction in earlier drafts, where US-02, US-15 and US-16 were
MVP stories whose acceptance criteria required a user identity that the MVP did not provide.

---

## 4. Stories

Priority uses MoSCoW. **Must** = MVP, exactly and only. A story **inherits the priority of
the requirement it delivers** in [requirements §6](requirements.md#6-requirements) — if a
requirement is a Should, no story against it can be a Must, and changing one means changing
the other in the same commit. Estimates are relative points, for sprint planning only.

### E7 — Identify the user and protect the calculation

---

**US-19 · Sign in** — Must · 5 pts · F15

> **As** a platform custodian
> **I want** to sign in before I start
> **so that** the record carries my name and nobody can create one anonymously.

**Acceptance criteria**

- A user authenticates before creating, editing or viewing any record.
- Every record carries the identity of the person who created, submitted and sealed it — this is what US-02, US-15 and US-16 depend on.
- Custodians see their own platform's records; administrators see all. *(Assumes [A6](requirements.md#8-assumptions); the role set beyond that depends on [Q4](requirements.md#9-open-questions).)*
- Authentication is local to the application for the MVP, behind a seam that UWA SSO can be fitted to later ([AQ3](architecture.md#11-open-architectural-questions)).
- The client acknowledged access control as needed but did not specify it **[K §7]**.

---

**US-18 · Stop me breaking it** — Must · 8 pts · N1, N2, N3, N8, N14

> **As** a platform custodian
> **I want** the tool to catch my mistakes
> **so that** I cannot silently produce a wrong rate the way I could in the spreadsheet.

**Acceptance criteria**

- Text in a numeric field is rejected at entry with a message naming the field and the expected format **[K §5]**.
- Amounts that are implausibly large for their category prompt a confirmation — the named case is $200,000 typed where $20,000 was meant **[K §5]**.
- Percentages outside 0–100 and negative costs are rejected.
- Zero or absent utilisation is caught as a validation failure, never a division error **[N8]**.
- A platform total is always summed over exactly the same set of capabilities as the figures it is compared against — the workbook's defect 3 is structurally impossible **[N14, requirements §2]**.
- Server-side validation is authoritative; client-side validation is convenience only.
- The client's requirement in full: "they can't break it, they can't mess it up" **[K §5]**.

---

### E1 — Start and manage a costing cycle

---

**US-01 · Start a new costing cycle** — Must · 3 pts · F10

> **As** a platform custodian
> **I want** to start a new costing exercise for my platform
> **so that** I can set rates for the period ahead without a spreadsheet.

**Acceptance criteria**

- Given I am signed in, when I start a new cycle, then I am asked for the platform name, the period the rates will cover, the billable unit (hours, days or samples **[G, Step 2]**), and the capabilities the platform holds.
- The cycle is created in **Draft** state and appears in my list of cycles.
- If a sealed cycle already exists for this platform, its key figures are shown alongside so I can set the new one against the last **[K §6]**.

---

**US-02 · Leave and resume a draft** — Must · 3 pts · F10

> **As** a platform custodian
> **I want** my work saved as I go
> **so that** I can leave this half-finished and come back next week without losing anything.

**Acceptance criteria**

- Every entered value persists on section navigation, without an explicit save action.
- Reopening a draft returns me to the section I left, with all values intact.
- A draft shows when it was last edited and by whom **[F15]**.
- The client's phrasing: while the document is live the user edits freely **[K §5]**.

---

### E2 — Capture operating costs and income

---

**US-03 · Enter per-capability operating costs** — Must · 8 pts · F2

> **As** a platform custodian
> **I want** to enter what each capability costs to run, separately
> **so that** the cheaper capabilities are not priced as though they were the expensive ones.

**Acceptance criteria**

- I can add capabilities to the platform; the demonstration platform held seven **[K §2, W]**, and the interface must stay workable at that number and beyond.
- For each capability I enter **directly incurred** costs by the client's categories: employee base salary and on-costs (platform leader, research officer), materials and supplies, non-capital equipment purchases, other expenses, rental/hiring/leasing fees, repairs and maintenance, maintenance contracts, R&M assumption threshold, decommissioning costs, utilities and rates **[W, sheet 1 rows 11–23]**.
- Every amount accepts currency input and rejects non-numeric input with a message naming the field **[N2]**.
- A running total per capability and for the platform is visible while I work, and the two reconcile **[N14]**.
- Each category offers an optional note field **[F9]**.

---

**US-04 · Enter platform-level costs and see how they are apportioned** — Must · 5 pts · F2

> **As** a platform custodian
> **I want** to enter costs that belong to the platform rather than to any one capability
> **so that** technician time and my own time are recovered too.

**Acceptance criteria**

- I can enter **directly allocated** costs once at platform level: materials and supplies, other expenses, repairs and maintenance, cleaning and waste disposal, IT costs, rental/hiring/leasing fees, anticipated R&M cost buffer, administration, platform leader salary and on-costs **[W, sheet 1 rows 27–36]**.
- I can enter **indirect** costs — laboratory and office floor area at a rate per m² per annum **[W, sheet 1 rows 40–41]**.
- The tool states plainly how each platform-level cost reaches the per-capability picture. The client's method is an **even split across capability columns** **[W]**; where a category includes the Analysis/Consulting column in its divisor and another does not, the tool says so rather than hiding it.
- The apportioned amount appears against each capability, labelled as allocated rather than directly incurred.

---

**US-06 · Record non-variable income on the client's four lines** — Must · 5 pts · F3

> **As** a platform custodian
> **I want** to record the recurrent income my platform receives
> **so that** users are not charged for costs that are already subsidised.

**Acceptance criteria**

- Income is entered on four separate lines: **UWA GP / in-kind**, **State**, **Federal (incl. NCRIS)**, **Other (e.g. philanthropic)** **[W, sheet 1 rows 47–50; G, Step 1]**.
- The tool shows which lines feed which formula — UWA money reduces the UWA Researcher rate only; non-UWA money reduces both the UWA Researcher and the APFR rate **[requirements §4]**.
- The tool states the guide's exclusion plainly: **user fees of any description are not entered here** **[G, Step 1]**.
- Entering nothing is valid; not every platform receives support **[K §2]**.
- The section shows the arithmetic shape: total operating cost, less non-variable income, equals the amount to recover in user fees.
- A justification field records where the income comes from and how long it is committed for **[F9]**.

---

**US-05 · Pre-fill salaries from a staff classification** — Should · 5 pts · F14

> **As** a platform custodian
> **I want** to pick "academic, level X" or "professional staff, level 8" and have the salary filled in
> **so that** I do not have to look it up, and cannot forget to include it.

**Acceptance criteria**

- Selecting a classification and an FTE fraction populates the annual cost from a versioned pay-scale table.
- The populated figure is editable, and an edit is marked as overridden.
- The pay-scale version used is stored on the record, so a 2026 record still explains its own numbers **[N7]**.
- Client's words: "pick academic level X and the salary is filled in, so it cannot be forgotten" **[K §5]**.

---

**US-23 · Add a replacement reserve** — Should · 3 pts · F19

> **As** a platform custodian
> **I want** to set aside a yearly amount towards replacing the equipment
> **so that** the capability is still fundable when this instrument reaches end of life.

**Acceptance criteria**

- I enter replacement value, the percentage to recover and the recovery period; the tool computes the annual contribution and adds it to the cost total.
- The guide's own example reconciles: $500,000 × 25% = $125,000 ÷ 5 years = **$25,000 per year** **[G, Step 1]**.
- The reserve is optional — the guide says "where appropriate" — and is shown as a distinct line, never folded silently into operating costs.
- The tool states the boundary: a forward replacement reserve is in scope, **historical capital expenditure is not** **[G, Step 1]**.

---

### E3 — Establish capacity and forecast utilisation

---

**US-07 · Establish usable annual capacity** — Must · 8 pts · F4

> **As** a platform custodian
> **I want** to work out how much time each capability is actually available
> **so that** the capacity figure reflects reality rather than 365 days.

**Acceptance criteria**

- I state the billable unit — hours, days or samples — and it is used consistently through capacity, utilisation and pricing **[G, Step 2]**.
- Capacity is built from a stated baseline. The two the client uses **[W, sheet 2 rows 3–4]**: machine availability, 365 − 104 weekends − 10 WA public holidays = 251 days × 7.5 h = **1,882.5 h**; staff availability, 230 working days per EBA × 7.5 h = **1,725 h**. Both are configuration, not constants **[N7]**.
- I can deduct maintenance, downtime, compliance requirements, setup and pack-down, and planned outages **[G, Step 2]**.
- Where a capability needs a person physically present, I mark it staff-reliant and its capacity is capped at the FTE allocated to it — the workbook's rule "if reliant on staff, row 17 = row 12" **[W, sheet 2 row 18]**.
- Each deduction takes a note explaining it.
- The resulting usable capacity is shown in the billable unit, with the deductions itemised.

---

**US-08 · Forecast utilisation, and understand it is not capacity** — Must · 5 pts · F5, N3

> **As** a platform custodian
> **I want** to forecast how much each capability will really be used
> **so that** the rate is divided by realistic use and the platform does not run at a loss.

**Acceptance criteria**

- Forecast utilisation is **mandatory**; I cannot proceed without it **[K §5]**.
- The screen states, unmissably, that this is *forecast use*, not capacity, shows my capacity figure beside it, and shows the forecast as a percentage of capacity — the client's example: 1,000 hours of capacity, 500 hours of real use **[K §2]**.
- The tool repeats the guide's warning that this is "one of the most significant assumptions underpinning the pricing model" **[G, Step 2]**.
- Entering a forecast **above** capacity produces a warning and requires a justification, but is not blocked.
- Entering zero is rejected with a plain-language message, not a division error **[N8]**.
- A justification field captures the reasoning, prompted by the guide's own list: historical usage trends, growth or decline in demand, new centres or major grants, strategic appointments, industry demand, competing facilities, regulatory change **[G, Step 2]**.

---

### E4 — Calculate and review charge-out rates

---

**US-09 · See the three calculated rates for each capability** — Must · 13 pts · F6, N1

> **As** a platform custodian
> **I want** to see the minimum sustainable charge-out rate for each user category, for every capability
> **so that** I have a defensible starting point for setting prices.

**Acceptance criteria**

- For **each capability**, three rates are shown: UWA Researcher, APFR, and Commercial **[G, Step 3; W, sheet 3 rows 25–27]**.
- Each rate shows the figures that produced it — total operating cost, which income lines were deducted, the divisor, and whether the 1.35 uplift was applied — so the answer to "why this number?" is on screen **[K §6]**.
- The uplift is labelled as UWA's standard indirect cost recovery under the University Indirect Cost Recovery Policy, covering insurance, legal, finance, library, buildings and IT — not the equipment **[K §2]**.
- Every rate is labelled **GST-exclusive** **[G, GST]**.
- Each user category carries the guide's eligibility wording, so the custodian knows which rate applies to whom — HERDC-aligned research for UWA Researcher; publicly funded entity objectives for APFR; all other non-research activity for Commercial **[G, Step 3]**.
- No formula and no cost coefficient is present in anything sent to the browser **[N1]**.
- Amounts are exact to the cent and rounded once, for display only **[N5]**.
- The engine reproduces the client's worked example — $100.00, $162.00, $202.50 from $150,000 / $20,000 / $30,000 / 1,000 h — to the cent **[N13, requirements §4]**.

---

**US-10 · Change an input and see the effect** — Must · 5 pts · F8

> **As** a platform custodian
> **I want** to go back, change a number, and watch the rates move
> **so that** I can explore before I commit to anything.

**Acceptance criteria**

- I can return to any earlier section, change any value, and return to the rates with the figures updated.
- Nothing is lost by navigating backwards.
- The client's requirement: room to explore, go back, change an input, see the numbers move, before committing **[K §5]**.

---

### E5 — Benchmark, propose rates and test the balance

---

**US-24 · Record the benchmarking I did** — Should · 5 pts · F20

> **As** a platform custodian
> **I want** to record what comparable facilities charge and what I concluded
> **so that** the approver can see my rate was set against the market, not just against my costs.

**Acceptance criteria**

- I can record benchmark sources and their rates, prompted by the guide's list: NCRIS facilities, other Australian universities, commercial laboratories, international facilities **[G, Step 4]**.
- The tool asks the guide's own questions, in its words, depending on whether my calculated rate is above or below market — additional value, cost reduction, utilisation growth, replacement reserves, competitive neutrality **[G, Step 4]**.
- The tool prompts me to allow for differences in location, technical capability, user support, accreditation, data quality, turnaround time and access arrangements **[G, Step 4]**.
- Benchmarking is carried into the sealed record — the guide requires it to be documented before approval **[G, Step 5]**.

---

**US-11 · Propose my own rates** — Must · 5 pts · F7

> **As** a platform custodian
> **I want** to propose sensible round rates rather than the raw calculated figures
> **so that** the price makes sense to the researchers who pay it.

**Acceptance criteria**

- For each capability and each user category I can enter a proposed rate alongside the calculated one.
- The tool shows the variance between calculated and proposed, in dollars and as a percentage.
- The tool states the guide's framing: calculated rates are "a starting point for pricing decisions before considering market conditions or strategic adjustments" **[G, Step 3]**. The client's workbook proposes $50 / $100 / $100 against calculated rates such as $36.64 **[W, sheet 3 rows 35–37]**.
- Where a commercial rate is varied, the tool surfaces the University's obligations under **competitive neutrality** **[G, Step 3]**.
- A justification is required whenever a proposed rate differs from the calculated one **[F9]**.

---

**US-12 · See what my proposed rates do to the balance** — Must · 13 pts · F7

> **As** a platform custodian
> **I want** to see the forecast surplus or deficit my proposed rates produce
> **so that** I know what I am signing up to before I submit.

**Acceptance criteria**

- The forecast year-end position is shown **per capability and for the platform**: proposed rates × forecast utilisation by user type, against total cost less income **[W, sheet 3 rows 39–42]**.
- University overheads recovered are shown separately from the recovery of full economic cost, as the workbook does.
- A deficit is shown clearly as a deficit — the client's own workbook forecasts −$21,372 on one capability and −$16,370 on another at its proposed rates, and that is exactly the kind of thing the client wants surfaced **[W]**.
- The platform roll-up is summed over every capability, with no column silently excluded **[N14]**.
- The position updates when a proposed rate changes.
- A deficit does not block submission; it requires a justification. *(Assumes [Q7](requirements.md#9-open-questions).)*

---

**US-13 · Explain myself throughout** — Must · 3 pts · F9

> **As** a platform custodian
> **I want** space to explain anomalies and how I reached a position
> **so that** somebody reading this in three years understands my reasoning, not just my arithmetic.

**Acceptance criteria**

- Free-text justification is available in every section, not only at the end **[K §5]**.
- The guide's Step 5 checklist is satisfiable from what I have written: costing assumptions documented, utilisation assumptions documented, benchmarking recorded **[G, Step 5]**.
- Justifications are carried into the sealed record and the export, attached to the figures they explain.
- Justifications where the tool requires one (utilisation, rate variance, deficit) are validated as non-empty.

---

### E6 — Seal, export and retrieve the record

---

**US-14 · Review everything before sealing** — Must · 5 pts · F11

> **As** a platform custodian
> **I want** to see the whole record on one page before I commit
> **so that** I can check it rather than trust that I got every screen right.

**Acceptance criteria**

- A single review page shows all inputs, both rate sets for every capability, the variance, the forecast balance and every justification.
- Each item links back to the section that produced it, for correction.
- Missing mandatory fields are listed with links, and sealing is blocked until they are resolved **[N3]**.

---

**US-15 · Seal the record** — Must · 8 pts · F11

> **As** a platform custodian
> **I want** to confirm that I am happy and have the record sealed
> **so that** it is final, on file, and I never have to revisit it.

**Acceptance criteria**

- Sealing requires an explicit confirmation that names the consequence: no further edits.
- A sealed record cannot be edited or deleted through the application, by anyone.
- Seal time, **the person sealing** (from their authenticated identity, **[F15]**) and the method version are recorded on the record **[N6, N7]**.
- Sealed figures are stored as computed — never recalculated on read **[N6]**.
- The client's phrasing: while live the user edits freely; once they confirm they are happy, it is sealed and does not need to be revisited **[K §5]**.

---

**US-16 · Export the record** — Must · 5 pts · F12

> **As** a research infrastructure administrator
> **I want** the sealed record as a document I can file and send
> **so that** it exists outside this application as well as inside it.

**Acceptance criteria**

- A sealed record exports to a self-contained document containing every input, both rate sets, the variance, the balance, all justifications, the benchmarking where recorded, and the method version.
- The export identifies the platform, the period, **who sealed it** (**[F15]**) and when.
- It satisfies the guide's retention requirement: "Supporting documentation should be retained for audit and review purposes" **[G, Step 5]**.
- Format is PDF for the MVP; the client suggested a printout or a generated email and left the format open **[K §6, [Q5](requirements.md#9-open-questions)]**.
- The export renders and prints legibly on A4.

---

**US-17 · Answer "why does it cost $50 an hour?"** — Must · 3 pts · F13

> **As** a research infrastructure administrator
> **I want** to open a record from three years ago and see what the rate was and why
> **so that** I can give a researcher a real answer instead of "we just picked that number."

**Acceptance criteria**

- Records are listed by platform and period, with their sealed date and state.
- Opening a sealed record shows the full picture — the numbers, the reasoning, the method version.
- The record renders correctly even if the calculation method or `k` has since changed **[N6, N7]**.
- This story is the client's stated definition of success **[K §6]** and should be demonstrated at handover.

---

### E8 — Approve, administer and communicate *(stretch)*

---

**US-20 · Approve a record as the delegated authority** — Should · 8 pts · F16

> **As** the delegated authority for this platform's business unit
> **I want** to review a submitted record and approve or return it
> **so that** rates are formally endorsed before they take effect.

**Acceptance criteria**

- A submitted record awaits approval; approving it seals it, returning it reopens it for editing with the approver's comment attached.
- The submission I receive contains what the guide requires me to see: costing assumptions, utilisation assumptions, benchmarking, proposed rates and the forecast balance **[G, Step 5]**.
- The approver and the approval date appear on the record and in the export.
- The guide names the role — "typically the head of the BU responsible for the operating costs of the infrastructure" — but not how it is assigned in the system. Confirm with the client ([Q4](requirements.md#9-open-questions)).

---

**US-21 · Maintain the method configuration** — Should · 5 pts · N7 *(admin screen only)*

> **As** a research infrastructure administrator
> **I want** to update `k`, pay scales, capacity baselines and cost categories without a code change
> **so that** the tool survives a change in UWA policy.

**Acceptance criteria**

- `k`, the pay-scale table, the capacity baselines (1,882.5 h machine, 1,725 h staff) and the cost and income category lists are configuration, versioned with an effective date.
- A change creates a new version; existing sealed records keep the version they were sealed with **[N6, N7]**.
- The active version is visible in the application.
- **N7 itself is a Must and ships in the MVP** — the engine reads all of this from versioned configuration from day one (US-09, US-15). This story adds only the *screen* for editing that configuration without a developer, which is why it is a Should and sits in a stretch epic.

---

**US-25 · Generate the price-change communication** — Could · 5 pts · F21

> **As** a platform custodian
> **I want** a draft notice explaining the new rates
> **so that** my users can plan their grant budgets around them.

**Acceptance criteria**

- From an approved record, the tool drafts a communication covering the guide's three points: why prices have changed, the methodology used, and the importance of sustainable infrastructure funding **[G, Step 6]**.
- The draft states the effective date and is editable before it is sent.
- The tool prompts for notice "well in advance of actual roll out" **[G, Step 6]**.

---

### E9 — Compare and analyse across cycles *(stretch)*

---

**US-22 · Compare this cycle with the last** — Could · 8 pts · F17

> **As** a research infrastructure administrator
> **I want** to see how costs and rates have moved between cycles
> **so that** I can see the trend and sanity-check a new proposal.

**Acceptance criteria**

- Two sealed records for the same platform can be shown side by side, by cost category, by capability and by rate.
- Movements are shown in dollars and percent.
- Proposed by the team, welcomed by the client — "if you want to give that a crack, even better" — while noting the calculator they had shown was a different kind and its logic may not transfer **[K §8]**.

---

## 5. MVP definition

**In the MVP:** the eighteen **Must** stories — **US-01 to US-04, US-06 to US-19** — one
platform with its capabilities, one cycle, the complete path from signing in to retrieving a
sealed record. **110 points.**

**Stretch, in this order if time allows:** US-24 (benchmarking record, F20), US-20 (delegated
approval, F16), US-05 (salary pre-fill, F14), US-23 (replacement reserve, F19), US-21 (method
configuration screen), US-22 (cycle comparison, F17), US-25 (price-change communication, F21).
All seven are Should or Could and none of them blocks the vertical slice.

**Out of scope entirely:** everything marked so in
[requirements §7](requirements.md#7-scope).

The split matches [requirements §7](requirements.md#7-scope) exactly: MVP = F1–F13, F15 and
N1–N9, N11, N13, N14; stretch = F14, F16, F17, F19, F20, F21, N10, N12.

**The demonstration that proves it works.** Priya signs in, starts a cycle for a
seven-capability microscopy platform, enters costs and income, builds capacity and forecasts
utilisation, sees three calculated rates for every capability, proposes round numbers, sees the
resulting balance, justifies it, seals the record and exports the PDF. Mark opens that record
and reads the answer to "why does it cost $50 an hour?" — and the engine reproduces the client
guide's worked example, $100.00 / $162.00 / $202.50, to the cent **[N13]**.

## 6. Traceability

Every requirement in [requirements §6](requirements.md#6-requirements) appears exactly once
below. Non-functional requirements that are enforced structurally rather than by a story say
where they are enforced, so that "no story" is never mistaken for "no coverage".

| Requirement | Stories | Also enforced by |
| --- | --- | --- |
| F1 guided three-section form | US-01 (entry), and the ordering of US-03 → US-08 → US-09 | Route structure; [architecture §2](architecture.md#2-system-shape) |
| F2 costs at capability and platform level | US-03, US-04 | |
| F3 non-variable income, four lines | US-06 | |
| F4 capacity | US-07 | |
| F5 forecast utilisation mandatory | US-08 | |
| F6 per-capability rate calculation, server-side | US-09 | |
| F7 proposed rates and balance | US-11, US-12 | |
| F8 change inputs and see the effect | US-10 | |
| F9 justifications throughout | US-13, and criteria within US-03, US-06, US-07, US-08, US-11, US-12 | |
| F10 draft save and resume | US-01, US-02 | |
| F11 seal | US-14, US-15 | |
| F12 export | US-16 | |
| F13 retrieve past records | US-01, US-17 | |
| F14 salary pre-fill | US-05 | |
| F15 authentication and identity | US-19 | Criteria in US-02, US-15, US-16 |
| F16 delegated approval | US-20 | |
| F17 dashboard | US-22 | |
| F19 replacement reserve | US-23 | |
| F20 benchmarking record | US-24 | |
| F21 price-change communication | US-25 | |
| N1 logic out of reach | US-09, US-18 | [architecture §2, §3](architecture.md#3-the-calculation-engine) |
| N2, N3 validation and mandatory fields | US-18, US-08 | |
| N4 usable without training | — | [architecture §1](architecture.md#1-what-the-architecture-has-to-achieve); verified by acceptance criterion 3 in [requirements §6.3](requirements.md#63-acceptance--how-we-will-know-the-mvp-is-right) |
| N5, N6, N7 exact, reproducible, versioned | US-09, US-15, US-17, US-21 | Engine rules R2, R3, R5, R6 in [architecture §3](architecture.md#3-the-calculation-engine) |
| N8 divide-by-zero | US-08, US-18 | Engine rule R4 |
| N9 transport and secrets | — | [architecture §6](architecture.md#6-security); `.gitignore` |
| N10 audit | US-15, US-19 | [architecture §4, §5](architecture.md#4-data-model) |
| N11 data handling | — | [architecture §6](architecture.md#6-security); [`reference/client/README.md`](../../reference/client/README.md) |
| N12 WCAG 2.1 AA | — | Applied across every screen; no single story owns it |
| N13 golden-file verification | US-09 | [architecture §3, Verification](architecture.md#3-the-calculation-engine); CI gate in [architecture §10](architecture.md#10-delivery-approach) |
| N14 consistent column contracts | US-03, US-12, US-18 | [architecture §3](architecture.md#3-the-calculation-engine) |

---

## Change log

| Version | Date | Change |
| --- | --- | --- |
| 1.0 | 14 Aug 2026 | First draft, derived from the 29 July kickoff minutes. Not yet reviewed by the client. |
| 1.1 | 14 Aug 2026 | MVP realigned to [requirements §7](requirements.md#7-scope): US-05 and US-19 left the MVP, which became seventeen Must stories at 95 points. US-20 raised to Should. Story priority formally inherits from the requirement. |
| 2.0 | 14 Aug 2026 | **Realigned to requirements v2.0, which was rewritten against the client's own documents.** US-19 (sign-in) returns to the MVP as a **Must**, resolving the contradiction where US-02, US-15 and US-16 required an identity the MVP did not provide; E7 renamed and US-19 moved to the front of the story list, because nothing else works without it. Rates are now **per capability** — US-09 and US-12 re-estimated 8 → 13 points to reflect it. Terminology follows the client's guide: capability, billable unit, APFR, custodian. US-06 rewritten for **four** income lines. US-07 carries the client's two capacity baselines and the staff-FTE cap. Three new stories from guide steps 1, 4 and 6: **US-23** replacement reserve, **US-24** benchmarking record, **US-25** price-change communication. Dr Chen is no longer *(inferred)* — the guide names the delegated authority. US-18 and US-12 gained criteria for **N14**. §6 traceability rebuilt so every requirement appears exactly once, with structural enforcement named where no story owns it. **MVP: eighteen stories, 110 points.** |
