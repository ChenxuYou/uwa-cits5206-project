#!/usr/bin/env python3
"""
Seed the GitHub Projects board from docs/spec/user-stories.md.

`docs/spec/user-stories.md` is the source of truth for stories, points and priority;
`docs/project/plan.md` is the source of truth for milestones and sprints. This script
reads the first and carries the second in SPRINT_OF below, so the board is created
*from* the plan of record rather than alongside it.

Usage
-----
    python3 scripts/seed-project-board.py                 # plan only, writes nothing
    python3 scripts/seed-project-board.py --apply all     # do everything
    python3 scripts/seed-project-board.py --apply issues  # one stage at a time

Stages, in order: milestones, labels, issues, project, populate.

Prerequisites
-------------
    gh auth login
    gh auth refresh -s project,read:project   # the default token cannot touch Projects v2

What this script cannot do, and you must do by hand afterwards (§ printed at the end):
  * create the Sprint **iteration** field — `gh project field-create` supports only
    TEXT, NUMBER, DATE and SINGLE_SELECT
  * rename the built-in Status options to Backlog / In Progress / Review / Done
Both take about two minutes in the web UI.
"""

from __future__ import annotations

import argparse
import json
import re
import shlex
import subprocess
import sys
from pathlib import Path

# --------------------------------------------------------------------------------------
# Configuration — change these, not the code below
# --------------------------------------------------------------------------------------

OWNER = "ChenxuYou"
REPO = "uwa-cits5206-project"
PROJECT_TITLE = "RIC Costing Tool — MVP delivery"

REPO_ROOT = Path(__file__).resolve().parent.parent
STORIES_MD = REPO_ROOT / "docs" / "spec" / "user-stories.md"
STORIES_URL = (
    f"https://github.com/{OWNER}/{REPO}/blob/main/docs/spec/user-stories.md#4-stories"
)

# docs/project/plan.md §1 — milestones. (title, due date, description)
MILESTONES = [
    ("M1 Engine provably correct", "2026-09-04",
     "The client's worked example reproduces to the cent, as a CI merge gate."),
    ("M2 Guided flow, validated server-side", "2026-09-11",
     "Costs, income, capacity and mandatory forecast utilisation captured and validated."),
    ("M3 Rates, proposed rates and balance", "2026-09-18",
     "Three rates per capability with the figures behind each."),
    ("M4 Vertical slice complete", "2026-09-25",
     "Sign in to sealed record to export to reopen."),
    ("M5 Staging live, client using it", "2026-10-02",
     "Deployed over HTTPS, seeded credentials replaced."),
    ("M6 Release candidate, feature freeze", "2026-10-09",
     "Critical fixes only; full regression pass."),
    ("M7 Final release and handover", "2026-10-13",
     "Tagged release deployed, handover notes written."),
]

# docs/project/plan.md §2 — which sprint finishes each story.
# US-09 and US-18 are worked in two sprints (engine first, screen later); each is
# recorded against the sprint in which it is *done*, which is what the board tracks.
SPRINT_OF = {
    "US-18": "S2", "US-09": "S4",
    "US-03": "S3", "US-04": "S3", "US-06": "S3", "US-07": "S3", "US-08": "S3",
    "US-10": "S4", "US-11": "S4", "US-12": "S4", "US-13": "S4",
    "US-14": "S5", "US-15": "S5", "US-16": "S5", "US-17": "S5",
    "US-01": "S5", "US-02": "S5",
    "US-19": "S6",
}

SPRINT_MILESTONE = {
    "S2": "M1 Engine provably correct",
    "S3": "M2 Guided flow, validated server-side",
    "S4": "M3 Rates, proposed rates and balance",
    "S5": "M4 Vertical slice complete",
    "S6": "M5 Staging live, client using it",
}

SPRINT_STARTS = {  # for the iteration field you create by hand
    "S1": "2026-08-24", "S2": "2026-08-31", "S3": "2026-09-07", "S4": "2026-09-14",
    "S5": "2026-09-21", "S6": "2026-09-28", "S7": "2026-10-05", "S8": "2026-10-12",
}

LABELS = [
    ("priority: must", "b60205", "MVP. Exactly and only the eighteen Must stories"),
    ("priority: should", "fbca04", "Stretch, in the order given in user-stories.md §5"),
    ("priority: could", "0e8a16", "Stretch, last"),
    ("user story", "1d76db", "A story from docs/spec/user-stories.md §4"),
]

EPIC_COLOR = "5319e7"

# --------------------------------------------------------------------------------------
# Parsing
# --------------------------------------------------------------------------------------

STORY_RE = re.compile(
    r"^\*\*(?P<id>US-\d+) · (?P<title>.+?)\*\* — "
    r"(?P<priority>Must|Should|Could) · (?P<points>\d+) pts · (?P<reqs>.+?)\s*$"
)
EPIC_RE = re.compile(r"^### (?P<epic>E\d+) — (?P<name>.+?)\s*$")


class Story:
    def __init__(self, sid, title, priority, points, reqs, epic, epic_name):
        self.id = sid
        self.title = title
        self.priority = priority
        self.points = points
        self.reqs = reqs
        self.epic = epic
        self.epic_name = epic_name
        self.narrative: list[str] = []
        self.criteria: list[str] = []

    @property
    def issue_title(self) -> str:
        return f"{self.id} · {self.title}"

    @property
    def sprint(self) -> str | None:
        return SPRINT_OF.get(self.id)

    @property
    def milestone(self) -> str | None:
        return SPRINT_MILESTONE.get(self.sprint) if self.sprint else None

    @property
    def labels(self) -> list[str]:
        return [
            "user story",
            f"priority: {self.priority.lower()}",
            f"epic: {self.epic}",
        ]

    def body(self) -> str:
        lines = list(self.narrative)
        lines += ["", "**Acceptance criteria**", ""]
        lines += [f"- [ ] {c}" for c in self.criteria]
        lines += [
            "",
            "---",
            "",
            f"**{self.priority} · {self.points} pts · {self.reqs} · "
            f"epic {self.epic} {self.epic_name}**",
            "",
            f"Source of truth: [`docs/spec/user-stories.md` §4]({STORIES_URL}) — **the "
            "document governs.** If a criterion here and a criterion there disagree, the "
            "document is right and this issue is stale. Change the document first, in the "
            "same commit.",
        ]
        return "\n".join(lines)


def parse_stories(path: Path) -> list[Story]:
    stories: list[Story] = []
    epic, epic_name = "", ""
    current: Story | None = None
    section = None  # None | "narrative" | "criteria"

    for raw in path.read_text(encoding="utf-8").splitlines():
        line = raw.rstrip()

        m = EPIC_RE.match(line)
        if m:
            epic, epic_name = m.group("epic"), m.group("name")
            current, section = None, None
            continue

        m = STORY_RE.match(line)
        if m:
            current = Story(
                m.group("id"), m.group("title"), m.group("priority"),
                int(m.group("points")), m.group("reqs"), epic, epic_name,
            )
            stories.append(current)
            section = "narrative"
            continue

        if current is None:
            continue

        if line.strip() == "---":
            current, section = None, None
            continue
        if line.startswith("**Acceptance criteria**"):
            section = "criteria"
            continue

        if section == "narrative" and line.startswith(">"):
            current.narrative.append(line)
        elif section == "criteria" and line.startswith("- "):
            current.criteria.append(line[2:].strip())
        elif section == "criteria" and line.startswith("  ") and current.criteria:
            current.criteria[-1] += " " + line.strip()

    return stories


# --------------------------------------------------------------------------------------
# gh plumbing
# --------------------------------------------------------------------------------------

class Runner:
    def __init__(self, apply: bool):
        self.apply = apply

    def run(self, args: list[str], capture_json: bool = False):
        printable = " ".join(shlex.quote(a) for a in args)
        if not self.apply:
            print(f"  would run: {printable}")
            return None
        print(f"  $ {printable}")
        proc = subprocess.run(args, capture_output=True, text=True)
        if proc.returncode != 0:
            err = (proc.stderr or "").strip()
            # Creating something that already exists is not a failure worth stopping for.
            if "already exists" in err.lower():
                print(f"    (exists already, skipped)")
                return None
            print(f"    FAILED: {err}", file=sys.stderr)
            raise SystemExit(1)
        if capture_json and proc.stdout.strip():
            return json.loads(proc.stdout)
        return proc.stdout.strip()


def require_gh() -> None:
    if subprocess.run(["which", "gh"], capture_output=True).returncode != 0:
        raise SystemExit("gh is not installed. See https://cli.github.com")


# --------------------------------------------------------------------------------------
# Stages
# --------------------------------------------------------------------------------------

def stage_milestones(r: Runner) -> None:
    print("\n== milestones ==")
    for title, due, desc in MILESTONES:
        r.run([
            "gh", "api", f"repos/{OWNER}/{REPO}/milestones",
            "-f", f"title={title}",
            "-f", f"description={desc}",
            "-f", f"due_on={due}T23:59:59Z",
        ])


def stage_labels(r: Runner, stories: list[Story]) -> None:
    print("\n== labels ==")
    epics = {}
    for s in stories:
        epics.setdefault(s.epic, s.epic_name)
    labels = list(LABELS) + [
        (f"epic: {e}", EPIC_COLOR, name) for e, name in sorted(epics.items())
    ]
    for name, color, desc in labels:
        r.run([
            "gh", "label", "create", name,
            "--repo", f"{OWNER}/{REPO}",
            "--color", color, "--description", desc, "--force",
        ])


def stage_issues(r: Runner, stories: list[Story]) -> None:
    print("\n== issues ==")
    for s in stories:
        args = [
            "gh", "issue", "create",
            "--repo", f"{OWNER}/{REPO}",
            "--title", s.issue_title,
            "--body", s.body(),
        ]
        for label in s.labels:
            args += ["--label", label]
        if s.milestone:
            args += ["--milestone", s.milestone]
        r.run(args)


def stage_project(r: Runner) -> dict | None:
    print("\n== project ==")
    created = r.run([
        "gh", "project", "create",
        "--owner", OWNER, "--title", PROJECT_TITLE, "--format", "json",
    ], capture_json=True)

    number = str(created["number"]) if created else "<number>"

    # Public, so the facilitator can open the link that is in the submitted PDF.
    r.run(["gh", "project", "edit", number, "--owner", OWNER, "--visibility", "PUBLIC"])
    # Linked, so it appears on the repository's Projects tab.
    r.run(["gh", "project", "link", number, "--owner", OWNER,
           "--repo", f"{OWNER}/{REPO}"])

    fields = [
        ("Story", "TEXT", None),
        ("Points", "NUMBER", None),
        ("Requirement", "TEXT", None),
        ("Priority", "SINGLE_SELECT", "Must,Should,Could"),
        ("Sprint (text)", "SINGLE_SELECT", ",".join(SPRINT_STARTS)),
    ]
    for name, dtype, options in fields:
        args = ["gh", "project", "field-create", number, "--owner", OWNER,
                "--name", name, "--data-type", dtype]
        if options:
            args += ["--single-select-options", options]
        r.run(args)

    return created


def stage_populate(r: Runner, stories: list[Story]) -> None:
    """Add every issue to the project and set its field values.

    Run this after `issues` and `project`. It reads the live ids back from gh rather
    than assuming anything about them.
    """
    print("\n== populate ==")
    if not r.apply:
        print("  would look up project/field/item ids, then for each story run:")
        print("    gh project item-add   <n> --owner ... --url <issue url>")
        print("    gh project item-edit  --id <item> --project-id <p> "
              "--field-id <f> --number <points>")
        print(f"  ({len(stories)} stories)")
        return

    projects = r.run(["gh", "project", "list", "--owner", OWNER, "--format", "json"],
                     capture_json=True)
    match = [p for p in projects["projects"] if p["title"] == PROJECT_TITLE]
    if not match:
        raise SystemExit(f"No project titled {PROJECT_TITLE!r}. Run --apply project first.")
    project = match[0]
    number, project_id = str(project["number"]), project["id"]

    field_data = r.run(["gh", "project", "field-list", number, "--owner", OWNER,
                        "--format", "json"], capture_json=True)
    fields = {f["name"]: f for f in field_data["fields"]}

    def option_id(field_name: str, option_name: str) -> str | None:
        for opt in fields.get(field_name, {}).get("options", []):
            if opt["name"] == option_name:
                return opt["id"]
        return None

    issue_data = r.run(["gh", "issue", "list", "--repo", f"{OWNER}/{REPO}",
                        "--limit", "200", "--state", "all",
                        "--json", "number,title,url"], capture_json=True)
    by_title = {i["title"]: i for i in issue_data}

    for s in stories:
        issue = by_title.get(s.issue_title)
        if not issue:
            print(f"  ! no issue for {s.id}, skipped")
            continue

        item = r.run(["gh", "project", "item-add", number, "--owner", OWNER,
                      "--url", issue["url"], "--format", "json"], capture_json=True)
        if not item:
            continue
        item_id = item["id"]

        def edit(field_name: str, flag: str, value: str) -> None:
            field = fields.get(field_name)
            if not field:
                return
            r.run(["gh", "project", "item-edit", "--id", item_id,
                   "--project-id", project_id, "--field-id", field["id"], flag, value])

        edit("Story", "--text", s.id)
        edit("Points", "--number", str(s.points))
        edit("Requirement", "--text", s.reqs)
        opt = option_id("Priority", s.priority)
        if opt:
            edit("Priority", "--single-select-option-id", opt)
        if s.sprint:
            opt = option_id("Sprint (text)", s.sprint)
            if opt:
                edit("Sprint (text)", "--single-select-option-id", opt)


# --------------------------------------------------------------------------------------

def summarise(stories: list[Story]) -> None:
    must = [s for s in stories if s.priority == "Must"]
    print(f"\nParsed {len(stories)} stories from {STORIES_MD.relative_to(REPO_ROOT)}")
    print(f"  Must: {len(must)} stories, {sum(s.points for s in must)} points")
    print(f"  Stretch: {len(stories) - len(must)} stories, "
          f"{sum(s.points for s in stories if s.priority != 'Must')} points")
    if len(must) != 18 or sum(s.points for s in must) != 110:
        print("  ! user-stories.md §5 says eighteen Must stories and 110 points. "
              "It no longer does. Check which one moved before seeding a board from it.")
    print()
    header = f"  {'Story':<8}{'Pri':<8}{'Pts':>4}  {'Sprint':<8}{'AC':>3}  Title"
    print(header)
    print("  " + "-" * (len(header) - 2))
    for s in stories:
        print(f"  {s.id:<8}{s.priority:<8}{s.points:>4}  "
              f"{(s.sprint or '—'):<8}{len(s.criteria):>3}  {s.title[:52]}")


def manual_steps() -> None:
    print("""
== two things to finish by hand, in the web UI ==

1. Sprint as a real iteration field.
   The CLI cannot create iteration fields. Project → Settings → + New field →
   Iteration, 1 week, starting Saturday 24 Aug 2026, 8 iterations. Then delete the
   "Sprint (text)" select this script made and re-set the values on the board — or
   keep the select, which sorts fine but gives you no burndown.

2. Status options.
   Projects v2 ships Todo / In Progress / Done. The submission promises
   Backlog · In Progress · Review · Done. Rename Todo to Backlog and add Review.

Then: Settings → Workflows → enable "Item added to project" → Backlog,
"Item closed" → Done, "Pull request merged" → Done. Two views: a board by Status,
and a table grouped by Sprint with Points summed.
""")


def main() -> None:
    ap = argparse.ArgumentParser(description=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--apply", choices=["milestones", "labels", "issues", "project",
                                        "populate", "all"],
                    help="actually write to GitHub; omit for a dry run")
    args = ap.parse_args()

    if not STORIES_MD.exists():
        raise SystemExit(f"Not found: {STORIES_MD}")

    stories = parse_stories(STORIES_MD)
    if not stories:
        raise SystemExit("Parsed no stories — has the format of §4 changed?")

    summarise(stories)

    stages = ["milestones", "labels", "issues", "project", "populate"]
    todo = stages if args.apply in (None, "all") else [args.apply]
    r = Runner(apply=bool(args.apply))
    if args.apply:
        require_gh()

    for stage in todo:
        if stage == "milestones":
            stage_milestones(r)
        elif stage == "labels":
            stage_labels(r, stories)
        elif stage == "issues":
            stage_issues(r, stories)
        elif stage == "project":
            stage_project(r)
        elif stage == "populate":
            stage_populate(r, stories)

    if not args.apply:
        print("\nDry run — nothing was written. Re-run with --apply all to do it.")
    manual_steps()


if __name__ == "__main__":
    main()
