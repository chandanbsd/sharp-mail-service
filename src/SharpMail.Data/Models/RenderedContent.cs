namespace SharpMail.Data.Models;

public sealed record RenderedContent(
    string Subject,
    string? HtmlBody,
    string? TextBody);
