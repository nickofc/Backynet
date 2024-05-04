using System.Reflection;

namespace Backynet.PostgreSql;

public static class EmbeddedResource
{
    public static string Read(string resourceName, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}