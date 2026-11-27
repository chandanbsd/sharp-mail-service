# Flags

`baseline/flags/` holds the current set. On every run delete the existing flag files, then write the flags that are true at the tip. Git history is the record. A flag the engineer disagrees with comes back until its cause changes, for example by editing the standard it cites.

## Kinds

| Kind | Write it when |
|---|---|
| `empty-family` | A needed family has nothing to document at the tip. Write no page. The check fails until the engineer skips the family in `documents_skipped` or fills it. Never edit `documents_skipped` yourself. |
| `stale-doc` | A document outside the run's diff says something the tip contradicts. Do not rewrite it. |
| `breaks-rule` | The code contradicts an ADR or a standard. Name the rule. |
| `inferred-reason` | A reason in a course chapter is your guess and the engineer should confirm it. One flag per chapter, listing its guesses. |
| `overlap` | Adopt: documents outside `baseline/docs/` cover the same ground as a baseline document. One flag for a folder or a group, not one for each file. The team deletes the superseded ones in review. |
| `command-failed` | A command that stays in the checkout failed for an environment reason. |
| `not-backed` | Revise: a review comment asks for something the code at the tip cannot back. |
| `link-readme` | The root README does not link `baseline/docs/README.md`. |

## File

One file per flag, named `<kind>-<slug>.md`, and the slug is never left out (for `link-readme` use `readme`). The check reads one thing from it: for an `empty-family` flag, `about:` is the family's key (`deployment`, not a path), and the file is named `empty-family-<family>.md`.

```markdown
# <one line: what is wrong>
kind: stale-doc
about: baseline/docs/architecture/README.md

Two or three lines: what, where, and what the engineer can do about it.
```
