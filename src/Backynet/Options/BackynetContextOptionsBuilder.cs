using Microsoft.Extensions.Logging;

namespace Backynet.Options;

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

    public virtual BackynetContextOptionsBuilder UseApplicationServiceProvider(IServiceProvider serviceProvider)
    {
        return WithOption(x => x.WithApplicationServiceProvider(serviceProvider));
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