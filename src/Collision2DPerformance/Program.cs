using OpenTK.Mathematics;
using OpenTkProjects.Core.App;
using OpenTkProjects.Core.Rendering;
using OpenTkProjects.Core.SceneGraph;

namespace Collision2DPerformance;

internal sealed class Collision2DApp(AppOptions options, int circleCount)
    : GameApp(options, "Collision 2D Performance")
{
    /// <summary>
    /// Few enough to watch individual collisions. Radii scale automatically with the count to hold
    /// coverage constant, so raising this with --circles stays readable and still stresses the
    /// broad phase.
    /// </summary>
    internal const int DefaultCircleCount = 140;

    private const int Seed = 8127;

    private static readonly Vector2 WorldSize = new(1024f, 576f);

    private readonly Camera2D camera = new();

    private CircleField? field;
    private Renderer2D? renderer;
    private float statsTimer;

    protected override void OnLoadScene()
    {
        field = new CircleField(circleCount, WorldSize, Seed);
        renderer = new Renderer2D();

        camera.Frame(Vector2.Zero, WorldSize);
    }

    protected override void OnUpdateScene(float deltaSeconds)
    {
        if (field is null)
        {
            return;
        }

        // A fixed step keeps a capture run deterministic rather than depending on frame timing.
        field.Update(IsCapturing ? 1f / 60f : deltaSeconds);

        ReportBroadPhase(deltaSeconds);
    }

    /// <summary>
    /// Prints how many pairs the broad phase actually examined against the exhaustive count.
    /// The ratio is the whole justification for the quadtree, so it is worth showing rather than
    /// assuming.
    /// </summary>
    private void ReportBroadPhase(float deltaSeconds)
    {
        if (field is null)
        {
            return;
        }

        statsTimer += deltaSeconds;

        if (statsTimer < 2f)
        {
            return;
        }

        statsTimer = 0f;

        CircleSimulation simulation = field.Simulation;
        long exhaustive = (long)simulation.Count * (simulation.Count - 1) / 2;

        Console.WriteLine(
            $"[broadphase] {simulation.CandidatePairs} candidate pairs vs {exhaustive} exhaustive " +
            $"({100.0 * simulation.CandidatePairs / exhaustive:F1}%), {simulation.CollidingPairs} pairs touching, " +
            $"{simulation.CollidingCircles}/{simulation.Count} circles highlighted");
    }

    protected override void OnRenderScene()
    {
        if (field is null || renderer is null)
        {
            return;
        }

        renderer.Render(field.Scene, camera, ClientSize.X, ClientSize.Y);
    }

    protected override void OnUnload()
    {
        renderer?.Dispose();
        field?.Dispose();
        base.OnUnload();
    }
}

internal static class Program
{
    private static void Main(string[] args)
    {
        int circleCount = Collision2DApp.DefaultCircleCount;
        List<string> remaining = [];

        // The demo strips its own option before handing the rest to the shared parser, so the
        // library never has to know that circles exist.
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] != "--circles")
            {
                remaining.Add(args[i]);
                continue;
            }

            if (i + 1 >= args.Length || !int.TryParse(args[i + 1], out circleCount) || circleCount < 1)
            {
                throw new ArgumentException("--circles requires a positive integer.", nameof(args));
            }

            i++;
        }

        using var app = new Collision2DApp(AppOptions.Parse([.. remaining]), circleCount);
        app.Run();
    }
}
