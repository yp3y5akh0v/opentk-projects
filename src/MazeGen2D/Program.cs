using OpenTkProjects.Core.App;
using OpenTkProjects.Core.Rendering;
using OpenTkProjects.Core.SceneGraph;

namespace MazeGen2D;

internal sealed class MazeGen2DApp(AppOptions options) : GameApp(options, "Maze Gen 2D")
{
    private const int Rows = 20;
    private const int Columns = 20;
    private const int MazeSeed = 4711;

    /// <summary>Carve steps per second, so the animation reads the same on any machine.</summary>
    private const float StepsPerSecond = 90f;

    /// <summary>Rooms carved per rendered frame while capturing, for a reproducible recording.</summary>
    private const int StepsPerCaptureFrame = 3;

    private readonly Camera2D camera = new();

    private MazeGrid? grid;
    private Renderer2D? renderer;
    private float pendingSteps;

    protected override void OnLoadScene()
    {
        grid = new MazeGrid(Rows, Columns, MazeSeed);
        renderer = new Renderer2D();

        camera.Frame(grid.WorldMin, grid.WorldMax);
    }

    protected override void OnUpdateScene(float deltaSeconds)
    {
        if (grid is null || grid.IsComplete)
        {
            return;
        }

        // Carving per frame rather than per second keeps a capture reproducible, and stops the
        // maze finishing before the first frame is grabbed.
        if (IsCapturing)
        {
            for (int i = 0; i < StepsPerCaptureFrame && !grid.IsComplete; i++)
            {
                grid.Step();
            }

            return;
        }

        pendingSteps += deltaSeconds * StepsPerSecond;

        while (pendingSteps >= 1f && !grid.IsComplete)
        {
            grid.Step();
            pendingSteps -= 1f;
        }
    }

    protected override void OnRenderScene()
    {
        if (grid is null || renderer is null)
        {
            return;
        }

        renderer.Render(grid.Scene, camera, ClientSize.X, ClientSize.Y);
    }

    protected override void OnUnload()
    {
        renderer?.Dispose();
        grid?.Dispose();
        base.OnUnload();
    }
}

internal static class Program
{
    private static void Main(string[] args)
    {
        using var app = new MazeGen2DApp(AppOptions.Parse(args));
        app.Run();
    }
}
