# Handover summary

**Research Infrastructure Costing & Pricing Tool** — CITS5206 Group 13 to UWA Research Infrastructure
(Erika Slavin, Mathew Hall) · **Monday 12 October 2026** · release `v1.0.0` of
[ChenxuYou/uwa-cits5206-project](https://github.com/ChenxuYou/uwa-cits5206-project)

This page is for a quick check and sign-off. The full detail is in the
[handover document](handover.md).

## 1. Please confirm you have received

- [ ] **The application**, with its source code, automated tests and CI — [`src/`](../src/), [`tests/`](../tests/)
- [ ] **User manual** for custodians, approvers and administrators — [`user-manual.md`](user-manual.md)
- [ ] **Deployment runbook**: server set-up, HTTPS, daily backups, restore and rollback — [`deploy/README.md`](../deploy/README.md)
- [ ] **Requirements, user stories and design decisions** — [`spec/`](spec/), [`decisions/`](decisions/)
- [ ] **Ownership and permission to use** — [`NOTICE`](../NOTICE)
- [ ] **Staging server** running this release: the address and one account per role, sent by email (not in this public repository)

## 2. What was delivered

| | |
| --- | --- |
| **Delivered** | All 18 Must stories and approval by the delegated authority (US-20): the six-step costing workflow, the three rates, submission, return, approval and sealing, sealed records with PDF export, and account management |
| **Partly delivered** | Benchmarking notes (US-24); comparison with the last sealed cycle (US-22) |
| **Not built** | Salary pre-fill from pay scales (US-05), method configuration screen (US-21), replacement reserve (US-23), price-change communication (US-25) |
| **Your 9 October requests** | Five of six done. Salary fill is partly done: the base salary is entered and each year is filled from it. Filling it from the level and step needs UWA's current pay scales |

## 3. Decisions UWA needs to make

The team cannot make these decisions, and its access ends with the unit.

| Decision | Team's suggestion | UWA owner and date |
| --- | --- | --- |
| Who runs the tool: accounts, releases, backups (Q14a) | A named person in Research Infrastructure, with UWA IT for the server | |
| Where it is hosted (Q14b) | A UWA server and domain | |
| SQLite or a managed database (Q14c) | SQLite, unless UWA IT requires otherwise | |
| Real platform figures on the team's staging server (Q14d) | Test figures only, until the tool is on UWA infrastructure | |
| Repository licence (Q8) | A noncommercial licence agreed with UWA, or keep `NOTICE` as the arrangement | |
| Where the repository lives | Transfer it to a UWA GitHub organisation, or UWA keeps a fork | |
| When the team's staging server closes | A fixed date, after UWA has its own server | |

## 4. How the tool reads the method: agree or change?

| # | Question | What the tool does now | Agree / change |
| --- | --- | --- | --- |
| Q11 | Who seals the record? | The delegated authority's approval seals it | |
| Q12 | Multi-year cost profiles | Averaged into one annual figure | |
| Q13 | The 1.35 indirect-cost uplift in the revenue projection | Shown as University overheads recovered, not kept by the platform | |
| Q15 | PDF of a superseded record | Unchanged; the screen shows that it is superseded | |
| Q16 | "LG funded" and "GP funded" | Labels only; they do not change the calculation | |

A change to any of these is contained, and is recorded against its issue on GitHub.

## 5. Sign-off

Signing confirms that the items in §1 have been received and that §3 and §4 are understood. It
does not close them.

| | For UWA Research Infrastructure | For Group 13 |
| --- | --- | --- |
| Name | | |
| Signature | | |
| Date | | |
