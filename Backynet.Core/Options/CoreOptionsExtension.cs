using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backynet.Core;

public class CoreOptionsExtension : IBackynetContextOptionsExtension
{
    private ILoggerFactory? _loggerFactory;

    // todo: osobny modul? 
    private int? _maxThreads;
    private string? _serverName;
    private TimeSpan? _poolingInterval;
    private TimeSpan? _heartbeatInterval;
    private TimeSpan? _maxTimeWithoutHeartbeat;

    public CoreOptionsExtension()
    {
        _maxThreads = Environment.ProcessorCount;
        _serverName = Environment.MachineName;
        _poolingInterval = TimeSpan.FromSeconds(5);
        _heartbeatInterval = TimeSpan.FromSeconds(10);
        _maxTimeWithoutHeartbeat = TimeSpan.FromMinutes(1);
    }

    protected CoreOptionsExtension(CoreOptionsExtension copyFrom)
    {
        _loggerFactory = copyFrom._loggerFactory;
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
    }

    public virtual ILoggerFactory? LoggerFactory => _loggerFactory;

    public virtual CoreOptionsExtension WithLoggerFactory(ILoggerFactory? loggerFactory)
    {
        var clone = Clone();

        clone._loggerFactory = loggerFactory;

        return clone;
    }

    public virtual int? MaxThreads => _maxThreads;

    public virtual CoreOptionsExtension WithMaxThreads(int? value)
    {
        var clone = Clone();

        clone._maxThreads = value;

        return clone;
    }

    public virtual TimeSpan? PoolingInterval => _poolingInterval;

    public virtual CoreOptionsExtension WithPoolingInterval(TimeSpan? value)
    {
        var clone = Clone();

        clone._poolingInterval = value;

        return clone;
    }

    public virtual string? ServerName => _serverName;

    public virtual CoreOptionsExtension WithServerName(string? value)
    {
        var clone = Clone();

        clone._serverName = value;

        return clone;
    }

    public virtual TimeSpan? HeartbeatInterval => _heartbeatInterval;

    public virtual CoreOptionsExtension WithHeartbeatInterval(TimeSpan? value)
    {
        var clone = Clone();

        clone._heartbeatInterval = value;

        return clone;
    }

    public virtual TimeSpan? MaxTimeWithoutHeartbeat => _maxTimeWithoutHeartbeat;

    public virtual CoreOptionsExtension WithMaxTimeWithoutHeartbeat(TimeSpan? value)
    {
        var clone = Clone();

        clone._maxTimeWithoutHeartbeat = value;

        return clone;
    }

    protected virtual CoreOptionsExtension Clone()
    {
        return new CoreOptionsExtension(this);
    }
}