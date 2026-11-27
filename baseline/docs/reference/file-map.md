# File map
covers: finding the runtime, storage adapters, demo and repository guidance

## Folders

| Path | What it holds | Explained in |
|---|---|---|
| `src/SharpMail.Core/` | Submission and lookup services, host interfaces and registration | [Architecture](../architecture/README.md) |
| `src/SharpMail.Data/` | Shared audit entities, validation and persistence contract | [History](../course/03-history.md) |
| `src/SharpMail.Context.AzureSqlServer/` | EF Core mappings, SQL persistence and migrations | [Architecture](../architecture/README.md) |
| `src/SharpMail.Context.AzureBlobStorage/` | Conditional blob creation and provider lookup blobs | [Architecture](../architecture/README.md) |
| `src/SharpMail.Context.AzureCosmosDb/` | Partitioned documents and cross-partition provider lookup | [Architecture](../architecture/README.md) |
| `src/SharpMail.Renderer.Liquid/` | File-based rendering | [Receipt preview](../course/01-preview.md) |
| `demo/` | Scripted walkthrough and shared demo build settings | [Submission](../course/02-submit.md) |
| `demo/SharpMail.Api/` | API routes, receipt templates, storage startup and Mailpit adapter | [Features](../features/README.md) |
| `demo/SharpMail.AppHost/` | Aspire topology, configuration and launch profiles | [Deployment](../deploy/README.md) |
| `.github/workflows/` | Baseline validation workflow | [Deployment](../deploy/README.md) |
| `.agents/skills/` | Repository baseline workflows | [Baseline](../../baseline.md) |
| `baseline/` | Standards, decisions, documentation and checks | [Baseline](../../baseline.md) |
| `docs/` | Original design review | [Design review](../../../docs/design.md) |
