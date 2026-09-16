# ADR-002 — The sealed record is rendered by MigraDoc, from the snapshot

**Status:** Accepted
**Date:** 13 September 2026
**Deciders:** Chenxu You (proposed), to be confirmed at the sprint review of 19 September 2026
**Amends:** [ADR-001](adr-001-technology-stack.md) — the stack table's *PDF export* row, which
said "server-side HTML → PDF, sharing one template with the on-screen record"
**Related:** [`user-stories.md`](../spec/user-stories.md) US-16, US-17 ·
[`architecture.md` §4](../spec/architecture.md) (the immutable record) ·
[`plan.md` M4, S5](../project/plan.md) · [`risks.md`](../project/risks.md) R1, R14 ·
[`skills-audit.md`](../project/skills-audit.md) G2

---

## Context

US-16 — *export the record* — is a Must story, scheduled for S5 (week commencing 21 September)
and required for **M4, the vertical slice, on 25 September**. It is the last unbuilt piece of the
end-to-end path, and it is the piece the client actually keeps: the custodian downloads it, the
delegated authority files it in Content Manager, and in three years someone answers a Freedom of
Information request out of it.

Nothing had been decided beyond one line in ADR-001's stack table. That line is now old enough to
be wrong in two ways, and both were found by writing the spike rather than by discussing it.

**First, the source of the document.** "Sharing one template with the on-screen record" means
rendering the Razor page. The Razor page renders the **live cycle**. The sealed record is the
**snapshot** — a JSON document written at approval, holding the inputs, the results *and the
workings*, with a SHA-256 hash over it ([`architecture.md` §4](../spec/architecture.md)). Those
two can differ: a superseding cycle, a renamed category, a later method version. A document that
claims to be the sealed record but is rendered from today's rows is exactly the class of quiet
disagreement this project exists to remove.

**Second, the cost of the renderer.** HTML → PDF at fidelity means a headless browser on the
server. The team has **no member who has taken an application to a running server with TLS, DNS
and a rollback path** (skills-audit G2), the hosting decision was still open when this was
written, and M5 gives us until 2 October. Adding "install and keep a Chromium on the server" to
that week is a self-inflicted deployment risk on the one milestone where we have the least depth.

## Decision

**The sealed record is rendered by MigraDoc (PDFsharp 6.2.4, MIT) in a separate project,
`src/CostingTool.Pdf`, from the sealed snapshot JSON and from nothing else.**

Three parts to that, each load-bearing:

| | Decision | Why it is in the decision rather than in the code review |
| --- | --- | --- |
| 1 | **From the snapshot, never from the database** | The renderer's only input is the snapshot string and its hash. It cannot read a row, so it cannot disagree with the record it prints. This is enforced by the project having no reference to EF Core or to the web application |
| 2 | **MigraDoc, not a browser** | No native dependency: one NuGet package, `dotnet publish`, done. Nothing to install on a server nobody has provisioned yet |
| 3 | **The font travels with the assembly** | See *The finding that decided it* below |

The renderer prints, for every capability, the three rates **beside the arithmetic that produced
them**, and names the method version and the factor `k` in force when the record was sealed. That
is US-17 answered from the page rather than from the application, and it is the client's own
request of 20 August 2026 that the record show "the workings for the calculator (for transparency
and traceability)".

## Options assessed

| Option | Licence | Server dependency | Verdict |
| --- | --- | --- | --- |
| **F1 · MigraDoc / PDFsharp 6.2.4** | **MIT** | None — pure managed | **Chosen** |
| F2 · QuestPDF | Community licence, free only below **USD 1M annual revenue** | None | **Rejected — see below** |
| F3 · Headless Chromium (Playwright / Puppeteer) rendering the Razor page | MIT | ~300 MB browser on the server, kept patched | Rejected: adds the largest unknown to the weakest week |
| F4 · iText 7 | AGPL, or paid | None | Rejected: AGPL is not compatible with the ownership position in [`NOTICE`](../../NOTICE) §2, and the commercial licence is not ours to buy |
| F5 · Print the Razor page from the browser | — | None | Rejected: the custodian's printer settings become part of the record, and a page rendered from live rows is not the snapshot |

**Why QuestPDF was rejected, specifically.** Its API is the nicest of the five and it was the
first choice until the licence was read. The Community licence is free for organisations under
USD 1M in annual revenue; **UWA is not one of those organisations**, and this tool is handed to
UWA at the end of the semester. Choosing it would mean handing the client software they must buy
a licence to keep running — a bill produced by our convenience, discovered after we had gone.
[`NOTICE`](../../NOTICE) §3 grants the University a royalty-free permission to run this software;
a dependency that contradicts that permission cannot be in it.

## The finding that decided the shape of the code

**PDFsharp's platform-agnostic build has no font of its own, and its default resolver throws on
Linux.** There is no font-resolution strategy common to every operating system .NET runs on, so
the library declines to guess and expects the application to supply a resolver.

Left alone, that defect surfaces on **2 October**, on staging, in front of the client — a
renderer that works on five Windows laptops and throws on the first Ubuntu server it meets. It is
precisely the failure mode G2 predicts.

So `src/CostingTool.Pdf/Fonts/` carries DejaVu Sans (regular and bold, 1.4 MB, Bitstream Vera
licence — redistribution and embedding permitted, text in `Fonts/LICENCE-DejaVu.txt`), embedded
as a resource and resolved from the assembly. The server needs no fonts installed, and a record
exported from staging is the same document as one exported from a developer's machine — which is
the least a *sealed* record can promise.

## Consequences

**Good**

- US-16 stops being the unknown in S5. The renderer exists, and what is left is wiring and review.
- The export is testable without a database, a browser or a signed-in user: give
  `SealedRecordPdf` a JSON string, get bytes. `tests/CostingTool.Pdf.Tests` asserts the client's
  worked example — **$100.00 / $162.00 / $202.50** — on the way *out* of the system, which is the
  first time that figure has been checked anywhere other than in the engine.
- One more compiler-enforced boundary: the renderer cannot reach a database row, in the same way
  the engine cannot ([`architecture.md` §3](../spec/architecture.md) rule R7).

**Bad, and accepted**

- The document is laid out in C#, not in HTML, so the on-screen record and the PDF are two
  layouts of the same figures. They can drift apart in *presentation*. They cannot drift apart in
  *numbers* — both read the same rounded values the engine produced — and it is the numbers that
  a Freedom of Information response turns on.
- 1.4 MB of font in the repository. Deliberate; the alternative is a server-configuration step
  that nobody would document.
- A third project under `src/`, which calls in the restructure debt recorded in
  `src/CostingTool.csproj` — now an owned, dated item in [`plan.md` §6](../project/plan.md)
  rather than a comment.

## Follow-on actions

| # | Action | Owner | By |
| --- | --- | --- | --- |
| 1 | Run `dotnet restore --force-evaluate` and commit the regenerated `src/packages.lock.json` and the new `src/CostingTool.Pdf/packages.lock.json` — CI restores against them | Chenxu You | 15 Sep |
| 2 | Confirm this ADR at the sprint review; status stays *Accepted (proposed)* until a second member has reviewed the pull request | Wenmin Luo | 19 Sep |
| 3 | Give the approver the same download link. Today only the custodian can export, because `/Approvals` is a separate folder with its own policy | Chenxu You | S5 |
| 4 | Decide whether a superseded record's PDF is watermarked as superseded — US-02 and US-15 imply it and no requirement says it | Dai Lam La La | With the next question batch |
| 5 | Amend ADR-001's stack table row so the two ADRs do not contradict each other in a reader's hands | Chenxu You | 15 Sep |
| 6 | Check the exported PDF opens in Adobe Reader, Preview and Edge, and prints on A4 without clipping | Jaswanth Vericherla | S5 |

## Change log

| Version | Date | Change |
| --- | --- | --- |
| 1.0 | 13 Sep 2026 | First version, written alongside the spike rather than before it — the font-resolver finding is the reason the decision has the shape it has, and it was found by building |
