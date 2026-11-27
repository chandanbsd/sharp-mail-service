#!/usr/bin/env python3
"""Set up a repository for the baseline.

Usage: init.py <target> [--skip FAMILY,FAMILY] [--dry-run]
Writes the baseline/ folder (baseline.md, baseline.yml, adr/ and standards/ with their templates), appends a short
pointer to AGENTS.md and creates CLAUDE.md when it is absent. Outside baseline/ it writes nothing else.
Prints one line per action: create <path>, append AGENTS.md or keep <path>, then any note: line.
Never overwrites a file. Exits 2 when the target is not a directory, already holds baseline/baseline.yml, a family
name is unknown or AGENTS.md is not UTF-8.
"""
from __future__ import annotations

import argparse
import sys
from pathlib import Path
from typing import List, Optional

import kit

NEW_AGENTS_MD = "# Instructions for working in this repository\n"


def main(argv: Optional[List[str]] = None) -> int:
    parser = argparse.ArgumentParser(description="Set up a repository for the baseline.")
    parser.add_argument("target", type=Path, help="the repository to set up")
    parser.add_argument(
        "--skip", default="", help="document families that do not apply, comma separated: %s" % ", ".join(kit.FAMILIES)
    )
    parser.add_argument("--dry-run", action="store_true", help="print what would happen and write nothing")
    args = parser.parse_args(argv)
    target = args.target
    if not target.is_dir():
        print("error: %s is not a directory" % target, file=sys.stderr)
        return 2
    try:
        skipped = kit.parse_skip(args.skip)
    except ValueError as error:
        print("error: %s" % error, file=sys.stderr)
        return 2
    if (target / "baseline" / "baseline.yml").exists():
        print(
            "error: %s is already initialised (baseline/baseline.yml exists); use the update mode" % target,
            file=sys.stderr,
        )
        return 2

    if (target / "baseline").exists() and not (target / "baseline").is_dir():
        print("error: %s exists and is not a folder; move it aside and run again" % (target / "baseline"), file=sys.stderr)
        return 2

    version = kit.installed_version()
    writes = []  # (relative path, content)

    agents = target / "AGENTS.md"
    if agents.exists():
        try:
            current = kit.read_text(agents)
        except UnicodeDecodeError:
            print("error: AGENTS.md is not UTF-8 text; fix it by hand and run again", file=sys.stderr)
            return 2
        verb = "keep" if kit.has_pointer(current) else "append"
    else:
        current, verb = NEW_AGENTS_MD, "create"
    print("%s AGENTS.md" % verb)
    if verb != "keep":
        writes.append(("AGENTS.md", kit.with_pointer(current)))

    for rel, content in kit.kit_files(version, skipped).items():
        if (target / rel).exists():
            print("keep %s" % rel)
        else:
            print("create %s" % rel)
            writes.append((rel, content))

    claude = target / "CLAUDE.md"
    if claude.exists() and b"@AGENTS.md" not in claude.read_bytes():
        print(
            "note: CLAUDE.md does not import AGENTS.md, so Claude Code will not see the baseline pointer; "
            "add the line @AGENTS.md to it"
        )

    if skipped:
        print("skipped families: %s" % ", ".join(skipped))
    if args.dry_run:
        print("dry run: nothing written")
        return 0
    for rel, content in writes:
        dest = target / rel
        dest.parent.mkdir(parents=True, exist_ok=True)
        kit.write_text(dest, content)
    return 0


if __name__ == "__main__":
    sys.exit(main())
