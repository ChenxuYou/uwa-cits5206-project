#!/usr/bin/env python3
"""
Fill the two gaps the issue review found.

**assign** — seven Must stories carry no assignee (US-04, US-06, US-07, US-08, US-09,
US-12, US-13 at the time of writing; the script derives the list rather than trusting
that). Every member of the team takes a share, dealt out at random but *balanced*: with
seven stories and five members nobody holds more than two. The deal is seeded, so a dry
run and the apply that follows it produce the same answer, and re-running changes
nothing. Pass --seed to deal again.

**deploy-issue** — `docs/project/plan.md` §2 puts "deployment" in S6 alongside US-19,
but only the story was ever opened. This creates the missing issue against M5, so the
sprint's work is visible on the board rather than living only in the plan.

Reads: issues.json  (see the export line below — refresh it first, or the script will
                     work from whatever the file last knew)

Usage
-----
    python3 scripts/backfill-issues.py                      # show the plan, write nothing
    python3 scripts/backfill-issues.py --apply assign
    python3 scripts/backfill-issues.py --apply deploy-issue
    python3 scripts/backfill-issues.py --apply all
    python3 scripts/backfill-issues.py --collaborators      # list logins, to fill HANDLES

Assignees
---------
All five logins in HANDLES were confirmed against `gh api repos/.../collaborators` on
25 August 2026. If someone joins or leaves, run --collaborators again: a login that is
not a collaborator cannot be assigned, and --exclude-unknown deals among the rest.

GitHub refuses an assignee who is not a collaborator on the repository, and does so
quietly — the API returns 200 and drops the name. The script re-reads each issue after
editing it and reports anyone who did not stick.
"""

from __future__ import annotations

import argparse
import importlib.util
import json
import random
import sys
import tempfile
from pathlib import Path

HERE = Path(__file__).resolve().parent
REPO_ROOT = HERE.parent
ISSUES_JSON = REPO_ROOT / "issues.json"

# Reuse OWNER/REPO and the Runner rather than restating them.
_spec = importlib.util.spec_from_file_location("seed", HERE / "seed-project-board.py")
seed = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(seed)

OWNER, REPO = seed.OWNER, seed.REPO
SLUG = f"{OWNER}/{REPO}"

EXPORT_CMD = (
    f"gh issue list --repo {SLUG} --limit 200 --state all "
    "--json number,title,state,url,labels,milestone,body,assignees > issues.json"
)

# docs/project/plan.md §3 — the five members. Logins for four of them are established
# by `git log`; the fifth has never committed. Do not guess it.
HANDLES: dict[str, str | None] = {
    "Chenxu You": "ChenxuYou",
    "Wenmin Luo": "onikirinana",
    "Yichen Zhao": "itsEvanZHAO",
    "Dai Lam La La": "ladailam382",
    "Jaswanth Vericherla": "jaswanth-kumar24",   # from --collaborators, 25 Aug 2026
}

DEFAULT_SEED = 5206                     # the unit code, so the deal is at least memorable

# --- the missing S6 issue ---------------------------------------------------------
DEPLOY_TITLE = "Deploy to staging — server, DNS, reverse proxy, CD"
DEPLOY_MILESTONE = "M5 Staging live, client using it"
DEPLOY_LABELS = ["chore"]               # already exists in the repo; nothing new created
DEPLOY_ASSIGNEES = ["ChenxuYou", "onikirinana"]   # plan.md S6: Chenxu You (CD), Wenmin Luo (server)
DEPLOY_BODY = """\
`docs/project/plan.md` §2 gives S6 (week commencing 28 September) two pieces of work,
**US-19** and **deployment**, worth ten points together. US-19 has an issue; this is the
other half, which until now existed only in the plan.

M5 is the milestone the client is told about — *staging live, client using it* — so this
issue is what that sentence actually costs.

## Scope

- [ ] Server provisioned, with the runtime the ADR settles on
      ([`adr-001-technology-stack.md`](https://github.com/ChenxuYou/uwa-cits5206-project/blob/main/docs/decisions/adr-001-technology-stack.md))
- [ ] DNS record pointing at it
- [ ] Reverse proxy in front, TLS terminated, HTTP redirected to HTTPS
- [ ] Database created and migrations applied from a clean state, not copied from a laptop
- [ ] Secrets held by the environment, never in the repository
- [ ] Continuous deployment from `main` once CI is green, and a documented way to roll back
- [ ] Smoke test after deploy: sign in, open a cycle, seal it, download the PDF
- [ ] The staging URL and how to reach it recorded where the client can find it

## Done when

The client can open the staging URL themselves and complete one costing cycle end to end,
and a merge to `main` reaches that URL without anyone running a command by hand.

## Owners

Chenxu You (CD), Wenmin Luo (server, DNS, proxy), verified by Jaswanth Vericherla —
`plan.md` §2, S6.
"""


# --- reading ----------------------------------------------------------------------
def load_issues() -> list[dict]:
    if not ISSUES_JSON.exists():
        raise SystemExit(f"Not found: {ISSUES_JSON}\nExport it first:\n  {EXPORT_CMD}")
    return json.loads(ISSUES_JSON.read_text(encoding="utf-8"))


def labels_of(issue: dict) -> set[str]:
    return {label["name"] for label in issue.get("labels", [])}


def unassigned_must_stories(issues: list[dict]) -> list[dict]:
    """Must-priority story issues with an empty assignee list, oldest first."""
    out = [
        i for i in issues
        if {"user story", "priority: must"} <= labels_of(i)
        and not i.get("assignees")
        and (i.get("state") or "OPEN").upper() != "CLOSED"
    ]
    return sorted(out, key=lambda i: i["number"])


def roster(exclude_unknown: bool) -> list[str]:
    missing = [name for name, login in HANDLES.items() if not login]
    if missing and not exclude_unknown:
        raise SystemExit(
            "No GitHub login recorded for: " + ", ".join(missing) + "\n"
            "Find it with:\n"
            f"  python3 {Path(__file__).name} --collaborators\n"
            "then fill HANDLES in this file. To deal among the others instead, pass "
            "--exclude-unknown."
        )
    if missing:
        print(f"  (dealing without {', '.join(missing)} — no login recorded)")
    return [login for login in HANDLES.values() if login]


# --- the deal ---------------------------------------------------------------------
def deal(issues: list[dict], people: list[str], seed_value: int) -> list[tuple[dict, str]]:
    """One person per issue, random but balanced: counts differ by at most one."""
    rng = random.Random(seed_value)
    bag: list[str] = []
    while len(bag) < len(issues):
        round_ = people[:]          # a full round keeps the shares even
        rng.shuffle(round_)
        bag += round_
    return list(zip(issues, bag[:len(issues)]))


# --- stages -----------------------------------------------------------------------
def stage_assign(r, issues: list[dict], seed_value: int, exclude_unknown: bool) -> None:
    print("\n== assign ==")
    targets = unassigned_must_stories(issues)
    if not targets:
        print("  Every Must story already has an assignee. Nothing to do.")
        print("  (If you expected otherwise, issues.json may be stale — re-export it.)")
        return

    people = roster(exclude_unknown)
    plan = deal(targets, people, seed_value)

    print(f"  seed {seed_value} — re-run with --seed N to deal again\n")
    print(f"  {'#':>5}  {'assignee':<16}  title")
    for issue, login in plan:
        print(f"  {issue['number']:>5}  {login:<16}  {issue['title'][:58]}")
    counts: dict[str, int] = {}
    for _, login in plan:
        counts[login] = counts.get(login, 0) + 1
    print("\n  share: " + ", ".join(f"{k} {v}" for k, v in sorted(counts.items())))
    print()

    for issue, login in plan:
        r.run(["gh", "issue", "edit", str(issue["number"]),
               "--repo", SLUG, "--add-assignee", login])

    if r.apply:
        verify_assignees(r, [(i["number"], login) for i, login in plan])


def verify_assignees(r, expected: list[tuple[int, str]]) -> None:
    """GitHub drops an assignee who is not a collaborator, and does not say so."""
    print("\n  -- checking they stuck --")
    dropped = []
    for number, login in expected:
        got = r.run(["gh", "issue", "view", str(number), "--repo", SLUG,
                     "--json", "assignees"], capture_json=True) or {}
        logins = {a["login"] for a in got.get("assignees", [])}
        if login not in logins:
            dropped.append((number, login))
    if dropped:
        print("  NOT APPLIED — is each of these a collaborator on the repository?")
        for number, login in dropped:
            print(f"    #{number}: {login}")
    else:
        print("  All assignees applied.")


def stage_deploy_issue(r, issues: list[dict]) -> None:
    print("\n== deploy-issue ==")
    existing = next((i for i in issues if i["title"].strip() == DEPLOY_TITLE), None)
    if existing:
        print(f"  #{existing['number']} already carries this title. Nothing to do.")
        return

    print(f"  create: {DEPLOY_TITLE}")
    print(f"  milestone {DEPLOY_MILESTONE} | labels {', '.join(DEPLOY_LABELS)} "
          f"| assignees {', '.join(DEPLOY_ASSIGNEES)}")

    # gh reads the body from a file, so that newlines and backticks survive the shell.
    # A temporary one, outside the repository, so a half-finished run leaves nothing behind.
    def build(path: str) -> list[str]:
        args = ["gh", "issue", "create", "--repo", SLUG,
                "--title", DEPLOY_TITLE,
                "--body-file", path,
                "--milestone", DEPLOY_MILESTONE]
        for label in DEPLOY_LABELS:
            args += ["--label", label]
        for login in DEPLOY_ASSIGNEES:
            args += ["--assignee", login]
        return args

    if not r.apply:
        r.run(build("<temporary file holding the body below>"))
        print("\n" + "\n".join("  | " + line for line in DEPLOY_BODY.splitlines()))
        return

    with tempfile.NamedTemporaryFile("w", suffix=".md", encoding="utf-8",
                                     delete=False) as fh:
        fh.write(DEPLOY_BODY)
        body_path = fh.name
    try:
        url = r.run(build(body_path))
        if url:
            print(f"  {url}")
    finally:
        Path(body_path).unlink(missing_ok=True)


def stage_collaborators(r) -> None:
    print("\n== collaborators ==")
    rows = r.run(["gh", "api", f"repos/{SLUG}/collaborators",
                  "--jq", ".[] | .login"], capture_json=False)
    print(rows or "  (none returned — are you authenticated, and an admin on the repo?)")
    print("\n  Put the missing one into HANDLES in this file.")


# --- entry ------------------------------------------------------------------------
def main() -> None:
    ap = argparse.ArgumentParser(description=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--apply", choices=["assign", "deploy-issue", "all"],
                    help="without this, nothing is written")
    ap.add_argument("--seed", type=int, default=DEFAULT_SEED,
                    help=f"seed for the assignee deal (default {DEFAULT_SEED})")
    ap.add_argument("--exclude-unknown", action="store_true",
                    help="deal among the members whose login is known")
    ap.add_argument("--collaborators", action="store_true",
                    help="list repository collaborators and exit")
    args = ap.parse_args()

    seed.require_gh()
    r = seed.Runner(apply=bool(args.apply))

    if args.collaborators:
        stage_collaborators(seed.Runner(apply=True))
        return

    issues = load_issues()
    print(f"issues.json: {len(issues)} issues")
    if issues and "state" not in issues[0]:
        print("  note: exported without `state`, so closed issues cannot be told apart.\n"
              f"  {EXPORT_CMD}")

    stages = ["assign", "deploy-issue"] if args.apply in (None, "all") else [args.apply]
    for stage in stages:
        if stage == "assign":
            stage_assign(r, issues, args.seed, args.exclude_unknown)
        else:
            stage_deploy_issue(r, issues)

    if not args.apply:
        print("\nDry run. Nothing was written. Re-run with --apply all when the plan reads right.")
    else:
        print("\nDone. Re-export issues.json so the next script sees this:")
        print(f"  {EXPORT_CMD}")


if __name__ == "__main__":
    main()
