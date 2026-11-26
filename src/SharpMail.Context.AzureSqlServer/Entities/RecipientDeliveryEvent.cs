namespace SharpMail.Context.AzureSqlServer.Entities;

internal sealed class RecipientDeliveryEvent
{
    public Guid AttemptId { get; set; }
    public required string EventKey { get; set; }
    public required string Payload { get; set; }
}
