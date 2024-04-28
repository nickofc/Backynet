using Microsoft.Extensions.Logging;

namespace Backynet.Core;

public class BackynetContextOptionsBuilder : IDbContextOptionsBuilderInfrastructure
{
    private BackynetContextOptions _options;

    public BackynetContextOptionsBuilder(BackynetContextOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
    }

    public virtual BackynetContextOptions Options => _options;

    public virtual BackynetContextOptionsBuilder UseLoggerFactory(ILoggerFactory? loggerFactory)
        => WithOption(e => e.WithLoggerFactory(loggerFactory));

    private BackynetContextOptionsBuilder WithOption(Func<CoreOptionsExtension, CoreOptionsExtension> withFunc)
    {
        ((IDbContextOptionsBuilderInfrastructure)this).AddOrUpdateExtension(
            withFunc(Options.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension()));

        return this;
    }

    public void AddOrUpdateExtension<TExtension>(TExtension extension) where TExtension : class, IBackynetContextOptionsExtension
    {
        throw new NotImplementedException();
    }
}