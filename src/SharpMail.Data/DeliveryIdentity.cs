using System.Security.Cryptography;
using System.Text;
using SharpMail.Data.Models;

namespace SharpMail.Data;

public static class DeliveryIdentity
{
    public static string Key(DeliveryNotification notification) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(AuditJson.Write(new
        {
            notification.RecipientRole,
            notification.RecipientIndex,
            notification.EventId
        }))));
}
