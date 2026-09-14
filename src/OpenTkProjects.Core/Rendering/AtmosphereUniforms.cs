using OpenTK.Mathematics;
using OpenTkProjects.Core.Graphics;
using OpenTkProjects.Core.Lighting;
using OpenTkProjects.Core.SceneGraph;

namespace OpenTkProjects.Core.Rendering;

/// <summary>
/// Uploads the shared sun, sky and fog block. The lit, sky and composite shaders all include the
/// same GLSL chunk, so they must all be fed from one place or the fog on geometry will stop
/// matching the sky behind it.
/// </summary>
internal static class AtmosphereUniforms
{
    public static void Apply(Shader shader, Scene scene)
    {
        Atmosphere atmosphere = scene.Atmosphere;
        DirectionalLight light = scene.Light;

        Set(shader, "sunDirection", light.ToLight);
        Set(shader, "sunColor", light.Color);
        // The shader's sunAngularRadius drives only the drawn disc, so the visual scale is folded
        // in here. Shadow penumbra reads the unscaled physical value in ForwardLitPass.
        Set(shader, "sunAngularRadius", atmosphere.SunAngularRadius * atmosphere.SunDiscScale);
        Set(shader, "sunGlowFalloff", atmosphere.SunGlowFalloff);

        Set(shader, "skyZenithColor", atmosphere.SkyZenithColor);
        Set(shader, "skyHorizonColor", atmosphere.SkyHorizonColor);
        Set(shader, "groundColor", atmosphere.GroundColor);

        Set(shader, "fogDensity", atmosphere.FogDensity);
        Set(shader, "fogHeightFalloff", atmosphere.FogHeightFalloff);
        Set(shader, "fogHeight", atmosphere.FogHeight);
    }

    private static void Set(Shader shader, string name, Vector3 value)
    {
        if (shader.HasUniform(name))
        {
            shader.SetUniform(name, value);
        }
    }

    private static void Set(Shader shader, string name, float value)
    {
        if (shader.HasUniform(name))
        {
            shader.SetUniform(name, value);
        }
    }
}
