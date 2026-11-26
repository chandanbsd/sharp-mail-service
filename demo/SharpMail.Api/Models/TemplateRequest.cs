namespace SharpMail.Api.Models;

internal sealed record TemplateRequest(
    string TemplateKey,
    Dictionary<string, string> Model);
