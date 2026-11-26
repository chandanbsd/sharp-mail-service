using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharpMail.Context.AzureSqlServer.DataContexts;
using SharpMail.Data.Interfaces;

namespace SharpMail.Context.AzureSqlServer.Extensions;

public static class SqlAuditServiceCollectionExtensions
{
    public static IServiceCollection AddSqlEmailAudit(this IServiceCollection services, string connectionString)
    {
        var independent = new SqlConnectionStringBuilder(connectionString) { Enlist = false };
        return services.AddDbContextFactory<EmailAuditContext>(o => o.UseSqlServer(independent.ConnectionString))
            .AddSingleton<IEmailAuditDataContext, AzureSqlServerDataContext>();
    }
}
