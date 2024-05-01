namespace Backynet.Core;

public class BackynetContextServices : IBackynetContextServices
{
    public IBackynetContextServices Initialize(
        IServiceProvider serviceProvider, 
        BackynetContextOptions optionsBuilderOptions, 
        BackynetContext backynetContext)
    {
        InternalServiceProvider = serviceProvider;
        
        return this;
    }

    public ICurrentBackynetContext CurrentContext { get; }
    public IServiceProvider InternalServiceProvider { get; private set; }
}