# Assignment 2 — Software Feature Report: Completion Plan

**Deliverable:** an individual report on your own contributions. **One PDF per member**, five in
total — this is not a group submission.
**Due:** Tuesday 29 September 2026, 11:59 pm (UTC+8) — **target upload 8:00 pm**
**Written:** Sunday 13 September 2026 — **sixteen days**
**Brief and rubric:** [`reference/unit/assignment-2-software-feature-report.md`](../../reference/unit/assignment-2-software-feature-report.md)
**Companions:** [`plan.md`](plan.md) · [`team.md`](team.md) ·
[`assignment-1-completion-plan.md`](assignment-1-completion-plan.md), whose shape this follows ·
[`assignment-1/feedback.md`](../assignments/assignment-1/feedback.md), which is why §7 is as long as it is

> **Who this is for.** Every member writes their own report, so the drafting in §5 is yours alone.
> The three things that are *not* yours alone — facilitator access, the file-naming convention,
> and the week-9 cut-off — are shared prerequisites with owners in
> [the brief, §5](../../reference/unit/assignment-2-software-feature-report.md#5-open-items-on-the-brief-itself),
> and one person failing them fails all five submissions.

---

## 1. The rule this plan runs on

**Evidence before prose**, the individual form of Assignment 1's *artefact before prose*.

The rubric's dividing line between *Competent* (2–4) and *Skilled* (5–8) is never the quality of
the writing. In all three criteria it is **attribution** — "Unclear which parts have been written
by the student", "Little evidence of systematic management", "Insufficient evidence of
collaboration". So:

1. **Harvest first, write second.** §4 produces a list of URLs. §5 turns it into prose. Doing
   these in the other order produces claims that then have to be hunted for, and the ones that
   cannot be found quietly stay in anyway.
2. **Every claim carries a link.** One sentence, one artefact. A paragraph about "improving the
   costing screens" with no commit behind it is worth less than a single line naming
   `src/Pages/Ric/Costs.cshtml.cs` with the PR that merged it.
3. **A link the marker cannot open is not evidence.** Assignment 1 proved this at a cost of two
   marks: the client's signed scope was in the repository and the criterion still scored
   *Competent*. Writing the report is not finished until §7's link checks pass against the
   built PDF.
4. **Do not claim the team's work as yours.** Five people share this repository and
   [`plan.md` §2](plan.md) names who built what. Where work was joint, say it was joint and name
   the other person — §6 of the rubric rewards that, and overclaiming is the one failure the
   marker can check in thirty seconds by opening the same board everyone else linked.
5. **The best evidence is created before the deadline, not described at it.** Anything you were
   going to do in S4 or S5 anyway — open the issue, write the test, request the review — is worth
   more done on time than described well. See §3.

---

## 2. Rubric to evidence — what each section must show

| § | Criterion | Pts | The claim it has to support | Where the evidence lives |
| --- | --- | --- | --- | --- |
| 1 | **Software Functionality** | 8 | Named functionality **you** wrote, that runs, is tested, and is documented | `src/`, `tests/`, your commits and branches, in-code comments, `src/README.md` |
| 2 | **Project Coordination** | 6 | You managed your own work **systematically**, with tools, not ad hoc | Issues you opened and closed, PRs you raised, board cards, milestones M1–M4, CI runs, minutes |
| 3 | **Collaboration** | 6 | You made **other people's** work land — reviews, joint work, milestone management | PR reviews you authored, verification you did of someone else's story, meetings you ran, ADRs |

**Weightings say where the effort goes.** Section 1 is 40% and is the only one that rewards
volume of delivered software; sections 2 and 3 are 30% each and reward *records*, which are
cheap to produce and cheap to lose. A member who wrote less code but reviewed carefully and kept
their issues current can still reach *Skilled* on 12 of the 20 points.

---

## 3. Evidence you can still create before the cut-off

Read this before §4, because §4 only finds what already exists. Between 14 and 27 September the
team runs **S4** (rates, proposed rates, balance, justification) and **S5** (seal, PDF, retrieval,
supersession), and **M4 — the vertical slice — falls on 25 September**, four days before this is
due. That ordering is an opportunity, not a coincidence to work around.

| Habit | Why it is worth doing this fortnight |
| --- | --- |
| **One issue per piece of work, closed by the PR that does it** | Turns §2 from an assertion into a linkable trail. `Closes #NN` in the PR body is the whole mechanism |
| **Descriptive branch names, one branch per story** | The brief names "related github branches" explicitly. `us-12-proposed-rates` reads as a contribution; `patch-3` does not |
| **Commit messages that say what and why** | The marker reads these. They are also the cheapest documentation in the repository |
| **Write the test in the same PR as the code** | §1's *Skilled* band says "has been tested". A separate later test PR is worth less than a green check on the PR that added the behaviour |
| **Review someone else's PR properly — comments, not just Approve** | §3's *Skilled* band says "professional conduct of code reviews". A rubber-stamp approval is visible as one |
| **Put a comment at the top of anything non-obvious you wrote** | §1 asks for "supporting documentation you developed such as in code comments, readme etc." |
| **Close the plan items that carry your name** | [`plan.md` §6](plan.md) currently has dated rows against every member. A missed row that stays missed is the opposite of §2's evidence |

**Do not manufacture evidence.** Back-dating, splitting one commit into ten, or opening issues for
work already merged is visible in the timeline and reads exactly as what it is.

---

## 4. Harvest — build your evidence list first

Run these against the repository and keep the output in a scratch file. Replace `YOU` with your
GitHub username from [`team.md`](team.md) — Chenxu You is `ChenxuYou`, Yichen Zhao is
`itsEvanZHAO`, Wenmin Luo is `onikirinana`, Dai Lam La La is `ladailam382`, Jaswanth Vericherla is
`jaswanth-kumar24`. Replace `SINCE`/`UNTIL` with the semester start and the week-9 cut-off once
item B1 in the brief is closed.

```bash
# The repository URL every link in your report is built from
git remote get-url origin

# Your commits, with the files each one touched
git log --author=YOU --since=SINCE --until=UNTIL --stat --date=short \
        --pretty=format:'%h %ad %s'

# How much of the tree is yours, file by file — a sanity check on what you claim in §1
git log --author=YOU --since=SINCE --until=UNTIL --name-only --pretty=format: \
  | sort | uniq -c | sort -rn | head -40

# Branches you created that still exist
git branch -r --sort=-committerdate --format='%(refname:short) %(committerdate:short) %(authorname)'
```

```bash
# Requires the GitHub CLI, authenticated: gh auth login
# PRs you raised
gh pr list --author YOU --state all --limit 100 \
   --json number,title,url,mergedAt,additions,deletions

# PRs you REVIEWED — this is the evidence for section 3, and it is the one
# people forget they have
gh pr list --search "reviewed-by:YOU" --state all --limit 100 \
   --json number,title,url,author

# Issues you opened, and issues assigned to you
gh issue list --author YOU  --state all --limit 100 --json number,title,url,state,closedAt
gh issue list --assignee YOU --state all --limit 100 --json number,title,url,state,milestone

# Milestones, so section 2 can cite them by URL rather than by name
gh api repos/:owner/:repo/milestones --paginate \
   --jq '.[] | "\(.number) \(.title) \(.due_on) \(.html_url)"'

# CI runs on your branches — "tested" with a green check behind it
gh run list --limit 50 --json displayTitle,conclusion,headBranch,url
```

Then, by hand:

- **The Projects board** — https://github.com/users/ChenxuYou/projects/2 — filtered to your
  cards. Note the filtered URL; a marker who opens an unfiltered board sees the team, not you.
- **Documents you authored** in `docs/`, with their paths. Check `git log --follow` on each rather
  than trusting memory — several documents in this repository had more than one hand in them.
- **Meetings you ran or minuted**, from `docs/meetings/`.
- **Client interactions**, from `docs/client/communication-history/` — the rubric's §2 names "the
  client and other stakeholders", and most members have something here.
- **Anything that is not in GitHub at all**: the MS Teams area, a walkthrough you gave a
  teammate, a debugging session on a call. These count under §3 and leave no git trace, so they
  have to be remembered deliberately. Say what happened, when, and with whom.

---

## 5. Drafting — section by section

**Draft in markdown, not in Word.** Put it at
`docs/assignments/assignment-2/<your-name>-submission-draft.md`, following Assignment 1's pattern
of keeping the source beside the submitted PDF. It diffs, it reviews like code, and the PDF is
built from it at the end.

Suggested length: **4–6 pages of PDF.** The rubric rewards traceability, not word count, and a
marker with five of these to read will thank a report that can be checked quickly.

### Section 1 — Software Functionality (8 pts)

**Anchor it in your layer.** Since 15 September each member owns one technical layer —
general backend, calculation, deployment, front end or authentication
([`plan.md` §3](plan.md), [`team.md`](team.md)). Open the section by naming yours in one sentence;
it tells the marker where to look, and five layers that do not overlap are the simplest defence
against A2-3. Work you did before the split still counts — describe it as it happened.

Lead with **what the feature does for the user**, then what you wrote, then the proof it works.

For each feature, a short block of:

| Field | What goes in it |
| --- | --- |
| **Feature** | In the client's language, with the story ID — "US-12: the approver sees the surplus or deficit each proposed rate produces" |
| **What I built** | The files and the design decision behind them, not a file list |
| **Where it is** | Full URLs to the files on GitHub, and to the branch |
| **How it is tested** | The test file and what it asserts; a link to a CI run that passed |
| **How it is documented** | In-code comments, a `README.md` section, or an ADR you wrote |
| **Worked with** | If it was joint, whose work it built on |

Close the section with **one screenshot of the running feature**. The rubric's top band says
"quality code that runs" — a screenshot is the cheapest proof that it does.

### Section 2 — Project Coordination (6 pts)

Answer four questions in order, each with links:

1. **How does work start?** How a story becomes an issue with an owner, a point estimate, a
   milestone and a board card — and where yours are.
2. **How does it get merged?** Branch, commits, PR, review by a second person, CI as a merge
   gate. Name the rule from [`plan.md` §2](plan.md) — *nothing merges on its author's approval* —
   and show a PR of yours where it was followed.
3. **How do problems get reported and resolved?** An issue you opened against someone else's
   work, or one raised against yours, and how it closed. This is the sub-criterion most people
   omit entirely.
4. **How does this connect to the plan?** Which milestone your work sits under (M1–M4), which
   sprint, and what you did when something slipped. A missed date with an honest explanation
   scores better than a plan that pretends nothing moved.

Include **meetings**: which you attended, which you ran, which you minuted, and one decision that
came out of one.

### Section 3 — Collaboration (6 pts)

The trap here is writing about the team instead of about yourself. Every sentence should have
**you** doing something *for* someone else.

- **Reviews you gave.** Two or three, by URL, with what the review actually changed. A review
  that caught a real defect is the single strongest artefact available for this section.
- **Verification you performed on work that was not yours.** [`plan.md` §2](plan.md) assigns a
  verifier to every sprint; if you were one, that is exactly what this criterion asks for.
- **Integration.** Where your component met someone else's — the engine and the screens, the PDF
  renderer and the sealed snapshot — and what you did to make the seam work.
- **Training or joint work.** Pairing, a walkthrough you gave, a convention you wrote down so
  others could follow it, tooling you built that the team uses (`scripts/`, `.vscode/`, CI).
- **Milestone management.** If you tracked a milestone, chased a dependency, or re-planned when
  one slipped, say so with the dates.

---

## 6. Day by day

| When | What | Who |
| --- | --- | --- |
| **Mon 14 – Fri 18 Sep** | S4 runs. Work normally, but **with the habits in §3** — issues, branches, tests in the same PR, real reviews | Everyone |
| **Tue 16 Sep** | ⚠️ **Close B1** — the calendar date week 9 ends. Everything below depends on it | Yichen Zhao |
| **Sat 19 Sep** | Team meeting. Close **B2** (facilitator access to GitHub, board, milestones, Teams) and **B3** (PDF naming). Agree who reviews whose draft on 24 Sep | Yichen Zhao, Chenxu You |
| **Sat 19 – Sun 20 Sep** | **Run §4.** Produce your evidence list. One hour, and it is the hour that decides the mark | Everyone, individually |
| **Mon 21 – Wed 23 Sep** | **Draft all three sections** in markdown. Section 1 first, while S5 is still in flight and you can still add a test or a comment that the report then cites | Everyone, individually |
| **Thu 24 Sep** | **Cross-read in pairs.** Not proofreading — *click every link as if you were the facilitator, signed out*. Pairs agreed on 19 Sep | Everyone |
| **Fri 25 Sep** | **M4 — vertical slice complete.** Fold it into Section 1: sign in → cycle → inputs → rates → propose → justify → seal → export → reopen is the strongest single claim any of us has | Everyone |
| **Sat 26 Sep** | Team meeting. Final read-through of all five drafts, checking that no two people claim the same work and that joint work is described the same way in both reports | Dai Lam La La |
| **Sun 27 Sep** | **Content freeze.** Build the PDF. Run §7 | Everyone; Chenxu You on PDF assembly |
| **Mon 28 Sep** | **Access check** — open every URL in every PDF from a signed-out browser, then confirm the facilitator can reach the private ones | Jaswanth Vericherla |
| **Tue 29 Sep** | **Upload by 8:00 pm.** The last four hours are for the upload failing, not for writing. Attempts are unlimited, so upload a complete draft early and replace it | Everyone, individually |

---

## 7. Pre-submission checklist

Run it against the built PDF, not the markdown.

**Format**

- [ ] One PDF. Not a zip, not a Word document, not two files
- [ ] Named per B3, and the name is the one it keeps afterwards
- [ ] Your **full name as it appears in [`team.md`](team.md)** and your student number on page 1.
      Chenxu You and Yichen Zhao also go by Oliver and Evan — use the enrolment name, so no
      person is credited twice under two names
- [ ] Three sections, in the brief's order, with the rubric's headings
- [ ] **A cover page and a table of contents.** Assignment 1 lost a point for having neither
      ([`feedback.md` §2](../assignments/assignment-1/feedback.md)); it is the cheapest mark on
      the rubric

**Links — the constraint that already cost this team two marks**

Assignment 1 scored *Competent* on client communication with a signed scope agreement sitting in
the repository, because the marker could not open the evidence. The two defects are diagnosed in
[`docs/assignments/assignment-1/feedback.md` §4](../assignments/assignment-1/feedback.md) and the
first three boxes below exist because of them.

- [ ] Every link is a **complete visible `https://…` URL**. **No anchor words** — no `link`, no
      `here`, no document name standing in for an address. Assignment 1's resources table was the
      word `link` ten times, and rendered as ten blue underlined words with no address on the page
- [ ] **No URL wraps mid-path.** Assignment 1's page 1 broke four URLs after `…/tree/main/doc`,
      so copying the visible line returned *"main does not contain the path doc"* — the exact
      error the marker quoted. Break only after a `/`, shorten the path, or give the URL its own
      line at a size it fits on
- [ ] Every URL **copied out of the built PDF by hand** and pasted into a browser. Not clicked —
      copied, because that is what a marker does when the annotation does not survive
- [ ] Every URL is absolute — no `../docs/…`, no `blob/main` link that assumes a branch that may
      be renamed. Prefer a permalink pinned to a commit SHA for code you are claiming
- [ ] Every URL opened once more **in a signed-out or private window**, and anything private is
      on the B2 access list

**Content**

- [ ] Every claim of authorship is backed by a commit, PR, issue, review or document link
- [ ] Joint work is described as joint, with the other person named
- [ ] Section 1 names tests and shows a passing CI run
- [ ] Section 2 shows at least one issue you opened *and* one you resolved
- [ ] Section 3 links at least two PR reviews **you** wrote
- [ ] Nothing confidential is in it — the client's own material stays in `reference/client/`,
      which is not committed ([`README.md` §Confidential material](../../README.md#confidential-material))

**After**

- [ ] The markdown source is committed to `docs/assignments/assignment-2/`
- [ ] The submitted PDF is committed beside it, under the name it was submitted with

---

## 8. Risks

| # | Risk | Trigger | Mitigation | Owner |
| --- | --- | --- | --- | --- |
| A2-1 | **The facilitator cannot open a linked resource**, and a whole section reads as unevidenced | B2 not closed by 19 Sep | Treat 19 Sep as hard. Test with a signed-out browser, not by asking "can you see it?" | Yichen Zhao |
| A2-2 | **Links break in the LMS PDF viewer** — the brief warns of it, and **it already happened to us**: two marks on Assignment 1, criterion 2 | Any anchor word standing in for a URL, or any URL that wraps mid-path | §7, and the diagnosis in [`feedback.md` §4](../assignments/assignment-1/feedback.md). Checked on 28 Sep by copying every URL out of the built PDF | Jaswanth Vericherla |
| A2-3 | **Two members claim the same work**, and both look like overclaiming | Overlapping stories, or an unclear split on joint work | The 26 Sep cross-read exists for this. [`plan.md` §3](plan.md)'s layers and §2's Build/Verify columns are the tie-breaker | Dai Lam La La |
| A2-4 | **Thin evidence for section 2 or 3** because the work was done without issues or reviews | Discovered when §4 is run on 19 Sep | Run §4 **early**. Ten days remain in which the habits in §3 can still produce a real trail | Everyone |
| A2-5 | **M4 slips past 25 September**, and Section 1's strongest claim goes with it | S5 not tracking by 22 Sep | Report what exists on the day. A partial slice described honestly beats a complete one described speculatively, and the marker can run the code | Chenxu You, with each layer's owner |
| A2-6 | **A member leaves it to 29 September** and submits an unevidenced narrative | No draft by the 24 Sep cross-read | The cross-read is the checkpoint; a missing draft that day is visible to the whole team while there is still time | Whole team |
| A2-7 | **`plan.md` §6 rows dated 15–19 Sep stay missed**, and the report cites a plan the repository contradicts | Any row still open on 26 Sep | These rows are individually owned and individually small. Closing yours is also §2 evidence | Row owners |

---

## 9. Change log

| Version | Date | Change |
| --- | --- | --- |
| 1.2 | 16 Sep 2026 | The five technical layers agreed on 15 September ([minutes](../meetings/2026-09-15-team-meeting.md)) are folded in: Section 1 opens by naming your layer, A2-3's tie-breaker cites `plan.md` §3, and A2-5 is shared with every layer owner rather than the two members who used to write most of the code |
| 1.1 | 13 Sep 2026 | Assignment 1's mark came back at 12/15 with two marks lost to unopenable links. §1 gains the rule that follows from it, §7 gains the cover page, the no-anchor-word and no-wrap checks, and A2-2 is rewritten from a warning into a recurrence. Diagnosis: [`docs/assignments/assignment-1/feedback.md`](../assignments/assignment-1/feedback.md) |
| 1.0 | 13 Sep 2026 | Written the day the brief was transcribed, sixteen days out, so that the fortnight of S4 and S5 can be used to *create* evidence rather than only to describe it |
