# The course family

Where: `baseline/docs/course/README.md`, the index, and `baseline/docs/course/<NN>-<outcome>.md`, one chapter per outcome.

An outcome is a result a user or a caller can reach: placing an order, signing in, exporting a report. A chapter follows it from its entries (a request, a command, a UI action, a job) to the result, across components and languages. Group the entries that serve one outcome into one chapter.

## Required sections

- `## Chapters` in the index
- `## The flow` in a chapter
- `## Walkthrough` in a chapter
- `## Alternatives and why this one` in a chapter
- `## Pitfalls and failure modes` in a chapter
- `## Practice` in a chapter

## Skeleton of a chapter

```markdown
# Place an order
covers: placing an order, from POST /orders to the confirmation email

## The flow
A diagram from the entries to the outcome, across components and languages, then two lines on what to notice.

## Walkthrough
Follow the flow in order with short snippets from the code, each with its path. Teach each language or framework idea at the point the flow first meets it.

## Alternatives and why this one
Each realistic alternative, and why this one was chosen. Mark a guessed reason `inferred`. Cite the ADR where one gives the reason.

## Pitfalls and failure modes
What goes wrong here and how it shows: retries, partial failure, ordering, bad input.

## Practice
Two or three exercises that change or trace something in this codebase. One line of expected answer each.
```

## Skeleton of the index

```markdown
# Course
covers: the course index, one chapter per outcome the system delivers

## Chapters
1. [Place an order](01-place-an-order.md), one line on the outcome
```

## Notes

- Nothing the code uses goes untaught, and nothing the code does not use is taught. There are no whole-language tutorials. Repeating an idea in two chapters is fine, because its context differs.
- The existing `covers:` lines are the list of outcomes. Add a chapter only for an outcome no chapter covers, and never regroup unless the engineer asks.
- A reason is stated when an ADR, a code comment, a rule in a `SKILL.md` or a test name gives it. Say where. A reason that nothing gives is your best guess: label it `inferred` and raise an `inferred-reason` flag.
- Link each chapter back to the key flow it follows in the architecture overview, with a relative link. Links go from later families to earlier ones, never forward.
