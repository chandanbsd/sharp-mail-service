using SharpMail.Data;
using SharpMail.Data.Models;

namespace SharpMail.Core.Models;

public sealed record SubmissionResult(
    EmailAttempt Attempt,
    bool Reused,
    bool OutcomePersisted);
