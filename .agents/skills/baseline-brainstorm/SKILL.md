---
name: baseline-brainstorm
description: Ask the engineer questions about new work one at a time, each with a diagram, then write the desired-state file they review and accept before any code. Use when the engineer describes new work and you have questions, and when a decision during coding deviates from the accepted file.
compatibility: Python 3.9+ (standard library only)
license: Apache-2.0
metadata:
  version: "0.2.0"
---

# baseline-brainstorm

Paths are relative to this skill's folder. Run the script with `python3`. Stay in the main session: this needs back and forth with the engineer, so never hand it to a subagent.

## When

- The engineer describes new work and you have questions about it. Skip it when the request already settles everything.
- Later, when a decision during coding deviates from the accepted file (step 5).

## 1. List the questions

Send one short message: your questions, one line each, and a proposed name for the file. Run `python3 scripts/next_number.py <repo>` for the number, then name the topic in two to four words, for example `00012-order-refunds`. The engineer may rename it.

## 2. Ask one question at a time

- Lead with a diagram that fits the question. A diagram is a must. `references/diagram-guide.md` says which kind fits which question. Add a snippet or text only where it says more.
- Give the options, your recommendation first, and one reason.
- Stay on the question until the engineer says done. Follow-up questions about the same decision come before the next question.
- Keep notes of every decision as you go: the decision, why, and the options not taken. Write no file yet.
- When an answer is a coding preference or an architectural decision worth keeping, suggest it: "I would make this a standard" or "I would make this an ADR". `references/deciding-adrs-and-standards.md` tells them apart. The engineer may say so first. Only their yes puts it in Settles.
- Keep text short. The engineer reads the diagram first.

## 3. Write the desired-state file

When no question is left, write `baseline/desired-state/<number>-<name>.md` from `assets/desired-state-template.md`.

- **End state:** diagrams, then the expected result as a checklist.
- **Arrival decisions:** one per answer that shaped the work: the decision, why, and each option not taken with a line on why it lost. Do not paste the Q&A.
- **Settles:** the ADRs and standards the engineer said yes to, or `none`.
- Write for two readers: the engineer, who reviews it before any code, and the coding agent, which gets it as its single context. It must stand alone. Say what and why, never how.

Put it on its own branch and open a PR if the repo has a GitHub remote and `gh` works. Otherwise commit it on a branch and say so. Tell the engineer to read it. If they comment, edit the file and wait again. Do not start coding before they accept it.

## 4. After acceptance

1. Write each ADR and standard in Settles, yourself, in this session, from the `TEMPLATE.md` in `baseline/adr/` or `baseline/standards/`. Put them in the next PR above the desired-state PR: the decisions PR. The engineer reviews it.
2. Implement in this same session, not in a subagent. Start from this prompt, with the path of the accepted file:

```text
The engineer accepted baseline/desired-state/00012-order-refunds.md.
It is the intended result: implement it, following the ADRs and standards it lists.
If something in it is unclear or wrong, ask the engineer. Do not guess.
If the engineer decides something that differs from the file, get their explicit confirmation, then amend the file: change the arrival decision, keep the old option under "Not taken", and say so in one line.
When the code is done, report against the file: what was built, what differs, why, and the amendments made.
Ask the engineer to accept the code. Start baseline-documentation only after they do.
After any compaction, re-read the file.
```

How the code is stacked is yours to decide: when to open PRs, how to cut slices. The ADRs and standards are context, and the code must meet them.

## 5. A deviation during coding

A deviation is a decision that contradicts or changes something the file says. Where the file is silent, you decide.

- Ask the engineer with the same protocol: one question, with a diagram of before and after.
- Only after their explicit yes, amend the file in the desired-state PR: change the arrival decision or the end state, keep the old option under "Not taken" with the reason it changed, and say so in one line. Rebase the PRs above it.
- Never decide a deviation silently.

## 6. When the code is done

Report against the file (what was built, what differs, why, the amendments) and ask the engineer to accept the code. Run `baseline-documentation` in a fresh session or subagent only after they accept.

A later decision or repeatable rule is suggested in the chat, or the engineer says "this is an ADR" or "make this a standard". After their yes it goes into the decisions PR.
