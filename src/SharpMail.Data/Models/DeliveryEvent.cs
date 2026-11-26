namespace SharpMail.Data.Models;

public sealed record DeliveryEvent(
    DeliveryNotification Notification,
    DateTimeOffset RecordedAtUtc);
