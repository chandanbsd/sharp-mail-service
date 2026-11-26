namespace SharpMail.Data.Models;

public sealed record MailboxInput(
    string Address,
    string? DisplayName = null);
