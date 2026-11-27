# Receipt preview
covers: inspect a subject and both body variants before submission

## What it is and why

Inspect a subject and both body variants before submission.

## Who can use it

Anyone with access to the demo API can use this operation.

## What a person can do

```mermaid
flowchart LR
  SelectTemplate --> SupplyModel --> InspectPreview
```

In Scalar, open POST /preview and submit the orders/receipt example. Read subject, htmlBody and textBody from the response.

## When things go wrong

A 400 response describes an invalid template or model. Check the key and supply the variables used by the template.
