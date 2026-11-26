using SharpMail.Data.Models;
namespace SharpMail.Data.Interfaces;

public interface IEmailAuditDataContext
{
    Task<bool> TryCreateAsync(EmailAttempt attempt, CancellationToken cancellationToken);
    Task<EmailAttempt?> FindAsync(Guid attemptId, CancellationToken cancellationToken);
    Task CompleteAsync(EmailAttempt completed, CancellationToken cancellationToken);
    Task<IReadOnlyList<Guid>> FindByProviderIdAsync(string transport, string messageId, CancellationToken cancellationToken);
    Task<bool> TryAppendAsync(Guid attemptId, DeliveryEvent delivery, CancellationToken cancellationToken);
    Task<IReadOnlyList<DeliveryEvent>> ReadEventsAsync(Guid attemptId, CancellationToken cancellationToken);
}
