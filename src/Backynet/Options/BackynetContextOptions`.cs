using System.Collections.Immutable;

namespace Backynet.Options;

public class BackynetContextOptions<TContext> : BackynetContextOptions
{
    public BackynetContextOptions()
    {
    }

    private BackynetContextOptions(ImmutableDictionary<Type, (IBackynetContextOptionsExtension Extension, int Ordinal)> extensions) : base(extensions)
    {
    }

    public override BackynetContextOptions WithExtension<TExtension>(TExtension extension)
    {
        var type = extension.GetType();
        var ordinal = ExtensionsMap.Count;

        if (ExtensionsMap.TryGetValue(type, out var existingValue))
        {
            ordinal = existingValue.Ordinal;
        }

        return new BackynetContextOptions<TContext>(ExtensionsMap.SetItem(type, (extension, ordinal)));
    }

    public override Type ContextType => typeof(TContext);
}