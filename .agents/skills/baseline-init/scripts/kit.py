"""Shared by init.py and update.py: the kit files, the baseline-owned files and the AGENTS.md pointer."""
from __future__ import annotations

import hashlib
import re
from dataclasses import dataclass
from pathlib import Path
from typing import Dict, List, Optional

SKILL_DIR = Path(__file__).resolve().parent.parent
KIT_DIR = SKILL_DIR / "assets" / "kit"
FAMILIES = ("readme", "architecture", "course", "features", "deployment", "reference")
POINTER_TOKEN = "baseline/baseline.md"
HEADER_RE = re.compile(
    r"^(?:<!-- baseline version=(\S+) sha256=([0-9a-f]{64}) -->|# baseline version=(\S+) sha256=([0-9a-f]{64}))$"
)
VERSION_RE = re.compile(r'^[ \t]+version:[ \t]*"?([^"\n]+?)"?[ \t]*$', re.M)


@dataclass(frozen=True)
class OwnedFile:
    """A file the baseline owns whole: init creates it and update replaces it."""

    path: str  # where it lives in a repository
    asset: str  # where its body lives under assets/kit/ (never a hidden folder, so every installer copies it)
    style: str  # "html" for a Markdown file, "hash" for a file that takes # comments
    label: str  # what update.py plan calls it
    recreate: bool  # update writes it again when it is missing


OWNED = (
    OwnedFile("baseline/baseline.md", "baseline/baseline.md", "html", "baseline.md", True),
    OwnedFile("baseline/check.py", "baseline/check.py", "hash", "check.py", False),
    OwnedFile(".github/workflows/baseline.yml", "github-workflow/baseline.yml", "hash", "workflow", False),
)
BY_PATH = {owned.path: owned for owned in OWNED}


@dataclass
class Owned:
    version: Optional[str]  # None when the first line is not a baseline header
    sha256: Optional[str]
    body: str  # everything after the first line, line endings normalised to LF so the hash survives CRLF


def read_text(path: Path) -> str:
    """Read a file keeping its line endings exactly as they are."""
    with open(path, encoding="utf-8", newline="") as f:
        return f.read()


def write_text(path: Path, text: str) -> None:
    """Write text without translating line endings."""
    with open(path, "w", encoding="utf-8", newline="") as f:
        f.write(text)


def installed_version() -> str:
    """The version this skill was installed at: metadata.version in its own SKILL.md."""
    return VERSION_RE.search(read_text(SKILL_DIR / "SKILL.md")).group(1)


def _lf(text: str) -> str:
    return text.replace("\r\n", "\n")


def newline_of(text: str) -> str:
    return "\r\n" if "\r\n" in text else "\n"


def _digest(body: str) -> str:
    return hashlib.sha256(_lf(body).encode("utf-8")).hexdigest()


def parse_owned(text: str) -> Owned:
    """Read the header line of a baseline-owned file. A file with no valid header has version None."""
    first, sep, rest = text.partition("\n")
    match = HEADER_RE.match(first.rstrip("\r"))
    if not match:
        return Owned(None, None, _lf(text))
    version = match.group(1) or match.group(3)
    digest = match.group(2) or match.group(4)
    return Owned(version, digest, _lf(rest if sep else ""))


def owned_is_edited(owned: Owned) -> bool:
    """True when the header is missing or the body no longer matches the hash in it."""
    return owned.version is None or _digest(owned.body) != owned.sha256


def render_owned(path: str, version: str, newline: str = "\n") -> str:
    """The shipped body of an owned file with its header: the version and a hash of the body."""
    spec = BY_PATH[path]
    body = _lf(read_text(KIT_DIR / spec.asset))
    if not body.endswith("\n"):
        body += "\n"
    template = "<!-- baseline version=%s sha256=%s -->" if spec.style == "html" else "# baseline version=%s sha256=%s"
    text = template % (version, _digest(body)) + "\n" + body
    return text if newline == "\n" else text.replace("\n", newline)


def render_baseline_md(version: str, newline: str = "\n") -> str:
    return render_owned("baseline/baseline.md", version, newline)


def pointer_text() -> str:
    text = _lf(read_text(KIT_DIR / "AGENTS.pointer.md"))
    return text if text.endswith("\n") else text + "\n"


def has_pointer(text: str) -> bool:
    return POINTER_TOKEN in text


def with_pointer(text: str) -> str:
    """Append the pointer after a blank line unless it is there already; keeps the file's line endings."""
    if has_pointer(text):
        return text
    newline = newline_of(text)
    pointer = pointer_text().replace("\n", newline)
    if text and not text.endswith("\n"):
        text += newline
    return text + (newline if text else "") + pointer


def parse_skip(value: str) -> List[str]:
    """The families named in a comma separated list, in family order and without duplicates."""
    named = [part.strip() for part in value.split(",") if part.strip()]
    for name in named:
        if name not in FAMILIES:
            raise ValueError("unknown document family %r; the families are %s" % (name, ", ".join(FAMILIES)))
    return [family for family in FAMILIES if family in named]


def baseline_yml(version: str, skipped: List[str]) -> str:
    return "baseline_version: %s\ndocuments_skipped: [%s]\n" % (version, ", ".join(skipped))


def kit_files(version: str, skipped: Optional[List[str]] = None) -> Dict[str, str]:
    """Relative path -> content for every kit file except AGENTS.md (which gets the pointer)."""
    files = {owned.path: render_owned(owned.path, version) for owned in OWNED}
    files["baseline/baseline.yml"] = baseline_yml(version, skipped or [])
    files["CLAUDE.md"] = "@AGENTS.md\n"
    for rel in ("baseline/adr/TEMPLATE.md", "baseline/standards/TEMPLATE.md"):
        files[rel] = read_text(KIT_DIR / rel)
    return files
