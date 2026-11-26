using SharpMail.Data.Entities;

namespace SharpMail.Data.Interfaces;

public interface IEmailAuditDataContext
{
    Task<bool> TryCreateAsync(EmailAttempt attempt, CancellationToken cancellationToken);
    Task<EmailAttempt?> FindAsync(Guid attemptId, CancellationToken cancellationToken);
    Task CompleteAsync(EmailAttempt completed, CancellationToken cancellationToken);
    Task<IReadOnlyList<Guid>> FindByProviderIdAsync(string transport, string messageId, CancellationToken cancellationToken);
    Task<bool> TryAppendAsync(Guid attemptId, RecipientDeliveryEvent delivery, CancellationToken cancellationToken);
    Task<IReadOnlyList<RecipientDeliveryEvent>> ReadEventsAsync(Guid attemptId, CancellationToken cancellationToken);
}
