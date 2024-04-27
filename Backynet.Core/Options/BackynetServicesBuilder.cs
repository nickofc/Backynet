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
        _services.TryAddScoped<IJobExecutor, JobExecutor>();
        _services.TryAddScoped<IThreadPool, DefaultThreadPool>();

        return this;
    }
}