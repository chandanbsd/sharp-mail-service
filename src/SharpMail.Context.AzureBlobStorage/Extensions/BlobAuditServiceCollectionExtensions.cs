using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using SharpMail.Context.AzureBlobStorage.DataContexts;
using SharpMail.Data.Interfaces;

namespace SharpMail.Context.AzureBlobStorage.Extensions;

public static class BlobAuditServiceCollectionExtensions
{
    public static IServiceCollection AddBlobEmailAudit(this IServiceCollection services, BlobContainerClient container) =>
        services.AddSingleton<IEmailAuditDataContext>(new AzureBlobStorageDataContext(container));
}
