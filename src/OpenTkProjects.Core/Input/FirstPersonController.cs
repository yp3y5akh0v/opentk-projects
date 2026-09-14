using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTkProjects.Core.SceneGraph;

namespace OpenTkProjects.Core.Input;

/// <summary>
/// Drives a camera from keyboard and mouse, relying on CursorState.Grabbed for the mouse delta.
/// </summary>
public sealed class FirstPersonController(Camera camera)
{
    public float MoveSpeed { get; set; } = 1000f;

    public float SprintMultiplier { get; set; } = 10f;

    public float MouseSensitivity { get; set; } = 0.002f;

    public void Update(KeyboardState keyboard, Vector2 mouseDelta, float deltaSeconds)
    {
        ArgumentNullException.ThrowIfNull(keyboard);

        camera.Yaw -= mouseDelta.X * MouseSensitivity;
        camera.Pitch -= mouseDelta.Y * MouseSensitivity;

        Vector3 movement = Vector3.Zero;

        if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up))
        {
            movement += camera.Forward;
        }

        if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down))
        {
            movement -= camera.Forward;
        }

        if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right))
        {
            movement += camera.Right;
        }

        if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left))
        {
            movement -= camera.Right;
        }

        if (keyboard.IsKeyDown(Keys.Q))
        {
            movement += Vector3.UnitY;
        }

        if (keyboard.IsKeyDown(Keys.E))
        {
            movement -= Vector3.UnitY;
        }

        if (movement.LengthSquared <= float.Epsilon)
        {
            return;
        }

        float speed = MoveSpeed * (keyboard.IsKeyDown(Keys.LeftShift) ? SprintMultiplier : 1f);
        camera.Position += Vector3.Normalize(movement) * speed * deltaSeconds;
    }
}
