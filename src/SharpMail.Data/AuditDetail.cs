namespace SharpMail.Data;

public sealed record AuditDetail(EmailAttempt Attempt, IReadOnlyList<DeliveryEvent> DeliveryEvents);
