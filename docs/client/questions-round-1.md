# Batched Question List — Round 1

**Project:** Research Infrastructure Costing & Pricing Tool
**Client:** UWA Research Infrastructure
**Prepared by:** Wenmin Luo and Chenxu You (Oliver)
**Version:** 1.4 — 15 August 2026
**Status:** **Drafted, not yet sent.**
**Goes out as:** Part 2 of [`2026-08-15-scope-and-questions.md`](2026-08-15-scope-and-questions.md)

---

## Status

| Event | Date | Recorded where |
| --- | --- | --- |
| Sent to the client | *(not yet)* | Teams thread / email — file the sent copy |
| Reply requested by | **Tue 18 August** | Stated in the outbound document |
| Chased | *(not yet — planned Wed 19 August, the day after)* | |
| Answers received | *(not yet)* | Fold into [`requirements.md` §9](../spec/requirements.md#9-open-questions) |

The client asked us to digest their material and come back with **batched** questions rather
than hold a fixed weekly slot ([kickoff §10](../meetings/2026-07-29-client-kickoff.md)). This is
that batch. It is the first one, so the numbering restarts at 1 for the client; our internal
Q-numbers are in Part B.

---

## How to use this document

**Nothing here goes to the client.** The questions themselves are Part 2 of the outbound
document, kept there so that the client receives one file rather than two. What matters is that
they sit **after** the sign-off request and are introduced as independent of it — a question the
client has not answered must never look like a reason to delay confirming the scope.

**Part B is ours.** It records where each question came from, what actually changes depending on
the answer, and which requirement or story moves.

**Five is the ceiling, not a target we happened to hit.** Three earlier questions were closed by
reading the client's own documents rather than by asking, and that is the standard: we ask only
what their material genuinely does not answer — and only what changes what we build.

---

# Part A — The questions themselves

**The wording that goes to the client is not held here.** The five questions travel with the MVP
statement in a single document — **[`2026-08-15-scope-and-questions.md`](2026-08-15-scope-and-questions.md)**,
Part 2 — because the client should receive one document, not two attachments that have to be
read against each other.

That file is the only home of the outbound wording. Part B below is the part that is ours: where
each question came from, what changes depending on the answer, and what has to be true before
any of it is sent.

---

# Part B — Internal notes

## Mapping to our open questions

| Client # | Ours | Origin | Status before this batch |
| --- | --- | --- | --- |
| 1 | **Q4** | Client's own, raised at kickoff §7 | Open |
| 2 | **Q3** | Client's own, raised at kickoff §6 | Open |
| 3 | **Q5** | Client's own, raised at kickoff §6 | Open |
| 4 | **Q9** | Ours — found by comparing guide and workbook | Open |
| 5 | **Q10** | Ours — the general form of Q9 | Open |

**Not in this batch:** Q1, Q2 and Q6 were closed by reading the client's documents. **Q7** was
answered by us, not the client, and does not need their time. **Q8** (repository licence) is
ours and closes at handover, not now — raising it in the same message as the sign-off would
invite a legal conversation we do not want in the way of a scope confirmation.

Source table: [`requirements.md` §9](../spec/requirements.md#9-open-questions). When answers arrive,
they go **into §9 first**, then anywhere that quotes it.

## What actually changes on each answer

| # | If the answer differs from our default | Cost to us |
| --- | --- | --- |
| 1 | A fourth role, or UWA single sign-on instead of local accounts, changes F15 and US-19 materially. **If approval must be in the core**, the delegated authority gains the action as well as the view: F16 moves from Should to Must and something else leaves the MVP | SSO would need a scope trade. Extra roles are cheap. In-core approval is the expensive one, which is why Part 1 flags it separately |
| 2 | If **mid-cycle amendment is required**, the sealed-record model gains an amendment chain — F11 and US-15 grow, and the data model changes | Real. Ask early precisely because of this |
| 3 | A prescribed UWA template changes the export layout only, not the data | Low, if we know before F12 is built |
| 4 | If the **workbook** governs, the engine's commercial formula changes and the golden-file test changes with it | Low now, high after the engine is written. This is why it is asked before any code |
| 5 | If **backward comparability** governs, we would be reproducing defects deliberately — see [`requirements.md` §9 Q10](../spec/requirements.md#9-open-questions) | We would push back, and record the objection |

## A note on single sign-on (question 1)

**Part 1 does not exclude SSO, and question 1 no longer says it does.** The exclusions in Part 1
name integration with UWA **finance, HR or booking** systems **[K §8]**; identity is not among
them, so the earlier wording — "that is an integration with a UWA system, which Part 1 puts out
of scope" — asserted something the client could disprove by reading the page above it.

The other repair was available: widen Part 1's bullet to name sign-on. We did not take it,
because Part 1 is the half being signed and it should not grow a new exclusion in the same
document that asks the client to confirm it. Question 1 now says we would **treat** SSO as a
system integration — the same class of work Part 1 defers — and come back with something to
trade for it, which is what we would actually do. The cost line above is unchanged.

## Two things to fix before this is sent

**Question 5 offers the defect list.** Before offering it, decide who writes it and by when. It
is largely done — [`requirements.md` §2](../spec/requirements.md#2-the-problem) holds the substance —
but `docs/internal/audit-2026-08-14.md` §B found several cell citations in that section
are wrong, and those corrections must land before anything goes to the client under our name.
Offering a list we then send late, or send wrong, costs more credibility than not offering it.

**That audit is not in the repository.** `docs/internal/` currently holds only its `README.md`,
so the corrections it calls for cannot be shown to have landed, and the reference above is
to a file nobody can open. Commit the audit, or record its §B findings somewhere that exists,
before question 5 goes out carrying an offer that depends on them.

**Question 2 now carries its line — closed in v1.4.** Questions 1, 4 and 5 gained one in v1.1;
question 2 did not, and it was the one where the promise in the outbound preamble mattered most.
The line now says what the cost table above says: if a sealed record must be amendable within
its own cycle, Part 1's flat statement of immutability gains an exception and the record keeps
an amendment history, which grows F11 and US-15 and **changes the data model**.

Question 3 still carries no such line, deliberately. Part 1 promises an exported record, not a
particular layout, so a prescribed UWA template changes nothing the client is being asked to
confirm — and the question already says in its own words that we would build to their template
if one exists.

---

## Change log

| Version | Date | Change |
| --- | --- | --- |
| 1.4 | 15 Aug 2026 | **Question 2 gained the "what changes in Part 1" line**, closing the third item on the pre-send list; the reasoning is above, and question 3's continued absence of one is now recorded as a decision rather than an oversight. The pre-send list is down to two, both concerning the defect list offered in question 5: who writes it and by when, and the fact that the 14 August audit it depends on **is still not in the repository**. Elsewhere in the outbound document, question 1 was corrected in two places — the client's data-sensitivity answer is no longer paraphrased as "while a pricing cycle is in progress" (kickoff §7 says *while the work is still in progress*), and the roles paragraph now states that the build is proved on one platform, matching [`requirements.md` §7](../spec/requirements.md#7-scope). The batch is still five questions and the mapping to Q3–Q5, Q9 and Q10 is unchanged. |
| 1.0 | 15 Aug 2026 | First version. Questions drawn from [`requirements.md` §9](../spec/requirements.md#9-open-questions) — Q4, Q3, Q5, Q9, Q10 — reordered for the client, each carrying its proposed default. Not yet sent. |
| 1.1 | 15 Aug 2026 | Client question 1 (our Q4) corrected: its default made the delegated authority an **approver**, contradicting Part 1, which keeps approval as a stretch item (F16). The core role is now a viewing role and the outbound wording quotes the guide's own answer on who holds the authority **[G, Step 5]**, so we ask only the part their material leaves open. Questions 1, 4 and 5 now each state what changes in Part 1 if the answer differs from our default — the earlier claim that the two parts were wholly independent did not survive this table. Reply date **Tue 18 August**, chase **Wed 19 August**. |
| 1.2 | 15 Aug 2026 | **Synchronised with the outbound document after a full review of it.** Client question 1 no longer claims Part 1 puts single sign-on out of scope — Part 1's exclusions name finance, HR and booking systems, not identity — and now says we would **treat** SSO as a system integration and come back with a trade; the reasoning and the rejected alternative are recorded above, and the cost line is unchanged. The pre-send list grows to three: the audit referenced under question 5 **is not in the repository**, and question 2 still carries no line saying what changes in Part 1, which the outbound preamble promises for exactly this case. |
| 1.3 | 15 Aug 2026 | **The batch is five questions, not six.** The question about our own working method was withdrawn — by its own cost line it changed nothing in the product, and a batch the client is asked to spend time on should contain only what moves the build. The remaining questions keep their order and are renumbered **1–5**; internal Q-numbers follow [`requirements.md` §9](../spec/requirements.md#9-open-questions) v2.2, in which old Q7–Q11 became **Q6–Q10**. Every cross-reference in this file, in the outbound document and in [`mvp-agreement.md`](mvp-agreement.md) was updated with it. |
