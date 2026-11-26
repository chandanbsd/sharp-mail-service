namespace SharpMail.Api.Models;

internal sealed record TemplateRenderInput(
    string TemplateKey,
    Dictionary<string, string> Model);
