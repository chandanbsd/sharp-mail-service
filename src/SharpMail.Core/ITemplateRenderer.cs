using SharpMail.Data;

namespace SharpMail.Core;

public interface ITemplateRenderer
{
    Task<RenderedContent> RenderAsync(string templateKey, IReadOnlyDictionary<string, string> model,
        CancellationToken cancellationToken);
}
