using System.Collections.Immutable;

namespace Backynet.Options;

public abstract class BackynetContextOptions : IBackynetContextOptions
{
    private readonly ImmutableSortedDictionary<Type, (IBackynetContextOptionsExtension Extension, int Ordinal)> _extensionsMap;

    protected BackynetContextOptions()
    {
        _extensionsMap = ImmutableSortedDictionary.Create<Type, (IBackynetContextOptionsExtension, int)>(TypeFullNameComparer.Instance);
    }

    protected BackynetContextOptions(ImmutableSortedDictionary<Type, (IBackynetContextOptionsExtension Extension, int Ordinal)> extensions)
    {
        _extensionsMap = extensions;
    }

    public virtual IEnumerable<IBackynetContextOptionsExtension> Extensions
        => _extensionsMap.Values.OrderBy(v => v.Ordinal).Select(v => v.Extension);

    public virtual TExtension? FindExtension<TExtension>()
        where TExtension : class, IBackynetContextOptionsExtension
        => _extensionsMap.TryGetValue(typeof(TExtension), out var value) ? (TExtension)value.Extension : null;

    protected virtual ImmutableSortedDictionary<Type, (IBackynetContextOptionsExtension Extension, int Ordinal)> ExtensionsMap => _extensionsMap;
    public abstract BackynetContextOptions WithExtension<TExtension>(TExtension extension) where TExtension : class, IBackynetContextOptionsExtension;

    public abstract Type ContextType { get; }
}