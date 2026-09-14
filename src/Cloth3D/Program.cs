using OpenTkProjects.Core.App;
using OpenTkProjects.Core.Input;
using OpenTkProjects.Core.Rendering;

namespace Cloth3D;

internal sealed class Cloth3DApp(AppOptions options) : GameApp(options, "Cloth 3D")
{
    /// <summary>
    /// Verlet integration is only stable while the step stays small, so a long frame is simulated
    /// as several short ones rather than one large jump.
    /// </summary>
    private const float FixedStep = 1f / 120f;

    private const float MaxStepsPerFrame = 8;

    private ClothScene? scene;
    private Renderer? renderer;
    private FirstPersonController? controller;
    private float accumulator;

    protected override void OnLoadScene()
    {
        scene = new ClothScene();
        scene.Build();

        renderer = new Renderer { DiagnosticsEnabled = IsCapturing };

        controller = new FirstPersonController(scene.Scene.Camera) { MoveSpeed = 180f };
    }

    protected override void OnUpdateScene(float deltaSeconds)
    {
        if (scene is null)
        {
            return;
        }

        if (!IsCapturing)
        {
            controller?.Update(KeyboardState, MouseState.Delta, deltaSeconds);
        }

        accumulator += IsCapturing ? FixedStep * 4f : deltaSeconds;

        int steps = 0;
        while (accumulator >= FixedStep && steps < MaxStepsPerFrame)
        {
            scene.Update(FixedStep);
            accumulator -= FixedStep;
            steps++;
        }

        // Drop any backlog a stall produced instead of trying to catch up in one burst.
        if (accumulator > FixedStep * MaxStepsPerFrame)
        {
            accumulator = 0f;
        }
    }

    protected override void OnRenderScene()
    {
        if (scene is null || renderer is null)
        {
            return;
        }

        renderer.Render(scene.Scene, ClientSize.X, ClientSize.Y);
    }

    protected override void OnUnload()
    {
        renderer?.Dispose();
        scene?.Dispose();
        base.OnUnload();
    }
}

internal static class Program
{
    private static void Main(string[] args)
    {
        using var app = new Cloth3DApp(AppOptions.Parse(args));
        app.Run();
    }
}
