using Backynet.Options;

namespace Backynet;

public interface IBackynetContextServices
{
    IBackynetContextServices Initialize(IServiceProvider serviceProvider, BackynetContextOptions optionsBuilderOptions, BackynetContext backynetContext);
    ICurrentBackynetContext CurrentContext { get; }
    IServiceProvider InternalServiceProvider { get; }
}