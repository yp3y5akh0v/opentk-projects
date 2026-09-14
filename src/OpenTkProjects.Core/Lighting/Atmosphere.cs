using OpenTK.Mathematics;

namespace OpenTkProjects.Core.Lighting;

/// <summary>
/// Sky, sun and fog parameters. Colours are linear radiance, not sRGB, because every consumer
/// uses them in lighting arithmetic and tone mapping happens once at the end of the frame.
/// </summary>
public sealed class Atmosphere
{
    /// <summary>Angular radius of the real sun: 0.53 degrees of diameter.</summary>
    public const float RealSunAngularRadius = 0.00465f;

    public Vector3 SkyZenithColor { get; set; } = new(0.16f, 0.30f, 0.62f);

    public Vector3 SkyHorizonColor { get; set; } = new(0.62f, 0.72f, 0.86f);

    public Vector3 GroundColor { get; set; } = new(0.22f, 0.19f, 0.15f);

    public float AmbientStrength { get; set; } = 0.5f;

    /// <summary>Sets shadow penumbra width. Leave at the real value; it is light geometry, not a look control.</summary>
    public float SunAngularRadius { get; set; } = RealSunAngularRadius;

    /// <summary>
    /// Visual enlargement of the drawn disc only. Kept separate from <see cref="SunAngularRadius"/>
    /// because a disc large enough to read on screen would widen penumbrae until small high-up
    /// objects cast no visible shadow at all.
    /// </summary>
    public float SunDiscScale { get; set; } = 3.5f;

    /// <summary>Exponent on the sun halo. Larger values pull the glow in tighter around the disc.</summary>
    public float SunGlowFalloff { get; set; } = 280f;

    /// <summary>Fog density at <see cref="FogHeight"/>, per world unit.</summary>
    public float FogDensity { get; set; } = 4e-5f;

    /// <summary>How quickly fog thins with altitude. Larger values keep haze closer to the ground.</summary>
    public float FogHeightFalloff { get; set; } = 0.0016f;

    public float FogHeight { get; set; }

    /// <summary>Henyey-Greenstein asymmetry. Positive scatters forward; atmospheric haze sits near 0.6.</summary>
    public float ScatteringAnisotropy { get; set; } = 0.62f;

    /// <summary>
    /// Multiplier on fog density for in-scattering; zero disables the volumetric pass. In-scatter
    /// is roughly <c>phase * sunColor * opticalDepth * ScatteringStrength</c>, so values much above
    /// 1 saturate the frame to white haze.
    /// </summary>
    public float ScatteringStrength { get; set; } = 0.8f;

    /// <summary>How far along a view ray to march. Beyond this, scattering is negligible.</summary>
    public float VolumetricDistance { get; set; } = 26_000f;

    public float Exposure { get; set; } = 1f;
}
