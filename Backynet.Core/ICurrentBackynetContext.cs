namespace Backynet.Core;

public interface ICurrentBackynetContext
{
    BackynetContext BackynetContext { get; }
    BackynetContextOptions ContextOptions { get; }
}