using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using SharpMail.Data;

namespace SharpMail.Sink.Cosmos;

internal sealed class CosmosAuditStore(Container container) : IAuditStore
{
    public Task<bool> TryCreateAsync(EmailAttempt attempt, CancellationToken token) =>
        CreateAsync(attempt.AttemptId, new { id = "attempt", attemptId = attempt.AttemptId, attempt }, token);

    public async Task<EmailAttempt?> FindAsync(Guid attemptId, CancellationToken token)
    {
        using var response = await container.ReadItemStreamAsync("attempt", Partition(attemptId), cancellationToken: token);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        using var document = await JsonDocument.ParseAsync(response.Content, cancellationToken: token);
        return document.RootElement.GetProperty("attempt").Deserialize<EmailAttempt>(AuditJson.Options);
    }

    public async Task CompleteAsync(EmailAttempt completed, CancellationToken token)
    {
        using var body = Serialize(new { id = "attempt", attemptId = completed.AttemptId, attempt = completed });
        using var response = await container.ReplaceItemStreamAsync(body, "attempt", Partition(completed.AttemptId), cancellationToken: token);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<Guid>> FindByProviderIdAsync(string transport, string messageId, CancellationToken token)
    {
        var query = new QueryDefinition("SELECT TOP 2 VALUE c.attemptId FROM c WHERE c.id = 'attempt' AND c.attempt.transport = @transport AND c.attempt.providerMessageId = @id")
            .WithParameter("@transport", transport).WithParameter("@id", messageId);
        return await QueryAsync<Guid>(query, null, token);
    }

    public Task<bool> TryAppendAsync(Guid attemptId, DeliveryEvent delivery, CancellationToken token) =>
        CreateAsync(attemptId, new { id = DeliveryIdentity.Key(delivery.Notification), attemptId, delivery }, token);

    public Task<IReadOnlyList<DeliveryEvent>> ReadEventsAsync(Guid attemptId, CancellationToken token) =>
        QueryAsync<DeliveryEvent>(new QueryDefinition("SELECT VALUE c.delivery FROM c WHERE IS_DEFINED(c.delivery)"),
            new QueryRequestOptions { PartitionKey = Partition(attemptId) }, token);

    private async Task<IReadOnlyList<T>> QueryAsync<T>(QueryDefinition query, QueryRequestOptions? options, CancellationToken token)
    {
        var results = new List<T>();
        using var pages = container.GetItemQueryStreamIterator(query, requestOptions: options);
        while (pages.HasMoreResults)
        {
            using var response = await pages.ReadNextAsync(token);
            response.EnsureSuccessStatusCode();
            using var page = await JsonDocument.ParseAsync(response.Content, cancellationToken: token);
            foreach (var item in page.RootElement.GetProperty("Documents").EnumerateArray())
                results.Add(item.Deserialize<T>(AuditJson.Options)!);
        }
        return results;
    }

    private async Task<bool> CreateAsync<T>(Guid id, T document, CancellationToken token)
    {
        using var body = Serialize(document);
        using var response = await container.CreateItemStreamAsync(body, Partition(id), cancellationToken: token);
        if (response.StatusCode == HttpStatusCode.Conflict) return false;
        response.EnsureSuccessStatusCode();
        return true;
    }

    private static MemoryStream Serialize<T>(T item) => new(Encoding.UTF8.GetBytes(AuditJson.Write(item)));
    private static PartitionKey Partition(Guid id) => new(id.ToString("D"));
}
