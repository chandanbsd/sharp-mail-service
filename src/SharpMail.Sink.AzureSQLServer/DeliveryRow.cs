namespace SharpMail.Sink.AzureSQLServer;

internal sealed class DeliveryRow
{
    public Guid AttemptId { get; set; }
    public required string EventKey { get; set; }
    public required string Payload { get; set; }
}
