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

    private BackynetContextOptionsBuilder WithOption(Func<CoreOptionsExtension, CoreOptionsExtension> withFunc)
    {
        ((IBackynetContextOptionsBuilderInfrastructure)this).AddOrUpdateExtension(
            withFunc(Options.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension()));

        return this;
    }

    void IBackynetContextOptionsBuilderInfrastructure.AddOrUpdateExtension<TExtension>(TExtension extension)
    {
        _options.WithExtension(extension);
    }
}