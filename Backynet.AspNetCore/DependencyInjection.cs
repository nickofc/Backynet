using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

// ReSharper disable once CheckNamespace
namespace Backynet.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddBackynetContext<TContext>(
        this IServiceCollection services,
        Action<IServiceProvider, BackynetContextOptionsBuilder> configure) where TContext : BackynetContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.TryAddSingleton(typeof(IBackynetContextOptionsConfiguration<TContext>),
            _ => new BackynetContextOptionsConfiguration<TContext>(configure));
        
        services.TryAddSingleton(typeof(BackynetContextOptions<TContext>), CreateDbContextOptions<TContext>);
        
        services.TryAddSingleton(typeof(TContext));

        services.TryAddSingleton<IHostedService>(sp =>
        {
            var backynetContext = sp.GetRequiredService<TContext>();
            return new BackynetServerHostedService<TContext>(backynetContext.Server);
        });

        return services;
    }
    
    private static BackynetContextOptions CreateDbContextOptions<TContext>(
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
