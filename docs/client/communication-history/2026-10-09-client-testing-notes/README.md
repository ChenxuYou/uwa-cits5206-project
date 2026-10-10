# Client Testing Notes — 9 October 2026

**The client used the staging server themselves, in all three roles, and sent written notes on
what they found.** This is the client-use half of M5 ([`plan.md`](../../../project/plan.md)) and
the acceptance-testing evidence the final report needs
([Assignment 5 brief, R5](../../../../reference/unit/assignment-5-final-group-report.md#4-reading-the-brief-and-rubric)).

| | |
| --- | --- |
| **Received** | Friday 9 October 2026, as a Word document of notes addressed to Group 13 |
| **From** | Mathew Hall, Strategic Development Coordinator, UWA Research Infrastructure ([contacts](../../contacts.md)) |
| **What was tested** | The staging server sent on 30 September ([#60](https://github.com/ChenxuYou/uwa-cits5206-project/issues/60)), signed in as platform custodian, delegated approver and administrator, with the client entering their own test data |
| **Build tested** | `feat/deployment_docker`, not `main` ([#101](https://github.com/ChenxuYou/uwa-cits5206-project/issues/101)) — see *Limits* below |
| **Issue** | [#99](https://github.com/ChenxuYou/uwa-cits5206-project/issues/99) |

**About this record.** The notes themselves are the client's material and are not committed
([`reference/client/README.md`](../../../../reference/client/README.md)); the original is kept
in the team's Teams area. What follows is our summary in our own words, written on the day
they arrived.

## What the client found works

**Platform custodian.** The capacity step was clear and intuitive; the client singled out the
deductions from the baseline and the distinction between capacity and forecast use. The
calculated rates were clear, and showing the figures that produce each rate was welcome. The
review page was clear. The explanatory text throughout was appreciated because it is visible
and easy to take in, rather than hidden.

**Delegated approver.** The review of a submission was clear and easy to understand. The return
and comment protocol works. The client noted that very detailed feedback could be cumbersome to
type into the tool, but that such feedback is probably better given in conversation anyway, so
they did not see it as a problem.

**Administrator.** The review pages were clear, and names, dates and actions are captured for
record keeping.

| Role | Stories exercised | Client's verdict |
| --- | --- | --- |
| Platform custodian | US-01, US-03, US-04, US-07, US-08, US-09, US-13, US-14 | Clear; six changes asked for (below) |
| Delegated approver | US-20 | Clear; no change asked for |
| Administrator | US-19 and the cycle views | Clear; no change asked for |

## What the client asked to change

All six concern the custodian's platform and cost steps.

| # | Requested | Outcome |
| --- | --- | --- |
| 1 | Go back to the platform step from the costs step, to correct a mistyped capability name without starting again | **Already on `main`** before the notes arrived; missing from staging only |
| 2 | Edit a saved cost line rather than delete it and add it again | **Already on `main`**; missing from staging only |
| 3 | Professional staff salary Levels 1–10, alongside academic Levels A–E | **Done 9 Oct** |
| 4 | Drop the low or high cost school field — it came from the example calculator and is not needed | **Done 9 Oct** |
| 5 | Add "LG funded" and "GP funded" as position funding types | **Done 9 Oct**, with the client's wording. Their meaning is put to the client as [Q16](../../../spec/requirements.md#9-open-questions) |
| 6 | Fill each year's salary in automatically for the years selected, still editable | **Partly done 10 Oct.** Each year is filled from a base salary the custodian enters, and stays editable. Filling the salary from the level and step is US-05 ([#42](https://github.com/ChenxuYou/uwa-cits5206-project/issues/42)) and needs UWA's pay scales. Recorded for after handover |

Items 3–5 were merged to `main` on 9 October in [#110](https://github.com/ChenxuYou/uwa-cits5206-project/pull/110), with tests, and are described
for the client in the [handover document](../../../handover.md) §3.

## Limits of this evidence

- **The build was older than `main`.** Items 1 and 2 above were reported as missing because
  staging runs a branch that left `main` on 22 September. Redeploying staging from the final
  release ([#101](https://github.com/ChenxuYou/uwa-cits5206-project/issues/101)) lets the client
  see that both are fixed.
- **Not every story was exercised.** The notes do not mention funding (US-06), proposed rates and
  the balance (US-11, US-12), sealing (US-15), the PDF export (US-16) or supersession.
- **The open questions were not answered.** Q11–Q15 on the method and Q14 on hosting were not in
  the notes; they go to the client with the handover.
- **The notes do not say "accepted".** They are a positive assessment with change requests. The
  client's formal acceptance is the signature on the [handover document](../../../handover.md) §11.

## Actions

| Action | Owner | By |
| --- | --- | --- |
| Merge items 3–5 to `main` after `dotnet test` passes | Chenxu You | ✅ Done 9 Oct, [#110](https://github.com/ChenxuYou/uwa-cits5206-project/pull/110) |
| Redeploy staging from `main` and tell the client items 1 and 2 are visible there | Dai Lam La La | Sun 11 Oct |
| Reply to the client: what changed, why salary pre-fill waits, Q11–Q16, and the handover document | Yichen Zhao | ✅ Done 9 Oct |
| Close [#99](https://github.com/ChenxuYou/uwa-cits5206-project/issues/99) with a link to this record | Yichen Zhao | ✅ Done 9 Oct |
