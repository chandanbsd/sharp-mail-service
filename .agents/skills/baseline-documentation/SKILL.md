---
name: baseline-documentation
description: Write and update the repository's documentation inside baseline/docs from the accepted code, in a fresh session or subagent that reads the diff and the code. Use only after the engineer has accepted the code, when adopting a repository that has no baseline documents, and when the engineer sends review comments on the documentation PRs.
compatibility: Python 3.9+ (standard library only), git
license: Apache-2.0
metadata:
  version: "0.2.0"
---

# baseline-documentation

Paths are relative to this skill's folder. Run the script with `python3`.

## Who you are and what you read

You are a fresh agent: a new session or a subagent, started after the engineer accepted the code. You document what the code at the tip does, and nothing else.

Read the code at the tip, `baseline/adr/`, `baseline/standards/`, `baseline/baseline.yml` and the documents already in `baseline/docs/`. Do not read `baseline/desired-state/` or the conversation: they say what was meant, and a document says what exists. In a repository of instructions, scripts or skills, the instruction files (`AGENTS.md`, the README, every `SKILL.md`), the scripts and the tests are the code, so read them too: they are where a command such as an install line comes from.

## The families

There are six. Each is needed unless `documents_skipped` in `baseline/baseline.yml` names it. Read a family's reference only when you write that family.

| Family | Where | Reference |
|---|---|---|
| `readme` | `baseline/docs/README.md` | `references/readme.md` |
| `architecture` | `baseline/docs/architecture/` | `references/architecture.md` |
| `course` | `baseline/docs/course/` | `references/course.md` |
| `features` | `baseline/docs/features/` | `references/features.md` |
| `deployment` | `baseline/docs/deploy/` | `references/deployment.md` |
| `reference` | `baseline/docs/reference/` | `references/reference.md` |

## Rules for every document

- Write only what exists at the tip. No intent and no plans. A box in a diagram has code behind it.
- Diagrams and snippets first, tight text. Mermaid only, and it must render.
- One reader: an engineer fluent in programming and new to this repository's languages and frameworks.
- Line 2 is `covers: <the behaviour the document describes>` (a flow, a feature, a component, never a list of files).
- Every required section of the family's reference is there. Other sections appear only when the code has them, and there are no "Not applicable" lines.
- Label what you did not check: a guessed reason is `inferred`, a command you did not run is `not run`.
- Write only inside `baseline/docs/` and `baseline/flags/`. Never edit the root README or any other document of the team's.

## 1. Find what to write

- **Update** (the normal run). Take the stack's diff: `git diff --name-only <base>...HEAD`, where the base is the stack's base branch, usually `main`. Run `python3 scripts/covers.py <repo>` and pick the documents whose behaviour the diff changes. Always reopen the README and the architecture overview. A diff that creates an outcome no chapter covers gets a new course chapter, and a new feature gets a feature page. Never regroup the existing chapters unless the engineer asks.
- **Adopt** (no `baseline/docs/` yet). Document the whole repository. Never rewrite an existing document outside `baseline/docs/`: write the baseline's documents inside the folder and raise an `overlap` flag for each existing folder or group of documents that covers the same ground, one flag for a folder and not one for each file. The team deletes the superseded ones in review.
- **Revise** (the engineer comments on the documentation PRs). Edit in place per the comments and keep each `covers:` line right. If a comment asks for something the code at the tip cannot back, raise a `not-backed` flag and do not write it. Run no review or verify loops of your own.

A document outside the diff that the tip contradicts gets a `stale-doc` flag. Do not rewrite it.

## 2. Write

Write the families in this order, so later ones can link to earlier ones: `architecture`, `course`, `features`, `deployment`, `reference`, `readme`. Skip a family named in `documents_skipped`. A needed family with nothing to document at the tip gets no page and an `empty-family` flag: never add it to `documents_skipped` yourself.

## 3. Run commands

Run only commands that change nothing beyond the checkout: build, test, lint, dry runs and `--help`. Record the short commit and the result beside the command, for example `ran on 9f3c2a1: 41 passed`. A document cannot name the commit that will hold it, so name the commit the run started from, or say "with this file added". Never run deploy, publish or migrate commands or anything that touches an outside system: write `not run`. If a command fails for an environment reason, raise a `command-failed` flag.

## 4. Flags

`baseline/flags/` holds the current set. On every run delete the existing flag files and write the ones that are true at the tip. The kinds and the file format are in `references/flags.md`.

## 5. PRs

One PR per family you touched, stacked on the accepted code in the order you wrote them, then a flags PR on top. Follow the repository's own conventions for branches and descriptions. Each description says what was documented, how it was checked and what was not verified. If `mmdc` is not on the PATH, say that the diagrams were checked for form and not rendered. Do not merge, deploy or publish.

If the root README does not link `baseline/docs/README.md`, raise a `link-readme` flag.
