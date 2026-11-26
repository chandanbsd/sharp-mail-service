using Microsoft.Extensions.DependencyInjection;
using SharpMail.Core.Services;
using SharpMail.Core.Services.Interfaces;

namespace SharpMail.Core.Extensions;

public static class SharpMailServiceCollectionExtensions
{
    public static IServiceCollection AddSharpMail<TSender>(this IServiceCollection services)
        where TSender : class, IEmailSender =>
        services.AddSingleton(TimeProvider.System).AddScoped<IEmailSender, TSender>()
            .AddScoped<IEmailSubmissionService, EmailSubmissionService>()
            .AddScoped<IEmailAuditService, EmailAuditService>();
}
