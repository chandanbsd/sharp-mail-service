using SharpMail.Data;

namespace SharpMail.Core;

public sealed record SubmissionResult(
    EmailAttempt Attempt,
    bool Reused,
    bool OutcomePersisted);
