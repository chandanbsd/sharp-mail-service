using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SharpMail.Data;
using SharpMail.Data.Interfaces;
using SharpMail.Data.Models;

namespace SharpMail.Sink.AzureSQLServer.DataContexts;

internal sealed class SqlAuditDataContext(
    IDbContextFactory<EmailAuditContext> contexts) : IEmailAuditDataContext
{
    public async Task<bool> TryCreateAsync(EmailAttempt attempt, CancellationToken token)
    {
        await using var db = await contexts.CreateDbContextAsync(token);
        db.Attempts.Add(new() { AttemptId = attempt.AttemptId, Transport = attempt.Transport, Payload = AuditJson.Write(attempt) });
        return await InsertAsync(db, token);
    }

    public async Task<EmailAttempt?> FindAsync(Guid attemptId, CancellationToken token)
    {
        await using var db = await contexts.CreateDbContextAsync(token);
        var payload = await db.Attempts.Where(a => a.AttemptId == attemptId).Select(a => a.Payload).SingleOrDefaultAsync(token);
        return payload is null ? null : AuditJson.Read<EmailAttempt>(payload);
    }

    public async Task CompleteAsync(EmailAttempt completed, CancellationToken token)
    {
        await using var db = await contexts.CreateDbContextAsync(token);
        var payload = AuditJson.Write(completed);
        var updated = await db.Attempts.Where(a => a.AttemptId == completed.AttemptId)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.Payload, payload)
                .SetProperty(a => a.ProviderMessageId, completed.ProviderMessageId), token);
        if (updated != 1) throw new InvalidOperationException("Attempt missing during completion.");
    }

    public async Task<IReadOnlyList<Guid>> FindByProviderIdAsync(string transport, string messageId, CancellationToken token)
    {
        await using var db = await contexts.CreateDbContextAsync(token);
        return await db.Attempts.Where(a => EF.Functions.Collate(a.Transport, "Latin1_General_100_BIN2") == transport
                && EF.Functions.Collate(a.ProviderMessageId!, "Latin1_General_100_BIN2") == messageId)
            .Select(a => a.AttemptId).Take(2).ToListAsync(token);
    }

    public async Task<bool> TryAppendAsync(Guid attemptId, DeliveryEvent delivery, CancellationToken token)
    {
        await using var db = await contexts.CreateDbContextAsync(token);
        db.Deliveries.Add(new() { AttemptId = attemptId, EventKey = DeliveryIdentity.Key(delivery.Notification), Payload = AuditJson.Write(delivery) });
        return await InsertAsync(db, token);
    }

    public async Task<IReadOnlyList<DeliveryEvent>> ReadEventsAsync(Guid attemptId, CancellationToken token)
    {
        await using var db = await contexts.CreateDbContextAsync(token);
        var rows = await db.Deliveries.Where(e => e.AttemptId == attemptId).Select(e => e.Payload).ToListAsync(token);
        return rows.Select(AuditJson.Read<DeliveryEvent>).ToArray();
    }

    private static async Task<bool> InsertAsync(EmailAuditContext db, CancellationToken token)
    {
        try { await db.SaveChangesAsync(token); return true; }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 2601 or 2627 }) { return false; }
    }
}
