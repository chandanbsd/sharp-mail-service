using SharpMail.Data.Models;

namespace SharpMail.Core.Models;

public sealed record EmailSubmissionResult(
    EmailAttemptResult Attempt,
    bool Reused,
    bool OutcomePersisted);
