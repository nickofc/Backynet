using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backynet.Options;

public class CoreOptionsExtension : IBackynetContextOptionsExtension
{
    private ILoggerFactory? _loggerFactory;
    private IServiceProvider? _applicationServiceProvider;

    // todo: osobny modul? 
    private int _maxThreads = Environment.ProcessorCount;
    private string _serverName = Environment.MachineName;
    private TimeSpan _poolingInterval = TimeSpan.FromSeconds(2);
    private TimeSpan _heartbeatInterval = TimeSpan.FromSeconds(5);
    private TimeSpan _maxTimeWithoutHeartbeat = TimeSpan.FromSeconds(60);
    private TimeSpan _watchdogPoolingInterval = TimeSpan.FromSeconds(5);

    public CoreOptionsExtension()
    {
    }

    protected CoreOptionsExtension(CoreOptionsExtension copyFrom)
    {
        _loggerFactory = copyFrom._loggerFactory;
        _applicationServiceProvider = copyFrom._applicationServiceProvider;

        _maxThreads = copyFrom._maxThreads;
        _serverName = copyFrom._serverName;
        _poolingInterval = copyFrom._poolingInterval;
        _heartbeatInterval = copyFrom._heartbeatInterval;
        _maxTimeWithoutHeartbeat = copyFrom._maxTimeWithoutHeartbeat;
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

    public virtual TimeSpan PoolingInterval => _poolingInterval;

    public virtual CoreOptionsExtension WithPoolingInterval(TimeSpan poolingInterval)
    {
        var clone = Clone();

        clone._poolingInterval = poolingInterval;

        return clone;
    }

    public virtual string ServerName => _serverName;

    public virtual CoreOptionsExtension WithServerName(string serverName)
    {
        var clone = Clone();

        clone._serverName = serverName;

        return clone;
    }

    public virtual TimeSpan HeartbeatInterval => _heartbeatInterval;

    public virtual CoreOptionsExtension WithHeartbeatInterval(TimeSpan heartbeatInterval)
    {
        var clone = Clone();

        clone._heartbeatInterval = heartbeatInterval;

        return clone;
    }

    public virtual TimeSpan MaxTimeWithoutHeartbeat => _maxTimeWithoutHeartbeat;

    public virtual CoreOptionsExtension WithMaxTimeWithoutHeartbeat(TimeSpan maxTimeWithoutHeartbeat)
    {
        var clone = Clone();

        clone._maxTimeWithoutHeartbeat = maxTimeWithoutHeartbeat;

        return clone;
    }

    public virtual TimeSpan WatchdogPoolingInterval => _watchdogPoolingInterval;

    protected virtual CoreOptionsExtension Clone()
    {
        return new CoreOptionsExtension(this);
    }
}