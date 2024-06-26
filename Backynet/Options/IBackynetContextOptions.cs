namespace Backynet.Options;

public interface IBackynetContextOptions
{
    IEnumerable<IBackynetContextOptionsExtension> Extensions { get; }

    TExtension? FindExtension<TExtension>() where TExtension : class, IBackynetContextOptionsExtension;
}