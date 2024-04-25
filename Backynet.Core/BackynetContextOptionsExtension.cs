using Backynet.Core.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Backynet.Core;

public class BackynetContextOptionsExtension : IBackynetContextOptionsExtension
{
    public void ApplyServices(IServiceCollection services)
    {
        services.TryAddSingleton<IJobExecutor, JobExecutor>();
        services.TryAddSingleton<IThreadPool, DefaultThreadPool>();
        services.TryAddSingleton<IJobDescriptorExecutor, JobDescriptorExecutor>();
        services.TryAddSingleton<ISerializer, DefaultJsonSerializer>();
    }
}