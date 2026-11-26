using SharpMail.Data.Models;

namespace SharpMail.Api.Models;

internal sealed record EmailSubmissionInput(
    string? CorrelationId,
    EmailMessageInput Message);
