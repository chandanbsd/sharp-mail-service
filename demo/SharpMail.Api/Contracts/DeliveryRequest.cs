using SharpMail.Data;

namespace SharpMail.Api.Contracts;

internal sealed record DeliveryRequest(
    Guid? AttemptId,
    string? Transport,
    string? ProviderMessageId,
    DeliveryNotification Notification);
