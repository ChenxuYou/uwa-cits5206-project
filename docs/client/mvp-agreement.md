# MVP Agreement — Statement Put to the Client for Sign-Off

**Project:** Research Infrastructure Costing & Pricing Tool
**Client:** UWA Research Infrastructure
**Prepared by:** Chenxu You (Oliver) and Wenmin Luo
**Version:** 1.7 — 20 August 2026
**Status:** **APPROVED — signed by the client on 20 August 2026.** Both confirmations ticked.

---

## Status of this agreement

This table is the evidence trail. Assignment 1 §2 quotes it, so every date in it must be a
date something actually happened — not a date we planned for.

| Event | Date | Recorded where |
| --- | --- | --- |
| Statement sent to the client | **Mon 17 August 2026**, by Yichen Zhao | [`communication-history/2026-08-17-email-scope-and-questions/`](communication-history/2026-08-17-email-scope-and-questions/) — email, scope summary and the five questions, all filed |
| Reply requested by | **Tue 18 August** — stated in the outbound document | |
| Client replied, confirming a time to meet | **Tue 18 August 2026**, by email | Teams / email thread |
| Client chased | *(never needed — the reply arrived first)* | |
| Meeting held | **Thu 20 August 2026**, in person | [Minutes](../meetings/2026-08-20-client-meeting.md) |
| Client response received | **Thu 20 August 2026** — signed document and written answers to all five questions, returned by email | [`communication-history/2026-08-20-client-meeting/`](communication-history/2026-08-20-client-meeting/) |
| **Outcome** | ✅ **APPROVED.** Signed by **Mathew Hall**, Strategic Development Coordinator, **20/8/2026**. Confirmation 1 (scope) ✓ · Confirmation 2 (ownership) ✓ | [`project-scope-summary-signed.pdf`](communication-history/2026-08-20-client-meeting/project-scope-summary-signed.pdf) |

**One person sends, one person is named.** Yichen Zhao owns client contact
([minutes of 15 August](../meetings/2026-08-15-team-weekly-meeting.md) §8), so the client is never
chased twice about the same thing by two of us. The sent copy is the evidence Assignment 1 §2 is
marked on — **a date nobody wrote down is a date we cannot cite.**

**Why the reply date was Tuesday and the chase Wednesday.** We asked for the reply by Tuesday and
planned the chase for the Wednesday, so it would fall *after* the date we named rather than on
it. Wednesday is the client's stated
preference for contact **[K §10]**. In the event the client replied on the Tuesday and the chase
was never sent.

**Part C is now dead letter.** It set out what we would write in the report if no answer came,
and how long we would wait before giving up on one. It is kept below as the record of a plan
we made and did not need, not as a live instruction — see the note at the head of it.

---

## How to use this document

**Nothing here goes to the client.** The client receives exactly one file —
[`2026-08-15-scope-and-questions.md`](2026-08-15-scope-and-questions.md) — and this document is
the reasoning behind the first half of it.

**Part B is the traceability**: every plain-language promise in the outbound statement mapped
back to a numbered requirement, so that what we sign and what we build cannot drift apart. It
also records what we deliberately left out of the client's copy, and why. **Part C** is what we
do, and what we write in the report, if no answer comes.

**Do not send `requirements.md` either.** It runs to sixty pages of internal working. A document
nobody reads is a document nobody signs; link it, do not attach it.

---

# Part A — The statement itself

**The text that goes to the client is not held here.** It lives, together with the question
list, in **[`2026-08-15-scope-and-questions.md`](2026-08-15-scope-and-questions.md)** — one
document, because the client should receive one document.

That file is the only home of the outbound wording, in the same way that
[`team.md`](../project/team.md) is the only home of the roster. Editing a paragraph here and
forgetting to edit it there is exactly the failure this project exists to make impossible, so
there is nothing here to edit.

What it contains, in the order the client reads it: the outcome we are building towards in the
client's own words · the three sections of the tool · how we will prove the engine is right
against their worked example · what we are **not** building · what we will attempt only if there
is time, including the flagged question on delegated approval · the two confirmations we are
asking for · a sign-off block · the reply date and the single chase · then Part 2, the five
questions.

**If the scope changes, that file changes and the version below is bumped.** Parts B and C of
this document explain and defend it; they do not restate it.

### The minutes are no longer part of what we ask them to confirm

An earlier draft asked the client to confirm our minutes of 29 July as an accurate record. The
minutes do not travel with the document — the client receives one file, and that file neither
attaches nor links them — so we were asking for confirmation of something they had not been
given. The [minutes](../meetings/2026-07-29-client-kickoff.md) also carry the 14 August
annotation withdrawing the figures in §2 Step 2 and §2 Step 5, which would have made the
request harder to answer rather than easier.

**That confirmation is withdrawn, not narrowed.** Attaching the minutes was the alternative and
we did not take it: the minutes are our record, the client's own guide and calculator are the
sources the scope is actually built on, and nothing in Part 1 depends on the client endorsing
our note-taking. What we need standing behind the scope is the scope itself, which is
confirmation 1. Ownership becomes confirmation 2 and keeps its wording unchanged.

**One consequence to keep in view.** Everything in the minutes that Part 1 relies on now stands
on the client's *silence* rather than their confirmation — the deferral of integration, the
dashboard remark, and the IP position. The first two are recorded in Part 1 in terms the client
can check against their own memory, and the third is confirmation 2 in its own right, which is
why it survives the deletion.

---

# Part B — Traceability (internal)

Every promise in Part A, back to the numbered requirement it came from. If Part A says it and
this table cannot place it, one of the two is wrong.

| Part A promise | Requirements | Stories |
| --- | --- | --- |
| Three guided sections, costs → capacity → rates | F1 | US-01, and the ordering US-03 → US-08 → US-09 |
| Costs at capability and platform level, client's categories | F2 | US-03, US-04 |
| Four income lines, UWA and non-UWA distinguished | F3 | US-06 |
| Billable unit, capacity from baselines less deductions | F4 | US-07 |
| Forecast utilisation mandatory and distinct from capacity | F5, N3 | US-08 |
| Three rates per capability, figures shown behind each | F6 | US-09 |
| Proposed rates and the resulting surplus or deficit | F7 | US-11, US-12 |
| Change any input and see the effect before committing | F8 | US-10 |
| Justification text throughout | F9 | US-13 |
| Save and resume a draft | F10 | US-01, US-02 |
| Seal on submit; inputs and results immutable | F11 | US-14, US-15 |
| Export a portable record readable years later | F12 | US-16 |
| List and reopen past sealed records | F13 | US-01, US-17 |
| Sign-in; no anonymous records; identity on every record | F15 | US-19 |
| Calculation server-side, never sent to the browser | N1 | US-09, US-18 |
| Type and range validation; the $200,000 typo | N2, N3 | US-18, US-08 |
| Worked example reproduced to the cent, before any UI | N13, §6.3(1) | US-09 |
| **Stretch:** salary pre-fill | F14 | US-05 |
| **Stretch:** delegated approval | F16 | US-20 |
| **Stretch:** replacement reserve | F19 | US-23 |
| **Stretch:** benchmarking record | F20 | US-24 |
| **Stretch:** dashboard | F17 | US-22 |
| **Stretch:** price-change communication | F21 | US-25 |
| **Out:** integration with UWA systems | F18 (Won't) | — |

Requirement-to-story mapping follows [`user-stories.md` §6](../spec/user-stories.md#6-traceability),
which is authoritative; this table is a view of it, not a second source. Scope baseline:
[`requirements.md` §7](../spec/requirements.md#7-scope). MVP story set:
[`user-stories.md` §5](../spec/user-stories.md#5-mvp-definition) — eighteen Must stories, 110 points.
**If Part A and §7 ever disagree, §7 wins and Part A is corrected**, because §7 is what the
build follows.

**F22** (a new cycle supersedes a sealed one by reference, added in
[`requirements.md` v2.3](../spec/requirements.md#61-functional)) carries no line in Part A either,
and for a different reason from the ones below: **how supersession works is the subject of Part 2
question 2**, so stating it as a Part 1 promise would present as settled the very thing we are
asking about. Part A's "past sealed records can be listed and reopened" is F13 and stands
unchanged; it promises retrieval, not the mechanism. If the client answers that a sealed record
must also be amendable within its cycle, F22 gains a sibling and *that* is when Part 1's
immutability sentence changes — not before.

Four further MVP items carry no line in Part A, deliberately: **N4** (usable without training), **N5**
and **N6** (exact and reproducible arithmetic), **N9** (transport and secrets) and **N14**
(same-range aggregates, so a total is never summed over a different range from the figures it is
compared against) are properties the client will experience rather than features they can
confirm, and listing them reads as padding. N14 in particular is a guard against a defect in
*their* workbook, and naming it in the sign-off document would be raising that defect obliquely
— it belongs in the defect list, not here. **N10** audit, **N12** accessibility and **US-21**
method configuration are stretch and too internal to be worth a client's attention.

## Deliberate omissions from Part A

Things we left out of the client-facing page on purpose, and why:

| Omitted | Why |
| --- | --- |
| Story IDs, points, requirement numbers | Internal bookkeeping. It makes the page look like a contract to be negotiated rather than a summary to be confirmed |
| The technology decision | Not settled, and not the client's to make. It changes nothing they are being asked to confirm |
| The defect list itself | Part 1 says nothing about it: putting "your spreadsheet is wrong" next to "please sign this" invites the wrong reply. Part 2 question 5 goes as far as **offering** to send the list as a separate document, which is where that conversation belongs — and only after the corrections in [`questions-round-1.md`](questions-round-1.md) Part B have landed |
| The five client questions | They travel as **Part 2 of the same file**, after the sign-off request and introduced as independent of it, so that an unanswered question cannot hold up the confirmation. Six of our questions are open in [`requirements.md` §9](../spec/requirements.md#9-open-questions); five travel, because **Q8** (repository licence) is ours and closes at handover |

---

# Part C — If no answer comes

> **Superseded 20 August 2026. This never became live.** The client replied on Tuesday 18
> August, met us on Thursday 20 August, and signed. The chase in the second paragraph was not
> sent and the decision point in the third was not reached. Part C is left unedited as the
> record of the fallback we prepared — a plan that is not needed still shows that we had one —
> and **nothing in it is an instruction any more.** What actually goes in the report is in the
> status table at the head of this document.

Three dates and one sentence.

**Tuesday 18 August** — the date we asked for a reply by, stated in the outbound document.

**Wednesday 19 August** — chase in the Teams thread, the day after that date rather than on it.
Wednesdays are the client's stated preference and it still leaves six days.

**Friday 21 August** — decision point. If nothing has come back, we stop waiting and write the
report against the proposed defaults in [`requirements.md` §9](../spec/requirements.md#9-open-questions).

**What goes in the report in that case**, stated plainly rather than blurred:

> The MVP statement in Appendix X was sent to the client on *(date)*, with a reply requested by
> 18 August, and chased on 19 August.
> No written confirmation had been received at the time of submission. The scope below is
> therefore our proposal, drawn from the client's own guide, workbook and the 29 July
> walkthrough, and it is what we will build unless the client tells us otherwise.

A team that can say exactly what it is waiting for and what it will do meanwhile reads better
than one that implies an agreement it does not have. The 5 August checkpoint deck already took
this line — *"the client's sign-off on the MVP is outstanding, not assumed"* — and we do not
retreat from it now.

---

## Covering message (draft — attach [`2026-08-15-scope-and-questions.md`](2026-08-15-scope-and-questions.md) as a PDF)

> **Subject: Costing & Pricing Tool — scope summary, and a quick confirmation**
>
> Hello *(name)*,
>
> Thank you again for the walkthrough on 29 July and for sharing the guide and the calculator —
> between them they answered several questions we would otherwise have had to ask.
>
> Attached is what we understand the tool needs to do: what we will build, what we are
> deliberately not building, and what we will only attempt if there is time. **If it looks
> right, a one-line reply saying so is all we need.** If anything is wrong or missing, tell us
> which part and we will change it.
>
> There is one thing we have flagged rather than buried — whether formal approval by a delegated
> authority needs to be in the core tool rather than a stretch item.
>
> We would be grateful for a reply by **Tuesday 18 August** if you can: our project
> specification is due on 25 August and your confirmation forms part of it. If we have not heard
> by then we will send one reminder on Wednesday 19 August.
>
> Five questions follow at the end of the same document. **None of them holds up the scope
> summary** — each has a default we will use if we do not hear back — so a partial reply is
> genuinely useful.
>
> Kind regards,
> Chenxu You and Wenmin Luo
> CITS5206 capstone team, The University of Western Australia

---

## Change log

| Version | Date | Change |
| --- | --- | --- |
| 1.7 | 20 Aug 2026 | **The client signed.** Mathew Hall, Strategic Development Coordinator, 20/8/2026, both confirmations ticked — the signed document and the client's written answers to all five questions are filed in [`communication-history/2026-08-20-client-meeting/`](communication-history/2026-08-20-client-meeting/) and the meeting is [minuted](../meetings/2026-08-20-client-meeting.md). Status moves from *Sent, awaiting reply* to **Approved**, and the status table now carries the two events it was missing: the client's reply of **Tue 18 August** confirming a time, and the **Thu 20 August** meeting. The chase planned for Wednesday 19 August was never sent, and the table says so rather than leaving the row hanging. **Part C is marked superseded** and left unedited — a fallback we prepared and did not need is still evidence that we prepared one. **Nothing in Part A or Part B changes**: the client signed the statement exactly as it stood, flag included, so no promise, requirement or story moves. Three things the client raised that Part A does not cover — the PDF showing the calculator's workings, HR-system integration for staff roles, and filing into Content Manager (TRIM) — are recorded in the [minutes](../meetings/2026-08-20-client-meeting.md) §5 as raised, and only the first is accepted in principle. |
| 1.6 | 19 Aug 2026 | **The statement was sent on Mon 17 August 2026** by Yichen Zhao, as an email to the client with the scope summary and the five questions attached; all three are filed in [`communication-history/2026-08-17-email-scope-and-questions/`](communication-history/2026-08-17-email-scope-and-questions/). Status moves from *Drafted, not yet sent* to *Sent, awaiting reply*. **Nothing in Part A, Part B or Part C changes** — no promise, requirement or story moves; only the record of what has happened does. The outbound email also proposed **Wednesday 4:00–4:30pm** for a short meeting, which is not a date this table tracks: it tracks the reply. Part C is now live: the reply date of **Tue 18 August** has passed, the single chase falls **today, Wed 19 August**, and **Fri 21 August** remains the point at which we stop waiting and write the scope as our proposal. |
| 1.5 | 15 Aug 2026 | **Part B follows requirements v2.3, which added F22** — a new cycle supersedes a sealed one by reference. Part B's rule cuts both ways, so a new MVP requirement has to be either placed against a Part A promise or recorded as deliberately absent from one; F22 is the second, and the reason is specific to it rather than shared with N4, N5, N6, N9 and N14. **How supersession works is what Part 2 question 2 asks**, so promising it in Part 1 would present as settled the thing we are asking about. Part A's "past sealed records can be listed and reopened" is F13 and is untouched — it promises retrieval, not the mechanism. **No line of the outbound document changes**, no promise is added or withdrawn, and the traceability table itself is unchanged; only the paragraph recording deliberate omissions grows. The related change in [`questions-round-1.md` v1.5](questions-round-1.md) — a sixth question considered and rejected — likewise leaves the batch at five. |
| 1.4 | 15 Aug 2026 | **Confirmation 2 — the minutes — is deleted from the outbound document, and with it the last pre-send blocker in Part A.** We were asking the client to confirm a file they were not being sent. There are now **two** confirmations, scope and ownership; ownership renumbers from 3 to 2 with its wording untouched, and the sign-off block gains a line for each rather than one cell covering all three. Five further edits went into the outbound file at the same time: **(1)** question 2 gained the line the preamble promises — if a sealed record must be amendable within its cycle, Part 1's immutability statement gains an exception and the storage model changes (F11, US-15); it was the most expensive question in the batch and read as the cheapest. **(2)** Part 1 no longer attributes the cost and income categories to the guide — the line-level breakdown is the **calculator's** **[W, sheet 1]**; the guide gives a prompting list, not a chart of accounts. **(3)** Question 1 no longer paraphrases the client's data-sensitivity answer as "while a pricing cycle is in progress"; kickoff §7 says *while the work is still in progress*, and we do not put words in their mouth in a document that asks them to confirm the rest. **(4)** "Deferred at your suggestion" becomes "following your own view that integration would be ideal but challenging" **[K §8]** — the client raised a difficulty, they did not propose the deferral. **(5)** Question 1 now states that the build is proved end to end on one platform, matching the scope baseline in [`requirements.md` §7](../spec/requirements.md#7-scope), so that "an administrator who can see all platforms" cannot be read as multi-platform delivery. No requirement, story or scope line changes. |
| 1.0 | 15 Aug 2026 | First version. Part A drawn from [`requirements.md` §7](../spec/requirements.md#7-scope) and [`user-stories.md` §5](../spec/user-stories.md#5-mvp-definition); figures restricted to the client's guide worked example. Not yet sent. |
| 1.2 | 15 Aug 2026 | **Synchronised with the outbound document after a full review of it.** (1) The omissions table said "the seven open questions" travel in the batched list — not all of them do; the repository licence question is ours and is not asked. It also implied a separate question document; there is one file, and the questions are Part 2 of it. (2) The defects row read as though the workbook defects were withheld entirely; the last of the Part 2 questions offers the list as a separate document, and the row now says so, with the precondition attached. (3) **N14** removed from the Part B traceability table — no promise in Part A carries it, and the rule at the head of that table cuts both ways. It moves to the list of MVP items deliberately unnamed, with the reason. (4) Part A now records the one thing still to settle before sending: **confirmation 2 asks the client to confirm minutes they are not being sent.** (5) The outbound Q2 no longer asserts that Part 1 excludes single sign-on — Part 1's exclusions name finance, HR and booking systems, not identity — and says instead that we would treat SSO as a system integration and come back with a trade. |
| 1.3 | 15 Aug 2026 | **Part 2 is five questions, not six.** The question about our own working method was withdrawn from the batch — it changed nothing in the product, and the questions we spend the client's time on should be the ones that move the build. The remaining five keep their order and renumber to 1–5; the omissions table, the covering message and Part A's description of the outbound file follow. Internal Q-numbers follow [`requirements.md` §9](../spec/requirements.md#9-open-questions) v2.2, in which old Q7–Q11 became **Q6–Q10**, so the repository licence question is now **Q8**. Nothing in Part A or Part B's traceability changes: no requirement, story or promise is affected. |
| 1.1 | 15 Aug 2026 | Four corrections found in review, all in the outbound document; this file follows them. **(1) Ownership.** Confirmation 3 said the overarching IP "remains UWA's", which contradicts [`NOTICE`](../../NOTICE) §2 and kickoff §9 D5 — the overarching IP is **joint**. Since NOTICE §1 closes the licensing question on the client's written confirmation of that position, the old wording would have had the client confirm the opposite of what we hold. Now states joint ownership and the reciprocal restriction on onward sale (NOTICE §3). **(2) Delegated authority.** Question 2's default committed to a role that "approves what is submitted to them" while Part 1 kept approval as stretch (F16) — two defaults in conflict in one document. The core role is now a **viewing** role (A6); the approval action stays stretch and is named as such. **(3) Independence of the two parts.** "None of the questions needs to be resolved" overstated it; the roles question, the guide-versus-calculator question and its general form each now carry an explicit line saying what changes in Part 1 if the answer differs from our default. **(4) Dates.** Reply requested by **Tue 18 August**, chase **Wed 19 August** — the chase previously fell on the deadline itself. |
