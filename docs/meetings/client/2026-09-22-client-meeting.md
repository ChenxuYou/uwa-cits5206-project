# Client Meeting — Deployment, and the First Look at the Working Tool

**Date:** Tuesday 22 September 2026
**Format:** _to confirm_
**Client:** Erika Slavin, Manager (Research Infrastructure & Partnerships), UWA Research
Infrastructure ([contacts](../../client/contacts.md))
**Present (team):** Chenxu You, Yichen Zhao, Dai Lam La La
**Not present:** Wenmin Luo, Jaswanth Vericherla
**Purpose:** show the client the tool running end to end, and settle how it gets onto a server
in time for staging on 2 October ([`plan.md` M5](../../project/plan.md)).

> **About this record.** Written on 24 September 2026 from Chenxu You's account of the meeting.
> The meeting format is marked _to confirm_. Anyone who was there fills it in.

---

## 1. Outcome in one line

**The client will ask UWA IT for a domain and hosting resources. Until those arrive, the team may
deploy on its own server so the tool can be shown and tried.** Staging is no longer waiting on
UWA IT.

## 2. What we brought

The [client deck](../../../presentations/2026-09-22-client-meeting.html) had one section per
technical layer, and a demonstration of the web application.

## 3. Deployment

The deck set out three hosting options and five questions about them:

- **A:** UWA infrastructure.
- **B:** a server the team provisions.
- **C:** B for staging now, moved to A for production at handover.

**The client's answer.** Erika Slavin will approach UWA IT about **a domain and resources** for
the tool. Until then, **we may deploy it ourselves** so that it can be demonstrated.

This is the first half of option C: staging runs on a server the team provisions, and the move
to UWA infrastructure follows once UWA IT responds. It unblocks provisioning, which
[`plan.md` §5](../../project/plan.md) had on hold for this answer since 18 September.

**Not answered at this meeting**, and still open (§6):

- who runs the tool after handover on 13 October: updates, backups and accounts;
- whether real platform figures may go on the team-held staging server, or test figures only;
- whether UWA IT requires a managed database.

## 4. Demonstration

Chenxu You demonstrated the web application as it stood on the day. The deck's slide *The whole
path runs today, end to end* lists what had been built.

**The client's response was very positive.** Erika Slavin said:

- the **business process is clear**;
- the **content is comprehensive**;
- the **interface looks good**.

## Decisions

| Decision |
| --- |
| **Staging is deployed on a team-provisioned server**, without waiting for UWA IT, so the client can see and try the tool |
| **The client leads the approach to UWA IT** for a UWA domain and hosting resources |

## Actions

Set when this record was written, not in the meeting. Owners confirm or re-date them at the next
Saturday meeting.


| Action | Owner | By |
| --- | --- | --- |
| Provision the team-held staging server and take it through [#60](https://github.com/ChenxuYou/uwa-cits5206-project/issues/60): DNS, reverse proxy, TLS, CD with rollback | Dai Lam La La | Fri 2 Oct (M5) |
| Record this answer in [`plan.md` §5](../../project/plan.md) and [`risks.md`](../../project/risks.md) R6 and R17a, both of which still show the hosting decision as unanswered | Chenxu You, Dai Lam La La | Sat 26 Sep |
| Follow up with the client on UWA IT's response | Yichen Zhao | Ongoing |
| Put the three unanswered hosting questions (§3) to the client in one message | Yichen Zhao | Sat 26 Sep |

## Open questions after this meeting

- **Real data on staging.** Until the client answers, staging holds **test figures only**, as the
  deck assumed for a team-held server.
- **Who runs the tool after 13 October.** Needed for the handover notes (M7).
- **Database.** Whether UWA IT requires a managed database, or accepts one database file on the
  server. The answer also settles [`risks.md`](../../project/risks.md) R17a.
- **A custodian to try staging.** The deck asked the client to nominate one platform custodian to
  work through the flow on staging unaccompanied. No nomination is recorded yet.

---

*Written up on 24 September 2026. No recording was made of this meeting.*
