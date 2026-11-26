using SharpMail.Data.Models;

namespace SharpMail.Core.Services.Interfaces;

public interface IEmailAuditService
{
    Task<AuditDetail?> GetAsync(Guid id, CancellationToken token);

    Task<bool?> RecordAsync(
        Guid? attemptId,
        string? transport,
        string? providerMessageId,
        DeliveryNotification notification,
        CancellationToken token);
}
