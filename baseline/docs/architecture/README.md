# Architecture
covers: how a host records a message, submits it once and retrieves recipient history

## What the system is

SharpMail is a .NET library for auditing the exact message passed to a host's mail adapter. Its ASP.NET Core demo connects three instances to different audit stores. The repository contains an ADR template and no recorded architecture decisions.

## Big picture

```mermaid
flowchart LR
  Caller[Host application] --> Core[SharpMail]
  Core --> Store[Audit storage]
  Core --> Adapter[Host mail adapter]
  Adapter --> Mail[Mail service]
  Caller -->|Recipient notification| Core
```

```mermaid
flowchart TD
  UI[Scalar or walkthrough] -->|HTTP| A[SQL API]
  UI -->|HTTP| B[Blob API]
  UI -->|HTTP| C[Cosmos API]
  A --> SQL[Azure SQL]
  B --> Blob[Azurite]
  C --> Cosmos[Cosmos emulator]
  A & B & C -->|SMTP| Inbox[Mailpit]
```

## Components

| Package | Responsibility and dependencies |
|---|---|
| Data | Owns message, attempt and event records, JSON representation, event identity and the storage interface. |
| Core | Depends on Data. Validates submissions, invokes the sender and resolves delivery notifications. |
| Renderer.Liquid | Implements the Core renderer contract using Fluid and files under a host-supplied root. |
| Context.AzureSqlServer | Implements Data's storage interface with EF Core, a JSON payload per row and separately keyed delivery rows. |
| Context.AzureBlobStorage | Uses Data's contracts to create separate snapshot, completion, provider lookup and event blobs. |
| Context.AzureCosmosDb | Stores Data's attempt and event payloads in items partitioned by attempt ID. |

## Key flows

```mermaid
sequenceDiagram
  participant H as Host
  participant R as Liquid renderer
  participant F as Template directory
  H->>R: Preview template key with model
  R->>F: Read subject and available body files
  R-->>H: Subject and rendered bodies
```

```mermaid
sequenceDiagram
  participant H as Host
  participant C as Submission service
  participant D as Store
  participant S as Sender
  H->>C: Submit ID and final message
  C->>D: Create Pending if absent
  alt Newly created
    C->>S: Submit once
    S-->>C: SDK outcome or exception
    C->>D: Write completion
    C-->>H: Observed outcome and persistence flag
  else Existing ID
    C->>D: Read attempt
    C-->>H: Recorded attempt or content conflict
  end
```

```mermaid
sequenceDiagram
  participant H as Host
  participant A as Audit service
  participant D as Store
  H->>A: Report recipient event
  opt Only provider reference supplied
    A->>D: Resolve transport and message ID
  end
  A->>D: Read attempt and validate recipient position
  A->>D: Append if identity is new
  H->>A: Look up attempt ID
  A->>D: Read attempt and delivery events
  A-->>H: Snapshot and history ordered by occurrence time
```

## Data

```mermaid
erDiagram
  ATTEMPT ||--o{ DELIVERY_EVENT : records
  ATTEMPT ||--|| MESSAGE : contains
  MESSAGE ||--|{ RECIPIENT : addresses
```

Submission writes the versioned snapshot and SDK outcome. Audit writes each event with a recorded time and a recipient role and index into the snapshot.

## Failure model

A failed initial write prevents sender invocation. If completion fails after invocation, the result carries the observed status and `OutcomePersisted=false`. A lookup can still show Pending. An exact repeat reads the stored state, even while the original call is running. SDK acceptance and delivery to a recipient are recorded independently.

## Interfaces and contracts

A host supplies an `IEmailSender` and calls `AddSharpMail<TSender>()` to register the scoped sender, submission and audit services. The sender receives the attempt ID and finalized message and returns an `EmailSendResult`. It supplies the transport name used for provider-ID lookup. `TimeProvider.System` supplies audit timestamps.

| Store registration | Host provides |
|---|---|
| `AddSqlEmailAudit` | SQL connection string and migrated schema. The registration disables ambient transaction enlistment. |
| `AddBlobEmailAudit` | An existing BlobContainerClient for the audit container. |
| `AddCosmosEmailAudit` | A Cosmos Container partitioned by /attemptId. |

A host can pass final content directly or register the optional renderer with `AddLiquidEmailTemplates(rootDirectory)`.
