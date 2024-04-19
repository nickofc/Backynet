using Backynet.Postgresql;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static BackynetServerOptionsBuilder UsePostgreSql(
        this BackynetServerOptionsBuilder backynetConfigurationBuilder,
        Action<BackynetServerPostgreSqlOptionsBuilder> configure)
    {
        return backynetConfigurationBuilder;
    }
}

public class BackynetServerPostgreSqlOptionsBuilder
{
    internal BackynetServerPostgreSqlOptions Options { get; } = new();

    public BackynetServerPostgreSqlOptionsBuilder UseConnectionString(string connectionString)
    {
        Options.ConnectionString = connectionString;
        return this;
    }
}