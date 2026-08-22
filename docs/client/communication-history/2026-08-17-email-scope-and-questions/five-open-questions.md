# Five Open Questions

**For:** UWA Research Infrastructure
**From:** CITS5206 Group 13 team — Chenxu You, Yichen Zhao, Wenmin Luo, Dai Lam La La, Jaswanth Vericherla
**Date:** 17 August 2026

---

Thank you for the 29 July walkthrough and for sharing the costing & pricing guide and calculator — together they answered several questions we'd planned to ask (how costs split across capabilities, how utilisation enters the calculation, which billable units are permitted), so those are already off this list.

**None of the five questions below hold up the scope document** — each carries the default answer we'll use if we don't hear back, so nothing stalls while we wait. Where a different answer would change something in the scope, we say so in one line under the question. Happy to talk any of them through in person.

## 1. Who is allowed to see and approve a record?

You confirmed the data is UWA-internal and FOI-subject, not commercially confidential, but not something to promote widely mid-progress. We need to turn that into something the software enforces.

We propose three roles: **custodian** (creates/submits records for their own platform), **delegated authority** (sees what's submitted to them), and **administrator** (not restricted to one platform). Sign-in would be local to the application rather than UWA single sign-on for this first version. We'll build and demonstrate the full path on one platform, with roles defined so more platforms can be added later without revisiting them.

**To be exact:** in the core version, "delegated authority" only means *who can see* a record — it's not a routed approval step. That's the stretch item flagged in the scope document; if approval must be in the core, this role gains the action as well as the view.

Your guide's Step 5 says the approver is "typically the head of the BU responsible for the operating costs of the infrastructure" — we can't tell from here whether *typically* holds for these platforms, or who that is in practice. That, and whether three roles is the right set, is what we need from you.

**If UWA single sign-on is needed**, we'd treat that as system integration (same class of work deferred in the scope) rather than something absorbed quietly — not off the table, but we'd come back with something to trade for it. Additional roles, by contrast, are cheap.

> **Default if no reply:** three roles, local sign-in, delegated authority as view-only.

## 2. How do multi-year cycles work?

Rates are set on a three-to-five year cycle, and you want to open an old record and set the new one against it. We propose each record carries a validity period, a new cycle **supersedes** the previous one by reference, and **nothing is ever overwritten or deleted** — the old record stays readable exactly as approved.

Two sub-questions: **how long is a typical validity period**, and **does a sealed record ever need amending within its own cycle** (a correction, or a mid-cycle rate change) rather than being replaced by the next one? An amendment and a supersession are different things in the software, so it helps to know early whether we need both.

**If amendment within a cycle is required**, the scope document's statement that a sealed record is immutable gains one exception: the record would keep a visible amendment history rather than a single frozen version — a change to how records are stored, cheaper to know now than later.

> **Default if no reply:** validity period entered by custodian, supersession only, no mid-cycle amendment.

## 3. What should the sealed record look like, and where does it get filed?

You mentioned a printout or generated email. We propose the tool generates a **PDF** (every input, both sets of rates, the variance, every justification) plus a permanent link to the record inside the tool.

What we can't see from here: **where the record is filed once it exists**, and whether that destination expects a particular format, template or field set. If there's an existing UWA document template, we'd rather build to it than to our own layout.

> **Default if no reply:** a PDF of our own design, plus a permanent in-tool link.

## 4. Where the guide and calculator disagree on commercial rates — which governs?

For commercial users, your guide's Step 3 gives the rate as total operating cost ÷ forecast utilisation, uplifted by the 1.35 indirect cost recovery factor, **with no income deducted**. The calculator's commercial row deducts federal and other income before applying the uplift — producing a **lower** commercial rate than the guide's method.

Our reading is that the **guide governs** (the commercial rate shouldn't be subsidised by grant income), but since this changes a published price, we'd rather ask than assume.

**If the calculator should govern instead**, one formula in the engine changes and nothing else in the scope does — a cheap change now, expensive once the engine is written.

> **Default if no reply:** we follow the guide.

## 5. More generally — guide or calculator?

That's not the only place the two differ. Working through them line by line, we found several points where the calculator's arithmetic doesn't match the guide's method, and a few we think are simple spreadsheet errors. Happy to send that list separately if useful — it's short and may be worth having regardless of this project.

The question: should the tool **follow the guide** (your policy), or **reproduce the calculator** so new figures reconcile against already-published rates?

Our proposal: follow the guide, and wherever a new figure is compared with an existing one, **show both** so the difference is visible and explainable rather than silent.

**If the calculator should govern instead**, we'd be reproducing arithmetic we believe is wrong, and would build it while recording that we raised the concern. Nothing else in the scope moves either way.

> **Default if no reply:** we follow the guide and show both figures on any comparison.

