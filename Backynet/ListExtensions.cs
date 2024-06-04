using System.Reflection;
using System.Reflection.Emit;

namespace Backynet;

/// <summary>
/// https://stackoverflow.com/a/17308019/6210759
/// </summary>
internal static class ListExtensions
{
    private static class ArrayAccessor<T>
    {
        public static readonly Func<List<T>, T[]> Getter;

        static ArrayAccessor()
        {
            var dm = new DynamicMethod("get", MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(T[]),
                new Type[] { typeof(List<T>) }, typeof(ArrayAccessor<T>), true);
            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0); // Load List<T> argument
            il.Emit(OpCodes.Ldfld, typeof(List<T>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance));
            il.Emit(OpCodes.Ret); // Return field
            Getter = (Func<List<T>, T[]>)dm.CreateDelegate(typeof(Func<List<T>, T[]>));
        }
    }

    public static T[] GetInternalArray<T>(this List<T> list)
    {
        return ArrayAccessor<T>.Getter(list);
    }
}