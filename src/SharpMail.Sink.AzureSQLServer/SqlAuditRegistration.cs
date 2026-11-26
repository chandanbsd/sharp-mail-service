using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharpMail.Data;

namespace SharpMail.Sink.AzureSQLServer;

public static class SqlAuditRegistration
{
    public static IServiceCollection AddSqlEmailAudit(this IServiceCollection services, string connectionString)
    {
        var independent = new SqlConnectionStringBuilder(connectionString) { Enlist = false };
        return services.AddDbContextFactory<EmailAuditContext>(o => o.UseSqlServer(independent.ConnectionString))
            .AddSingleton<IAuditStore, SqlAuditStore>();
    }
}
