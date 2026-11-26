using SharpMail.Data.Models;

namespace SharpMail.Data.Interfaces;

public interface IEmailAuditDataContext
{
    Task<bool> TryCreateAsync(EmailAttemptResult attempt, CancellationToken cancellationToken);
    Task<EmailAttemptResult?> FindAsync(Guid attemptId, CancellationToken cancellationToken);
    Task CompleteAsync(EmailAttemptResult completed, CancellationToken cancellationToken);
    Task<IReadOnlyList<Guid>> FindByProviderIdAsync(string transport, string messageId, CancellationToken cancellationToken);
    Task<bool> TryAppendAsync(Guid attemptId, RecordedDeliveryEventResult delivery, CancellationToken cancellationToken);
    Task<IReadOnlyList<RecordedDeliveryEventResult>> ReadEventsAsync(Guid attemptId, CancellationToken cancellationToken);
}
