using SharpMail.Core.Exceptions;
using SharpMail.Core.Models;
using SharpMail.Core.Services.Interfaces;
using SharpMail.Data;
using SharpMail.Data.Entities;
using SharpMail.Data.Interfaces;
using SharpMail.Data.Models;

namespace SharpMail.Core.Services;

internal sealed class EmailSubmissionService(
    IEmailAuditDataContext dataContext,
    IEmailSender sender,
    TimeProvider clock) : IEmailSubmissionService
{
    public Guid NewAttemptId() => Guid.CreateVersion7();

    public async Task<EmailSubmissionResult> SubmitAsync(Guid attemptId, string? correlationId, EmailMessageInput message,
        CancellationToken cancellationToken = default)
    {
        if (attemptId == Guid.Empty) throw new ArgumentException("An attempt ID is required.");
        ArgumentNullException.ThrowIfNull(message);
        message.Validate();
        var attempt = new EmailAttempt(attemptId, correlationId, sender.Transport, message, clock.GetUtcNow());
        if (!await dataContext.TryCreateAsync(attempt, cancellationToken))
        {
            var existing = await dataContext.FindAsync(attemptId, cancellationToken)
                ?? throw new InvalidOperationException("The existing audit record could not be read.");
            if (AuditJson.Write(existing.Message) != AuditJson.Write(message))
                throw new AttemptConflictException(attemptId);
            return new(existing.ToResult(), Reused: true, OutcomePersisted: true);
        }

        EmailSendResult outcome;
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
            await dataContext.CompleteAsync(completed, CancellationToken.None);
            return new(completed.ToResult(), Reused: false, OutcomePersisted: true);
        }
        catch (Exception)
        {
            return new(completed.ToResult(), Reused: false, OutcomePersisted: false);
        }
    }
}
