namespace SharpMail.Data.Models;

public sealed record TemplateRenderResult(
    string Subject,
    string? HtmlBody,
    string? TextBody);
