namespace Backynet.Core;

public interface IBackynetContextServices
{
    IBackynetContextServices Initialize(IServiceProvider serviceProvider);

    ICurrentBackynetContext CurrentContext { get; }
    IServiceProvider InternalServiceProvider { get; }
}