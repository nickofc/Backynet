namespace Backynet.Core;

public interface IBackynetContextOptions
{
    IEnumerable<IBackynetContextOptionsExtension> Extensions { get; }

    IBackynetContextOptionsExtension? FindExtension<TExtension>() where TExtension : class, IBackynetContextOptionsExtension;
}