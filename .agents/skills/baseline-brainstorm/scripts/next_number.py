#!/usr/bin/env python3
"""Print the number for the next desired-state file: the highest in use plus 1, as five digits.

Usage: next_number.py <repo>
Looks in <repo>/baseline/desired-state/ for names that start with five digits and a dash, files or folders alike,
and prints 00001 when there are none. Writes nothing. Exits 2 when <repo> is not a directory.
"""
from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path
from typing import List, Optional

NUMBERED_RE = re.compile(r"^(\d{5})-")


def next_number(repo: Path) -> str:
    folder = repo / "baseline" / "desired-state"
    used = []
    if folder.is_dir():
        for entry in folder.iterdir():
            match = NUMBERED_RE.match(entry.name)
            if match:
                used.append(int(match.group(1)))
    return "%05d" % (max(used) + 1 if used else 1)


def main(argv: Optional[List[str]] = None) -> int:
    parser = argparse.ArgumentParser(description="Print the number for the next desired-state file.")
    parser.add_argument("repo", type=Path, help="the repository")
    args = parser.parse_args(argv)
    if not args.repo.is_dir():
        print("error: %s is not a directory" % args.repo, file=sys.stderr)
        return 2
    print(next_number(args.repo))
    return 0


if __name__ == "__main__":
    sys.exit(main())
