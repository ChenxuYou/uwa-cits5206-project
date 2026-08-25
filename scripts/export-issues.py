#!/usr/bin/env python3
"""
Refresh `issues.json` from GitHub.

The equivalent one-liner is

    gh issue list --repo OWNER/REPO --limit 200 --state all \
        --json number,title,state,url,labels,milestone,body,assignees > issues.json

but a shell redirect writes whatever encoding the shell feels like — on Windows
PowerShell that is UTF-16 with a BOM, which every reader in `scripts/` then has to
work around. This script parses what `gh` returns and writes it back itself: UTF-8,
LF endings, sorted newest issue first, so the only thing that ever moves in the diff
is an issue that actually changed.

It also says what changed, which the redirect cannot.

Usage
-----
    python3 scripts/export-issues.py                 # refresh and report
    python3 scripts/export-issues.py --dry-run       # report, write nothing
    python3 scripts/export-issues.py --pretty        # indented, larger diffs
    python3 scripts/export-issues.py --open-only     # skip closed issues
"""

from __future__ import annotations

import argparse
import json
import shutil
import subprocess
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
ISSUES_JSON = REPO_ROOT / "issues.json"

OWNER = "ChenxuYou"
REPO = "uwa-cits5206-project"
SLUG = f"{OWNER}/{REPO}"

# The field set the rest of `scripts/` expects. Deliberately no `updatedAt` — it
# changes on every touch and would put noise in the diff of every export.
FIELDS = "number,title,state,url,labels,milestone,body,assignees"
LIMIT = 200


def require_gh() -> None:
    if not shutil.which("gh"):
        raise SystemExit(
            "gh is not on PATH. Install the GitHub CLI: https://cli.github.com"
        )
    probe = subprocess.run(["gh", "auth", "status"], capture_output=True, text=True)
    if probe.returncode != 0:
        raise SystemExit("gh is not authenticated. Run:  gh auth login")


def fetch(open_only: bool) -> list[dict]:
    args = [
        "gh", "issue", "list",
        "--repo", SLUG,
        "--limit", str(LIMIT),
        "--state", "open" if open_only else "all",
        "--json", FIELDS,
    ]
    print("  " + " ".join(args))
    # text=False, then decode UTF-8 ourselves: gh always emits UTF-8, whatever the
    # console code page claims, and the § and — in the issue bodies do not survive
    # cp1252.
    done = subprocess.run(args, capture_output=True)
    if done.returncode != 0:
        raise SystemExit("gh failed:\n" + done.stderr.decode("utf-8", "replace"))
    issues = json.loads(done.stdout.decode("utf-8"))
    if len(issues) >= LIMIT:
        print(f"  warning: hit the --limit of {LIMIT}; raise LIMIT in this file.")
    return sorted(issues, key=lambda i: i["number"], reverse=True)


def load_previous() -> list[dict]:
    if not ISSUES_JSON.exists():
        return []
    raw = ISSUES_JSON.read_bytes()
    for encoding in ("utf-8-sig", "utf-16", "utf-8"):
        try:
            return json.loads(raw.decode(encoding))
        except (UnicodeDecodeError, json.JSONDecodeError):
            continue
    print("  (could not read the existing issues.json — treating it as empty)")
    return []


def summarise(before: list[dict], after: list[dict]) -> None:
    old = {i["number"]: i for i in before}
    new = {i["number"]: i for i in after}

    added = sorted(set(new) - set(old), reverse=True)
    removed = sorted(set(old) - set(new), reverse=True)
    changed = sorted(
        (n for n in set(old) & set(new) if old[n] != new[n]), reverse=True
    )

    if not (added or removed or changed):
        print("\n  No change — issues.json already matches GitHub.")
        return

    def title(n: int) -> str:
        return new.get(n, old.get(n, {})).get("title", "")[:60]

    for label, numbers in (("new", added), ("gone", removed), ("changed", changed)):
        for n in numbers:
            print(f"  {label:>8}  #{n:<4} {title(n)}")
            if label == "changed":
                for key in sorted(set(old[n]) | set(new[n])):
                    if old[n].get(key) != new[n].get(key):
                        print(f"           └─ {key}")

    print(
        f"\n  {len(added)} new, {len(removed)} gone, {len(changed)} changed"
        f"  ({len(before)} → {len(after)} issues)"
    )


def write(issues: list[dict], pretty: bool) -> None:
    text = json.dumps(
        issues,
        ensure_ascii=False,
        indent=2 if pretty else None,
        separators=None if pretty else (",", ":"),
    )
    # newline="\n" so the file does not pick up CRLF on Windows and rewrite itself
    # wholesale in the next diff.
    with ISSUES_JSON.open("w", encoding="utf-8", newline="\n") as fh:
        fh.write(text + "\n")


def main() -> None:
    ap = argparse.ArgumentParser(
        description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter
    )
    ap.add_argument("--dry-run", action="store_true", help="report, write nothing")
    ap.add_argument("--pretty", action="store_true", help="indent the JSON")
    ap.add_argument("--open-only", action="store_true", help="skip closed issues")
    args = ap.parse_args()

    require_gh()
    print(f"Exporting {SLUG} issues")

    before = load_previous()
    after = fetch(args.open_only)
    summarise(before, after)

    if args.dry_run:
        print("\nDry run. issues.json untouched.")
        return

    write(after, args.pretty)
    print(f"\nWrote {len(after)} issues to {ISSUES_JSON.relative_to(REPO_ROOT)}")


if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        sys.exit(130)
