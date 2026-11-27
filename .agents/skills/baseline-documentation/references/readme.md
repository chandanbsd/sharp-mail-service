# The README family

Where: `baseline/docs/README.md`, one file. It is the project overview for a reader who has never seen the repository. It is not the root `README.md`, which stays the team's.

## Required sections

- `## What this is`
- `## Prerequisites`
- `## Get it running`
- `## Repo map`
- `## Configuration`
- `## Troubleshooting`

## Skeleton

```markdown
# <Project name>
covers: the whole repository, how to run it and where things are

## What this is
One paragraph, then a diagram of the system and what it talks to.

## Prerequisites
The tools and versions needed, how to install each, and the versions this was checked with.

## Get it running
Numbered commands from clone to a running project. After each command say what happened:
`ran on 9f3c2a1: 41 passed`, or `not run`. For a repository that is a library or a set of skills, "running" means the tests, the check and the first use: the install line comes from `AGENTS.md` or the README, and the clone URL from `git remote get-url origin`.

## Repo map
The tree, one line per folder. Link the architecture overview and the course index.

## Configuration
A table: name, purpose, default, where set, secret or not. Never a secret's value.

## Troubleshooting
A table of symptom, check and fix, for the first hour only.
```

## Notes

- Build the repo map from the tree that exists, and the commands from the repository's own scripts, Makefile or CI files.
- The README is written last, so it can link everything else.
