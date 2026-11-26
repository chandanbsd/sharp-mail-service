using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using SharpMail.Data;

namespace SharpMail.Sink.Cosmos;

public static class CosmosAuditRegistration
{
    public static IServiceCollection AddCosmosEmailAudit(this IServiceCollection services, Container container) =>
        services.AddSingleton<IAuditStore>(new CosmosAuditStore(container));
}
