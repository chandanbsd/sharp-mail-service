using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharpMail.Data.Interfaces;
using SharpMail.Sink.AzureSQLServer.DataContexts;

namespace SharpMail.Sink.AzureSQLServer.Extensions;

public static class SqlAuditServiceCollectionExtensions
{
    public static IServiceCollection AddSqlEmailAudit(this IServiceCollection services, string connectionString)
    {
        var independent = new SqlConnectionStringBuilder(connectionString) { Enlist = false };
        return services.AddDbContextFactory<EmailAuditContext>(o => o.UseSqlServer(independent.ConnectionString))
            .AddSingleton<IEmailAuditDataContext, SqlAuditDataContext>();
    }
}
