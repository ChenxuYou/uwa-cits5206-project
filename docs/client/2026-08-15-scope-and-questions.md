# Research Infrastructure Costing & Pricing Tool

## Scope summary and five questions

**For:** UWA Research Infrastructure
**From:** the CITS5206 capstone team — Chenxu You, Yichen Zhao, Wenmin Luo, Dai Lam La La,
Jaswanth Vericherla
**Date:** 15 August 2026

---

Thank you again for the walkthrough on 29 July, and for sharing the costing & pricing guide and
the calculator. Between them they answered several questions we had planned to ask — how costs
are split across capabilities, how utilisation enters the calculation, and which billable units
are permitted — so those have gone from the list at the end of this document.

This document has two parts. **Part 1** sets out what we understand we are building, what we
are deliberately not building, and how we will show it works; it ends with two things we would
like you to confirm. **Part 2** is a short list of questions your material does not answer.

**Part 2 does not hold up Part 1.** Every question carries the answer we will use if we do not
hear back, so nothing stalls while we wait, and a partial reply is genuinely useful to us. Where
a different answer would change something in Part 1, we say so under the question itself — in
each case it is one line, not a reopening of the scope.

---

# Part 1 — What we are building

## The outcome we are building towards

A guided web application that takes a platform custodian through the University's costing and
pricing method and produces a record of the result that can be filed, approved, and read again
years later.

You described the outcome this way at our meeting, and it is the sentence we are building
against:

> If someone comes to us and says "why does it cost $50 an hour for me to use?", we want to be
> able to say: well, it costs $100,000 a year to run, this is how many hours a year it's going
> to be used, we divide one by the other — $50 an hour. That's the reason we charge that price.

Everything below follows from your guide and your calculator. Where the two disagreed, we
followed the guide.

## What the tool will do

**1. Costs.** The custodian enters what it costs to run the capability for a year — staffing,
consumables, maintenance and service contracts, utilities and other operating costs — captured
at both capability and platform level, following the cost lines your calculator already carries.
Then the four lines of non-variable income that offset them, as the calculator sets them out:
UWA GP/in-kind, State, Federal including NCRIS, and Other.

**2. Capacity and utilisation.** The custodian picks the billable unit — hours, days or samples
— works out annual capacity from the standard baselines less whatever constrains that
capability, and then forecasts realistic utilisation. **Forecast use is a mandatory field and is
kept visibly separate from capacity**, because it is the divisor and it is the number that is
easiest to get wrong.

**3. Rates.** The tool returns the three minimum sustainable charge-out rates — UWA researcher,
APFR, commercial — **for every capability on the platform**, with the figures behind each one
shown. The custodian can then propose their own rounded rates, immediately see the surplus or
deficit that follows, and record why.

**Throughout:** users sign in, so no record is anonymous and every record carries who made it.
Every field is validated for type and range — a maintenance contract cannot be entered as
$200,000 instead of $20,000 without challenge. Free-text justification boxes sit alongside the
numbers, because the rate has to be defensible, not only correct. Work can be saved and resumed,
and any input can be changed to see its effect before anything is committed.

**At the end:** the custodian submits, and the record is **sealed** — inputs, calculated rates,
proposed rates and justifications become immutable — then exported to a file that can be filed
and read in three years' time. Past sealed records can be listed and reopened, so a new cycle
can be set against the last one.

**The calculation runs on the server and is never sent to the browser.** There is no formula for
a user to overwrite, and no way to break the tool by using it.

## How we will prove it is right

Before we write a single screen, the calculation engine is tested against **your own worked
example**: $150,000 operating costs, $20,000 UWA in-kind, $30,000 WA Government support, 1,000
forecast hours — producing **$100.00, $162.00 and $202.50** per hour. The test is automated and
must pass to the cent.

We then run one complete path end to end: a custodian signs in, starts a cycle for a
multi-capability platform, enters costs and income, builds capacity, forecasts utilisation, sees
three rates per capability, proposes round numbers, sees the resulting balance, justifies it,
seals the record, and exports it. Someone else opens that record and finds the answer to *"why
does it cost $50 an hour?"* — with every figure behind it.

## What we are not building

Naming these matters as much as the list above, because it is what makes the rest deliverable in
one semester:

- **Integration with UWA finance, HR or booking systems.** Deferred following your own view on 29 July that integration would be ideal but challenging — a working standalone website first.
- **Billing or invoicing of actual usage.** The tool sets rates; it does not charge anyone.
- **A researcher-facing price lookup.** The user is the custodian, not the researcher buying time.
- **Migration of your historical spreadsheets.** Past cycles are not imported.
- **Multi-institution operation.** UWA only.

## What we will attempt only if there is time

These are real intentions, not commitments. We would rather under-promise and deliver the list
above completely than spread ourselves across all of it:

- Salary pre-fill from academic and professional staff levels
- **Formal approval by a delegated authority as a step inside the tool** — see the note below
- An optional replacement reserve carried into the cost total
- Recording the benchmarking behind a proposed rate (your guide's Step 4)
- A dashboard comparing costs across years, categories and suppliers
- Generating the Step 6 price-change communication

**One thing inside that list we want to flag rather than bury.** Your guide makes approval by a
delegated authority part of the method, and we have it as a stretch item — the core version
records who created, submitted and sealed a record, but does not route it to an approver. **If
in-system approval is essential rather than desirable, tell us and we will move it into the core
and drop something else.** We would rather hear that now than at handover.

## What we are asking you to confirm

Two things, and neither needs a meeting:

1. **The scope above is the right thing to build** — what is in, what is out, and what is only attempted if time allows.
2. **The ownership position we recorded is correct:** the costing and pricing method is UWA's; the team writes and owns the source code, and each member may show their own contribution in a personal portfolio; the overarching intellectual property in the tool is **held jointly by UWA and the team**, so neither party would sell it onward without the other — and, as you put it on 29 July, selling it onward would not be appropriate in any case.

If both are right, **a one-line reply saying so is all we need.** If either is wrong, tell us
which and we will change it — that is a better outcome than a signature on something
inaccurate.

We would be grateful for a reply by **Tuesday 18 August**, because our project specification is
due on 25 August and your confirmation forms part of it. If we have not heard by then we will
send one reminder on Wednesday 19 August, and otherwise leave you alone.

### Sign-off

**One signature is enough.** The second column is there only if the two of you would rather
confirm jointly.

| | Signatory | Second signatory *(optional)* |
| --- | --- | --- |
| Name | | |
| Role | | |
| Date | | |
| Confirmation 1 — scope | | |
| Confirmation 2 — ownership | | |

---

# Part 2 — Five questions

Again: **none of these holds up Part 1.** Each carries the answer we will use if we do not hear
back. If it is easier to talk any of them through, we are happy to take twenty minutes on a
Wednesday.

## 1. Who is allowed to see and approve a record?

You confirmed the data is UWA-internal and subject to FOI, not commercially confidential, but not
something to promote widely while the work is still in progress. We need to turn that into
something the software can enforce.

We propose three roles: a **custodian**, who creates and submits records for their own platform;
a **delegated authority**, who sees what is submitted to them; and an **administrator**, who is
not restricted to a single platform. For this first version, sign-in would be local to the
application rather than UWA single sign-on. We will build and demonstrate the whole path on one
platform and its capabilities, and the roles are defined so that further platforms can be added
without revisiting them.

**One thing to be exact about.** In the core version, "delegated authority" is a matter of *who
can see a record* — it is not a routed approval step. Approval as a step inside the tool is the
stretch item we flagged in Part 1, and the two travel together: tell us approval must be in the
core, and this role gains the action as well as the view.

Your guide's Step 5 says the approver is "typically the head of the BU responsible for the
operating costs of the infrastructure". What we cannot tell from here is whether *typically*
holds for these platforms and who that is in practice — that, and whether three roles is the
right set, is what we need from you.

**If you need UWA single sign-on**, we would treat that as a system integration — the same class
of work Part 1 defers — rather than something we could absorb quietly. It is not out of the
question, but we would come back to you with something to trade for it. Additional roles, by
contrast, are cheap.

> **If we do not hear back:** three roles with local sign-in, the delegated authority as a
> viewing role only.

## 2. How do multi-year cycles work?

Rates are set on a three-to-five year cycle, and you want to open an old record and set the new
one against it. We propose that each record carries a validity period, that a new cycle
**supersedes** the previous one by reference, and that **nothing is ever overwritten or
deleted** — the old record stays readable exactly as it was approved.

Two questions inside that: **how long is a typical validity period**, and **does a sealed record
ever need to be amended within its own cycle** — a correction, or a mid-cycle rate change —
rather than replaced by the next one? An amendment and a supersession are different things in the
software, so it helps to know early whether you need both.

**If a sealed record must be amendable within its own cycle**, Part 1's statement that a sealed
record is immutable gains one exception: the record would keep a visible amendment history
rather than a single frozen version. That is a change to how records are stored, which is the
one thing that is far cheaper to know now than later.

> **If we do not hear back:** validity period is entered by the custodian, supersession only, no
> mid-cycle amendment.

## 3. What should the sealed record look like, and where does it get filed?

You mentioned a printout or a generated email. We propose the tool generates a **PDF** — every
input, both sets of rates, the variance and every justification — plus a permanent link to the
same record inside the tool.

What we cannot see from here: **where the record is filed once it exists**, and whether that
destination expects a particular format, template or set of fields. If there is an existing UWA
document template these should match, we would rather build to it than to our own layout.

> **If we do not hear back:** a PDF of our own design, plus a permanent in-tool link.

## 4. One place where the guide and the calculator disagree — which governs?

For commercial users, your guide's Step 3 gives the rate as total operating cost divided by
forecast utilisation, uplifted by the 1.35 indirect cost recovery, **with no income deducted**.
The calculator's commercial row deducts federal and other income before applying the uplift.

They produce different numbers: following the guide gives a **higher** commercial rate than the
calculator currently does, because there is less deducted from the cost base. Our reading is that
the **guide governs** — the commercial rate should not be subsidised by grant income — but this
changes a published price, so we would rather ask than assume.

**If you tell us the calculator governs**, one formula inside the engine changes and nothing else
in Part 1 does. It is a cheap change now and an expensive one once the engine is written, which
is why we are asking before we write it.

> **If we do not hear back:** we follow the guide.

## 5. And more generally — guide or calculator?

That is not the only place the two differ. Working through them line by line, we found several
points where the calculator's arithmetic does not match the method the guide describes, and a few
we think are simply errors in the spreadsheet. We are happy to send that list separately if it is
useful to you — it is a short document and it may be worth having regardless of this project.

The question for us is what the tool should do: **follow the guide**, which is your policy, or
**reproduce the calculator**, so that new figures reconcile against rates that have already been
published from it?

Our proposal is to follow the guide, and — wherever a new figure is compared with an existing one
— show both, so the difference is visible and explainable rather than silent.

**If you tell us the calculator governs**, we would be reproducing arithmetic we believe to be
wrong, so we would build it and record that we raised it. Nothing else in Part 1 moves either
way.

> **If we do not hear back:** we follow the guide and show both figures on any comparison.

---

Thank you — we know this is administrative work on top of your own, and we have tried to make it
answerable in one reply.

**Chenxu You and Wenmin Luo**, on behalf of the CITS5206 capstone team
The University of Western Australia · 15 August 2026

*Our full requirements, user stories and architecture notes are in the project repository, and we
are happy to walk through any part of them.*
