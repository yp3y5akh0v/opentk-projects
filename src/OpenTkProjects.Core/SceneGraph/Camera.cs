using OpenTK.Mathematics;

namespace OpenTkProjects.Core.SceneGraph;

public sealed class Camera
{
    /// <summary>Keeps the view direction clear of the poles, where the up vector degenerates.</summary>
    private const float PitchLimit = MathHelper.PiOver2 - 0.05f;

    private float pitch;

    public Vector3 Position { get; set; } = Vector3.Zero;

    public float Yaw { get; set; } = MathHelper.PiOver2;

    public float Pitch
    {
        get => pitch;
        set => pitch = MathHelper.Clamp(value, -PitchLimit, PitchLimit);
    }

    public float FieldOfView { get; set; } = MathHelper.DegreesToRadians(70f);

    public float NearPlane { get; set; } = 1f;

    public float FarPlane { get; set; } = 100_000f;

    public Vector3 Forward => Vector3.Normalize(new Vector3(
        MathF.Sin(Yaw) * MathF.Cos(Pitch),
        MathF.Sin(Pitch),
        MathF.Cos(Yaw) * MathF.Cos(Pitch)));

    /// <summary>
    /// Right-handed: (Right, Up, -Forward) forms a right-handed basis. Note cross(Forward, Up),
    /// not cross(Up, Forward), which would yield the left vector.
    /// </summary>
    public Vector3 Right => Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitY));

    /// <summary>Aims the camera at a world point by solving for yaw and pitch.</summary>
    public void LookAt(Vector3 target)
    {
        Vector3 direction = target - Position;

        if (direction.LengthSquared <= float.Epsilon)
        {
            throw new ArgumentException("Target coincides with the camera position.", nameof(target));
        }

        direction = Vector3.Normalize(direction);

        Pitch = MathF.Asin(direction.Y);
        Yaw = MathF.Atan2(direction.X, direction.Z);
    }

    /// <summary>
    /// The eight world-space corners of the view frustum between two distances.
    /// </summary>
    /// <remarks>
    /// Used to fit the shadow map to the slice of view that needs shadows, rather than spreading
    /// its texels over ground the camera cannot see.
    /// </remarks>
    public void GetFrustumCorners(float aspectRatio, float near, float far, Span<Vector3> destination)
    {
        if (destination.Length < 8)
        {
            throw new ArgumentException("Destination must hold at least 8 corners.", nameof(destination));
        }

        Vector3 forward = Forward;
        Vector3 right = Right;
        Vector3 up = Vector3.Normalize(Vector3.Cross(right, forward));

        float tangent = MathF.Tan(FieldOfView * 0.5f);

        WritePlane(destination, 0, forward, right, up, tangent, aspectRatio, near);
        WritePlane(destination, 4, forward, right, up, tangent, aspectRatio, far);
    }

    public Matrix4 GetViewMatrix() => Matrix4.LookAt(Position, Position + Forward, Vector3.UnitY);

    private void WritePlane(
        Span<Vector3> destination,
        int offset,
        Vector3 forward,
        Vector3 right,
        Vector3 up,
        float tangent,
        float aspectRatio,
        float distance)
    {
        float halfHeight = tangent * distance;
        float halfWidth = halfHeight * aspectRatio;

        Vector3 center = Position + (forward * distance);
        Vector3 wide = right * halfWidth;
        Vector3 tall = up * halfHeight;

        destination[offset + 0] = center - wide - tall;
        destination[offset + 1] = center + wide - tall;
        destination[offset + 2] = center - wide + tall;
        destination[offset + 3] = center + wide + tall;
    }

    public Matrix4 GetProjectionMatrix(float aspectRatio) =>
        Matrix4.CreatePerspectiveFieldOfView(FieldOfView, aspectRatio, NearPlane, FarPlane);
}
