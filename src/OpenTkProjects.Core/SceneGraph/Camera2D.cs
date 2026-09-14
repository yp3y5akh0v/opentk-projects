using OpenTK.Mathematics;

namespace OpenTkProjects.Core.SceneGraph;

/// <summary>
/// Orthographic view for the flat demos, defined in world units rather than pixels so resizing
/// the window changes how much is visible instead of stretching the contents.
/// </summary>
public sealed class Camera2D
{
    public Vector2 Center { get; set; } = Vector2.Zero;

    /// <summary>World units visible vertically. Horizontal extent follows the aspect ratio.</summary>
    public float ViewHeight { get; set; } = 1000f;

    public Matrix4 GetViewProjection(float aspectRatio)
    {
        float halfHeight = ViewHeight * 0.5f;
        float halfWidth = halfHeight * aspectRatio;

        return Matrix4.CreateOrthographicOffCenter(
            Center.X - halfWidth,
            Center.X + halfWidth,
            Center.Y - halfHeight,
            Center.Y + halfHeight,
            -1f,
            1f);
    }

    /// <summary>Frames a world-space rectangle with a little margin around it.</summary>
    public void Frame(Vector2 min, Vector2 max, float margin = 1.05f)
    {
        Center = (min + max) * 0.5f;
        ViewHeight = MathF.Max(max.Y - min.Y, 0.001f) * margin;
    }
}
