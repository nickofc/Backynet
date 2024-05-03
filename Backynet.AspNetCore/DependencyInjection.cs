using Backynet.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

// ReSharper disable once CheckNamespace
namespace Backynet.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddBackynetContext<TContext>(
        this IServiceCollection services,
        Action<IServiceProvider, BackynetContextOptionsBuilder> optionsAction) where TContext : BackynetContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(optionsAction);

        services.TryAddSingleton(typeof(IBackynetContextOptionsConfiguration<TContext>),
            _ => new BackynetContextOptionsConfiguration<TContext>(optionsAction));

        services.TryAddSingleton(typeof(BackynetContextOptions<TContext>), CreateContextOptions<TContext>);
        services.TryAddSingleton(typeof(BackynetContextOptions), sp => sp.GetRequiredService<BackynetContextOptions<TContext>>());

        services.TryAddSingleton(typeof(TContext));

        services.TryAddSingleton<IHostedService>(sp =>
        {
            var backynetContext = sp.GetRequiredService<TContext>();
            return new BackynetServerHostedService<TContext>(backynetContext.Server);
        });

        return services;
    }

    private static BackynetContextOptions CreateContextOptions<TContext>(
        IServiceProvider applicationServiceProvider)
        where TContext : BackynetContext
    {
        var builder = new BackynetContextOptionsBuilder(new BackynetContextOptions<TContext>());

        builder.UseApplicationServiceProvider(applicationServiceProvider);

        foreach (var configuration in applicationServiceProvider.GetServices<IBackynetContextOptionsConfiguration<TContext>>())
        {
            configuration.Configure(applicationServiceProvider, builder);
        }

        return builder.Options;
    }
}