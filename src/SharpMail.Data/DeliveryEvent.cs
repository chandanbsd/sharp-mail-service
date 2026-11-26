namespace SharpMail.Data;

public sealed record DeliveryEvent(
    DeliveryNotification Notification,
    DateTimeOffset RecordedAtUtc);
