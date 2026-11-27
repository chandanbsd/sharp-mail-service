---
name: baseline-init
description: Set up a repository for the baseline (ADRs and coding standards that steer agents) or update a repository that already uses it. Use when the engineer asks to initialise, install, set up, adopt or update the baseline in a repo.
compatibility: Python 3.9+ (standard library only), git
license: Apache-2.0
metadata:
  version: "0.2.0"
---

# baseline-init

Paths are relative to this skill's folder. Run the scripts with `python3`.

## Init: set up a repository

Everything the baseline writes goes inside `baseline/`. Outside it, init writes only a pointer in `AGENTS.md`, `CLAUDE.md` and the CI workflow, which GitHub reads only from `.github/workflows/`.

```text
baseline/baseline.md              the baseline's instructions: owned by the baseline, replaced whole on update
baseline/check.py                 the check CI runs: owned by the baseline, replaced whole on update
baseline/baseline.yml             the project's: baseline_version and documents_skipped
baseline/adr/TEMPLATE.md          the ADR template
baseline/standards/TEMPLATE.md    the coding standard template
.github/workflows/baseline.yml    runs the check on pull requests: owned by the baseline, replaced whole on update
AGENTS.md                         a short pointer to baseline/baseline.md (created, or appended once)
CLAUDE.md                         @AGENTS.md, only if absent
```

1. Put one question to the engineer: which of the six document families do not apply? All six are needed by default, so the answer is usually none. The families are `readme`, `architecture`, `course`, `features`, `deployment` and `reference`.
2. Run `python3 scripts/init.py <repo> --dry-run` (add `--skip a,b` for the families that do not apply) and show the engineer the lines it prints.
3. After their yes, run the same command without `--dry-run`.
4. Tell the engineer what was created, that nothing is committed, and any `note:` line the script printed. Say that the workflow installs Node and Mermaid's CLI so CI renders the diagrams, and that `python3 baseline/check.py` runs the same check locally (without the renderer it checks the form of each diagram and says so).

The script is `scripts/init.py`. It never overwrites a file. It leaves the rest of an existing `AGENTS.md` as it is, line endings included. It refuses a repo that already has `baseline/baseline.yml` (use update mode there), an unknown family name and an `AGENTS.md` that is not UTF-8. A root `baseline.yml` of an older layout is left alone. The files it copies come from `assets/kit/`.

If the output has `note: CLAUDE.md does not import AGENTS.md`, tell the engineer: Claude Code reads `CLAUDE.md` and will not see the pointer until that file has the line `@AGENTS.md`. Add it only after their yes.

## Update: bring a repository to the installed version

The skills are updated by the package manager first. Then, in the repo:

1. Run `python3 scripts/update.py plan <repo>`. It prints the versions, the next pending migration with its dry run, the later ones by name, the state of `baseline.md`, `check.py` and the workflow (unchanged, outdated, edited or missing), and whether `AGENTS.md` still has the pointer.
   - Exit 3: the repo is newer than the installed skills. Stop and tell the engineer to update the skills first.
   - Exit 2: it refuses (for example, `baseline/baseline.yml` is missing or its `baseline_version` is unreadable). Show the engineer the message and do not edit around it.
   - `AGENTS.md pointer: missing`: update never writes `AGENTS.md`. Show the engineer the pointer in `assets/kit/AGENTS.pointer.md` and append it only after their yes.
2. Put one decision to the engineer for that migration: the recommendation first, with one reason. Only after their yes: if the plan names instructions for it, read and follow them, then run `python3 scripts/update.py apply <repo> <migration-id>`. Run `plan` again for the next migration.
   - Exit 1 means a migration failed. The version stays and nothing is rolled back, so tell the engineer to check `git status`.
3. When none is pending, run `python3 scripts/update.py finish <repo>`. It replaces `baseline/baseline.md`, `baseline/check.py` and `.github/workflows/baseline.yml` whole and sets `baseline_version`. A `check.py` or workflow the team deleted stays deleted. Exit 4 means a baseline-owned file was edited, or replaced by other text, and the message names it: show the engineer the difference and let them decide; `--discard-edits` replaces it. If they want to keep the edit, stop there and say that the repo is not fully at the installed version (`plan` keeps reporting the file as edited), because recording an override is not built yet.
4. Leave committing and PRs to the repo's own conventions.

`apply` and `finish` take `--dry-run` to show what they would do and change nothing.

Rules:
- A migration may change project-owned files (`baseline/baseline.yml`, ADRs, standards, project docs) only after the engineer has seen its dry run and approved.
- A declined migration stops the update. Say that the repo stays at its current version, and why. Never skip a migration and never record an override.
- The script is `scripts/update.py`; the migration contract is in `migrations/README.md`.
