using OpenTK.Mathematics;
using OpenTkProjects.Core.App;
using OpenTkProjects.Core.Input;
using OpenTkProjects.Core.Rendering;

namespace MazeGen3D;

internal sealed class MazeGen3DApp(AppOptions options, CameraPose pose) : GameApp(options, "Maze Gen 3D")
{
    private const int MazeSeed = 32;


    private float baseYaw;
    private MazeScene? scene;
    private Renderer? renderer;
    private FirstPersonController? controller;
    private float captureTime;

    protected override void OnLoadScene()
    {
        scene = new MazeScene { Pose = pose };
        scene.Build(MazeSeed);

        renderer = new Renderer { DiagnosticsEnabled = IsCapturing };

        // Pan around whatever pose the scene framed, rather than a hardcoded bearing that silently
        // stops matching the moment the framing is retuned.
        baseYaw = scene.Scene.Camera.Yaw;
        controller = new FirstPersonController(scene.Scene.Camera);
    }

    protected override void OnUpdateScene(float deltaSeconds)
    {
        if (scene is null)
        {
            return;
        }

        // A still frame cannot show whether shadow edges crawl as the view moves, which is the
        // artefact a single shadow map is most prone to.
        if (IsCapturingMotion)
        {
            FlyCaptureRoute(scene, deltaSeconds);
            return;
        }

        if (IsCapturing)
        {
            return;
        }

        controller?.Update(KeyboardState, MouseState.Delta, deltaSeconds);
    }

    /// <summary>
    /// Climbs away from the shadow while orbiting it, always looking back down at the ground.
    /// </summary>
    /// <remarks>
    /// A static capture cannot reveal shadow edges crawling as the view moves, or shadows vanishing
    /// once the ground leaves the shadowed range. A fixed ascent exercises both.
    /// </remarks>
    private void FlyCaptureRoute(MazeScene maze, float deltaSeconds)
    {
        const float climbSeconds = 9f;
        const float startHeight = 520f;
        const float endHeight = 7_000f;
        const float startRadius = 170f;
        const float endRadius = 3_200f;

        captureTime += deltaSeconds;

        float t = MathF.Min(captureTime / climbSeconds, 1f);
        float eased = t * t * (3f - (2f * t));

        float height = MathHelper.Lerp(startHeight, endHeight, eased);
        float radius = MathHelper.Lerp(startRadius, endRadius, eased);
        float angle = baseYaw + (eased * MathHelper.DegreesToRadians(70f));

        maze.Scene.Camera.Position = maze.ShadowCentre
            + new Vector3(MathF.Sin(angle) * radius, height, MathF.Cos(angle) * radius);

        maze.Scene.Camera.LookAt(maze.ShadowCentre);
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
        CameraPose pose = CameraPose.Showcase;
        List<string> remaining = [];

        // Demo-specific option stripped before the shared parser sees it.
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] != "--pose")
            {
                remaining.Add(args[i]);
                continue;
            }

            if (i + 1 >= args.Length || !Enum.TryParse(args[i + 1], ignoreCase: true, out pose))
            {
                throw new ArgumentException("--pose requires 'showcase' or 'shadowdetail'.", nameof(args));
            }

            i++;
        }

        using var app = new MazeGen3DApp(AppOptions.Parse([.. remaining]), pose);
        app.Run();
    }
}
