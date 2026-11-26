using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using SharpMail.Context.AzureCosmosDb.DataContexts;
using SharpMail.Data.Interfaces;

namespace SharpMail.Context.AzureCosmosDb.Extensions;

public static class CosmosAuditServiceCollectionExtensions
{
    public static IServiceCollection AddCosmosEmailAudit(this IServiceCollection services, Container container) =>
        services.AddSingleton<IEmailAuditDataContext>(new AzureCosmosDbDataContext(container));
}
