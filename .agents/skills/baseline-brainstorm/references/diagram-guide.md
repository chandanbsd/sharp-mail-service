# Which diagram for which question

Every question gets a diagram. Pick the kind that shows the decision, not the one that is easiest to draw.

| The question is about | Diagram |
|---|---|
| A schema or data change | ER diagram, before and after |
| A flow between parts | Sequence diagram, or a flowchart when there is no timeline |
| A status or lifecycle | State diagram |
| Which parts exist and how they depend | Component flowchart |
| Choosing between options | The options side by side, one box each |

Rules: one idea per diagram, about twelve nodes at most, every edge labelled when the label says something, Mermaid only so it renders on GitHub. Add one italic line under it that names the thing to notice.

## A schema change

```mermaid
erDiagram
    ORDER ||--o{ REFUND : has
    REFUND {
        int id
        int order_id
        string reason
    }
```

## A flow

```mermaid
sequenceDiagram
    participant C as Client
    participant A as API
    C->>A: POST /refunds
    A-->>C: 201 Created
```

## A lifecycle

```mermaid
stateDiagram-v2
    [*] --> Requested
    Requested --> Approved: engineer approves
    Approved --> Paid
```

## Parts and dependencies

```mermaid
flowchart LR
    API["API"] --> DB[("database")]
    API --> Queue["queue"] --> Worker["worker"]
```

## Options side by side

```mermaid
flowchart LR
    subgraph A["A: a refunds table"]
        a1["refunds"]
    end
    subgraph B["B: a column on orders"]
        b1["orders.refunded_at"]
    end
```
