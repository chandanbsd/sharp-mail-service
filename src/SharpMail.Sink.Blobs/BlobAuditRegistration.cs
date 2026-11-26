using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using SharpMail.Data;

namespace SharpMail.Sink.Blobs;

public static class BlobAuditRegistration
{
    public static IServiceCollection AddBlobEmailAudit(this IServiceCollection services, BlobContainerClient container) =>
        services.AddSingleton<IAuditStore>(new BlobAuditStore(container));
}
