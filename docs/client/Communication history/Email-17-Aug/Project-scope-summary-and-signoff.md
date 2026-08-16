# Project Scope Summary and Signoff

**For:** UWA Research Infrastructure
**From:** CITS5206 Group 13 team — Chenxu You, Yichen Zhao, Wenmin Luo, Dai Lam La La, Jaswanth Vericherla
**Date:** 17 August 2026

---

This document sets out what we understand we are building, what we are deliberately not building, and how we will prove it works. It ends with two things we'd like you to confirm — a one-line reply is all that's needed if both are correct.

*(A separate short document lists five open questions our material doesn't answer — none of them hold up this scope.)*

## The outcome we're building towards

A guided web application that takes a platform custodian through UWA's costing and pricing method and produces a record that can be filed, approved, and read again years later.

You described the target outcome at our meeting:
> If someone comes to us and says "why does it cost $50 an hour for me to use?", we want to be able to say: well, it costs $100,000 a year to run, this is how many hours a year it's going to be used, we divide one by the other — $50 an hour. That's the reason we charge that price.

Everything below follows your guide and calculator. Where they disagreed, we followed the guide.

## What the tool will do

1. **Costs.** Custodian enters annual running costs (staffing, consumables, maintenance/service contracts, utilities) at capability and platform level, plus the four non-variable income lines: UWA GP/in-kind, State, Federal (incl. NCRIS), and Other.
2. **Capacity and utilisation.** Custodian picks the billable unit (hours/days/samples), derives annual capacity from standard baselines minus constraints, then forecasts realistic utilisation. **Forecast use is mandatory and kept visibly separate from capacity** — it's the divisor, and the number easiest to get wrong.
3. **Rates.** Tool returns the three minimum sustainable charge-out rates — UWA researcher, APFR, commercial — **for every capability**, with the underlying figures shown. Custodian can propose their own rounded rates, see the resulting surplus/deficit, and record why.

**Throughout:** users sign in (no anonymous records); every field is validated for type/range (e.g. a $200,000 entry can't silently mean $20,000); free-text justification boxes sit next to the numbers; work can be saved, resumed, and changed before anything is committed.

**At the end:** custodian submits and the record is **sealed** — inputs, rates, justifications become immutable — then exported to a file. Past sealed records can be listed and reopened, so a new cycle can be set against the last one.

**The calculation runs server-side only** — never sent to the browser, so there's no formula to overwrite.

<div style="page-break-after: always;"></div>

## How we'll prove it's right

Before building any screens, the calculation engine is tested against **your own worked example** ($150,000 costs, $20,000 UWA in-kind, $30,000 WA Government, 1,000 forecast hours → **$100.00, $162.00, $202.50** per hour). This test is automated and must pass to the cent.

We then run one complete end-to-end path: sign in → start a cycle → enter costs/income → build capacity → forecast utilisation → see three rates per capability → propose rounded rates → see balance → justify → seal → export. Someone else opens that record and finds the full answer to "why does it cost $50/hour?"

## What we're not building

- **Integration with UWA finance, HR or booking systems** — deferred per your 29 July view that this would be ideal but challenging; standalone website first.
- **Billing or invoicing of actual usage** — the tool sets rates, doesn't charge anyone.
- **A researcher-facing price lookup** — the user is the custodian, not the researcher.
- **Migration of historical spreadsheets** — past cycles aren't imported.
- **Multi-institution operation** — UWA only.

## Only if there's time

Real intentions, not commitments — we'd rather deliver the above completely than spread thin:

- Salary pre-fill from staff levels
- **Formal approval by a delegated authority as an in-tool step** *(see flag below)*
- Optional replacement reserve in the cost total
- Recording benchmarking behind a proposed rate (guide's Step 4)
- Dashboard comparing costs across years/categories/suppliers
- Generating the Step 6 price-change communication

**Flag:** your guide makes delegated-authority approval part of the method; we have it as a stretch item. The core version records who created, submitted and sealed a record, but doesn't route it to an approver. **If in-system approval is essential, tell us and we'll move it into the core and drop something else.**

<div style="page-break-after: always;"></div>

## What we're asking you to confirm

1. **The scope above is right** — what's in, what's out, what's stretch-only.
2. **The ownership position is correct:** the costing/pricing method is UWA's; the team writes and owns the source code (each member may show their contribution in a personal portfolio); overarching IP in the tool is **held jointly by UWA and the team** — neither party sells it onward without the other (and, as you noted on 29 July, selling it onward wouldn't be appropriate anyway).

If both are right, a one-line reply is all we need. If either is wrong, tell us and we'll change it.

**We'd be grateful for a reply by Tuesday 18 August** — our project specification is due 25 August and your confirmation is part of it. One reminder will follow on Wednesday 19 August if we haven't heard back; otherwise we'll leave you alone.

---

## Sign-off

**One signature is enough.** The second column is only for a joint confirmation.

|                            | Signatory | Second signatory *(optional)* |
|----------------------------|-----------|-------------------------------|
| Name                       |           |                                |
| Role                       |           |                                |
| Date                       |           |                                |
| Confirmation 1 — scope     |           |                                |
| Confirmation 2 — ownership |           |                                |

