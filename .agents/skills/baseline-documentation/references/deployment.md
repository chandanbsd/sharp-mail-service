# The deployment family

Where: `baseline/docs/deploy/README.md`, one file. It says how the system gets from a commit to running, and how to undo it.

## Required sections

- `## Topology`
- `## Environments`
- `## Configuration and secrets`
- `## Pipeline`
- `## Deploying and rolling back`
- `## Troubleshooting`

## Only when the code has it

`## Infrastructure as code` (what the repository holds, how it is applied, where its state lives) · `## Operations` (dashboards, alerts, logs, incident steps) · `## Security and hardening` · `## Cost`.

## Skeleton

```markdown
# Deployment
covers: how the system is deployed, configured, rolled back and troubleshot

## Topology
A diagram of the deployment: environments, hosts or clusters, networks and trust boundaries.

## Environments
Local, CI, staging, production: the purpose of each, how it is created and how it differs.

## Configuration and secrets
A table of every setting: name, purpose, default, where set, secret or not. Names only, never a secret's value.

## Pipeline
A flow diagram of the workflows, gates and artifacts, with how a version is chosen and promoted.

## Deploying and rolling back
The runbook, step by step, with the order across components. After each command: `ran on 9f3c2a1: result`, or `not run`. How to roll back each component, and what cannot be undone.

## Troubleshooting
A table of symptom, check and fix.
```

## Notes

- Deploy, publish and migrate commands are never run. Write `not run` beside them.
- Take the pipeline from the repository's CI files and the topology from its infrastructure files. If neither exists, the family has nothing to document: raise an `empty-family` flag.
