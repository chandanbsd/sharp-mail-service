using Microsoft.Extensions.DependencyInjection;
using SharpMail.Core;

namespace SharpMail.Renderer.Liquid;

public static class LiquidRegistration
{
    public static IServiceCollection AddLiquidEmailTemplates(this IServiceCollection services, string rootDirectory) =>
        services.AddSingleton<ITemplateRenderer>(new LiquidEmailRenderer(rootDirectory));
}
