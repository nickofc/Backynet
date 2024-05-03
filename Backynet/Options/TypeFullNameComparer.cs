namespace Backynet.Options;

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