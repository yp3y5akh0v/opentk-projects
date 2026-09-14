using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OpenTkProjects.Core.Graphics;

public static class GraphicsDevice
{
    /// <summary>Held in a static field so the delegate is not collected while GL still holds it.</summary>
    private static DebugProc? debugCallback;

    /// <summary>Routes driver diagnostics to stderr, catching failures GL otherwise reports silently.</summary>
    public static void EnableDebugOutput()
    {
        debugCallback = OnDebugMessage;

        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);
        GL.DebugMessageCallback(debugCallback, IntPtr.Zero);
    }

    /// <summary>Which GPU the context landed on, which is rarely what the IDE was told to prefer.</summary>
    public static string DescribeDevice() =>
        $"{GL.GetString(StringName.Vendor)} | {GL.GetString(StringName.Renderer)} | GL {GL.GetString(StringName.Version)}";

    /// <summary>
    /// Whether the context looks integrated. The Windows preference that forces a discrete GPU is
    /// keyed to the executable path, so it stops applying whenever the build moves.
    /// </summary>
    public static bool IsLikelyIntegratedDevice()
    {
        string renderer = GL.GetString(StringName.Renderer) ?? string.Empty;

        return renderer.Contains("Intel", StringComparison.OrdinalIgnoreCase)
            || renderer.Contains("Microsoft Basic Render", StringComparison.OrdinalIgnoreCase)
            || renderer.Contains("llvmpipe", StringComparison.OrdinalIgnoreCase)
            || renderer.Contains("softpipe", StringComparison.OrdinalIgnoreCase);
    }

    public static void SetViewport(int width, int height) => GL.Viewport(0, 0, width, height);

    public static void Clear(Color4 color)
    {
        GL.ClearColor(color);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }

    public static void ClearDepthOnly() => GL.Clear(ClearBufferMask.DepthBufferBit);

    public static void SetDepthTest(bool enabled)
    {
        if (enabled)
        {
            GL.Enable(EnableCap.DepthTest);
        }
        else
        {
            GL.Disable(EnableCap.DepthTest);
        }
    }

    /// <summary>
    /// Depth writes independent of the depth test, so the sky can draw behind everything without
    /// leaving depth values that occlude the scene.
    /// </summary>
    public static void SetDepthWrite(bool enabled) => GL.DepthMask(enabled);

    /// <summary>Additive blending, for passes that contribute light rather than replacing it.</summary>
    public static void SetAdditiveBlend(bool enabled)
    {
        if (enabled)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);
        }
        else
        {
            GL.Disable(EnableCap.Blend);
        }
    }

    /// <summary>Hardware multisample resolve. The context must have been created with samples requested.</summary>
    public static void SetMultisample(bool enabled)
    {
        if (enabled)
        {
            GL.Enable(EnableCap.Multisample);
        }
        else
        {
            GL.Disable(EnableCap.Multisample);
        }
    }

    /// <summary>
    /// Slope-scaled depth offset in depth-buffer units. Being buffer-relative rather than
    /// world-relative, it cannot be mis-scaled by a change of scene size.
    /// </summary>
    public static void SetPolygonOffset(float factor, float units)
    {
        GL.Enable(EnableCap.PolygonOffsetFill);
        GL.PolygonOffset(factor, units);
    }

    public static void ClearPolygonOffset()
    {
        GL.PolygonOffset(0f, 0f);
        GL.Disable(EnableCap.PolygonOffsetFill);
    }

    private static void OnDebugMessage(
        DebugSource source,
        DebugType type,
        int id,
        DebugSeverity severity,
        int length,
        IntPtr message,
        IntPtr userParam)
    {
        if (severity == DebugSeverity.DebugSeverityNotification)
        {
            return;
        }

        string text = Marshal.PtrToStringAnsi(message, length);
        Console.Error.WriteLine($"[GL {severity}] {type} from {source} (id {id}): {text}");
    }
}
