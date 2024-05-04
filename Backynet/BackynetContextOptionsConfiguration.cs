using Backynet.Options;

namespace Backynet;

public sealed class BackynetContextOptionsConfiguration<TContext> : IBackynetContextOptionsConfiguration<TContext> where TContext : BackynetContext
{
    private readonly Action<IServiceProvider, BackynetContextOptionsBuilder> _configure;

    public BackynetContextOptionsConfiguration(Action<IServiceProvider, BackynetContextOptionsBuilder> configure)
    {
        _configure = configure;
    }

    public void Configure(IServiceProvider serviceProvider, BackynetContextOptionsBuilder optionsBuilder)
    {
        _configure(serviceProvider, optionsBuilder);
    }
}