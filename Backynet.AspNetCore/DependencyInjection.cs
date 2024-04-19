using Backynet.AspNetCore;
using Backynet.Core;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddBackynet(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddBackynetClient(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddBackynetServer(this IServiceCollection services,
        Action<BackynetServerOptionsBuilder> configure)
    {
        var builder = new BackynetServerOptionsBuilder();

        configure(builder);

        return services;
    }

    public static IServiceCollection AddBackynetServer(this IServiceCollection services,
        Action<IServiceProvider, BackynetServerOptions> configure)
    {
        services.AddHostedService<BackynetServerHostedService>(sp =>
        {
            var options = new BackynetServerOptions();
            configure.Invoke(sp, options);

            throw new NotImplementedException();
        });

        return services;
    }
}

public class BackynetServerOptionsBuilder
{
    internal BackynetServerOptions Options { get; } = new();

    public BackynetServerOptionsBuilder UseMaximumConcurrencyThreads(int maximumConcurrencyThreads)
    {
        Options.MaxThreads = maximumConcurrencyThreads;
        return this;
    }

    public BackynetServerOptionsBuilder UseServerName(string serverName)
    {
        Options.ServerName = serverName;
        return this;
    }
}