using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using SharpMail.Data;
using SharpMail.Data.Interfaces;
using SharpMail.Sink.Blobs.DataContexts;

namespace SharpMail.Sink.Blobs.Extensions;

public static class BlobAuditServiceCollectionExtensions
{
    public static IServiceCollection AddBlobEmailAudit(this IServiceCollection services, BlobContainerClient container) =>
        services.AddSingleton<IEmailAuditDataContext>(new BlobAuditDataContext(container));
}
