namespace SharpMail.Data.Models;

public sealed record EmailAttempt(
    Guid AttemptId,
    string? CorrelationId,
    string Transport,
    EmailMessage Message,
    DateTimeOffset CreatedAtUtc,
    SubmissionStatus Status = SubmissionStatus.Pending,
    DateTimeOffset? CompletedAtUtc = null,
    DateTimeOffset? AcceptedBySdkAtUtc = null,
    string? ProviderMessageId = null,
    string? FailureCode = null,
    int SchemaVersion = 1);
