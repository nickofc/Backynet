using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backynet.Core;

public class CoreOptionsExtension : IBackynetContextOptionsExtension
{
    private ILoggerFactory? _loggerFactory;
    private IServiceProvider _internalServiceProvider;
    
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

    public void Validate(IBackynetContextOptions options)
    {
    }

    public virtual ILoggerFactory? LoggerFactory => _loggerFactory;

    public virtual IServiceProvider? InternalServiceProvider => _internalServiceProvider;
    
    public virtual CoreOptionsExtension WithLoggerFactory(ILoggerFactory? loggerFactory)
    {
        var clone = Clone();

        clone._loggerFactory = loggerFactory;

        return clone;
    }

    public virtual CoreOptionsExtension WithInternalServiceProvider(IServiceProvider? serviceProvider)
    {
        var clone = Clone();

        clone._internalServiceProvider = _internalServiceProvider;

        return clone;
    }

    protected virtual CoreOptionsExtension Clone()
    {
        return new CoreOptionsExtension(this);
    }
}