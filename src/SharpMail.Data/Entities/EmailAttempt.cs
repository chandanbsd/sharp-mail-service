using SharpMail.Data.Models;

namespace SharpMail.Data.Entities;

public sealed record EmailAttempt(
    Guid AttemptId,
    string? CorrelationId,
    string Transport,
    EmailMessageInput Message,
    DateTimeOffset CreatedAtUtc,
    SubmissionStatus Status = SubmissionStatus.Pending,
    DateTimeOffset? CompletedAtUtc = null,
    DateTimeOffset? AcceptedBySdkAtUtc = null,
    string? ProviderMessageId = null,
    string? FailureCode = null,
    int SchemaVersion = 1)
{
    public EmailAttemptResult ToResult() => new(
        AttemptId,
        CorrelationId,
        Transport,
        Message,
        CreatedAtUtc,
        Status,
        CompletedAtUtc,
        AcceptedBySdkAtUtc,
        ProviderMessageId,
        FailureCode,
        SchemaVersion);
}
