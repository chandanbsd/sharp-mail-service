using System.Security.Cryptography;
using System.Text;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SharpMail.Data;
using SharpMail.Data.Interfaces;
using SharpMail.Data.Models;

namespace SharpMail.Context.AzureBlobStorage.DataContexts;

internal sealed class AzureBlobStorageDataContext(
    BlobContainerClient container) : IEmailAuditDataContext
{
    private BlobClient Snapshot(Guid id) => container.GetBlobClient($"attempts/{id:D}/snapshot");
    private BlobClient Outcome(Guid id) => container.GetBlobClient($"attempts/{id:D}/outcome");

    public Task<bool> TryCreateAsync(EmailAttemptResult attempt, CancellationToken token) =>
        CreateAsync(Snapshot(attempt.AttemptId), attempt, token);

    public async Task<EmailAttemptResult?> FindAsync(Guid attemptId, CancellationToken token) =>
        await ReadAsync<EmailAttemptResult>(Outcome(attemptId), token) ?? await ReadAsync<EmailAttemptResult>(Snapshot(attemptId), token);

    public async Task CompleteAsync(EmailAttemptResult completed, CancellationToken token)
    {
        if (completed.ProviderMessageId is { } messageId)
        {
            var index = container.GetBlobClient($"{ProviderPrefix(completed.Transport, messageId)}{completed.AttemptId:D}");
            await CreateAsync(index, completed.AttemptId, token);
        }
        if (!await CreateAsync(Outcome(completed.AttemptId), completed, token))
            throw new InvalidOperationException("Attempt already completed.");
    }

    private static string ProviderPrefix(string transport, string messageId) =>
        $"providers/{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(AuditJson.Write(new[] { transport, messageId }))))}/";

    public async Task<IReadOnlyList<Guid>> FindByProviderIdAsync(string transport, string messageId, CancellationToken token)
    {
        var matches = new List<Guid>();
        await foreach (var blob in container.GetBlobsAsync(new GetBlobsOptions { Prefix = ProviderPrefix(transport, messageId) }, token))
        {
            matches.Add(Guid.Parse(Path.GetFileName(blob.Name)));
            if (matches.Count == 2) break;
        }
        return matches;
    }

    public Task<bool> TryAppendAsync(Guid attemptId, RecordedDeliveryEventResult delivery, CancellationToken token) =>
        CreateAsync(container.GetBlobClient($"attempts/{attemptId:D}/events/{DeliveryIdentity.Key(delivery.Notification)}"), delivery, token);

    public async Task<IReadOnlyList<RecordedDeliveryEventResult>> ReadEventsAsync(Guid attemptId, CancellationToken token)
    {
        var events = new List<RecordedDeliveryEventResult>();
        await foreach (var blob in container.GetBlobsAsync(new GetBlobsOptions { Prefix = $"attempts/{attemptId:D}/events/" }, token))
        {
            var content = await container.GetBlobClient(blob.Name).DownloadContentAsync(token);
            events.Add(AuditJson.Read<RecordedDeliveryEventResult>(content.Value.Content.ToString()));
        }
        return events;
    }

    private static async Task<bool> CreateAsync<T>(BlobClient blob, T item, CancellationToken token)
    {
        try
        {
            await blob.UploadAsync(BinaryData.FromString(AuditJson.Write(item)),
                new BlobUploadOptions { Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All } }, token);
            return true;
        }
        catch (RequestFailedException e) when (e.ErrorCode is "BlobAlreadyExists" or "ConditionNotMet") { return false; }
    }

    private static async Task<T?> ReadAsync<T>(BlobClient blob, CancellationToken token) where T : class
    {
        try { return AuditJson.Read<T>((await blob.DownloadContentAsync(token)).Value.Content.ToString()); }
        catch (RequestFailedException e) when (e.ErrorCode == "BlobNotFound") { return null; }
    }
}
