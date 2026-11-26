using SharpMail.Data.Entities;

namespace SharpMail.Context.AzureCosmosDb.Entities;

internal sealed record EmailSubmissionDocument(
    EmailAttempt Attempt)
{
    public string Id => "attempt";
    public Guid AttemptId => Attempt.AttemptId;
}
