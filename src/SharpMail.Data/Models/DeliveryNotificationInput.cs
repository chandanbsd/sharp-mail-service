namespace SharpMail.Data.Models;

public sealed record DeliveryNotificationInput(
    string EventId,
    string RecipientRole,
    int RecipientIndex,
    DeliveryKind Kind,
    DateTimeOffset OccurredAtUtc,
    string? Reason = null);
