using SharpMail.Data;

namespace SharpMail.Api.Contracts;

internal sealed record SubmitRequest(
    string? CorrelationId,
    EmailMessage Message);
