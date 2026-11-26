using SharpMail.Data;

namespace SharpMail.Core;

public interface IEmailSender
{
    string Transport { get; }
    Task<SdkOutcome> SubmitAsync(Guid attemptId, EmailMessage message, CancellationToken cancellationToken);
}
