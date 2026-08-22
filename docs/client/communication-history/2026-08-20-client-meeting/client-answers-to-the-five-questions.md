# Client's Written Answers to the Five Open Questions

**From:** Mathew Hall, Strategic Development Coordinator, UWA Research Infrastructure
**To:** CITS5206 Group 13
**Date:** 20 August 2026
**Received:** by email, following the in-person meeting of 20 August 2026
**Answers:** [`five-open-questions.md`](../2026-08-17-email-scope-and-questions/five-open-questions.md), sent 17 August 2026

> **This is the client's own text, transcribed verbatim and not edited.** It arrived as a plain
> text file; only the numbering and the question headings are ours, added so each answer sits
> under the question it answers. Where our reading of an answer goes further than the words
> themselves, that reading is recorded in the [minutes](../../../meetings/2026-08-20-client-meeting.md)
> or in [`requirements.md` §9](../../../spec/requirements.md#9-open-questions) — not here.

---

## 1. Who is allowed to see and approve a record?

> Agreed that administrator is approver of the record.

## 2. How do multi-year cycles work?

> Multi-year cycles – retain all records, most recent approved record is current and supersedes
> prior records. Note that we are aiming for a validity period of 3-5 years (capability
> dependent) and don't expect to review records/pricing annually. Only in rare cases would we
> review annually – and in that case we would anticipate that the new approved record would
> supersede the previous year record.

## 3. What should the sealed record look like, and where does it get filed?

> Confirm that .pdf is appropriate format. No such template exists currently – ideally the .pdf
> record would include workings for the calculator (for transparency and traceability) and
> outputs. Records to be stored on UWA records management system: Content Manager (TRIM).

## 4. Where the guide and calculator disagree on commercial rates — which governs?

> Guide governs. Where a discrepancy occurs, we'd appreciate if these can be flagged to us for
> our knowledge and guidance.

## 5. More generally — guide or calculator?

> Tool should follow the guide.

---

## What each answer settles

| Q | Our default | The client's answer | Effect |
| --- | --- | --- | --- |
| 1 | Three roles, local sign-in, delegated authority view-only | **Administrator is the approver** | Settles who approves. Does *not* by itself move approval into the MVP — see the minutes §3.1 and the [annotated question sheet](five-open-questions-annotated.pdf) |
| 2 | Validity period entered by custodian, supersession only, no mid-cycle amendment | **Default confirmed**, with the validity period stated as **3–5 years, capability dependent** | Q3 closes. No amendment chain in the data model |
| 3 | A PDF of our own design, plus a permanent in-tool link | **PDF confirmed. No UWA template exists.** Record must carry the **workings** as well as the outputs. Filed in **Content Manager (TRIM)** | Q5 closes, and the export gains a requirement: show the calculation, not only the result |
| 4 | We follow the guide | **Guide governs**, and discrepancies are to be **flagged to the client** | Q9 closes, plus a new obligation on us |
| 5 | Follow the guide and show both figures | **Tool follows the guide** | Q10 closes |

**The one thing to notice.** Answer 3 asks for something our scope statement did not promise:
the sealed PDF must show *the workings of the calculator*, not just the inputs and the three
rates. That is a real addition, and it is tracked as such rather than absorbed quietly — see the
[minutes](../../../meetings/2026-08-20-client-meeting.md) §5.
