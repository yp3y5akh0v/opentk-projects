using System.Reflection;
using System.Text;

namespace OpenTkProjects.Core.Graphics;

/// <summary>
/// Loads shader sources from embedded resources and caches the linked programs. Embedding avoids
/// relative-path loading, which only resolves when the working directory equals the output one.
/// </summary>
public sealed class ShaderLibrary : IDisposable
{
    private const string ResourcePrefix = "OpenTkProjects.Core.Assets.Shaders.";

    private readonly Dictionary<string, Shader> cache = [];
    private bool disposed;

    public Shader Load(string vertexResourceName, string fragmentResourceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vertexResourceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(fragmentResourceName);

        string key = $"{vertexResourceName}|{fragmentResourceName}";

        if (cache.TryGetValue(key, out Shader? cached))
        {
            return cached;
        }

        var shader = new Shader(ReadResource(vertexResourceName), ReadResource(fragmentResourceName));
        cache[key] = shader;
        return shader;
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        foreach (Shader shader in cache.Values)
        {
            shader.Dispose();
        }

        cache.Clear();
        disposed = true;
    }

    /// <summary>
    /// Reads a shader and resolves <c>#include "name.glsl"</c> directives. GLSL has no include
    /// mechanism of its own, and without one the sky and fog functions would have to be duplicated
    /// between the lit pass and the sky pass, where they would inevitably drift apart.
    /// </summary>
    private static string ReadResource(string name, HashSet<string>? including = null)
    {
        including ??= [];

        if (!including.Add(name))
        {
            throw new InvalidOperationException($"Circular shader include detected at '{name}'.");
        }

        Assembly assembly = typeof(ShaderLibrary).Assembly;
        string fullName = ResourcePrefix + name;

        using Stream stream = assembly.GetManifestResourceStream(fullName)
            ?? throw new FileNotFoundException(
                $"Embedded shader '{fullName}' not found. Available: {string.Join(", ", assembly.GetManifestResourceNames())}");

        using var reader = new StreamReader(stream);

        var builder = new StringBuilder();
        while (reader.ReadLine() is { } line)
        {
            string trimmed = line.TrimStart();

            if (!trimmed.StartsWith("#include", StringComparison.Ordinal))
            {
                builder.AppendLine(line);
                continue;
            }

            int open = trimmed.IndexOf('"', StringComparison.Ordinal);
            int close = trimmed.LastIndexOf('"');

            if (open < 0 || close <= open)
            {
                throw new InvalidOperationException($"Malformed #include in '{name}': {line}");
            }

            builder.AppendLine(ReadResource(trimmed[(open + 1)..close], including));
        }

        including.Remove(name);
        return builder.ToString();
    }
}
