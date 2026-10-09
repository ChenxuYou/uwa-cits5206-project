# Handover

**Research Infrastructure Costing & Pricing Tool**
CITS5206 Professional Computing capstone, The University of Western Australia — Group 13

| | |
| --- | --- |
| **To** | UWA Research Infrastructure — Erika Slavin and Mathew Hall |
| **From** | Chenxu You, Yichen Zhao, Wenmin Luo, Dai Lam La La, Jaswanth Vericherla |
| **Handover date** | Monday 12 October 2026 (milestone M7) |
| **Prepared** | 9 October 2026 |
| **Release** | `v1.0.0` on the `main` branch of [ChenxuYou/uwa-cits5206-project](https://github.com/ChenxuYou/uwa-cits5206-project) |

This document is the formal handover of the tool to UWA Research Infrastructure. It says what
is being handed over, what state it is in, what UWA needs to decide to run it, and what is
still open. Every statement links to the document in the repository that holds the detail.

---

## 1. What is handed over

| Item | Where |
| --- | --- |
| The application — ASP.NET Core Razor Pages on .NET 10, with EF Core and SQLite | [`src/`](../src/), how to run it in [`src/README.md`](../src/README.md) |
| The calculation engine, separate from the web application and with no database or UI dependency | [`src/CostingTool.Engine/`](../src/CostingTool.Engine/) |
| Automated tests — engine, sealed-record PDF and web workflow; the client's worked example is asserted to the cent | [`tests/`](../tests/), run on every push by [CI](../.github/workflows/ci.yml) |
| User manual for platform custodians, delegated approvers and administrators | [`docs/user-manual.md`](user-manual.md) |
| Deployment runbook: one Ubuntu server, HTTPS, daily backups, restore and rollback | [`deploy/README.md`](../deploy/README.md) |
| Requirements, user stories and architecture, with the source of every rule marked | [`docs/spec/`](spec/) |
| Decision records — technology stack and PDF generation | [`docs/decisions/`](decisions/) |
| Ownership and the permission granted to UWA | [`NOTICE`](../NOTICE) |
| The staging server, live since 30 September 2026 | Address and accounts sent to the client by email on 30 September; not recorded in this public repository |

## 2. What the tool does

A platform custodian is guided through six steps — **Platform → Costs → Funding → Capacity →
Rates → Review** — and the tool computes three hourly (or per-unit) rates for each capability
from the client's method:

```
R_uwa        = (C − I_total)   / U
R_apfr       = ((C − I_nonuwa) / U) × 1.35
R_commercial = (C / U)               × 1.35
```

The custodian proposes rates and sees their effect on the platform's balance, explains their
assumptions as they go, and submits the cycle. The delegated authority reviews it and either
returns it with comments or approves it. **Approval seals the record**: its inputs, the
calculation's workings and the decision are frozen, and the record can be exported as a PDF
for Content Manager (TRIM). A sealed record can be superseded by a new cycle but never edited.

### Scope delivered

| Priority | Stories | State |
| --- | --- | --- |
| Must | All 18, US-01 to US-04, US-06 to US-19 | Delivered |
| Should | US-20 approval by the delegated authority | Delivered |
| Should | US-24 record the benchmarking | Partly: a free-text field on the rates step, shown to the approver and carried into the sealed record and its PDF. The guide's structured sources and prompts are not built |
| Should | US-05 salary pre-fill, US-21 method configuration screen, US-23 replacement reserve | Not built |
| Could | US-22 compare with the last cycle | Partly: a new cycle shows the previous sealed record's key figures beside it |
| Could | US-25 price-change communication | Not built |

The stories and their acceptance criteria are in
[`docs/spec/user-stories.md`](spec/user-stories.md).

## 3. Client testing and what changed after it

The client tested staging in all three roles and sent written notes on 9 October 2026,
[summarised here](client/communication-history/2026-10-09-client-testing-notes/README.md). They
found the capacity, rates, review and approval screens clear, and the explanatory text useful.
They asked for six changes:

| Requested | Outcome |
| --- | --- |
| Return to step 1 from the costs step, to correct a capability name | **Done.** Every completed step in the step bar is a link, and step 1 renames or removes capabilities |
| Edit a saved cost line instead of deleting it and adding it again | **Done.** Each line has an **Edit** link; the line is changed in place and held to the same checks as a new one |
| Professional staff Levels 1–10 alongside academic Levels A–E | **Done.** The level list follows the staff type, and the server refuses a level from the other scale |
| Remove the low or high cost school field | **Done.** No longer asked. Lines saved before the change keep the value they were saved with until next edited |
| Add "LG funded" and "GP funded" as position funding types | **Done**, using the client's wording. See Q16 below |
| Fill each year's salary in from the pay scales, still editable | **Not built.** This is US-05. It needs UWA's current academic and professional pay scales, which are not in the material we hold; see §7 |

The first two were already on `main` when the client tested, but not on the staging server,
which was deployed from an earlier branch (§4).

The delegated approver also observed that long, detailed feedback may be awkward to type into
the return comment, and that a conversation may suit it better. No change is made; the comment
records the outcome of that conversation.

## 4. Environments

### Staging

The staging server was set up by the team on 30 September 2026, with the client's agreement of
22 September that the team may host it until UWA IT provides a domain and resources. It is
reached by IP address, so browsers show a certificate warning.

**It does not run this release.** It was deployed from the branch `feat/deployment_docker`
(Docker Compose and PostgreSQL), which left `main` on 22 September. Everything since — editing
a line, renaming a capability, the staff-field changes in §3, the sealed records register,
supersession and the forced password change — is missing there
([#101](https://github.com/ChenxuYou/uwa-cits5206-project/issues/101)).

To bring it up to this release, the server is redeployed from `main` with
[`deploy/README.md`](../deploy/README.md). That runbook uses SQLite, so **figures entered on the
current staging server do not carry over**. They are test figures only — real platform figures
have not been cleared for a team-held server (Q14) — so they are recorded for reference before
the switch and the client re-enters what they need.

### Production

There is no production server yet. The runbook in [`deploy/README.md`](../deploy/README.md) is
written for production and was rehearsed end to end on 25 September 2026: first release, sign-in
over HTTPS, forced password change, backup, restore, and an automatic rollback of a broken
release.

What a production server needs:

| | |
| --- | --- |
| Server | Ubuntu 24.04, with the ASP.NET Core 10 runtime, Caddy and `sqlite3` |
| Network | Ports 80 and 443 open; nothing else. A UWA domain name, so Caddy can obtain a trusted certificate |
| Data | One SQLite file under `/var/lib/ric-costing`, outside the application release. **Sealed records live in it** |
| Backups | Daily at 02:00 Perth time, checked and kept 30 days on the server. They must also be copied off the server |
| First start | Creates one administrator, who must change the password at first sign-in and then creates every other account |

The PostgreSQL configuration on `feat/deployment_docker` remains available if UWA IT requires a
managed database (Q14). It is not part of this release, and adopting it means a new set of
database migrations on `main`.

## 5. Running it day to day

| Task | Who | How |
| --- | --- | --- |
| Create accounts, reset passwords | Administrator | In the tool, under **Admin → Users** — [user manual §5](user-manual.md) |
| Release a new version | Whoever holds the server | `deploy/release.sh` — backs up the database, switches release, rolls back by itself if the new one does not start |
| Restore from a backup | Whoever holds the server | `deploy/restore.sh <backup>` |
| Change the method — `k`, rounding, capacity baselines | A developer | Add a **new** method version; never edit an existing one, because sealed records reproduce their figures from the version they were sealed under — [`src/README.md`](../src/README.md) |
| Change the data model | A developer | An EF Core migration; CI fails if the model changes without one — [`src/README.md`](../src/README.md) |

A custodian uses the tool roughly once every three to five years, so the user manual is written
to be read cold.

## 6. Decisions UWA needs to make

These cannot be settled by the team. Each one changes what happens after handover.

| # | Decision | Why it matters | The team's suggestion |
| --- | --- | --- | --- |
| Q14a | **Who runs the tool** — releases, backups, accounts | The team's access ends with the unit | A named person in Research Infrastructure as the administrator, and UWA IT for the server |
| Q14b | **Where it is hosted** | The staging server is a team resource, not a UWA service | A UWA server and domain, as already requested from UWA IT |
| Q14c | **Whether UWA IT requires a managed database** | Decides between this release's SQLite and the PostgreSQL branch | SQLite is sufficient for a handful of custodians and is simpler to back up; change only if UWA IT requires it |
| Q14d | **Whether real platform figures may go on the team-held server** | Until answered, staging holds test figures only | Test figures only until the tool is on UWA infrastructure |
| Q8 | **The repository licence** | The IP is jointly held, so the team cannot grant a licence alone. [`NOTICE`](../NOTICE) already gives UWA a perpetual permission to use, modify and host the tool | A noncommercial licence, agreed with UWA, or leave `NOTICE` as the arrangement |

## 7. Open questions on the method

The tool has a working answer to each of these, recorded where it is implemented. The client's
confirmation is still needed; a different answer is a contained change.

| # | Question | What the tool does now | Issue |
| --- | --- | --- | --- |
| Q11 | Who seals the record? | The delegated authority's approval seals it, not the custodian's submission | [#105](https://github.com/ChenxuYou/uwa-cits5206-project/issues/105) |
| Q12 | Is a multi-year cost profile averaged into one annual figure? | Yes — each line's yearly amounts are averaged | [#103](https://github.com/ChenxuYou/uwa-cits5206-project/issues/103) |
| Q13 | Does the platform keep the 1.35 indirect-cost uplift in its revenue projection? | No — the uplift is shown as University overheads recovered | [#104](https://github.com/ChenxuYou/uwa-cits5206-project/issues/104) |
| Q15 | Should a superseded record's PDF say it is superseded? | No — the screen says so; the PDF is unchanged | [#106](https://github.com/ChenxuYou/uwa-cits5206-project/issues/106) |
| Q16 | What do "LG funded" and "GP funded" stand for, and should either change how a line is costed? | Both are offered as labels with the client's wording; neither changes a calculation | — |

The full list, with the reasoning and sources, is in
[`docs/spec/requirements.md` §9](spec/requirements.md#9-open-questions).

## 8. Known limitations and recommended next work

In order of what we would do next:

1. **Redeploy staging from this release** (§4), so that what the client sees is what is handed over.
2. **Salary pre-fill (US-05).** Load UWA's academic and professional pay scales into the method configuration, tie the salary steps to each level (the step list is currently 01–05 for every level), and fill each year's amount from the level, step, FTE and superannuation, left editable. Needs the current pay scales from UWA.
3. **Reconcile the client's calculator line by line** against the engine. The guide governs where they disagree; the commercial-rate divergence is already answered.
4. **Test the folder access rules through routing** ([#86](https://github.com/ChenxuYou/uwa-cits5206-project/issues/86)). The rules work, but removing one would not fail a test.
5. **A PDF link on the approval page** and a recorded A4 print check ([#102](https://github.com/ChenxuYou/uwa-cits5206-project/issues/102)).
6. **Deleting a draft cycle, withdrawing a submission** ([#107](https://github.com/ChenxuYou/uwa-cits5206-project/issues/107)) and **email notifications** ([#108](https://github.com/ChenxuYou/uwa-cits5206-project/issues/108)). Notifications are currently in the tool only.
7. The remaining Should and Could stories in §2.

The current list of gaps is kept in [`src/README.md` — Known gaps](../src/README.md#known-gaps),
and every one has an open issue.

## 9. Ownership

UWA owns the costing method. The team owns the source code. The overarching IP in the tool is
held jointly, and the client's position — that the tool should not be sold onward — is accepted
by the team. This was agreed on 29 July 2026 and signed by Mathew Hall on 20 August 2026.
[`NOTICE`](../NOTICE) grants UWA a perpetual, royalty-free permission to use, run, copy, modify,
host and internally distribute the tool.

## 10. Contacts

| Name | Area | GitHub |
| --- | --- | --- |
| Chenxu You | General backend, documentation | ChenxuYou |
| Yichen Zhao | Front end, client liaison | itsEvanZHAO |
| Wenmin Luo | Calculation engine | onikirinana |
| Dai Lam La La | Deployment | ladailam382 |
| Jaswanth Vericherla | Authentication and accounts | jaswanth-kumar24 |

Questions after handover go to the repository's
[issues](https://github.com/ChenxuYou/uwa-cits5206-project/issues). The team's availability after
the end of semester is not guaranteed, which is why §6 matters.

## 11. Acceptance

By signing, the client confirms that the items in §1 have been received and that the open
decisions in §6 and questions in §7 are understood. It does not close them.

| | For UWA Research Infrastructure | For Group 13 |
| --- | --- | --- |
| Name | | |
| Signature | | |
| Date | | |
