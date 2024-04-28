using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backynet.Core;

public class CoreOptionsExtension : IBackynetContextOptionsExtension
{
    private ILoggerFactory? _loggerFactory;

    public CoreOptionsExtension()
    {
        
    }
    
    protected CoreOptionsExtension(CoreOptionsExtension copyFrom)
    {
        _loggerFactory = copyFrom._loggerFactory;
    }

    public virtual void ApplyServices(IServiceCollection services)
    {
    }
    
    public virtual CoreOptionsExtension WithLoggerFactory(ILoggerFactory? loggerFactory)
    {
        var clone = Clone();

        clone._loggerFactory = loggerFactory;

        return clone;
    }

    protected virtual CoreOptionsExtension Clone()
    {
        return new CoreOptionsExtension(this);
    }
}