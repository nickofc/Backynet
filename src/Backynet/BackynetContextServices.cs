using Backynet.Options;

namespace Backynet;

public class BackynetContextServices : IBackynetContextServices
{
    public IBackynetContextServices Initialize(
        IServiceProvider serviceProvider, 
        BackynetContextOptions optionsBuilderOptions, 
        BackynetContext backynetContext)
    {
        InternalServiceProvider = serviceProvider;

        CurrentContext = new CurrentContext { BackynetContext = backynetContext, ContextOptions = optionsBuilderOptions };
        
        return this;
    }

    public ICurrentBackynetContext CurrentContext { get; private set; } = null!;
    public IServiceProvider InternalServiceProvider { get; private set; } = null!;
}

public class CurrentContext : ICurrentBackynetContext
{
    public BackynetContext BackynetContext { get; internal set; } = null!;
    public BackynetContextOptions ContextOptions { get; internal set; } = null!;
}