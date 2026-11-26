using SharpMail.Data;
using SharpMail.Data.Models;

namespace SharpMail.Api.Models;

internal sealed record DeliveryRequest(
    Guid? AttemptId,
    string? Transport,
    string? ProviderMessageId,
    DeliveryNotification Notification);
