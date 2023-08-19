using System.IO;
using System.Reflection;

namespace TypeSharper;

public static class AssemblyExtensions
{
    public static string ReadEmbeddedResource(this Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream!);
        return reader.ReadToEnd();
    }
}
