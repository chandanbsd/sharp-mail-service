namespace SharpMail.Core.Models;

public sealed record EmailSendResult(
    bool Accepted,
    string? ProviderMessageId = null,
    string? FailureCode = null);
