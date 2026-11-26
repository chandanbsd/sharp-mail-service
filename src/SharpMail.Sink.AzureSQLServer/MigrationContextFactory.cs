using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SharpMail.Sink.AzureSQLServer;

internal sealed class MigrationContextFactory : IDesignTimeDbContextFactory<EmailAuditContext>
{
    public EmailAuditContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<EmailAuditContext>().UseSqlServer().Options);
}
