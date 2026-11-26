using SharpMail.Core.Models;
using SharpMail.Data.Models;

namespace SharpMail.Core.Services.Interfaces;

public interface IEmailSender
{
    string Transport { get; }
    Task<EmailSendResult> SubmitAsync(Guid attemptId, EmailMessageInput message, CancellationToken cancellationToken);
}
