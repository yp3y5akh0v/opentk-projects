using System.Reflection;
using OpenTkProjects.Core.Graphics;

namespace OpenTkProjects.Core.Tests.Graphics;

/// <summary>
/// Guards the embedded resource contract between the csproj glob and ShaderLibrary. A broken glob
/// or a renamed file would otherwise only surface at runtime as a missing shader.
/// </summary>
public class EmbeddedShaderTests
{
    private const string Prefix = "OpenTkProjects.Core.Assets.Shaders.";

    private static Assembly CoreAssembly => typeof(ShaderLibrary).Assembly;

    /// <summary>Entry-point shaders, each compiled as a whole translation unit.</summary>
    public static TheoryData<string> EntryPointShaders =>
    [
        "lit.vert.glsl",
        "lit.frag.glsl",
        "depth.vert.glsl",
        "depth.frag.glsl",
        "unlit.vert.glsl",
        "unlit.frag.glsl",
        "fullscreen.vert.glsl",
        "sky.frag.glsl",
        "volumetric.frag.glsl",
        "composite.frag.glsl",
        "normals.vert.glsl",
        "normals.frag.glsl",
        "ssao.frag.glsl",
        "blur.frag.glsl",
    ];

    /// <summary>Shared chunks pulled in by #include. They carry no version directive of their own.</summary>
    public static TheoryData<string> IncludeChunks => ["atmosphere.glsl", "shadows.glsl"];

    [Theory]
    [MemberData(nameof(EntryPointShaders))]
    public void EveryEntryPointShader_IsEmbeddedUnderTheExpectedResourceName(string shaderName)
    {
        Assert.Contains(Prefix + shaderName, CoreAssembly.GetManifestResourceNames());
    }

    [Theory]
    [MemberData(nameof(IncludeChunks))]
    public void EveryIncludeChunk_IsEmbeddedUnderTheExpectedResourceName(string shaderName)
    {
        Assert.Contains(Prefix + shaderName, CoreAssembly.GetManifestResourceNames());
    }

    [Theory]
    [MemberData(nameof(EntryPointShaders))]
    public void EveryEntryPointShader_DeclaresTheExpectedGlslVersion(string shaderName)
    {
        Assert.StartsWith("#version 330 core", Read(shaderName), StringComparison.Ordinal);
    }

    [Fact]
    public void ShadowChunk_GuardsAgainstSamplingOutsideTheLightFrustum()
    {
        string source = Read("shadows.glsl");

        // Fragments outside the light frustum must resolve as lit rather than sampling the
        // clamped border texel, which would otherwise paint bands across the scene.
        Assert.Contains("projected.z > 1.0", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ShadowChunk_DerivesPenumbraFromTheSunAngularRadius()
    {
        string source = Read("shadows.glsl");

        // The directional-light penumbra is 2 * blockerToReceiverDistance * tan(angularRadius).
        // A fixed-radius kernel here would lose the contact-sharp to distant-soft gradient.
        Assert.Contains("sunTanAngularRadius", source, StringComparison.Ordinal);
        Assert.Contains("FindBlockerDepth", source, StringComparison.Ordinal);
    }

    [Fact]
    public void LitShader_ConsumesTheSharedAtmosphereAndShadowChunks()
    {
        string source = Read("lit.frag.glsl");

        Assert.Contains("#include \"atmosphere.glsl\"", source, StringComparison.Ordinal);
        Assert.Contains("#include \"shadows.glsl\"", source, StringComparison.Ordinal);
    }

    private static string Read(string shaderName)
    {
        using Stream stream = CoreAssembly.GetManifestResourceStream(Prefix + shaderName)
            ?? throw new FileNotFoundException($"Missing embedded shader '{shaderName}'.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
