# The features family

Where: `baseline/docs/features/README.md`, the overview, and `baseline/docs/features/<feature>.md`, one page per feature. A feature is something a person can do or see, described for what they experience. How the code does it belongs to the course.

## Required sections

- `## Features` in the overview
- `## What it is and why` in a feature page
- `## Who can use it` in a feature page
- `## What a person can do` in a feature page
- `## When things go wrong` in a feature page

## Only when the code has it

A page `users-and-access.md` with `## Users and access` when the code has roles: the kinds of user, what each can do, and a who-can-do-what matrix. `## Limits today` in a feature page, for what it does not do yet.

## Skeleton of a feature page

```markdown
# Refunds
covers: refunding an order, who may do it and what the person sees

## What it is and why
One paragraph.

## Who can use it
The roles, or "anyone".

## What a person can do
A diagram of the journey (a flowchart or a sequence of the person's steps), then what they see at each step.

## When things go wrong
What the person sees and what they can do about it.
```

## Skeleton of the overview

```markdown
# Features
covers: the features of the system, one page each

## Features
One line per feature with a link to its page.
```
