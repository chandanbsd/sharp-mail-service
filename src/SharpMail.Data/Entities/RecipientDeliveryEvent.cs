using SharpMail.Data.Models;

namespace SharpMail.Data.Entities;

public sealed record RecipientDeliveryEvent(
    DeliveryNotificationInput Notification,
    DateTimeOffset RecordedAtUtc)
{
    public RecordedDeliveryEventResult ToResult() => new(
        Notification,
        RecordedAtUtc);
}
