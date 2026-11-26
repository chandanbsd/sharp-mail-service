namespace SharpMail.Data;

public sealed record DeliveryNotification(string EventId, string RecipientRole, int RecipientIndex,
    DeliveryKind Kind, DateTimeOffset OccurredAtUtc, string? Reason = null);
