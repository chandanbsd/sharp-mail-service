#!/usr/bin/env python3
"""List the covers: line of every document under baseline/docs, so a run can pick the documents a diff affects.

Usage: covers.py <repo>
Prints one line per Markdown file under <repo>/baseline/docs/, in path order: <path><TAB><what it covers>.
A file with no covers: line in its first ten lines prints "(no covers line)", an unreadable one "(unreadable)".
Writes nothing. Exits 2 when <repo> is not a directory.
"""
from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path
from typing import List, Optional

COVERS_RE = re.compile(r"^covers:[ \t]*(.*?)[ \t]*$")
MISSING = "(no covers line)"
UNREADABLE = "(unreadable)"


def covers_of(path: Path) -> str:
    try:
        with open(path, encoding="utf-8", newline="") as f:
            for _, line in zip(range(10), f):
                match = COVERS_RE.match(line.rstrip("\r\n"))
                if match and match.group(1):
                    return match.group(1)
    except UnicodeDecodeError:
        return UNREADABLE
    return MISSING


def main(argv: Optional[List[str]] = None) -> int:
    parser = argparse.ArgumentParser(description="List the covers: line of each document under baseline/docs.")
    parser.add_argument("repo", type=Path, help="the repository")
    args = parser.parse_args(argv)
    if not args.repo.is_dir():
        print("error: %s is not a directory" % args.repo, file=sys.stderr)
        return 2
    docs = args.repo / "baseline" / "docs"
    if docs.is_dir():
        for path in sorted(docs.rglob("*.md")):
            print("%s\t%s" % (path.relative_to(args.repo).as_posix(), covers_of(path)))
    return 0


if __name__ == "__main__":
    sys.exit(main())
