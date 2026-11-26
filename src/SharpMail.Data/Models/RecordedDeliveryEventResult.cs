namespace SharpMail.Data.Models;

public sealed record RecordedDeliveryEventResult(
    DeliveryNotificationInput Notification,
    DateTimeOffset RecordedAtUtc);
