namespace SharpMail.Data;

public sealed record RenderedContent(
    string Subject,
    string? HtmlBody,
    string? TextBody);
