using System.Reflection;

namespace Backynet.PostgreSql;

public static class EmbeddedResource
{
    public static IEnumerable<string> Find(Func<string, bool> predicate, params Assembly[]? assemblies)
    {
        assemblies ??= new[] { Assembly.GetExecutingAssembly() };

        var output = new List<string>(1);

        foreach (var assembly in assemblies)
        {
            var names = assembly.GetManifestResourceNames();
            output.AddRange(names.Where(predicate));
        }

        return output;
    }

    public static string Read(string resourceName, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}