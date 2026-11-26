using Microsoft.Extensions.DependencyInjection;

namespace SharpMail.Core;

public static class SharpMailRegistration
{
    public static IServiceCollection AddSharpMail<TSender>(this IServiceCollection services)
        where TSender : class, IEmailSender =>
        services.AddSingleton(TimeProvider.System).AddScoped<IEmailSender, TSender>()
            .AddScoped<EmailSubmission>().AddScoped<EmailAudit>();
}
