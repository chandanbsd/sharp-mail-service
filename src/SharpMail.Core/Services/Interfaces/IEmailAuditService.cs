using SharpMail.Data.Models;

namespace SharpMail.Core.Services.Interfaces;

public interface IEmailAuditService
{
    Task<EmailAuditResult?> GetAsync(Guid id, CancellationToken token);

    Task<bool?> RecordAsync(
        Guid? attemptId,
        string? transport,
        string? providerMessageId,
        DeliveryNotificationInput notification,
        CancellationToken token);
}
