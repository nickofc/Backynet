using Backynet.Abstraction;
using Backynet.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Backynet;

public class BackynetServicesBuilder
{
    private readonly IServiceCollection _services;

    public BackynetServicesBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public virtual BackynetServicesBuilder TryAddCoreServices()
    {
        _services.TryAddSingleton<ISystemClock, SystemClock>();
        _services.TryAddSingleton<ILoggerFactory>(sp =>
        {
            var contextOptions = sp.GetRequiredService<IBackynetContextOptions>();
            var coreOptionsExtension = contextOptions.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension();

            return coreOptionsExtension.LoggerFactory ?? new NullLoggerFactory();
        });
        _services.TryAddSingleton(typeof(ILogger<>), typeof(Logger<>));
        _services.TryAddSingleton<IBackynetContextServices, BackynetContextServices>();
        _services.TryAddSingleton<IBackynetContextOptions>(sp =>
        {
            var contextServices = sp.GetRequiredService<IBackynetContextServices>();
            return contextServices.CurrentContext.ContextOptions;
        });
        _services.TryAddSingleton<ISerializer, MessagePackSerializerProvider>();
        _services.TryAddSingleton<IBackynetClient, BackynetClient>();
        //_services.TryAddSingleton<IBackynetServer, BackynetServer>();
        _services.TryAddSingleton<IJobExecutor, JobExecutor>();

        _services.TryAddSingleton<IJobDescriptorExecutor>((sp) =>
        {
            var contextOptions = sp.GetRequiredService<IBackynetContextOptions>();
            var coreOptionsExtension = contextOptions.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension();

            if (coreOptionsExtension.ApplicationServiceProvider != null)
            {
                return new ScopedJobDescriptorExecutor(coreOptionsExtension.ApplicationServiceProvider);
            }

            return new JobDescriptorExecutor();
        });

        _services.TryAddSingleton<ITransactionScopeFactory, NullTransactionScopeFactory>();
        
        return this;
    }
}