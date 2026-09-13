#!/usr/bin/env python3
"""
Finish the GitHub Projects board, and audit what is left.

`seed-project-board.py` built the board from `docs/spec/user-stories.md`. Three things
it could not do are recorded in `docs/project/plan.md` §6 as "finish the board by hand",
and that row has been carried since 5 September. This script closes the half of it that
can be closed safely, and then *tells you* about the other half instead of pretending.

    python3 scripts/finish-project-board.py                # audit only, writes nothing
    python3 scripts/finish-project-board.py --apply        # add the missing issues, then audit

WHAT IT DOES
    Adds the three issues that are not user stories, and so are not in user-stories.md,
    to the board: #10 (the requirement ID for the PDF workings), #21 (CI path filter) and
    #60 (deploy to staging). A card that is not on the board is work nobody can see.

WHAT IT DELIBERATELY DOES NOT DO, AND WHY
    Rename the Status option `Todo` to `Backlog`, or add `Review`.

    The only API for this is the `updateProjectV2Field` mutation, and its
    `singleSelectOptions` input **replaces the entire option list**. Options are
    identified by name, not by id, so a "rename" is a delete followed by a create — and
    every card whose Status pointed at the deleted option is left with no Status at all.
    GitHub's own community threads carry the reports. It is a two-minute job in the web
    UI that cannot lose data, so it stays a two-minute job in the web UI. This paragraph
    is the reason, so that nobody spends an afternoon rediscovering it.

    Board *views* and *workflows* have no public API either. Both are printed as steps.

PREREQUISITES
    gh auth login
    gh auth refresh -s project,read:project    # the default token cannot touch Projects v2
"""

from __future__ import annotations

import argparse
import json
import shlex
import subprocess
import sys

OWNER = "ChenxuYou"
REPO = "uwa-cits5206-project"

# Must match the board's title on GitHub exactly — the same trap seed-project-board.py
# documents: a mismatch here finds nothing rather than failing loudly.
PROJECT_TITLE = "uwa-cits5206-project-board"

# plan.md §6 — the issues that are not stories. Number, and why it belongs on the board.
NON_STORY_ISSUES = {
    10: "A17 — the requirement ID for 'the sealed PDF shows the workings'. Gates a Must story's estimate.",
    21: "CI runs on documentation-only changes. Closed by the paths-ignore filter in ci.yml.",
    60: "Deploy to staging — server, DNS, reverse proxy, CD. This is what M5 actually costs.",
}

# The four columns §4 and the submitted PDF promise.
EXPECTED_STATUS_OPTIONS = ["Backlog", "In Progress", "Review", "Done"]


class Runner:
    def __init__(self, apply: bool):
        self.apply = apply

    def run(self, args: list[str], capture_json: bool = False, writes: bool = False):
        printable = " ".join(shlex.quote(a) for a in args)
        if writes and not self.apply:
            print(f"  would run: {printable}")
            return None
        # encoding is explicit for the same reason as in seed-project-board.py: text=True
        # decodes with the locale codec, which on a Chinese Windows is GBK, and gh emits
        # UTF-8.
        proc = subprocess.run(args, capture_output=True, text=True,
                              encoding="utf-8", errors="replace")
        out = (proc.stdout or "").strip()
        if proc.returncode != 0:
            err = (proc.stderr or "").strip()
            print(f"    FAILED: {err}", file=sys.stderr)
            raise SystemExit(1)
        if capture_json and out:
            return json.loads(out)
        return out


def find_project(r: Runner) -> dict:
    listing = r.run(["gh", "project", "list", "--owner", OWNER, "--format", "json"],
                    capture_json=True) or {}
    for project in listing.get("projects", []):
        if project.get("title") == PROJECT_TITLE:
            return project
    raise SystemExit(
        f"No board titled {PROJECT_TITLE!r} under {OWNER}. Run seed-project-board.py first, "
        "or correct PROJECT_TITLE — this script will not create a second, empty board."
    )


def add_missing_issues(r: Runner, number: str) -> None:
    print("\n== issues that are not stories ==")

    items = r.run(["gh", "project", "item-list", number, "--owner", OWNER,
                   "--limit", "200", "--format", "json"], capture_json=True) or {}
    on_board = {item.get("content", {}).get("number") for item in items.get("items", [])}

    for issue, why in NON_STORY_ISSUES.items():
        if issue in on_board:
            print(f"  #{issue} already on the board")
            continue
        print(f"  #{issue} missing — {why}")
        r.run(["gh", "project", "item-add", number, "--owner", OWNER,
               "--url", f"https://github.com/{OWNER}/{REPO}/issues/{issue}"], writes=True)


def audit(r: Runner, number: str) -> list[str]:
    """What plan.md §6 promises, checked against what the board actually has."""
    outstanding: list[str] = []

    fields = r.run(["gh", "project", "field-list", number, "--owner", OWNER,
                    "--format", "json"], capture_json=True) or {}

    status = next((f for f in fields.get("fields", []) if f.get("name") == "Status"), None)
    options = [o.get("name") for o in (status or {}).get("options", [])]

    print("\n== audit ==")
    print(f"  Status options: {', '.join(options) if options else '(none found)'}")

    for expected in EXPECTED_STATUS_OPTIONS:
        if expected not in options:
            outstanding.append(f"Status option {expected!r} does not exist")

    items = r.run(["gh", "project", "item-list", number, "--owner", OWNER,
                   "--limit", "200", "--format", "json"], capture_json=True) or {}
    on_board = {item.get("content", {}).get("number") for item in items.get("items", [])}
    for issue in NON_STORY_ISSUES:
        if issue not in on_board:
            outstanding.append(f"issue #{issue} is not on the board")

    print(f"  Cards on the board: {len(items.get('items', []))}")
    return outstanding


def manual_steps(outstanding: list[str]) -> None:
    if not outstanding:
        print("\n  Nothing outstanding. plan.md §6's board row can be closed.")
        return

    print("\n== still outstanding ==")
    for item in outstanding:
        print(f"  - {item}")

    print("""
== by hand, in the web UI, about two minutes ==

1. Status options — Project → Settings → Status.
   Rename "Todo" to "Backlog". Add "Review" between "In Progress" and "Done".
   Do this in the UI: the API's option editor replaces the whole list and clears
   every card's Status. See the note at the top of this file.

2. A view grouped by Status — new Board view, group by Status, save as "Sprint board".
   §4 and the submitted Assignment 1 PDF both promise these four columns; until the
   view exists, the promise is only in the documents.

3. Workflows — Settings → Workflows, enable three:
     Item added to project  → set Status to Backlog
     Item closed            → set Status to Done
     Pull request merged    → set Status to Done
   These are what stop the board going stale between Saturday meetings, which is the
   only way a board ever becomes wrong.

Then re-run this script; it should print "Nothing outstanding".
""")


def main() -> None:
    for stream in (sys.stdout, sys.stderr):
        try:
            stream.reconfigure(encoding="utf-8", errors="replace")
        except (AttributeError, ValueError):
            pass

    parser = argparse.ArgumentParser(
        description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--apply", action="store_true",
                        help="add the missing issues to the board; omit for an audit that writes nothing")
    args = parser.parse_args()

    runner = Runner(apply=args.apply)
    project = find_project(runner)
    number = str(project["number"])
    print(f"Board: {PROJECT_TITLE} (#{number})")

    add_missing_issues(runner, number)
    manual_steps(audit(runner, number))


if __name__ == "__main__":
    main()
