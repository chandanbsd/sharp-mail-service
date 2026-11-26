using SharpMail.Data;
using SharpMail.Data.Models;

namespace SharpMail.Api.Models;

internal sealed record SubmitRequest(
    string? CorrelationId,
    EmailMessage Message);
