using Backynet.Core.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Backynet.Core;

public interface IBackynetContextOptions
{
    IEnumerable<IBackynetContextOptionsExtension> Extensions { get; }

    IBackynetContextOptionsExtension? FindExtension<TExtension>()
        where TExtension : class, IBackynetContextOptionsExtension;
}

public abstract class BackynetContextOptions : IBackynetContextOptions
{
    private readonly IReadOnlyDictionary<Type, IBackynetContextOptionsExtension> _extensionsMap;

    protected BackynetContextOptions()
    {
        _extensionsMap = new Dictionary<Type, IBackynetContextOptionsExtension>();
        Extensions = _extensionsMap.Values;
    }

    public IEnumerable<IBackynetContextOptionsExtension> Extensions { get; } 

    public IBackynetContextOptionsExtension? FindExtension<TExtension>() 
        where TExtension : class, IBackynetContextOptionsExtension
    {
        return _extensionsMap.GetValueOrDefault(typeof(TExtension));
    }
    
    public abstract IBackynetContextOptions WithExtension<TExtension>(TExtension extension) 
        where TExtension : class, IBackynetContextOptionsExtension;
}

public class BackynetContextOptions<TContext>
{
}

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