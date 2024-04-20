using Backynet.Postgresql;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static BackynetServerOptionsBuilder UsePostgreSql(
        this BackynetServerOptionsBuilder backynetConfigurationBuilder,
        Action<BackynetServerPostgreSqlOptionsBuilder> configure)
    {
        var backynetServerPostgreSqlOptionsBuilder = new BackynetServerPostgreSqlOptionsBuilder(backynetConfigurationBuilder.Services);
        configure(backynetServerPostgreSqlOptionsBuilder);
        
        return backynetConfigurationBuilder;
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