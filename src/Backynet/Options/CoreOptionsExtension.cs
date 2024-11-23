using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backynet.Options;

public class CoreOptionsExtension : IBackynetContextOptionsExtension
{
    private ILoggerFactory? _loggerFactory;
    private IServiceProvider? _applicationServiceProvider;
    private int _maxThreads = Environment.ProcessorCount;
    private string _serverName = Environment.MachineName;

    public CoreOptionsExtension()
    {
    }

    protected CoreOptionsExtension(CoreOptionsExtension copyFrom)
    {
        _loggerFactory = copyFrom._loggerFactory;
        _applicationServiceProvider = copyFrom._applicationServiceProvider;
        _maxThreads = copyFrom._maxThreads;
        _serverName = copyFrom._serverName;
    }

    public virtual void ApplyServices(IServiceCollection services)
    {
    }

    public void Validate(IBackynetContextOptions options)
    {
        // todo: walidacja czy konfiguracje jest poprawna
    }

    public virtual ILoggerFactory? LoggerFactory => _loggerFactory;

    public virtual CoreOptionsExtension WithLoggerFactory(ILoggerFactory? loggerFactory)
    {
        var clone = Clone();

        clone._loggerFactory = loggerFactory;

        return clone;
    }

    public virtual IServiceProvider? ApplicationServiceProvider => _applicationServiceProvider;

    public virtual CoreOptionsExtension WithApplicationServiceProvider(IServiceProvider serviceProvider)
    {
        var clone = Clone();

        clone._applicationServiceProvider = serviceProvider;

        return clone;
    }

    public virtual int MaxThreads => _maxThreads;

    public virtual CoreOptionsExtension WithMaxThreads(int value)
    {
        var clone = Clone();

        clone._maxThreads = value;

        return clone;
    }

    public virtual string ServerName => _serverName;

    public virtual CoreOptionsExtension WithServerName(string serverName)
    {
        var clone = Clone();

        clone._serverName = serverName;

        return clone;
    }

    protected virtual CoreOptionsExtension Clone()
    {
        return new CoreOptionsExtension(this);
    }
}