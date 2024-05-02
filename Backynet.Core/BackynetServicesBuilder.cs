using Backynet.Core.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Backynet.Core;

public class BackynetServicesBuilder
{
    private readonly IServiceCollection _services;

    public BackynetServicesBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public virtual BackynetServicesBuilder TryAddCoreServices()
    {
        _services.TryAddSingleton<IBackynetContextServices, BackynetContextServices>();
        _services.TryAddSingleton<IBackynetContextOptions>(sp =>
        {
            var contextServices = sp.GetRequiredService<IBackynetContextServices>();
            return contextServices.CurrentContext.ContextOptions;
        });
        _services.TryAddSingleton<ISerializer, DefaultJsonSerializer>();
        _services.TryAddSingleton<IBackynetClient, BackynetClient>();
        _services.TryAddSingleton<IBackynetServer, BackynetServer>();
        _services.TryAddSingleton<IJobExecutor, JobExecutor>();
        _services.TryAddSingleton<IThreadPool, DefaultThreadPool>();
        _services.TryAddSingleton<IJobDescriptorExecutor, JobDescriptorExecutor>();

        return this;
    }
}