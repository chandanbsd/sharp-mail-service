namespace SharpMail.Data.Models;

public sealed record EmailAuditResult(
    EmailAttemptResult Attempt,
    IReadOnlyList<RecordedDeliveryEventResult> DeliveryEvents);
