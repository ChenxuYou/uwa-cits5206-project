# Client Kickoff — Walkthrough of the Costing & Pricing Model

**Date:** 29 July 2026
**Duration:** approximately 34 minutes
**Client:** UWA Research Infrastructure — two representatives
**Team:** three members attended; two were absent and were briefed from these minutes
**Purpose:** first meeting with the client. Understand the costing and pricing problem, see
the existing Excel tool, and establish what they want built.

> **About this record.** These minutes are written from the meeting recording. The recording
> and its raw transcript are **not committed** — see [`.gitignore`](../../.gitignore) §3 — so
> this file is the project's record of the meeting. Figures and quotations below were checked
> against the recording. Client representatives are referred to by role rather than by name;
> their names are in the team's Teams area. Individual team attendance is not listed because
> the recording carries no speaker labels; confirm it at the next stand-up if it is needed for
> the assignment.

> ### ⚠ Annotation added 14 August 2026 — do not cite the figures in §2
>
> These minutes are left **unedited**, as a record of what was said. But when the client's own
> documents arrived, the numbers in §2 Step 2 and §2 Step 5 could not be found in any of them,
> and one of them is internally inconsistent: a calculated rate cannot be both **$3,291** and
> "five cents on top of **$42**". They were most likely mis-heard, or fused from two separate
> moments in a 34-minute walkthrough.
>
> **The figures in §2 Step 2 and §2 Step 5 are withdrawn for all downstream use.** The
> authoritative worked example is the client's costing guide, restated in
> [`requirements.md` §4](../spec/requirements.md#4-the-calculation): $150,000 operating costs,
> $20,000 UWA in-kind, $30,000 WA Gov, 1,000 hours → **$100.00 / $162.00 / $202.50** per hour.
>
> Everything else in these minutes — the method, the three formulas, `k = 1.35`, the seven
> capabilities, the design requirements, the IP position, ways of working — was confirmed
> against the client's documents and stands. See
> [`requirements.md`, *Source precedence*](../spec/requirements.md), which now ranks spoken minutes
> below written client material for exactly this reason.

---

## 1. Background — what the client does and why this matters

UWA operates **research infrastructure**: enabling technologies that support researchers.
The client gave electron microscopes, a human MRI machine, radio astronomy telescopes and
mobile drones used to assess plant phenotypes as examples. This equipment is expensive to
acquire and expensive to run, so part of the cost is passed on to the users, who embed it in
grant funding and then buy time on the instrument.

Two constraints follow, and they drive the whole project:

- **UWA is a publicly funded institution.** Much of this infrastructure is paid for with state and federal government support, so there must be transparency about how it is costed and priced.
- **The method must be identical across the institution.** Every platform operates differently and draws on different funding, but the protocol applied to all of them has to be the same — "apply the same logic for everything".

The client noted that the research funding environment across Australia has shifted in recent
years, which is why every university is now uplifting the frameworks it uses to look at
research infrastructure. That is why this is being developed now.

**The objective is break-even, not profit.** In the client's words: work out how much to
charge per unit of time so the equipment is sustainable for UWA and its operation can continue
to be funded — not to recover replacement cost, and not to make money.

## 2. The costing method

The client walked through the logic. They were explicit that the team does **not** need to
master the reasoning behind it — the tool takes the inputs and applies it.

### Step 1 — Total operating costs

A series of budget lines, which the client's principles prompt platform leaders to think
through. Costs are captured at two levels:

- **Per instrument.** The demonstration facility held **seven different instruments**. Some are cheaper to run than others, so the breakdown is a distinction worth keeping.
- **Per platform.** Costs that cannot meaningfully be attributed to one instrument — technician time, and the platform leader who works across all of them.

### Step 2 — Non-variable income

Some platforms — not all — receive income from the university or from government to support
operations. It never covers the full cost, but it is taken into account.

On the dummy platform demonstrated — **figures withdrawn, see the annotation above**:

| | Amount |
|---|---|
| ~~Total to recover with no subsidy~~ | ~~$380,000~~ |
| ~~Less non-variable income~~ | ~~($230,000)~~ |
| ~~**To recover in user fees**~~ | ~~**$150,000**~~ |

### Step 3 — Capacity, then utilisation

First the platform states its **charging unit** — hours, days or weeks — and its usable
capacity across the year. The baseline is 365 days less weekends and public holidays, then
reduced by whatever constrains that particular instrument: maintenance downtime, a staff
member who must be physically present and whose FTE therefore caps availability, and in some
cases weather, where an instrument can only operate in the right conditions.

Then the platform leader forecasts **utilisation**, reflecting on historic use and on what
might change. This is the critical figure:

> While the capacity might be a thousand hours per year, in practice it might only be used
> 500 hours a year.

**Forecast utilisation, not capacity, is the divisor.**

### Step 4 — The three charge-out rates

Three categories of user may buy time on an instrument, and each is charged differently:

| User type | Rate |
|---|---|
| UWA internal researcher | `(C − I_uwa − I_gov) / U` |
| Australian researcher at another institution | `((C − I_gov) / U) × k` |
| Commercial / industry user | `(C / U) × k` |

Where `C` is total operating cost, `I_uwa` and `I_gov` are the university and government
contributions, `U` is forecast annual utilisation, and `k = 1.35`.

`k` is UWA's **standard indirect cost recovery**, applied whenever an external party engages
UWA services: a 0.35 uplift covering insurance, legal, finance, library, buildings and IT
infrastructure — but not the equipment or the capability itself.

### Step 5 — Proposed rates and the resulting balance

The raw calculated rates are reference figures, not prices. ~~The client's example produced
$3,291, and made the point that nobody is going to charge five cents on top of $42.~~
*(Figures withdrawn — see the annotation above. The point stands: a calculated rate is rounded
to something a user can understand. The client's own workbook proposes $50 / $100 / $100
against calculated rates such as $36.64.)* The platform leader is expected to step back,
benchmark, think about the user experience, and **propose their own rates**.

The tool must then show what those proposed rates do to the platform's forecast balance. ~~In
the demonstration, the proposed rates still left a **$15,000 deficit** at year end~~ — a
deficit at the proposed rates is exactly the kind of thing the client wants surfaced before a
decision is made. *(The specific figure is withdrawn; the client's workbook does carry deficits
at its proposed rates, of −$21,372 and −$16,370 on two capabilities.)*

## 3. The current tool and what is wrong with it

The client's existing tool is an Excel workbook. Their assessment:

> Our problem with this is it's functional, but nobody can really use it because it's easy to
> break.

Elaborated over the meeting:

- It is complex, and it does not look as complex as it is.
- Sharing it gives the recipient control to delete formulas and break the calculation. It is not foolproof.
- It is not user friendly.
- The calculations should happen behind the scenes.

Structurally the workbook is already three sections, with **yellow cells for the user to fill
in and blue cells holding the logic**. The client was clear that only the yellow sections are
being asked of the user. That structure is the shape of the product.

## 4. Who uses it

**Platform leaders** — not the researchers who buy time.

- Academic or professional staff who manage a facility or a piece of equipment: bookings, maintenance, training.
- They are best placed to forecast utilisation because they know how popular their equipment is.
- All UWA staff.
- They run the exercise on a **three-to-five year cycle** to set rates for the period ahead, then review and repeat.

The client also observed that these are busy, technical people — "I do microscopes, I'm really
good at microscopes" — for whom this is administrative work they tolerate rather than enjoy.
The tool has to make it as easy as possible.

## 5. What the client asked us to build

A guided web application. The client's own description of the flow:

1. **Costs.** Ask what it costs to run the platform — materials such as gas and water, staffing and how much of it the university funds, maintenance and service contracts, other operating costs. Produce a total.
2. **Utilisation.** Ask how much it has been used and how much it is expected to be used.
3. **Rates.** Return the charge-out rates, with space to adjust and see the effect.

Design requirements stated in the meeting:

- **Walk the user through it.** Sequential pages that prompt, rather than a spreadsheet.
- **Pre-fill what can be pre-filled.** Pick "academic, level X" or "professional staff, level 8" and the salary is filled in, so it cannot be forgotten.
- **Mandatory fields.** Utilisation in particular — the user cannot proceed without it.
- **Type validation.** Enter a word where a percentage belongs and the tool rejects it. The client's example of the failure to prevent: a maintenance contract typed as $200,000 instead of $20,000 because of one extra zero.
- **They can't break it, they can't mess it up.** The logic stays out of the user's reach.
- **Room to explore.** Go back, change an input, see the numbers move, before committing.
- **Justification text throughout.** Space to explain anomalies and how a position was reached.
- **Submit, then seal.** While the document is live the user can edit freely; once they confirm they are happy, it is sealed and does not need to be revisited.

## 6. The record — the artefact they actually want

This came up repeatedly and is the heart of the requirement. The output — all inputs, the
calculation and the justifications — must be saved somewhere visible, formally approved, and
put on file.

> If someone comes to us and says "why does it cost $50 an hour for me to use?", we want to be
> able to say: well, it costs $100,000 a year to run, this is how many hours a year it's going
> to be used, we divide one by the other — $50 an hour. That's the reason we charge that price.

And the reason it must survive:

> We can't say "we don't know, it's $50 an hour because we just picked that number". That
> doesn't sound very good and that doesn't inspire confidence in the people that are using our
> services.

Records are kept across cycles. In three years the client wants to open the old record, see
what the pricing model was and why, and set the new one against it. The client suggested a
printout or a generated email; format is open.

## 7. Data sensitivity

Asked directly whether the data is confidential:

- Relatively sensitive — visible to people within the organisation and those granted access.
- **Not commercially confidential.** UWA is publicly funded, everything is subject to FOI, and the entire goal is transparency about pricing.
- But not something to promote widely, **especially while it is still being worked on**.

Access control was raised by the team and acknowledged by the client. It has not been
specified.

## 8. Scope

- **Standalone website first.** Integration with existing UWA systems "would be ideal, but I think that might be a bit challenging" — deferred. Get a functional website working first.
- **A dashboard was proposed by the team** — comparing costs over past years, across suppliers and cost categories. The client's response: "if you want to give that a crack, even better", while noting they had shown a different kind of calculator whose logic may not transfer.
- The client shared a **project cost calculator** used for funding research projects as a user-experience model — a form with prompts that produces a number. Not the same calculation, but the right shape.

## 9. Intellectual property and portfolio use

Raised by the team, since this is a capstone and members want to show the work.

- The team writes the code and **owns that code**.
- The client has no issue with the team sharing the product they have produced, including in portfolios.
- The **costing logic is UWA's**. It is not secret, but building the calculator and then going out and selling it would not be appropriate, because the overarching IP would be **jointly owned**.

## 10. Ways of working

- **No fixed weekly cadence.** The client proposed the team go away, digest, and schedule a meeting once they have questions — expecting a lot of them.
- **Support tapers.** Heavier up front, easing off later.
- **Wednesdays** are generally good for the client; both representatives are on campus most days, so in person or Teams both work.
- A **Teams group chat** was agreed for asynchronous questions, with the client managing expectations that replies will not always be immediate.

---

## Decisions

| # | Decision |
|---|---|
| D1 | Build a guided web application, not a spreadsheet replacement. Three sequential sections mirroring the workbook: costs → utilisation → rates. |
| D2 | Calculation logic is hidden from the user. Mandatory fields and type validation throughout. |
| D3 | The submitted record — inputs, results and justifications — is sealed and exportable, and must be readable years later. |
| D4 | Standalone web application. Integration with UWA systems is out of scope for now. |
| D5 | The team owns the code and may use it in portfolios. The costing logic and overarching IP remain UWA's, jointly. |
| D6 | Meetings on request rather than a fixed weekly slot, Wednesdays preferred, with a Teams group chat for async questions. |

## Actions

| # | Action | Owner |
|---|---|---|
| A1 | Client to share the walkthrough material as documents, since the live pages are staff-protected | Client |
| A2 | Team to create the Teams group chat including both client representatives | Team |
| A3 | Team to digest the material and return with a consolidated question list | Team |
| A4 | Brief the two members who could not attend | Team |

## Open questions

- How costs are allocated between the per-instrument and per-platform levels.
- How utilisation is split across the three user types.
- How multi-year cycles are handled, and how a sealed record is superseded rather than overwritten.
- What access control the client needs — who may see and approve a record.
- What format the sealed record takes: printout, generated email, or something else.
- Whether AI tools may be used on client material. **Asked; not yet answered.**

---

*Written from the meeting recording, 29 July 2026. The recording and raw transcript are held
in the team's Teams area and are deliberately excluded from this repository.*
