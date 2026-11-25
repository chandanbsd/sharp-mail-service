using System.Text.Encodings.Web;
using Fluid;
using Microsoft.Extensions.DependencyInjection;

namespace SharpMail.Liquid;

public static class LiquidRegistration
{
    public static IServiceCollection AddLiquidEmailTemplates(this IServiceCollection services, string rootDirectory) =>
        services.AddSingleton<ITemplateRenderer>(new LiquidEmailRenderer(rootDirectory));
}

public sealed class LiquidEmailRenderer(string rootDirectory) : ITemplateRenderer
{
    private readonly FluidParser parser = new();
    private readonly TemplateOptions options = new TemplateOptionsBuilder().WithStrictVariables(true)
        .WithStrictFilters(true).WithMaxSteps(100_000).Build();

    public async Task<RenderedContent> RenderAsync(string templateKey, IReadOnlyDictionary<string, string> model,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateKey);
        ArgumentNullException.ThrowIfNull(model);
        if (templateKey.Split('/').Any(segment => segment.Length == 0 ||
            segment.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '-' and not '_')))
            throw new ArgumentException("Use alphanumeric template folders, hyphens or underscores.");
        var directory = Path.Combine(rootDirectory, templateKey);
        var subject = await RenderFileAsync("subject.liquid", null)
            ?? throw new ArgumentException("Template subject was not found.");
        subject = subject.TrimEnd('\r', '\n');
        if (string.IsNullOrWhiteSpace(subject) || subject.IndexOfAny(['\r', '\n']) >= 0)
            throw new ArgumentException("Rendered subject must occupy one line.");
        var html = await RenderFileAsync("body.html.liquid", HtmlEncoder.Default);
        var text = await RenderFileAsync("body.txt.liquid", null);
        if (html is null && text is null) throw new ArgumentException("Template requires a body.");
        return new(subject, html, text);

        async Task<string?> RenderFileAsync(string name, TextEncoder? encoder)
        {
            var path = Path.Combine(directory, name);
            if (!File.Exists(path)) return null;
            var source = await File.ReadAllTextAsync(path, cancellationToken);
            if (!parser.TryParse(source, out var template, out var parseError))
                throw new ArgumentException($"Invalid template {name}: {parseError}");
            var context = new TemplateContext(options);
            foreach (var entry in model) context.SetValue(entry.Key, entry.Value);
            try
            {
                return encoder is null
                    ? await template.RenderAsync(context)
                    : await template.RenderAsync(context, encoder);
            }
            catch (InvalidOperationException error)
            {
                throw new ArgumentException($"Could not render {name}: {error.Message}", error);
            }
        }
    }
}
