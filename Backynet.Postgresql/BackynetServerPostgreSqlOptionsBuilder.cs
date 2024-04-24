using Backynet.Postgresql;
using Microsoft.Extensions.DependencyInjection;

namespace Backynet.PostgreSql;

public class BackynetServerPostgreSqlOptionsBuilder
{
    public IServiceCollection Services { get; }

    public BackynetServerPostgreSqlOptionsBuilder(IServiceCollection services)
    {
        Services = services;
    }

    internal BackynetServerPostgreSqlOptions Options { get; } = new();

    public BackynetServerPostgreSqlOptionsBuilder UseConnectionString(string connectionString)
    {
        Options.ConnectionString = connectionString;
        return this;
    }
}