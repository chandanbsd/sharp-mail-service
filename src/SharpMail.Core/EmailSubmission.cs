using SharpMail.Data;

namespace SharpMail.Core;

public sealed class EmailSubmission(IAuditStore store, IEmailSender sender, TimeProvider clock)
{
    public static Guid NewAttemptId() => Guid.CreateVersion7();

    public async Task<SubmissionResult> SubmitAsync(Guid attemptId, string? correlationId, EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        if (attemptId == Guid.Empty) throw new ArgumentException("An attempt ID is required.");
        ArgumentNullException.ThrowIfNull(message);
        message.Validate();
        var attempt = new EmailAttempt(attemptId, correlationId, sender.Transport, message, clock.GetUtcNow());
        if (!await store.TryCreateAsync(attempt, cancellationToken))
        {
            var existing = await store.FindAsync(attemptId, cancellationToken)
                ?? throw new InvalidOperationException("The existing audit record could not be read.");
            if (AuditJson.Write(existing.Message) != AuditJson.Write(message))
                throw new AttemptConflictException(attemptId);
            return new(existing, Reused: true, OutcomePersisted: true);
        }

        SdkOutcome outcome;
        try
        {
            outcome = await sender.SubmitAsync(attemptId, message, cancellationToken);
        }
        catch (Exception error)
        {
            outcome = new(false, FailureCode: error.GetType().Name);
        }

        var completedAt = clock.GetUtcNow();
        var completed = attempt with
        {
            Status = outcome.Accepted ? SubmissionStatus.Accepted : SubmissionStatus.Failed,
            CompletedAtUtc = completedAt,
            AcceptedBySdkAtUtc = outcome.Accepted ? completedAt : null,
            ProviderMessageId = outcome.ProviderMessageId,
            FailureCode = outcome.FailureCode
        };
        try
        {
            await store.CompleteAsync(completed, CancellationToken.None);
            return new(completed, Reused: false, OutcomePersisted: true);
        }
        catch (Exception)
        {
            return new(completed, Reused: false, OutcomePersisted: false);
        }
    }
}
