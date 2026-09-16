# Team Meeting — Technical Ownership From Sprint 4 to Handover

**Date:** Tuesday 15 September 2026
**Format:** _to confirm_
**Present:** _to confirm_
**Purpose:** give every remaining layer of the build one named owner, so that the work from M3 to
the final release on 13 October is not carried by two people.

> **About this record.** Written on 16 September 2026 from the decision as reported by Chenxu You,
> in time for the lab facilitator checkpoint the same day. Attendance and format are left marked
> _to confirm_ rather than guessed; anyone who was there fills them in. The decision itself is
> what the plan and the facilitator deck now rely on.

---

## 1. Why the split was needed

[`skills-audit.md`](../project/skills-audit.md) G6 records that two of five members wrote
production code, and the team is already down one member. [`plan.md` v1.4 §3](../project/plan.md)
gave everyone an *area*, but three of the five areas were not code — client liaison, costing
logic, verification — so deployment, authentication and the screens had no single owner, and
Assignment 2 asks each of us to evidence software we wrote ourselves.

## 2. Decision — one layer, one owner

| Member | Technical responsibility | In practice |
| --- | --- | --- |
| **Chenxu You** | **General backend** | Page models and services, the data model and EF Core migrations, the seal and retrieval, CI, repository structure |
| **Wenmin Luo** | **Backend — calculation** | `CostingTool.Engine`, method configuration, the calculation service, the workings printed in the sealed PDF, the modelling assumptions |
| **Dai Lam La La** | **Backend — deployment** | Server provisioning, CD with a rollback, DNS, reverse proxy and TLS, staging, the release |
| **Yichen Zhao** | **Front end** | Razor views and `site.css` for the guided flow and the approver's side; what the client sees in a demo |
| **Jaswanth Vericherla** | **Authentication** | Sign-in, roles and authorisation policies, ownership checks, account provisioning for staging, US-19 |

Standing, non-code responsibilities in [`plan.md` §3](../project/plan.md) are unchanged unless
recorded otherwise: Yichen Zhao remains the single point of client contact, Dai Lam La La owns the
risk register, Jaswanth Vericherla keeps the minutes.

**The review rule does not change.** Nothing merges on its author's approval, and nobody approves
a pull request in their own layer.

## 3. What follows from it

- Build assignments for S4 onwards in [`plan.md` §2](../project/plan.md) re-mapped to the layers.
- Deployment rows in `plan.md` §5 move to Dai Lam La La; the seeded-credentials gate moves to
  Jaswanth Vericherla; migrations and the `src/CostingTool.Web/` move sit with Chenxu You; the two
  unconfirmed modelling decisions sit with Wenmin Luo.
- Owners updated to match in [`risks.md`](../project/risks.md),
  [`skills-audit.md`](../project/skills-audit.md) §2 and
  [ADR-001](../decisions/adr-001-technology-stack.md)'s follow-on actions.
- The facilitator checkpoint of 16 September
  ([deck](../../presentations/2026-09-16-facilitator-checkpoint.html)) presents one section per
  layer.
- Each Assignment 2 report can lead Section 1 with its owner's layer.

## Actions

| Action | Owner | By |
| --- | --- | --- |
| Fill in attendance and format on this record | Jaswanth Vericherla | Sat 19 Sep |
| Reassign the open issues on the [board](https://github.com/users/ChenxuYou/projects/2) to match the layers | Chenxu You | Sat 19 Sep |
| Confirm at the Saturday meeting that the standing responsibilities in §2 still hold | Whole team | Sat 19 Sep |
