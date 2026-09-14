using OpenTK.Mathematics;
using OpenTkProjects.Core.SceneGraph;

namespace OpenTkProjects.Core.Lighting;

public sealed class DirectionalLight
{
    /// <summary>Beyond this alignment with world up, an up vector of Y would be degenerate.</summary>
    private const float UpDegeneracyThreshold = 0.99f;

    public DirectionalLight(Vector3 direction)
    {
        if (direction.LengthSquared <= float.Epsilon)
        {
            throw new ArgumentException("Light direction must be non-zero.", nameof(direction));
        }

        Direction = Vector3.Normalize(direction);
        Color = Vector3.One;
        AmbientStrength = 0.25f;
    }

    /// <summary>Direction the light travels, normalized.</summary>
    public Vector3 Direction { get; }

    /// <summary>Direction from a surface toward the light. Shaders consume only this.</summary>
    public Vector3 ToLight => -Direction;

    public Vector3 Color { get; set; }

    public float AmbientStrength { get; set; }

    public LightFrustum ComputeShadowFrustum(Aabb sceneBounds, int shadowMapResolution = 4096, float padding = 1f)
    {
        Span<Vector3> corners = stackalloc Vector3[8];
        sceneBounds.GetCorners(corners);

        return ComputeShadowFrustum(corners, sceneBounds, shadowMapResolution, padding);
    }

    /// <summary>
    /// Fits width and height to <paramref name="focusPoints"/>, typically the view frustum corners,
    /// while extending depth to cover <paramref name="casterBounds"/>. Width and height set the
    /// texel footprint so they should cover only what the camera sees; depth decides which casters
    /// are rendered so it must reach objects far overhead.
    /// </summary>
    public LightFrustum ComputeShadowFrustum(
        ReadOnlySpan<Vector3> focusPoints,
        Aabb casterBounds,
        int shadowMapResolution,
        float padding = 1f)
    {
        if (focusPoints.Length == 0)
        {
            throw new ArgumentException("At least one focus point is required.", nameof(focusPoints));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(shadowMapResolution);

        Vector3 up = MathF.Abs(Vector3.Dot(Direction, Vector3.UnitY)) > UpDegeneracyThreshold
            ? Vector3.UnitZ
            : Vector3.UnitY;

        // A sphere, not a box: its radius is rotation invariant, so the frustum does not resize
        // as the camera turns and shadows stop subtly rescaling while looking around.
        Vector3 center = Centroid(focusPoints);
        float radius = 0f;

        foreach (Vector3 point in focusPoints)
        {
            radius = MathF.Max(radius, (point - center).Length);
        }

        radius = MathF.Ceiling(radius + padding);

        // In light space the light runs down the Z axis, so a shadow lands on exactly the XY its
        // caster occupies. Texels outside that footprint can never hold a blocker, so fitting past
        // it spends the budget on ground that is provably lit.
        float casterRadius = MathF.Ceiling(casterBounds.Radius + padding);

        if (casterRadius < radius)
        {
            center = casterBounds.Center;
            radius = casterRadius;
        }

        Vector3 eye = center - (Direction * (casterBounds.Radius + radius + padding));
        Matrix4 view = Matrix4.LookAt(eye, center, up);

        // Quantise the centre to whole texels, otherwise the refitted frustum drifts by fractions
        // of a texel each frame and every shadow edge crawls.
        float texelWorldSize = (radius * 2f) / shadowMapResolution;
        Vector3 centerInLight = Vector3.TransformPosition(center, view);

        centerInLight.X = MathF.Floor(centerInLight.X / texelWorldSize) * texelWorldSize;
        centerInLight.Y = MathF.Floor(centerInLight.Y / texelWorldSize) * texelWorldSize;

        float left = centerInLight.X - radius;
        float right = centerInLight.X + radius;
        float bottom = centerInLight.Y - radius;
        float top = centerInLight.Y + radius;

        Span<Vector3> casterCorners = stackalloc Vector3[8];
        casterBounds.GetCorners(casterCorners);

        float nearestZ = centerInLight.Z - radius;
        float farthestZ = centerInLight.Z + radius;

        foreach (Vector3 corner in casterCorners)
        {
            float z = Vector3.TransformPosition(corner, view).Z;
            nearestZ = MathF.Min(nearestZ, z);
            farthestZ = MathF.Max(farthestZ, z);
        }

        // Unclamped on purpose. Orthographic projection has no perspective divide, so a negative
        // near plane is valid; clamping it positive would clip away casters above the eye plane.
        float near = -farthestZ - padding;
        float far = -nearestZ + padding;

        Matrix4 projection = Matrix4.CreateOrthographicOffCenter(left, right, bottom, top, near, far);

        return new LightFrustum(view, projection, near, far, right - left, top - bottom);
    }

    private static Vector3 Centroid(ReadOnlySpan<Vector3> points)
    {
        Vector3 total = Vector3.Zero;

        foreach (Vector3 point in points)
        {
            total += point;
        }

        return total / points.Length;
    }
}
