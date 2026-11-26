namespace SharpMail.Data.Models;

public sealed record AuditDetail(
    EmailAttempt Attempt,
    IReadOnlyList<DeliveryEvent> DeliveryEvents);
