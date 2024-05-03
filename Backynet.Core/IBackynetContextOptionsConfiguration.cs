namespace Backynet.Core;

public interface IBackynetContextOptionsConfiguration<TContext> where TContext : BackynetContext
{
    void Configure(IServiceProvider serviceProvider, BackynetContextOptionsBuilder optionsBuilder);
}

public class BackynetContextOptionsConfiguration<TContext> : IBackynetContextOptionsConfiguration<TContext> where TContext : BackynetContext
{
    private readonly Action<IServiceProvider, BackynetContextOptionsBuilder> _configure;

    public BackynetContextOptionsConfiguration(Action<IServiceProvider, BackynetContextOptionsBuilder> configure)
    {
        _configure = configure;
    }

    public virtual void Configure(IServiceProvider serviceProvider, BackynetContextOptionsBuilder optionsBuilder)
        => _configure(serviceProvider, optionsBuilder);
}