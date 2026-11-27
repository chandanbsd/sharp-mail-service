using SharpMail.Core.Services.Interfaces;
using SharpMail.Data.Interfaces;
using SharpMail.Data.Models;

namespace SharpMail.Core.Services;

internal sealed class EmailAuditService(
    IEmailAuditDataContext dataContext,
    TimeProvider clock) : IEmailAuditService
{
    public async Task<EmailAuditResult?> GetAsync(Guid id, CancellationToken token)
    {
        var attempt = await dataContext.FindAsync(id, token);
        if (attempt is null) return null;
        var events = await dataContext.ReadEventsAsync(id, token);
        var history = events.OrderBy(e => e.Notification.OccurredAtUtc).Select(e => e.ToResult()).ToArray();
        return new(attempt.ToResult(), history);
    }

    public async Task<bool?> RecordAsync(Guid? attemptId, string? transport, string? providerMessageId,
        DeliveryNotificationInput notification, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(notification);
        ArgumentException.ThrowIfNullOrWhiteSpace(notification.EventId);
        if (!Enum.IsDefined(notification.Kind) || notification.OccurredAtUtc == default)
            throw new ArgumentException("Supply a delivery kind and occurrence time.");
        if (attemptId is null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(transport);
            ArgumentException.ThrowIfNullOrWhiteSpace(providerMessageId);
            var matches = await dataContext.FindByProviderIdAsync(transport, providerMessageId, token);
            if (matches.Count != 1) return null;
            attemptId = matches[0];
        }
        var attempt = await dataContext.FindAsync(attemptId.Value, token);
        if (attempt is null) return null;
        var recipients = notification.RecipientRole switch
        {
            "To" => attempt.Message.To,
            "Cc" => attempt.Message.Cc,
            "Bcc" => attempt.Message.Bcc,
            _ => throw new ArgumentException("Unknown recipient role.")
        };
        if ((uint)notification.RecipientIndex >= recipients.Length)
            throw new ArgumentException("Recipient index is outside the stored list.");
        var normalized = notification with { OccurredAtUtc = notification.OccurredAtUtc.ToUniversalTime() };
        return await dataContext.TryAppendAsync(attemptId.Value, new(normalized, clock.GetUtcNow()), token);
    }
}
