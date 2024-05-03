using Backynet.Options;

namespace Backynet;

public interface ICurrentBackynetContext
{
    BackynetContext BackynetContext { get; }
    BackynetContextOptions ContextOptions { get; }
}