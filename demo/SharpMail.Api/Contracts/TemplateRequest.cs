namespace SharpMail.Api.Contracts;

internal sealed record TemplateRequest(string TemplateKey, Dictionary<string, string> Model);
