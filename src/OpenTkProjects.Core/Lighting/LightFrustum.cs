using OpenTK.Mathematics;

namespace OpenTkProjects.Core.Lighting;

/// <summary>The view and projection a shadow pass renders with, plus the depth range they bracket.</summary>
public readonly record struct LightFrustum(
    Matrix4 View,
    Matrix4 Projection,
    float NearPlane,
    float FarPlane,
    float WorldWidth,
    float WorldHeight)
{
    /// <summary>World space covered by one shadow map texel. Bias and normal offset are sized in these units.</summary>
    public float TexelWorldSize(int shadowMapResolution) =>
        MathF.Max(WorldWidth, WorldHeight) / shadowMapResolution;

    /// <summary>
    /// View first, per OpenTK's row-vector convention. Uploaded untransposed, the GPU reads this as
    /// the column-major equivalent, matching GLSL projection * view * world * position.
    /// </summary>
    public Matrix4 ViewProjection => View * Projection;

    public Vector3 ProjectToNdc(Vector3 worldPoint)
    {
        Vector4 clip = new Vector4(worldPoint, 1f) * ViewProjection;
        return clip.W != 0f ? clip.Xyz / clip.W : clip.Xyz;
    }

    /// <summary>Depth in the [0,1] range a depth texture stores.</summary>
    public float NormalizedDepthOf(Vector3 worldPoint) => (ProjectToNdc(worldPoint).Z * 0.5f) + 0.5f;
}
