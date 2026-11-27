# The reference family

Where: `baseline/docs/reference/glossary.md` and `baseline/docs/reference/file-map.md`. Two lookup pages. Both always have something to say, so this family is never empty.

## Required sections

- `## Terms` in the glossary
- `## Folders` in the file map

## Skeleton of the glossary

```markdown
# Glossary
covers: the terms the documents use, product and technical

## Terms
| Term | Meaning | Where it appears |
|---|---|---|
| Refund | Money returned for an order. | features/refunds.md |
```

Include every term a new reader may not know: product terms and technical terms in one table, in alphabetical order.

## Skeleton of the file map

```markdown
# File map
covers: what each folder of the repository holds and which document explains it

## Folders
| Path | What it holds | Explained in |
|---|---|---|
| `src/orders/` | Order creation and refunds. | course/01-place-an-order.md |
```

One row per folder, not per file. Update only the rows a diff touches, and add a row for a new folder.
