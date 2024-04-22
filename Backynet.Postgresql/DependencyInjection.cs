using Backynet.Core;
using Backynet.Postgresql;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static BackynetServerOptionsBuilder UsePostgreSql(
        this BackynetServerOptionsBuilder optionsBuilder,
        Action<BackynetServerPostgreSqlOptionsBuilder> configure)
    {
        return optionsBuilder;
    }
}

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