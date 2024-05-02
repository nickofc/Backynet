using Microsoft.Extensions.Logging;

namespace Backynet.Core;

public class BackynetContextOptionsBuilder : IBackynetContextOptionsBuilderInfrastructure
{
    private BackynetContextOptions _options;

    public BackynetContextOptionsBuilder() : this(new BackynetContextOptions<BackynetContext>())
    {
    }

    public BackynetContextOptionsBuilder(BackynetContextOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
    }

    public virtual BackynetContextOptions Options => _options;

    public virtual BackynetContextOptionsBuilder UseLoggerFactory(ILoggerFactory? loggerFactory)
    {
        return WithOption(x => x.WithLoggerFactory(loggerFactory));
    }

    public virtual BackynetContextOptionsBuilder UseMaxThreads(int maxThreads)
    {
        return WithOption(x => x.WithMaxThreads(maxThreads));
    }

    public virtual BackynetContextOptionsBuilder UseServerName(string serverName)
    {
        return WithOption(x => x.WithServerName(serverName));
    }

    public virtual BackynetContextOptionsBuilder UsePoolingInterval(TimeSpan poolingInterval)
    {
        return WithOption(x => x.WithPoolingInterval(poolingInterval));
    }

    public virtual BackynetContextOptionsBuilder UseHeartbeatInterval(TimeSpan value)
    {
        return WithOption(x => x.WithHeartbeatInterval(value));
    }

    public virtual BackynetContextOptionsBuilder UseMaxTimeWithoutHeartbeat(TimeSpan value)
    {
        return WithOption(x => x.WithMaxTimeWithoutHeartbeat(value));
    }

    private BackynetContextOptionsBuilder WithOption(Func<CoreOptionsExtension, CoreOptionsExtension> withFunc)
    {
        ((IBackynetContextOptionsBuilderInfrastructure)this).AddOrUpdateExtension(
            withFunc(Options.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension()));

        return this;
    }

    void IBackynetContextOptionsBuilderInfrastructure.AddOrUpdateExtension<TExtension>(TExtension extension)
    {
        _options = _options.WithExtension(extension);
    }
}