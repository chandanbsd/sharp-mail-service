using SharpMail.Core.Models;
using SharpMail.Data.Models;

namespace SharpMail.Core.Services.Interfaces;

public interface IEmailSubmissionService
{
    Guid NewAttemptId();

    Task<SubmissionResult> SubmitAsync(
        Guid attemptId,
        string? correlationId,
        EmailMessage message,
        CancellationToken cancellationToken = default);
}
