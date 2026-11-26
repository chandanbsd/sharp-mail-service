namespace SharpMail.Context.AzureSqlServer.Entities;

internal sealed class EmailSubmission
{
    public Guid AttemptId { get; set; }
    public required string Transport { get; set; }
    public string? ProviderMessageId { get; set; }
    public required string Payload { get; set; }
}
