# Audited submission
covers: reserve a reference for a support ticket and submit its final message

## What it is and why

Reserve a reference for a support ticket and submit its final message.

## Who can use it

Anyone with access to the demo API can use this operation.

## What a person can do

```mermaid
flowchart LR
  ReserveID --> Submit --> InspectOutcome
```

POST /attempt-ids supplies the reference. PUT /attempts/{id} returns the attempt and whether it was reused. Mailpit shows the captured SMTP message.

## When things go wrong

A content conflict returns 409. A 503 carries the observed result when completion persistence failed. Look up the original ID to inspect durable state.
