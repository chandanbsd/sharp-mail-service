using Microsoft.EntityFrameworkCore;
using SharpMail.Context.AzureSqlServer.Entities;

namespace SharpMail.Context.AzureSqlServer;

public sealed class EmailAuditContext(
    DbContextOptions<EmailAuditContext> options) : DbContext(options)
{
    internal DbSet<EmailSubmission> Attempts => Set<EmailSubmission>();
    internal DbSet<RecipientDeliveryEvent> Deliveries => Set<RecipientDeliveryEvent>();
    protected override void OnModelCreating(ModelBuilder model)
    {
        var attempts = model.Entity<EmailSubmission>().ToTable("SharpMailAttempts");
        attempts.HasKey(a => a.AttemptId);
        attempts.Property(a => a.Transport).HasMaxLength(128);
        attempts.Property(a => a.ProviderMessageId).HasMaxLength(256);
        attempts.HasIndex(a => new { a.Transport, a.ProviderMessageId });
        var events = model.Entity<RecipientDeliveryEvent>().ToTable("SharpMailDeliveries");
        events.HasKey(e => new { e.AttemptId, e.EventKey });
        events.Property(e => e.EventKey).HasMaxLength(64);
        events.HasOne<EmailSubmission>().WithMany().HasForeignKey(e => e.AttemptId);
    }
}
