using Backynet.Options;

namespace Backynet;

public interface IBackynetContextOptionsConfiguration<TContext> where TContext : BackynetContext
{
    void Configure(IServiceProvider serviceProvider, BackynetContextOptionsBuilder optionsBuilder);
}