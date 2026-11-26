namespace SharpMail.Data.Models;

public sealed record Mailbox(
    string Address,
    string? DisplayName = null);
