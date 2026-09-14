using OpenTK.Mathematics;

namespace OpenTkProjects.Core.SceneGraph;

public sealed class Transform
{
    public Vector3 Position { get; set; } = Vector3.Zero;

    /// <summary>Euler angles in radians, applied X then Y then Z.</summary>
    public Vector3 Rotation { get; set; } = Vector3.Zero;

    public float Scale { get; set; } = 1f;

    public Matrix4 ToMatrix() =>
        Matrix4.CreateScale(Scale) *
        Matrix4.CreateRotationX(Rotation.X) *
        Matrix4.CreateRotationY(Rotation.Y) *
        Matrix4.CreateRotationZ(Rotation.Z) *
        Matrix4.CreateTranslation(Position);
}
