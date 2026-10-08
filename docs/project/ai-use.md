# Use of Generative AI — Statement and Record

- **For:** the *Responsible use of AI* criterion of the final group report, and anyone who needs
  to know how AI was used in building this project.
- **Covers:** the whole project, from the team's first rules on 5 August 2026 to the final
  release on 13 October 2026 — code, tests, documents, presentations and the individual
  deliverables that draw on them.
- **Status, 8 October 2026:** complete, with every member's account in §6. The one gap is the
  tools each member other than Chenxu You used, marked _to confirm_ in §3 and §6.
- **How this document was written:** drafted with Claude from the repository record — commits,
  pull requests, issues, minutes and decks — and from each member's own account, collected by
  Chenxu You. Each account was then checked against the code and the record, and corrected where
  they disagreed. Every incident below is first-hand, and where the code or a pull request shows
  the fix, it is named.

---

## Contents

1. [In short](#1-in-short)
2. [The rules we set ourselves](#2-the-rules-we-set-ourselves)
3. [Tools, and what they were used for](#3-tools-and-what-they-were-used-for)
4. [How AI output was checked](#4-how-ai-output-was-checked)
5. [Against UWA, ACS and unit requirements](#5-against-uwa-acs-and-unit-requirements)
6. [Individual accounts](#6-individual-accounts)
7. [How AI helped the team work together](#7-how-ai-helped-the-team-work-together)
8. [What we learned as a team](#8-what-we-learned-as-a-team)
9. [Acknowledgement statement for the report](#9-acknowledgement-statement-for-the-report)
10. [Sources](#10-sources)

---

## 1. In short

We used generative AI as a **tool that drafts, never as an author that decides**. It wrote
first versions of code, tests and documents, reviewed pull requests, and helped us read
unfamiliar parts of ASP.NET Core and EF Core. Every output was checked by a person, against
something other than the AI's own say-so — the client's worked example, the test suite, the
code itself, or the live record on GitHub — and the person who committed it is responsible
for it.

Three principles ran through the project:

1. **The person is accountable, not the tool.** A model can be wrong; only a person can be held
   to account for being wrong. So the person who commits a change owns it, whoever or whatever
   typed it.
2. **Verification scales with consequence.** A wording change is read once. A change to the
   calculation, the seal or sign-in has to pass tests that would fail if it were wrong.
3. **What goes into an AI tool is a decision, not a habit.** No client data, personal data or
   third-party IP went into a public AI tool.

We gained speed and breadth from AI, and we learned where it fails: it is confidently wrong,
it knows only what it is shown, and it states intentions as if they were facts. §6, §7 and §8
give the specific cases.

---

## 2. The rules we set ourselves

**Set on 5 August 2026**, and presented to our facilitator at the first checkpoint ([deck](../../presentations/2026-08-05-facilitator-checkpoint.html), section 3;
[minutes](../meetings/facilitator/2026-08-05-facilitator-meeting.md)). The slide's heading was
*Used deliberately, declared openly, never as a substitute for our thinking*.

| We do | We never do |
| --- | --- |
| Follow the AI use tier and AI use statement set for each assessment — the assessment brief is definitive, not the general guide | Put client data, personal data or third-party IP into a public AI tool |
| Keep a record of tools, prompts and outputs, and submit an acknowledgement statement | Use AI to replace critical thinking or analysis |
| Check every AI output ourselves — we remain responsible for its accuracy | Use DeepSeek, prohibited on UWA networks and devices |

The deck cited *Using Artificial Intelligence Tools at UWA: A Guide for Students* and the UWA
Academic Integrity Policy as the source of these rules.

**Refined on 15 August** at the team meeting
([minutes](../meetings/team/2026-08-15-team-meeting.md) §2 and §6):

- Keeping every project document in one repository is for the team's convenience, and **is not a
  licence to feed client material to AI tools**.
- The client had been asked on 29 July whether AI tools may be used on their material
  ([kickoff minutes](../meetings/client/2026-07-29-client-meeting.md)). No answer came. The team
  decided not to chase it, on the basis that **AI would be used to help write code, not to process
  client data** — so the question blocks nothing.

**Kept out of the repository by rule.** The client's guide, calculator and worked examples are
never committed ([`.gitignore`](../../.gitignore) §1), and neither is the working output that AI
coding assistants write into the tree (§3c, added on 18 September — see §6.1, incident 5).

---

## 3. Tools, and what they were used for

| Tool | Used by | Used for |
| --- | --- | --- |
| **Claude** — Claude Code and the Claude app | Chenxu You | Drafting and refactoring backend code and tests; pull-request reviews; repository maintenance; drafting and checking documents, decks and this statement |
| **ChatGPT** | Chenxu You | Questions, explanations and first drafts, alongside Claude |
| **GitHub Copilot** | Chenxu You | Line and block completion in VS Code |
| _To confirm_ | Yichen Zhao | Generating views and layouts; summarising client feedback; polishing emails to the client |
| _To confirm_ | Wenmin Luo | Refactoring the engine; explaining `decimal` arithmetic and rounding |
| _To confirm_ | Dai Lam La La | Generating deployment configuration; listing failure modes to rehearse |
| _To confirm_ | Jaswanth Vericherla | Drafting access checks; playing the attacker to find cases to test |

**What AI was not used for, by anyone:**

- **Decisions.** The stack (ADR-001), the scope signed by the client, the five technical layers,
  and what to cut were decided by the team, in meetings that are minuted.
- **The client's data.** No client spreadsheet, guide or figure was pasted into a public AI tool;
  each member confirms this in their account in §6. The test fixtures are our own rewrite of the
  worked example in the client's guide.
- **Our faces and voices.** The pitch-video brief forbids AI voices and faces
  ([brief](../../reference/unit/assignment-4-pitch-video.md) §2).
- **Our individual accounts.** Each member's account in §6 is their own. AI was used, at most,
  to tidy the wording.

---

## 4. How AI output was checked

AI output was never accepted on the AI's own word. These are the checks it had to pass, and
each one would fail if the output were wrong.

| Check | What it catches | Where |
| --- | --- | --- |
| **The golden file** — the client's worked example, $150,000 / $20,000 / $30,000 / 1,000 h → $100.00 / $162.00 / $202.50, to the cent | Any change to the arithmetic, however plausible it looks | `tests/CostingTool.Engine.Tests`; a CI merge gate |
| **The same figures, on the way out of the PDF** | A correct engine behind a document that prints something else | `tests/CostingTool.Pdf.Tests` |
| **The web test suite** — identity, access control, the guided flow, the seal | Code that works for one request and fails for the next | `tests/CostingTool.Web.Tests` |
| **Concurrency tests on a real database** | Two requests racing — invisible to tests that run one request at a time | [`ConcurrencyTests`](../../tests/CostingTool.Web.Tests/ConcurrencyTests.cs) |
| **Migration tests** | A schema change with no migration; a generated migration that does not match the model | [`MigrationTests`](../../tests/CostingTool.Web.Tests/MigrationTests.cs) |
| **Formatting and dependency scan** | Untidy diffs; a vulnerable package an AI suggestion pulled in | [`ci.yml`](../../.github/workflows/ci.yml) |
| **Source precedence** — the client's written answers, then the guide, then the calculator, then our minutes | An AI "fact" that no client document supports | [`requirements.md`](../spec/requirements.md) |
| **Reading the code** | A description of behaviour that the code does not have | — |
| **Checking the live record** — GitHub, the client's emails, the team | A claim the repository's documents make but reality does not support | — |
| **A second member's review** | Anything the author is too close to see | Pull requests — see the caveat in §6.1, incident 6 |

---

## 5. Against UWA, ACS and unit requirements

Each requirement, what we did, and where the evidence is.

### UWA

| Requirement | What we did | Evidence |
| --- | --- | --- |
| AI may be used in an assessment only where the unit permits it, and the unit's rules come first | The final report's rubric assesses *Responsible use of AI* directly, so the unit expects AI use in this deliverable to be declared and evaluated. Where a brief sets its own rule, we followed that rule — the pitch video uses our own faces and voices | The LMS rubric; [pitch-video brief](../../reference/unit/assignment-4-pitch-video.md); rule 1 in §2 |
| AI must not replace your own critical thinking and analysis | AI drafted; people decided. Every decision of consequence is in a minuted meeting or a decision record | [ADR-001](../decisions/adr-001-technology-stack.md); [`docs/meetings/`](../meetings/README.md) |
| The student is responsible for checking the accuracy of AI output | Every output passed the checks in §4. §6 gives cases, from every member, where the check caught the AI | §4; §6.1 |
| Cite and acknowledge AI use: the tool, the dates, what it helped with, and where | This document, and the statement in §9 for the report's title page | §3; §9 |
| Keep records of prompts and outputs, which the unit coordinator may ask for | AI sessions are kept in the members' own accounts with each tool and can be produced if asked — each member confirms theirs in §6. The repository records what each session produced: the commit, the pull request and its review | §2, rule 2; §6 |
| UWA Code of Conduct — honesty and integrity, care and diligence | We do not overstate what AI did, or what we did: §6 records failures as well as successes | §6; §7 |

### ACS Code of Professional Ethics (2023)

| Clause | How it applied to our use of AI |
| --- | --- |
| **Honesty 1** — be honest, open and truthful | AI use is declared here, in full, including where it went wrong |
| **Honesty 2** — do not misrepresent the capability of yourself or colleagues, directly or by omission | We do not claim AI-written work as unaided work, and no member's account was written for them |
| **Trustworthiness 1** — be accountable for all you undertake; take responsibility for failures as well as successes | The person who commits a change is responsible for it, however it was produced |
| **Trustworthiness 4** — respect the privacy and confidentiality of information in your possession | No client data or personal data went into a public AI tool; client material stays out of the repository |
| **Trustworthiness 6 and 7** — do not undertake work you lack the skills for; be competent in what you attempt | AI was not used to stand in for understanding. Where it introduced something new to us — migrations, concurrency tokens — we learned it and proved it with tests before relying on it |
| **Trustworthiness 8** — develop systems that are robust, secure and user-friendly | AI-written code met the same gates as any other; the defects it introduced were fixed before staging (§6.1) |
| **Respect for Others 2** — respect others' views; take account of others' points of view | A teammate's work was not overwritten by AI-generated changes (§6.1, incident 6) |
| **Respect for Others 7** — respect others' intellectual property | Our own unit guide maps "unattributed GenAI output" to this clause ([ethics guide](../../reference/unit/CITS5206-Ethics-Test-Guide.md)); this statement is the attribution |

---

## 6. Individual accounts

Each account is written by its member, in their own words, and covers the same four things:
**the tools they used and for what; where AI helped; where it was wrong and how they caught it;
and what they will do differently.** AI may tidy the wording; it does not supply the content.

### 6.1 Chenxu You — general backend, repository and documents

**Tools:** Claude (Claude Code and the Claude app), ChatGPT and GitHub Copilot.

**What I used AI for.** I own the general backend — page models, the data model, EF Core,
sealing, retrieval and supersession — and CI, the repository's structure and much of its
documentation. AI drafted first versions of page models and their tests, reviewed my pull
requests, and explained the parts of ASP.NET Core and EF Core I had not used before. In documentation it drafted, and I corrected. It made me much faster at
producing a first version, and it taught me things — concurrency tokens, migrations — that I
now understand properly because I had to check its use of them.

**Where it was wrong.** Six cases, each with what happened, how it was caught and what changed.

**1 · The double-clicked approval that sealed a record twice.** The approval handler that AI
drafted for sealing a record read the cycle, checked that it was *Submitted*, built the sealed
snapshot and its SHA-256 hash, and saved. It read well and passed every test we had. But
nothing stopped two requests passing the same check at the same moment: a double-click on
*Approve & seal*, or two approvers at once, could each write — rewriting the snapshot and hash
of a record that is meant never to change, or returning a sealed cycle to editable. The seal is
the tool's central promise; a record that can be rewritten after approval is not a record.

*Caught* in the audit of 25 September, before staging. *Fixed* in
[#90](https://github.com/ChenxuYou/uwa-cits5206-project/pull/90): a concurrency stamp on every
cycle, rotated on every save, so a request that read the cycle before it was sealed cannot
write afterwards; the approval page reports a conflicting decision instead of failing; and the
buttons disable after the first click. *Proved* by tests that stage the race on a real SQLite
database — `ADoubleClickedApprovalSealsOnce`,
`ARequestThatReadTheCycleBeforeItWasSealedCannotOverwriteTheSeal` and
`ASecondApproverReturningASealedCycleIsToldAndChangesNothing`.

*What I learned:* AI writes the path where one person does one thing very convincingly. It
checked the state but not the gap between checking and writing, and tests that send one
request at a time can never show that gap. I now ask, of anything that changes a record:
*what if this arrives twice?*

**2 · `EnsureCreated`, and the client's data on the second deployment.** The start-up code AI
scaffolded built the database with `EnsureCreated()`. That is fine for a demonstration: it
creates the schema once. It cannot change it afterwards. Every time the data model changed,
the local database had to start again under a new file name — `ric-costing-v7.db`, then
`v8.db`. On a developer's machine that costs a few minutes. On a server, the first release with
a model change would either have failed against the old schema or started a new, empty
database — leaving every cycle the client had entered, and every record they had sealed,
behind.

*Caught* in the same audit. *Fixed* in #90: EF Core migrations, applied at start-up; a database
made by `EnsureCreated` is refused with the reason rather than silently replaced; and
`MigrationTests` fails CI when the model changes without a migration. The baseline migration was
generated from the runtime model, because the EF tools could not be installed where it was
written — so it is *trusted only because a test proves* it builds exactly the model's schema.

*What I learned:* AI optimises for *it runs now*. The question *what happens on the second
release, when the data is the client's?* has to come from a person who knows whose data it will
be.

**3 · A finding that was itself wrong.** The same audit listed a medium finding, M5, that CI did
not scan dependencies. It did: `ci.yml` already ran the vulnerable-dependency job. The finding
was checked against `ci.yml`, recorded as wrong in #90, and not acted on. A review is a source like
any other — its findings are claims to check, not instructions to follow.

**4 · Six team members, in a team of five.** While preparing my pitch video, the AI described
the team as me and five others. There are five of us in total. It was probably counting
DongSheng Li, who withdrew on 27 July and still appears in records from before that date
([`team.md`](team.md)). An error like that, said aloud to the client in a video, would have
undermined everything else in it. I caught it because I know my own team — and because
`team.md` is the roster's only home, there was one place to check. *What I learned:* AI cannot
tell a current record from a historical one unless it is told which is which.

**5 · AI working files committed to a public repository.** Claude Code writes its working
output — pull-request reviews and scratch analysis — into a folder in the tree. That folder went
into a commit by accident. It was untracked on 18 September, and
[`.gitignore`](../../.gitignore) §3c now keeps it out
([`9596f4f`](https://github.com/ChenxuYou/uwa-cits5206-project/commit/9596f4f)). *What I
learned:* an AI tool leaves artefacts where you work, and in a public repository *where you
work* is one `git add` from *what you publish*. I now read what I stage.

**6 · Documents that described the plan instead of the project.** On 7 and 8 October I used
Claude to audit the repository's documents against the final report's rubric and bring them up
to date. It was fast and thorough, and it was wrong in ways worth recording:

| What the AI wrote | What was true | How it was caught |
| --- | --- | --- |
| That staging had not been deployed and M5 was overdue | Staging had been live since 30 September; the deployment was recorded in my email and notes, not yet in the repository | I corrected it — and the repository now records it ([#60](https://github.com/ChenxuYou/uwa-cits5206-project/issues/60)) |
| In a README draft: "every pull request is reviewed by a second member" | Of the 37 pull requests merged by 3 October, 22 were merged by their own author with no approving review recorded on GitHub — several of them mine | Checked against GitHub before the commit; the claim was removed, and the gap is recorded honestly |
| That the tool assumes the platform keeps the indirect-cost uplift in its revenue projection | The code assumes the opposite: the uplift is shown as University overheads recovered | Re-reading `RateEngine` |
| That each member had uploaded their Assignment 2 report | Nothing in the repository records it | Replaced with "not recorded" |
| A rewrite of the README, plan and risk register | A teammate had updated the same three files in his own pull request that day ([#94](https://github.com/ChenxuYou/uwa-cits5206-project/pull/94)) | I chose not to overwrite his work, and had the changes rebuilt without those files |

The pattern is the lesson. AI treats **absence of evidence as evidence of absence** (staging),
turns **an intention into a fact** (reviews), and fills a gap with **the most plausible
answer** (uploads). None of these is a typo; each is a claim a marker could have checked and
found false. The fix each time was the same: check the claim against the thing itself — GitHub,
the code, the person — and not against another document.

The last row taught me something different. The AI had no sense that those files were someone
else's work. It could rewrite them; it could not know whether it should. That judgment —
whose work this is, and what respect for it requires — stayed with me.

**What I think about it.** A language model produces the most plausible continuation of what it
is shown, not the true one. Most of the time the two coincide — which is exactly why the
exceptions are dangerous: they arrive in the same confident voice as everything else. An IBM
training document from 1979 put the consequence in one line: *a computer can never be held
accountable, therefore a computer must never make a management decision.* I take that
literally. The AI can draft the approval handler; it cannot be answerable to the client for a
sealed record that changed. So the review is not a ritual performed at the end — it is the work.
AI made me faster at writing and slower at believing, and I think that is the right trade.

**What I will do differently.**

- **Keep the AI record as I go**, per pull request — what was generated, what was changed, what
  was rejected — rather than reconstructing it at the end, as this document had to.
- **Ask the hostile questions first**: twice, at the same time, on the second release, with real
  data. AI rarely asks them unprompted.
- **Write the test that would catch the AI being wrong before accepting what it wrote**, not
  after — the golden file is the model, because it was written before the engine.
- **Check claims against the source**, never against another document that may be equally
  stale.

### 6.2 Yichen Zhao — front end and client contact

> *"AI can draw the screen. It doesn't stand on the user's side for you."*

**Tools:** _to confirm_.

**1 · A clean form with a hole in it.** Yichen asked AI to generate the Razor form for entering
costs. The result looked clean, but it echoed the description a user typed back onto the page
with `@Html.Raw`, which switches off Razor's automatic encoding — a cross-site scripting hole in
a form every custodian fills in. He tested it by typing a `<script>` element into the field, and
the page ran it. The view now uses Razor's normal, encoded output; the only `@Html.Raw` left in
the application writes constant lists of category names into the page's script, never anything a
user typed. *The rule since:* every AI-generated view that shows user input back is tested with
hostile input before it merges ([`risks.md`](risks.md) R11).

**2 · $162.00000000000003.** For a live running total, AI summed money in JavaScript, in binary
floating point. The screen showed $162.00000000000003 where the PDF said $162.00 — a figure a
custodian would rightly distrust. The fix put the arithmetic back where the project says it
belongs: every figure that is stored, sealed or printed is calculated and formatted on the
server, in `decimal`; what the browser still works out as you type is a preview, rounded to the
cent, and the server's figure is the one kept. He checked the screens against the client's worked
example.

**3 · "Would be nice" became "must".** He used AI to turn the client's spoken feedback into a
to-do list, then checked every item against the client's email. One had changed in the summary:
*it would be good to be able to export* had become *must support Excel export*. He put it back
as it was said. Left in, it would have been scope the client never asked for, built in the
weeks we had least of.

**What he gained.** AI let him try three layouts for a screen in the time one would have taken
— but the choice between them stayed his. In his words: *"AI gives me options, the client gives
me constraints, and the trade-off is mine."*

### 6.3 Wenmin Luo — calculation engine

> *"Money has to be right to the cent. AI's 'close enough' doesn't count."*

**Tools:** _to confirm_.

**1 · One cent.** Wenmin asked AI to refactor `RateEngine`. It moved `decimal` rounding forward
into intermediate steps. Each function still looked right on its own; the chain as a whole came
out one cent wrong on a boundary input. The engine tests stopped it before it merged — the
boundary tests that sit beside the golden file, among them
`StoredValuesStayUnroundedAndRoundingHappensOnceAtPresentation` and
`AHalfCentRoundsAwayFromZero`. *What the whole team took from it:* a test's expected values are
written before the code, and never by AI — otherwise the test checks the AI against itself.

**2 · A formula with no source.** She asked AI for the conventional way to calculate indirect
cost recovery. It gave a formula that sounded authoritative and appears in no client document.
The team's source-precedence rule — the client's written answers, then the guide, then the
calculator, then our own minutes — decided what happened next: anything with no source marker is
recorded as *our* assumption and put to the client, as the two modelling decisions in
[`requirements.md` §9](../spec/requirements.md#9-open-questions) are.

**What she gained.** AI explained the difference between `decimal` and `double`, and between
banker's rounding and rounding half away from zero — which is why the method version records its
rounding rule as `MidpointRounding.AwayFromZero`. She can now explain both without it.

### 6.4 Dai Lam La La — deployment

> *"A command that runs isn't a command that's safe to run."*

**Tools:** _to confirm_.

**1 · The defaults in `docker-compose.yml`.** The Compose file AI generated for staging put a
default database password in plain text and published the PostgreSQL port to the outside. He
moved the secrets into an environment file on the server, kept out of the repository, and left
the database reachable only on Docker's internal network, with the application bound to the
server itself behind the proxy. The checks are written into the deployment guide.

**2 · The shortcut past the certificate warning.** Staging is reached by IP address until UWA IT
provides a domain, so browsers show a certificate warning. AI suggested switching TLS
verification off in the scripts to make the warning go away. He refused: the warning is the
browser telling the truth. Testers are told instead how to proceed past it by hand — the client's
email of 30 September says exactly that — and a trusted certificate for the domain is on the list
for when the domain exists.

**3 · Ten ways it could go wrong.** He asked AI for ten things that could go wrong in a
deployment, then rehearsed the recovery from each himself, on the server. The list came from AI;
the rollback steps are the ones he ran and watched work.

**What he gained.** A deployment is the first thing about the project the client sees. *"AI saved
me hours of reading documentation. But 'is this safe?' is a question only I can answer."*

### 6.5 Jaswanth Vericherla — authentication

> *"In security, what's dangerous is AI that looks right."*

**Tools:** _to confirm_.

**1 · 403 for yours, 404 for nobody's.** AI's first version of the access checks answered a
request for another custodian's cycle with *403 Forbidden* and a request for one that did not
exist with *404 Not Found*. That difference is enough to discover which records exist. He made
both answer identically, and wrote the tests that hold it there —
`ACustodianCannotOpenAnotherCustodiansCycle` and
`AMissingCycleAndSomeoneElsesCycleAnswerTheSameWay` — along with the rule that an unknown
username is refused in the same words, and in the same time, as a wrong password.

**2 · A lockout that barely locked.** AI's default lockout settings were too lenient. He set them
to what the tool uses now — five failed attempts lock an account for fifteen minutes, the same for
every role — and tested the edges: four failures do not lock it, the fifth does, the lockout holds
until the last second and lifts at the next, and each role lands on its own page after signing in
(`FiveFailedAttemptsLockTheAccount`, `TheLockoutRunsForFifteenMinutes`,
`AnApproverLandsOnTheApprovalQueue`; [#85](https://github.com/ChenxuYou/uwa-cits5206-project/pull/85)).

**3 · "The administrator can do everything."** To make debugging easier, AI had given the
administrator a way past every check. He took it out. An administrator sees every cycle and
manages the accounts, but cannot edit, submit or seal another person's cycle — each of those writes
a name into the record — and a sealed record is read-only for every role. That is the product's
central promise, and a convenience for developers does not get to break it.

**What he gained.** He asked AI to play the attacker — to list the ways it would try to break
sign-in and access control — and then wrote a test for each idea himself. AI's value was the angles
he had not thought of; whether the system resisted them was for the tests to say.

---

## 7. How AI helped the team work together

Beyond the code, AI changed how the five of us communicated. Each practice below came with a
rule, because each had a way of going wrong.

**1 · Minutes: from "who writes them" to "everyone checks them".** AI drafts the minutes, and the
meeting's chair checks the draft against what was actually said before it is merged. Writing up a
meeting fell from about thirty minutes to about ten, action items came out clearer, and arguments
about who had agreed to do what became rare. Not every record met the 24-hour rule: several were
written up weeks later, and each says so in its own *About this record* note. *The rule:* AI
writes *plans to* as *has done*, so every item a set of minutes marks as done has to point to a
pull request or an issue.

**2 · One glossary.** Words such as *indirect cost*, *uplift* and *overheads recovered* meant
slightly different things to different members. AI helped turn the terms into a single table —
working from our own rewritten notes, never from the client's documents — and Wenmin Luo checked
every entry against the calculation. Screens, the PDF and our own discussions have used the same
words since.

**3 · Pull-request summaries and an AI first pass.** Before a pull request goes up, its author asks
AI for a summary of the change and the places most likely to hide a problem, and edits the result.
A reviewer opening it knows where to look first. The first pass has caught small things — a missing
migration, an unused variable. It does not replace a person's review, and we have not pretended it
does: §6.1, incident 6, records that most pull requests were merged without a second member's
approval on GitHub.

**4 · Emails to the client.** Our English is uneven. Drafts to the client are polished for tone with
AI, the facts are confirmed by Yichen Zhao as client contact, and only then are they sent. Replies
went out sooner and read more professionally. The facts were never left to AI, and anything
sensitive was taken out before a draft went near an AI tool.

**5 · Keeping the record.** Each member keeps their AI sessions in their own account with each
tool, so they can be produced if the unit coordinator asks, and each account in §6 says what AI
did, where it was wrong and how that was caught.

---

## 8. What we learned as a team

From the cases in §6 and §7:

1. **AI's errors are plausible, not random.** The seal that could be rewritten, the cent lost in
   a refactor, the 403 that gave records away — each looked like correct work and passed a shallow
   check. The defence is a check that does not depend on the output looking right: a test with a
   known answer, hostile input, the code itself, the live record.
2. **AI's defaults are where the risk hides.** A plain-text password and an open database port, a
   lenient lockout, `@Html.Raw` on user input, an administrator who can do anything. AI chooses
   whatever makes the example work; whether it is safe has to be asked separately, every time.
3. **AI knows only what it is shown.** It counted a member who had left, and missed a deployment
   recorded only in an email. Context is the human's job: what is current, what is historical,
   what lives outside the repository.
4. **AI turns intentions into facts, and suggestions into requirements.** *Every pull request is
   reviewed* became *every pull request was reviewed*; *it would be good to export* became *must
   support Excel*; a formula from nowhere sounded like policy. Anything written with AI is checked
   against what happened and what the client actually said.
5. **A test's answer is never written by the thing being tested.** Expected values come from the
   client's worked example and from people, before the code — so AI cannot pass its own exam.
6. **Authorship is a human judgment.** AI can change anything it can reach, including a
   teammate's work. Whether it should is a question about respect and ownership, and it is ours
   to answer.
7. **Review effort follows consequence.** A sentence gets a read; the seal, the arithmetic,
   sign-in and the server get tests and rehearsals designed to fail if the work is wrong.
8. **The gains are real.** Faster first drafts, layouts tried in minutes, broader reviews, an
   attacker's eye on demand, and an always-available explainer for unfamiliar frameworks let five
   students with uneven experience ship a working, tested, deployed application in a semester.

---

## 9. Acknowledgement statement for the report

For the title page of the final report, as UWA Library guidance asks: the tool, the dates, the
kind of assistance, and where. **Complete the tool list once each member has confirmed theirs in §3.**

> **Acknowledgement of generative AI use.** In preparing this project and report, the team used
> Claude (Anthropic; Claude Code and the Claude app), ChatGPT (OpenAI) and GitHub Copilot
> *[and the tools named in each member's account]*, between August and October 2026. They were
> used to draft and review code and tests, to explain unfamiliar framework behaviour, to review
> pull requests, and to draft and edit documentation, including parts of this report. No client
> data, personal data or third-party intellectual property was entered into a public AI tool.
> Every AI output was checked by a team member — against the client's worked example, the
> automated test suite, the source code and the project record — and the member who committed
> each change is responsible for it. How AI was used, where it was wrong and what we changed as a
> result are set out in `docs/project/ai-use.md` in the project repository.

---

## 10. Sources

- UWA — *Using AI Tools at UWA*, guide for students:
  https://teaching.csse.uwa.edu.au/units/CITS2003/Using_AI_Tools_at_UWA.pdf
- UWA Library — *Acknowledging and referencing AI*:
  https://guides.library.uwa.edu.au/artificial_intelligence/acknowledging_and_referencing_AI
- ACS — *Code of Professional Ethics* (March 2023):
  https://www.acs.org.au/content/dam/acs/rules-and-regulations/CodeOfProfessionalEthics_Mar_2023.pdf
- The team's rules: [5 August deck](../../presentations/2026-08-05-facilitator-checkpoint.html),
  [5 August minutes](../meetings/facilitator/2026-08-05-facilitator-meeting.md),
  [15 August minutes](../meetings/team/2026-08-15-team-meeting.md)
- The defects and fixes in §6.1: [#90](https://github.com/ChenxuYou/uwa-cits5206-project/pull/90),
  [`9596f4f`](https://github.com/ChenxuYou/uwa-cits5206-project/commit/9596f4f),
  [#60](https://github.com/ChenxuYou/uwa-cits5206-project/issues/60),
  [#94](https://github.com/ChenxuYou/uwa-cits5206-project/pull/94)
