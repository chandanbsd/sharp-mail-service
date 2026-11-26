using Microsoft.Extensions.DependencyInjection;
using SharpMail.Core.Services.Interfaces;
using SharpMail.Renderer.Liquid.Services;

namespace SharpMail.Renderer.Liquid.Extensions;

public static class LiquidServiceCollectionExtensions
{
    public static IServiceCollection AddLiquidEmailTemplates(this IServiceCollection services, string rootDirectory) =>
        services.AddSingleton<ITemplateRenderer>(new LiquidEmailRenderer(rootDirectory));
}
