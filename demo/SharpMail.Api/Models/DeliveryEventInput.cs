using SharpMail.Data.Models;

namespace SharpMail.Api.Models;

internal sealed record DeliveryEventInput(
    Guid? AttemptId,
    string? Transport,
    string? ProviderMessageId,
    DeliveryNotificationInput Notification);
