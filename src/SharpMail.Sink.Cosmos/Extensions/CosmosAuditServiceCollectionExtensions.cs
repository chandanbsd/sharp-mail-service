using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using SharpMail.Data;
using SharpMail.Data.Interfaces;
using SharpMail.Sink.Cosmos.DataContexts;

namespace SharpMail.Sink.Cosmos.Extensions;

public static class CosmosAuditServiceCollectionExtensions
{
    public static IServiceCollection AddCosmosEmailAudit(this IServiceCollection services, Container container) =>
        services.AddSingleton<IEmailAuditDataContext>(new CosmosAuditDataContext(container));
}
