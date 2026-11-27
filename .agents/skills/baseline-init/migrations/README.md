# Migrations

A migration moves a repository from one baseline version to the next. None ships in 0.2.0.

## Layout

```text
migrations/<from>-to-<to>/migrate.py     the deterministic change
migrations/<from>-to-<to>/MIGRATION.md   optional: instructions for changes a script cannot make
```

A made-up example: `0.3.0-to-0.4.0` moves one folder to a new place.

## Contract of `migrate.py`

```text
python3 migrate.py <repo> --dry-run    print exactly what would move or change; change nothing; exit 0
python3 migrate.py <repo> --confirm    apply it; exit non-zero on any failure
```

- Standard library only, non-interactive.
- It may change project-owned files (`baseline/baseline.yml`, ADRs, standards, project docs) because the skill defines their layout. `update.py` runs it only after the engineer has seen the dry run and approved.
- Ship a before-and-after fixture test with every migration.

## Dry run and instructions

`update.py plan` dry-runs only the next pending migration, because a later one may depend on what the earlier one changes; the later ones are listed by name. If a migration folder holds a `MIGRATION.md`, `plan` names it; the agent reads and follows it after the engineer approves, before running `apply`.

## Which migrations run

Pending migrations are those with `from` at or above the repo's `baseline_version` and `to` at or below the installed version, in order of `from`.
