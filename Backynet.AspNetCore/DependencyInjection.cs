using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

// ReSharper disable once CheckNamespace
namespace Backynet.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddBackynetContext<TContextImplementation>(
        this IServiceCollection services,
        Action<BackynetContextOptionsBuilder>? configure = null) where TContextImplementation : BackynetContext
    {
        ArgumentNullException.ThrowIfNull(services);

        var optionsBuilder = new BackynetContextOptionsBuilder(new BackynetContextOptions<TContextImplementation>());
        configure?.Invoke(optionsBuilder);

        services.TryAddSingleton(optionsBuilder.Options);
        services.TryAddSingleton(typeof(TContextImplementation));

        services.TryAddSingleton<IHostedService>(sp =>
        {
            var backynetContext = sp.GetRequiredService<TContextImplementation>();
            return new BackynetServerHostedService(backynetContext.Server);
        });

        return services;
    }
}