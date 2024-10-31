using System.Reflection;

namespace Backynet.PostgreSql;

public static class EmbeddedResource
{
    public static Stream? Read(string resourceName, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceStream(resourceName);
    }
}