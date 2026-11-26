using SharpMail.Core.Models;
using SharpMail.Data;
using SharpMail.Data.Models;

namespace SharpMail.Core.Services.Interfaces;

public interface IEmailSender
{
    string Transport { get; }
    Task<SdkOutcome> SubmitAsync(Guid attemptId, EmailMessage message, CancellationToken cancellationToken);
}
