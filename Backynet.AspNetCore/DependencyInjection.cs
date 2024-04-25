using Backynet.AspNetCore;
using Backynet.Core;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddBackynetClient(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddBackynetServer(this IServiceCollection services,
        Action<BackynetServerOptionsBuilder> configure)
    {
        return services;
    }

    public static IServiceCollection AddBackynetServer(this IServiceCollection services,
        Action<IServiceProvider, BackynetServerOptions> configure)
    {
        services.AddHostedService<BackynetServerHostedService>(sp =>
        {
            var options = new BackynetServerOptions();
            configure.Invoke(sp, options);

            // todo: build services and return hosted service 
            
            return new BackynetServerHostedService(null);
        });

        return services;
    }
}