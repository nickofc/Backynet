using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Backynet.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddBackynetContext<TContextImplementation>(
        this IServiceCollection services,
        Action<BackynetContextOptionsBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services;
    }
}