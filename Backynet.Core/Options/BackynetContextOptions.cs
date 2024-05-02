using System.Collections.Immutable;

namespace Backynet.Core;

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

    protected BackynetContextOptions(IReadOnlyDictionary<Type, IBackynetContextOptionsExtension> extensions)
    {
        _extensionsMap = ImmutableSortedDictionary.Create<Type, (IBackynetContextOptionsExtension, int)>(TypeFullNameComparer.Instance)
            .AddRange(extensions.Select((p, i) => new KeyValuePair<Type, (IBackynetContextOptionsExtension, int)>(p.Key, (p.Value, i))));
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

public sealed class TypeFullNameComparer : IComparer<Type>, IEqualityComparer<Type>
{
    private TypeFullNameComparer()
    {
    }
    
    public static readonly TypeFullNameComparer Instance = new();
    
    public int Compare(Type? x, Type? y) => string.CompareOrdinal(x.FullName, y.FullName);

    public bool Equals(Type? x, Type? y) => Compare(x, y) == 0;
    
    public int GetHashCode(Type obj) => obj.FullName.GetHashCode();
}
