using SharpMail.Core.Models;
using SharpMail.Data.Models;

namespace SharpMail.Core.Services.Interfaces;

public interface IEmailSubmissionService
{
    Guid NewAttemptId();

    Task<EmailSubmissionResult> SubmitAsync(
        Guid attemptId,
        string? correlationId,
        EmailMessageInput message,
        CancellationToken cancellationToken = default);
}
