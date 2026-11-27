using SharpMail.Data;
using SharpMail.Data.Entities;

namespace SharpMail.Context.AzureCosmosDb.Entities;

internal sealed record RecipientDeliveryEventDocument(
    Guid AttemptId,
    RecipientDeliveryEvent Delivery)
{
    public string Id => DeliveryIdentity.Key(Delivery.Notification);
}
