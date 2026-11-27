"""The baseline check. CI runs it over baseline/ and nothing else.

Usage: python3 baseline/check.py [repo] [--no-render]
It reads only the state of baseline/ and never asks who wrote what. With no baseline/baseline.yml it passes.
It fails when a document that exists lacks a required section, a Mermaid block is malformed or does not render,
a link between baseline documents does not resolve, or an empty-family flag stands for a family that is neither
in documents_skipped nor written. A needed family that is not written yet is only a notice.
Mermaid blocks are rendered when mmdc is on the PATH; otherwise they are checked for form and a notice says so.
Exit codes: 0 passed, 1 failed, 2 the repository is not a directory.
"""
from __future__ import annotations

import argparse
import os
import re
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path
from typing import List, Optional, Set, Tuple
from urllib.parse import unquote

FAMILIES = ("readme", "architecture", "course", "features", "deployment", "reference")

# family -> (glob under baseline/docs, paths the glob leaves out, the exact headings a document there must have)
REQUIRED = {
    "readme": [
        ("README.md", (), ["## What this is", "## Prerequisites", "## Get it running", "## Repo map", "## Configuration", "## Troubleshooting"]),
    ],
    "architecture": [
        ("architecture/README.md", (), ["## What the system is", "## Big picture", "## Components", "## Key flows"]),
    ],
    "course": [
        ("course/README.md", (), ["## Chapters"]),
        ("course/*.md", ("course/README.md",), ["## The flow", "## Walkthrough", "## Alternatives and why this one", "## Pitfalls and failure modes", "## Practice"]),
    ],
    "features": [
        ("features/README.md", (), ["## Features"]),
        ("features/*.md", ("features/README.md", "features/users-and-access.md"), ["## What it is and why", "## Who can use it", "## What a person can do", "## When things go wrong"]),
    ],
    "deployment": [
        ("deploy/README.md", (), ["## Topology", "## Environments", "## Configuration and secrets", "## Pipeline", "## Deploying and rolling back", "## Troubleshooting"]),
    ],
    "reference": [
        ("reference/glossary.md", (), ["## Terms"]),
        ("reference/file-map.md", (), ["## Folders"]),
    ],
}

DIAGRAMS = {
    "flowchart", "graph", "sequenceDiagram", "classDiagram", "stateDiagram", "stateDiagram-v2", "erDiagram", "journey",
    "gantt", "pie", "quadrantChart", "requirementDiagram", "gitGraph", "mindmap", "timeline", "sankey-beta",
    "xychart-beta", "block-beta", "packet-beta", "architecture-beta", "kanban", "C4Context", "C4Container",
    "C4Component", "C4Dynamic", "C4Deployment",
}

FENCE_RE = re.compile(r"^ {0,3}(`{3,}|~{3,})\s*(.*)$")
INLINE_CODE_RE = re.compile(r"`[^`\n]*`")
LINK_RE = re.compile(r"!?\[[^\]\n]*\]\(\s*<?([^)\s>]+)>?(?:\s+\"[^\"]*\")?\s*\)")
SCHEME_RE = re.compile(r"^[A-Za-z][A-Za-z0-9+.-]*:")


class Block:
    """A fenced block of a Markdown document."""

    def __init__(self, info: str, start: int) -> None:
        self.info = info
        self.start = start
        self.body: List[str] = []
        self.closed = False


def read(path: Path) -> str:
    return path.read_text(encoding="utf-8", errors="replace")


def scan(text: str) -> Tuple[List[Tuple[int, str]], List[Block]]:
    """The lines outside fenced blocks, and the fenced blocks."""
    prose: List[Tuple[int, str]] = []
    blocks: List[Block] = []
    open_fence: Optional[Tuple[str, Block]] = None
    for number, line in enumerate(text.splitlines(), 1):
        if open_fence:
            marker, block = open_fence
            stripped = line.strip()
            if stripped and set(stripped) == {marker[0]} and len(stripped) >= len(marker):
                block.closed = True
                open_fence = None
            else:
                block.body.append(line)
            continue
        match = FENCE_RE.match(line)
        if match:
            block = Block(match.group(2).strip(), number)
            blocks.append(block)
            open_fence = (match.group(1), block)
            continue
        prose.append((number, line))
    return prose, blocks


def read_skipped(text: str) -> List[str]:
    """The names in documents_skipped, written as a flow list [a, b] or a block list."""
    match = re.search(r"^documents_skipped:[ \t]*(.*?)[ \t]*(?:#.*)?$", text, re.M)
    if not match:
        return []
    rest = match.group(1)
    if rest.startswith("["):
        inner = rest[1:rest.index("]")] if "]" in rest else rest[1:]
        names = [part.strip().strip("'\"") for part in inner.split(",")]
    elif rest:
        names = [rest.strip("'\"")]
    else:
        names = []
        for line in text[match.end():].splitlines()[1:]:
            item = re.match(r"^[ \t]+-[ \t]*(\S+)", line)
            if not item:
                break
            names.append(item.group(1).strip("'\""))
    return [name for name in names if name]


def mermaid_problem(body: List[str]) -> Optional[str]:
    lines = [line.strip() for line in body]
    if not any(lines):
        return "empty Mermaid block"
    i = 0
    while i < len(lines):
        line = lines[i]
        if not line or line.startswith("%%"):
            i += 1
        elif line == "---":
            end = i + 1
            while end < len(lines) and lines[end] != "---":
                end += 1
            if end >= len(lines):
                return "Mermaid front matter is not closed"
            i = end + 1
        else:
            break
    if i >= len(lines):
        return "empty Mermaid block"
    if lines[i].split()[0] not in DIAGRAMS:
        return "unknown Mermaid diagram type: %r" % lines[i]
    return None


def link_target(doc: Path, target: str, repo: Path) -> Optional[Path]:
    """Where a link points, or None for a link the check does not follow."""
    if not target or target.startswith(("#", "//")) or SCHEME_RE.match(target):
        return None
    target = unquote(re.split(r"[#?]", target, maxsplit=1)[0])
    if not target:
        return None
    start = repo if target.startswith("/") else doc.parent
    return Path(os.path.normpath(str(start / target.lstrip("/"))))


def error_detail(output: str) -> str:
    """The first lines of a renderer's error that say what is wrong, without its stack frames."""
    lines = [line.strip() for line in output.splitlines() if line.strip()]
    useful = [line for line in lines if not line.startswith("at ") and "node_modules" not in line and "file://" not in line]
    return " ".join((useful or lines[-1:])[:3])


def render(blocks: List[Tuple[str, Block]], errors: List[str], notices: List[str]) -> None:
    if not blocks:
        return
    mmdc = shutil.which("mmdc")
    if not mmdc:
        notices.append("mermaid: %d block(s) not rendered because mmdc is not on the PATH; their form was checked" % len(blocks))
        return
    with tempfile.TemporaryDirectory() as tmp:
        extra: List[str] = []
        if os.environ.get("CI") or os.environ.get("GITHUB_ACTIONS"):
            config = Path(tmp) / "puppeteer.json"
            config.write_text('{"args": ["--no-sandbox"]}')
            extra = ["-p", str(config)]
        rendered = 0
        for number, (rel, block) in enumerate(blocks):
            source = Path(tmp) / ("block%d.mmd" % number)
            source.write_text("\n".join(block.body) + "\n", encoding="utf-8")
            try:
                done = subprocess.run(
                    [mmdc, "-i", str(source), "-o", str(Path(tmp) / ("block%d.svg" % number))] + extra,
                    capture_output=True, text=True, timeout=180,
                )
            except (OSError, subprocess.TimeoutExpired) as error:
                errors.append("%s:%d: the Mermaid renderer could not run: %s" % (rel, block.start, error))
                continue
            if done.returncode != 0:
                errors.append("%s:%d: Mermaid block does not render: %s" % (rel, block.start, error_detail(done.stderr or done.stdout)))
            else:
                rendered += 1
        if rendered:
            notices.append("mermaid: rendered %d block(s) with mmdc" % rendered)


def check(repo: Path, do_render: bool) -> Tuple[List[str], List[str]]:
    errors: List[str] = []
    notices: List[str] = []
    base = repo / "baseline"
    config = base / "baseline.yml"
    if not config.is_file():
        notices.append("baseline: no baseline/baseline.yml, so there is nothing to check")
        return errors, notices

    skipped = read_skipped(read(config))
    for name in skipped:
        if name not in FAMILIES:
            errors.append("baseline/baseline.yml: documents_skipped names an unknown family %r; the families are %s" % (name, ", ".join(FAMILIES)))

    docs = base / "docs"
    written: Set[str] = set()
    for family in FAMILIES:
        for pattern, left_out, headings in REQUIRED[family]:
            for path in sorted(docs.glob(pattern)) if docs.is_dir() else []:
                relative = path.relative_to(docs).as_posix()
                if not path.is_file() or relative in left_out:
                    continue
                written.add(family)
                prose, _blocks = scan(read(path))
                present = {line.rstrip() for _, line in prose}
                for heading in headings:
                    if heading not in present:
                        errors.append('%s: missing section "%s"' % (path.relative_to(repo).as_posix(), heading))

    flags = base / "flags"
    for path in sorted(flags.glob("empty-family-*.md")) if flags.is_dir() else []:
        found = re.search(r"^about:[ \t]*(\S+)", read(path), re.M)
        family = found.group(1) if found else ""
        rel = path.relative_to(repo).as_posix()
        if family not in FAMILIES:
            errors.append("%s: an empty-family flag must say about: followed by a family (found %r)" % (rel, family))
        elif family not in skipped and family not in written:
            errors.append("%s: the empty-family flag for %s stands; add it to documents_skipped in baseline/baseline.yml or write it" % (rel, family))

    for family in FAMILIES:
        if family not in skipped and family not in written:
            notices.append("not yet documented: %s" % family)

    base_norm = Path(os.path.normpath(str(base)))
    to_render: List[Tuple[str, Block]] = []
    for path in sorted(base.rglob("*.md")):
        if not path.is_file():
            continue
        rel = path.relative_to(repo).as_posix()
        prose, blocks = scan(read(path))
        for block in blocks:
            if not block.closed:
                errors.append("%s:%d: unclosed code fence" % (rel, block.start))
            elif block.info.split(" ")[0] == "mermaid":
                problem = mermaid_problem(block.body)
                if problem:
                    errors.append("%s:%d: %s" % (rel, block.start, problem))
                else:
                    to_render.append((rel, block))
        for number, line in prose:
            for target in LINK_RE.findall(INLINE_CODE_RE.sub("", line)):
                resolved = link_target(path, target, repo)
                if resolved is None:
                    continue
                inside = resolved == base_norm or base_norm in resolved.parents
                if inside and not resolved.exists():
                    errors.append("%s:%d: the link to %s does not resolve" % (rel, number, target))
    if do_render:
        render(to_render, errors, notices)
    return errors, notices


def main(argv: Optional[List[str]] = None) -> int:
    parser = argparse.ArgumentParser(description="Check baseline/ and nothing else.")
    parser.add_argument("repo", nargs="?", type=Path, default=Path(__file__).resolve().parent.parent, help="the repository")
    parser.add_argument("--no-render", action="store_true", help="check the form of Mermaid blocks but never render them")
    args = parser.parse_args(argv)
    if not args.repo.is_dir():
        print("error: %s is not a directory" % args.repo, file=sys.stderr)
        return 2
    errors, notices = check(args.repo.absolute(), not args.no_render)
    for line in errors:
        print("error: %s" % line)
    for line in notices:
        print("notice: %s" % line)
    print("baseline check: %d error%s, %d notice%s" % (len(errors), "" if len(errors) == 1 else "s", len(notices), "" if len(notices) == 1 else "s"))
    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(main())
