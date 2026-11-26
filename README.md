# SharpMail

SharpMail records the finalized message before invoking an application's mailer and lets support retrieve the exact attempt later. An attempt includes the sender, reply-to, ordered To/Cc/Bcc lists, subject, both supplied bodies, correlation ID, UTC timestamps, SDK outcome, and recipient delivery history. The [design review](docs/design.md) explains the lifecycle.

## Library structure

| Package | Responsibility |
|---|---|
| SharpMail.Data | Message and audit records, delivery identity, serialization, and the storage contract. |
| SharpMail.Core | Submission, lookup, delivery recording, and host-facing sender and renderer interfaces. Depends on Data. |
| SharpMail.Sink.Blobs | Azure Blob Storage persistence. Depends on Data. |
| SharpMail.Sink.Cosmos | Cosmos DB persistence. Depends on Data. |
| SharpMail.Sink.AzureSQLServer | EF Core context, migrations, and Azure SQL persistence. Depends on Data. |
| SharpMail.Renderer.Liquid | Liquid rendering and template registration. Implements the Core renderer interface. |

Place each primary constructor parameter on its own line, starting on the line after the opening parenthesis. Use one named type per file and match its namespace to its project and folder. The build enforces C# style through SDK analyzers, EditorConfig, and the one-type-per-file analyzer. Run `dotnet format --verify-no-changes` to check formatting.

## What is an attempt?

An attempt is one intentional submission of a finalized email to the mailer. SharpMail allocates an attempt ID so a support ticket can point to that exact message, even if submission fails. Calling submit again with the same ID and content returns the recorded result. Deliberately sending another copy uses a new ID. Both copies can share the application's correlation ID, such as an order number. Later delivery or bounce events belong to the original submission.

Inject `IEmailSubmissionService` to allocate an ID and submit, and `IEmailAuditService` to retrieve the record or append delivery events. Both live under `SharpMail.Core.Services.Interfaces`, alongside the sender and renderer interfaces. Storage providers implement `SharpMail.Data.Interfaces.IEmailAuditDataContext`. Custom exceptions live in `SharpMail.Core.Exceptions`, data contracts in the owning project's `Models` namespace, and DI extension methods in each package's `Extensions` namespace.

## Run locally

Use .NET SDK 11.0.100 or a compatible 11.0.1xx patch, Aspire CLI 17, and a running Podman machine. From the repository root run:

```sh
ASPIRE_CONTAINER_RUNTIME=podman aspire run
```

If the SDK is installed under your user profile, set `DOTNET_ROOT="$HOME/.dotnet"` and prepend it to `PATH` before launching Aspire.

Aspire launches the local control plane and builds the ASP.NET Core API in containers. Each API runs the same application with a different provider. The first run pulls Microsoft’s native Azure SQL Database development image, the Linux Cosmos DB emulator, Azurite, and Mailpit. SQL and Cosmos need several minutes on a cold start. Open the dashboard URL printed by Aspire for resource status and logs.

| Local service | Address |
|---|---|
| Azure SQL audit API | http://localhost:18081 |
| Azure Blob audit API | http://localhost:18082 |
| Cosmos DB audit API | http://localhost:18083 |
| Mailpit inbox | http://localhost:18025 |

The demo sender connects only to the Mailpit endpoint supplied by Aspire. Mailpit accepts recipients ending in `@demo.invalid`. Forwarding and relay are disabled. Its inbox shows the SMTP message; the audit retains Bcc recipients even when the MIME headers omit them. The emulator credentials and Cosmos development certificate handling belong to the local demo.

## Explore the API in Scalar

Open [SQL](http://localhost:18081/scalar), [Blob](http://localhost:18082/scalar), or [Cosmos](http://localhost:18083/scalar). Each UI uses the Microsoft-generated document at `/openapi/v1.json` and calls its own API instance. In Scalar, send **POST /attempt-ids**, copy the returned ID into **PUT /attempts/{id}**, and send the prefilled receipt. Use **GET /attempts/{id}** to inspect it and open [Mailpit](http://localhost:18025) to see the captured message. To simulate delivery, replace the example attempt ID in **POST /delivery-events** with the same ID.

## Step through a request

The demo container publishes a Debug build with portable symbols for the API and libraries. In Visual Studio for Windows 18.10 or later, open the matching source checkout, choose **Debug > Attach to Process**, select **Podman** as the connection type, then select the API container for the backend you are exercising and its `dotnet` process. Set a breakpoint in `EmailSubmissionService.SubmitAsync` and send a PUT request through that backend’s Scalar page. The IDE must be able to reach the container engine. Podman discovery on a Windows workstation and access to a separate Mac’s Podman VM are different setups. See [Microsoft’s container attachment guide](https://learn.microsoft.com/en-us/visualstudio/debugger/attach-to-process-running-in-docker-container).

## Exercise the API

With the APIs ready, run the walkthrough from another terminal:

```sh
dotnet run demo/Walkthrough.cs --
```

To exercise one backend, append `sql`, `blobs`, or `cosmos`. Each run renders a receipt, submits it twice with the same ID, checks for one captured message, submits conflicting content, records separate To and Cc delivery events, repeats the callbacks, and looks up the original receipt. It also submits a synthetic address rejected by Mailpit and checks that a failed attempt has no acceptance timestamp. Six parallel submissions with different subjects then compete for one reference. The walkthrough checks that just one subject wins and exactly one message is captured. The program prints the ticket lookup URLs. Each run leaves its audit evidence in the local stores.

| API operation | Result |
|---|---|
| POST /attempt-ids | Generate an attempt ID before attaching it to a ticket. |
| POST /preview | Render a template key and string-valued model into a subject, HTML and plain text. |
| PUT /attempts/{id} | Submit final content using an optional application correlation ID. |
| GET /attempts/{id} | Retrieve the snapshot, SDK outcome, and all known recipient events. |
| POST /delivery-events | Record a simulated host callback using an attempt ID or transport and provider message ID. |

Look at [the walkthrough](demo/Walkthrough.cs) for the request and response examples. The delivery endpoint returns 422 for unresolved references so the host can retain them for reconciliation. Duplicate event identities return success with `appended=false`. The walkthrough simulates delayed provider events because Mailpit captures messages locally.

## Integrate in a host

Reference the core library and one store package. Implement `IEmailSender` using your SDK, assign a transport name scoped to the provider account, then register it with `AddSharpMail<TSender>()`. The adapter receives the attempt ID and finalized message. Return an SDK outcome and, if available, a provider message ID. The optional Liquid package provides `AddLiquidEmailTemplates(rootDirectory)`; any renderer that produces `RenderedContent` can feed the same submission service.

| Store registration | Host provisioning |
|---|---|
| AddSqlEmailAudit(connectionString) | Apply the included EF Core migrations using EmailAuditContext. The package uses an independent context with ambient enlistment disabled. |
| AddBlobEmailAudit(containerClient) | Supply an existing blob container. Snapshot, outcome, event and provider-ID lookup blobs have distinct keys. |
| AddCosmosEmailAudit(container) | Supply an existing container partitioned by /attemptId. Events and the attempt share a partition. The serialized attempt must fit the Cosmos item limit. |

The demo provisions the resources on startup. The SQL provider stores the complete versioned attempt as JSON in an EF-mapped row, with indexed transport and provider ID and separately keyed delivery rows. Recipient role and position identify the original recipient even when an address occurs more than once. All named audit volumes persist across normal Aspire stops. SharpMail sets no expiry and exposes no delete API.

## Submission semantics

- A storage error before the initial commit prevents the adapter call. An uncertain write can leave a Pending record. Reuse the same attempt ID for inspection.
- An exact repeat returns the recorded attempt. Equality includes sender and display names, reply-to, ordered recipient lists, subject, and both bodies. A changed correlation ID alone leaves the original correlation ID intact. Changed message values return a conflict.
- A successful call records Accepted and its UTC acceptance time. An exception or SDK rejection records Failed and a diagnostic code. A timeout can still leave the remote outcome uncertain.
- If the completion write fails, the caller receives the observed SDK outcome with `OutcomePersisted=false`; the demo returns HTTP 503. A later lookup may still be Pending. An intentional resend requires a new attempt ID. SharpMail performs no automatic resubmission.
- An authenticated host callback can supply a delivery event independently of SDK completion. A stable event ID and recipient position deduplicate repeat notifications. Use the echoed attempt ID for early callbacks; provider-ID lookup becomes available when the mapping has been stored.

Build the solution with `dotnet build`. The six library projects under `src` produce NuGet packages with `dotnet pack -c Release`. The demo API and Aspire AppHost consume these projects directly.
