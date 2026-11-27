# The architecture family

Where: `baseline/docs/architecture/README.md`, the overview, plus further files in the same folder when a section grows too long. Link them from the overview. The overview is reopened on every run.

## Required sections

- `## What the system is`
- `## Big picture`
- `## Components`
- `## Key flows`

## Only when the code has it

`## Data` (the data model as an ER diagram, and who writes what) · `## Interfaces and contracts` · `## State machines` · `## Failure model` (retries, timeouts, ordering) · `## Security` (trust boundaries, who may do what) · `## Quality` (tests, and numbers only if measured) · `## Known limits`. Write one only when the code has the thing.

## Skeleton

```markdown
# Architecture
covers: how the system is built: its parts, how they talk and the main flows

## What the system is
A paragraph on the purpose, then the technology choices, each linking to its ADR in `baseline/adr/`. Where there is no ADR, say so in one line.

## Big picture
A context diagram (users and outside systems) and a container diagram (deployable parts, datastores and protocols).

## Components
The module map, the direction of dependencies and the rule that keeps it so. One line of responsibility per module.

## Key flows
One sequence diagram per major flow. Do not link forward to the course: it is written after this family, and a link to a file that does not exist yet fails the check. The course chapters link back to these flows.
```

## Notes

- Draw from the code, not from memory. A box with no code behind it is not drawn.
- A number such as a latency or a size appears only if you measured it. Say how.
