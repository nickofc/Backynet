using System.Collections.Immutable;

namespace Backynet.Core;

public abstract class BackynetContextOptions : IBackynetContextOptions
{
    private readonly ImmutableSortedDictionary<Type, (IBackynetContextOptionsExtension Extension, int Ordinal)> _extensionsMap;

    protected BackynetContextOptions()
    {
        _extensionsMap = ImmutableSortedDictionary.Create<Type, (IBackynetContextOptionsExtension, int)>();
    }

    protected BackynetContextOptions(ImmutableSortedDictionary<Type, (IBackynetContextOptionsExtension Extension, int Ordinal)> extensions)
    {
        _extensionsMap = extensions;
    }

    protected BackynetContextOptions(IReadOnlyDictionary<Type, IBackynetContextOptionsExtension> extensions)
    {
        _extensionsMap = ImmutableSortedDictionary.Create<Type, (IBackynetContextOptionsExtension, int)>()
            .AddRange(extensions.Select((p, i) => new KeyValuePair<Type, (IBackynetContextOptionsExtension, int)>(p.Key, (p.Value, i))));
    }

    public virtual IEnumerable<IBackynetContextOptionsExtension> Extensions
        => _extensionsMap.Values.OrderBy(v => v.Ordinal).Select(v => v.Extension);

    public IBackynetContextOptionsExtension? FindExtension<TExtension>() where TExtension : class, IBackynetContextOptionsExtension
    {
        return _extensionsMap.TryGetValue(typeof(TExtension), out var value) ? (TExtension)value.Extension : null;
    }

    protected virtual ImmutableSortedDictionary<Type, (IBackynetContextOptionsExtension Extension, int Ordinal)> ExtensionsMap => _extensionsMap;
    public abstract IBackynetContextOptions WithExtension<TExtension>(TExtension extension) where TExtension : class, IBackynetContextOptionsExtension;

    public abstract Type ContextType { get; }
}