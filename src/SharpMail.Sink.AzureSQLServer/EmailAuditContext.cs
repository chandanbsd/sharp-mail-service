using Microsoft.EntityFrameworkCore;
using SharpMail.Sink.AzureSQLServer.Models;

namespace SharpMail.Sink.AzureSQLServer;

public sealed class EmailAuditContext(
    DbContextOptions<EmailAuditContext> options) : DbContext(options)
{
    internal DbSet<AttemptRow> Attempts => Set<AttemptRow>();
    internal DbSet<DeliveryRow> Deliveries => Set<DeliveryRow>();
    protected override void OnModelCreating(ModelBuilder model)
    {
        var attempts = model.Entity<AttemptRow>().ToTable("SharpMailAttempts");
        attempts.HasKey(a => a.AttemptId);
        attempts.Property(a => a.Transport).HasMaxLength(128);
        attempts.Property(a => a.ProviderMessageId).HasMaxLength(256);
        attempts.HasIndex(a => new { a.Transport, a.ProviderMessageId });
        var events = model.Entity<DeliveryRow>().ToTable("SharpMailDeliveries");
        events.HasKey(e => new { e.AttemptId, e.EventKey });
        events.Property(e => e.EventKey).HasMaxLength(64);
        events.HasOne<AttemptRow>().WithMany().HasForeignKey(e => e.AttemptId);
    }
}
