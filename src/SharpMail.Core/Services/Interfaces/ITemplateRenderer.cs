using SharpMail.Data.Models;

namespace SharpMail.Core.Services.Interfaces;

public interface ITemplateRenderer
{
    Task<TemplateRenderResult> RenderAsync(string templateKey, IReadOnlyDictionary<string, string> model,
        CancellationToken cancellationToken);
}
