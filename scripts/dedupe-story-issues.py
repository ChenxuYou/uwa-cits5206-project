#!/usr/bin/env python3
"""
Repair pass: one issue per story, and it is the one that was there first.

seed-project-board.py created a second set of story issues without checking whether
the team had already opened them. It had. This script resolves the collision the way
the team decided: **the lower issue number wins** — it carries the assignees and the
history — and the duplicate is deleted. The keeper is then brought up to date with
what the newer issue had and it did not: the current title, the acceptance criteria,
the epic and priority labels, and the milestone.

Reads: issues.json  (gh issue list --json number,title,labels,milestone,body,assignees)
       docs/spec/user-stories.md  (through seed-project-board.py's parser)

Usage
-----
    python3 scripts/dedupe-story-issues.py                # show the plan, write nothing
    python3 scripts/dedupe-story-issues.py --apply update # bring the keepers up to date
    python3 scripts/dedupe-story-issues.py --apply delete # delete the duplicates
    python3 scripts/dedupe-story-issues.py --apply all

Run `update` before `delete`, so that nothing is destroyed until the keeper is good.

Afterwards
----------
    python3 scripts/seed-project-board.py --apply populate

The board matches issues by title, and until the duplicates are gone two issues answer
to the same title. Populate after this script, never before.

Deletion is permanent and needs admin rights on the repository. If `gh issue delete`
is refused, use --apply close instead: it closes the duplicate with a comment pointing
at the keeper, which is reversible and loses nothing but tidiness.
"""

from __future__ import annotations

import argparse
import importlib.util
import json
import re
import sys
import tempfile
from pathlib import Path

HERE = Path(__file__).resolve().parent
REPO_ROOT = HERE.parent
ISSUES_JSON = REPO_ROOT / "issues.json"

# Reuse the parser, the Runner and the sprint/milestone mapping rather than restating them.
_spec = importlib.util.spec_from_file_location("seed", HERE / "seed-project-board.py")
seed = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(seed)

STORY_ID_RE = re.compile(r"\b(US-\d+)\b")

# Labels the old issues carry that the current scheme replaces.
SUPERSEDED_LABELS = {"must", "should", "could"}


def story_id(title: str) -> str | None:
    m = STORY_ID_RE.search(title)
    return m.group(1) if m else None


def load_issues() -> list[dict]:
    if not ISSUES_JSON.exists():
        raise SystemExit(
            f"Not found: {ISSUES_JSON}\n"
            "Export it first:\n"
            "  gh issue list --repo ChenxuYou/uwa-cits5206-project --limit 200 "
            "--state all --json number,title,labels,milestone,body,assignees "
            "> issues.json"
        )
    return json.loads(ISSUES_JSON.read_text(encoding="utf-8"))


def build_plan(issues: list[dict], stories: dict[str, object]) -> tuple[list, list, list]:
    """Return (keepers, duplicates, unmatched)."""
    by_story: dict[str, list[dict]] = {}
    for issue in issues:
        sid = story_id(issue["title"])
        if sid and sid in stories:
            by_story.setdefault(sid, []).append(issue)

    keepers, duplicates = [], []
    for sid, group in sorted(by_story.items()):
        group.sort(key=lambda i: i["number"])   # lowest number was there first
        keepers.append((sid, group[0]))
        duplicates.extend((sid, i) for i in group[1:])

    unmatched = sorted(set(stories) - set(by_story))
    return keepers, duplicates, unmatched


def describe(keepers, duplicates, unmatched, stories) -> None:
    print(f"\n{len(keepers)} stories matched to an issue, "
          f"{len(duplicates)} duplicates to remove\n")
    header = f"  {'Story':<8}{'Keep':>6}  {'Assignees':<24}{'Also open':<10}  Retitle to"
    print(header)
    print("  " + "-" * (len(header) - 2))
    dup_of = {}
    for sid, issue in duplicates:
        dup_of.setdefault(sid, []).append(issue["number"])
    for sid, issue in keepers:
        story = stories[sid]
        assignees = ",".join(a["login"] for a in issue["assignees"]) or "—"
        dups = ",".join(f"#{n}" for n in dup_of.get(sid, [])) or "—"
        changed = "" if issue["title"] == story.issue_title else story.issue_title
        print(f"  {sid:<8}{'#' + str(issue['number']):>6}  {assignees:<24}{dups:<10}  "
              f"{changed[:44]}")
    if unmatched:
        print(f"\n  ! no issue found for: {', '.join(unmatched)}")


def stage_update(r, keepers, stories) -> None:
    print("\n== update the keepers ==")
    for sid, issue in keepers:
        story = stories[sid]
        number = str(issue["number"])
        have = {l["name"] for l in issue["labels"]}

        args = ["gh", "issue", "edit", number,
                "--repo", f"{seed.OWNER}/{seed.REPO}",
                "--title", story.issue_title]

        for label in story.labels:
            if label not in have:
                args += ["--add-label", label]
        for label in sorted(have & SUPERSEDED_LABELS):
            args += ["--remove-label", label]

        current_ms = (issue["milestone"] or {}).get("title")
        if story.milestone and current_ms != story.milestone:
            args += ["--milestone", story.milestone]

        # A body this long goes through a file: Windows caps a command line at 8191
        # characters and several of these bodies are over two thousand on their own.
        if r.apply:
            tmp = Path(tempfile.mkdtemp()) / f"{sid}.md"
            tmp.write_text(story.body(), encoding="utf-8")
            args += ["--body-file", str(tmp)]
        else:
            args += ["--body-file", f"<{sid}.md, {len(story.body())} chars>"]

        r.run(args)


def stage_remove(r, duplicates, keepers, mode: str) -> None:
    print(f"\n== {mode} the duplicates ==")
    keep_number = {sid: issue["number"] for sid, issue in keepers}
    for sid, issue in duplicates:
        number = str(issue["number"])
        if mode == "delete":
            r.run(["gh", "issue", "delete", number,
                   "--repo", f"{seed.OWNER}/{seed.REPO}", "--yes"])
        else:
            r.run(["gh", "issue", "comment", number,
                   "--repo", f"{seed.OWNER}/{seed.REPO}",
                   "--body", f"Duplicate of #{keep_number[sid]}, which was opened first "
                             f"and carries the assignees. Closing this one; "
                             f"#{keep_number[sid]} is the live {sid}."])
            r.run(["gh", "issue", "close", number,
                   "--repo", f"{seed.OWNER}/{seed.REPO}",
                   "--reason", "not planned"])


def main() -> None:
    for stream in (sys.stdout, sys.stderr):
        try:
            stream.reconfigure(encoding="utf-8", errors="replace")
        except (AttributeError, ValueError):
            pass

    ap = argparse.ArgumentParser(description=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--apply", choices=["update", "delete", "close", "all"],
                    help="actually write to GitHub; omit for a dry run")
    args = ap.parse_args()

    stories = {s.id: s for s in seed.parse_stories(seed.STORIES_MD)}
    issues = load_issues()
    keepers, duplicates, unmatched = build_plan(issues, stories)
    describe(keepers, duplicates, unmatched, stories)

    if not duplicates:
        print("\nNothing duplicated. Either this has already run, or issues.json is stale.")

    r = seed.Runner(apply=bool(args.apply))
    if args.apply:
        seed.require_gh()

    do = args.apply
    if do in ("update", "all", None):
        stage_update(r, keepers, stories)
    if do in ("delete", "all"):
        stage_remove(r, duplicates, keepers, "delete")
    elif do == "close":
        stage_remove(r, duplicates, keepers, "close")
    elif do is None:
        stage_remove(r, duplicates, keepers, "delete")

    if not args.apply:
        print("\nDry run — nothing was written.")
    print("\nWhen this is done, and only then:")
    print("  python3 scripts/seed-project-board.py --apply populate")


if __name__ == "__main__":
    main()
