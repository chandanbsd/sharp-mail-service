namespace SharpMail.Sink.AzureSQLServer.Models;

internal sealed class AttemptRow
{
    public Guid AttemptId { get; set; }
    public required string Transport { get; set; }
    public string? ProviderMessageId { get; set; }
    public required string Payload { get; set; }
}
