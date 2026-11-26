namespace SharpMail.Core;

public sealed record SdkOutcome(
    bool Accepted,
    string? ProviderMessageId = null,
    string? FailureCode = null);
