# Glossary
covers: the terms used to describe submission and recipient history

## Terms

| Term | Meaning | Where it appears |
|---|---|---|
| Accepted | Adapter reported success, with a UTC acceptance timestamp. | [Submission](../features/submission.md) |
| Aspire | Orchestrates the local API and dependencies. | [Deployment](../deploy/README.md) |
| Attempt ID | GUID for one intentional submission. | [Submission](../features/submission.md) |
| Azurite | Local emulator for Azure Blob Storage. | [Deployment](../deploy/README.md) |
| Correlation ID | Application reference that can be shared across attempts. | [Submission](../course/02-submit.md) |
| Delivery event | Delivered, Deferred or Bounced notification for one stored recipient position. | [History](../course/03-history.md) |
| Failed | Adapter reported rejection or threw an exception. | [Submission](../course/02-submit.md) |
| Fluid | Liquid template engine used to generate the subject and bodies. | [Preview](../course/01-preview.md) |
| Mailpit | Local SMTP capture service with a web inbox. | [Deployment](../deploy/README.md) |
| OutcomePersisted | Indicates whether the completion write succeeded on a new submission. | [Submission](../course/02-submit.md) |
| Pending | Stored attempt awaiting a durable completion. | [Architecture](../architecture/README.md) |
| Provider message ID | Adapter-supplied message identifier resolved within a transport. | [History](../course/03-history.md) |
| Recipient position | Zero-based index within the original To, Cc or Bcc list. | [History](../course/03-history.md) |
| Scalar | Interactive HTTP client provided by the demo API. | [Deployment](../deploy/README.md) |
| Snapshot | Finalized message and attempt metadata recorded before sender invocation. | [Architecture](../architecture/README.md) |
| Transport | Adapter name recorded with the attempt and used for provider lookup. | [History](../course/03-history.md) |
