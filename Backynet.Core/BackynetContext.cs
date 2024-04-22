using Backynet.Core.Abstraction;

namespace Backynet.Core;

public class BackynetContext
{
    public IBackynetServer Server { get; }
    public IBackynetClient Client { get; }

    public virtual void OnConfiguring(BackynetServerOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseServerName("");
    }
}