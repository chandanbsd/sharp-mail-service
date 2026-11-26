using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using SharpMail.Context.AzureBlobStorage.Extensions;
using SharpMail.Context.AzureCosmosDb.Extensions;
using SharpMail.Context.AzureSqlServer;
using SharpMail.Context.AzureSqlServer.Extensions;

namespace SharpMail.Api;

internal static class DemoStorage
{
    public static Func<IServiceProvider, CancellationToken, Task> Configure(WebApplicationBuilder builder)
    {
        Uri Endpoint() => new Uri(builder.Configuration["StoreEndpoint"] ?? throw new InvalidOperationException("StoreEndpoint missing."));
        switch (builder.Configuration["AuditProvider"])
        {
            case "sql":
                builder.Services.AddSqlEmailAudit(builder.Configuration.GetConnectionString("SharpMail")!);
                return async (services, token) =>
                {
                    var factory = services.GetRequiredService<IDbContextFactory<EmailAuditContext>>();
                    await using var db = await factory.CreateDbContextAsync(token);
                    await db.Database.MigrateAsync(token);
                };
            case "blobs":
                var container = new BlobContainerClient(new Uri(Endpoint(), "/sharpmail/email-audit"),
                    new StorageSharedKeyCredential("sharpmail", builder.Configuration["StorageKey"]!));
                builder.Services.AddBlobEmailAudit(container);
                return async (_, token) => await container.CreateIfNotExistsAsync(cancellationToken: token);
            case "cosmos":
                var options = new CosmosClientOptions
                {
                    ConnectionMode = ConnectionMode.Gateway,
                    LimitToEndpoint = true,
                    HttpClientFactory = () => new HttpClient(new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    })
                };
                var client = new CosmosClient(Endpoint().ToString(), Convert.ToBase64String("local-emulator"u8), options);
                builder.Services.AddSingleton(_ => client);
                builder.Services.AddCosmosEmailAudit(client.GetContainer("SharpMail", "attempts"));
                return async (_, token) =>
                {
                    var database = await client.CreateDatabaseIfNotExistsAsync("SharpMail", cancellationToken: token);
                    await database.Database.CreateContainerIfNotExistsAsync("attempts", "/attemptId", cancellationToken: token);
                };
            default: throw new InvalidOperationException("Choose sql, blobs or cosmos.");
        }
    }
}
