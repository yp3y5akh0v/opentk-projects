using OpenTK.Mathematics;
using OpenTkProjects.Core.Geometry;
using OpenTkProjects.Core.Graphics;
using OpenTkProjects.Core.Rendering;
using OpenTkProjects.Core.SceneGraph;

namespace Collision2DPerformance;

/// <summary>
/// Binds a <see cref="CircleSimulation"/> to the scene graph: one shared unit-circle mesh scaled
/// per node, tinted by whether that circle is currently touching another.
/// </summary>
public sealed class CircleField : IDisposable
{
    private static readonly Vector3 IdleColor = new(0.62f, 0.78f, 0.95f);
    private static readonly Vector3 HitColor = new(0.95f, 0.34f, 0.30f);

    /// <summary>
    /// Seconds a circle keeps its contact highlight after separating. Purely a readability device:
    /// resolved collisions separate within a frame or two, so a strictly current highlight is a
    /// blink too short to follow. The simulation's own flag stays exact.
    /// </summary>
    private const float HighlightFadeSeconds = 0.3f;

    private readonly Material[] materials;
    private readonly MeshNode[] nodes;
    private readonly float[] highlight;
    private readonly Mesh circleMesh;

    private bool disposed;

    public CircleField(int count, Vector2 worldSize, int seed)
    {
        Simulation = new CircleSimulation(count, worldSize, seed);

        materials = new Material[count];
        nodes = new MeshNode[count];
        highlight = new float[count];

        // One mesh for every circle: the geometry is uploaded once and scaled per node.
        circleMesh = new Mesh(Circle2DBuilder.Build(1f, segments: 20, Vector4.One));

        for (int i = 0; i < count; i++)
        {
            materials[i] = new Material { Color = IdleColor };
            nodes[i] = new MeshNode(circleMesh, materials[i]);
            nodes[i].Transform.Scale = Simulation.RadiusOf(i);

            Scene.Root.Add(nodes[i]);
        }
    }

    public Scene Scene { get; } = new();

    public CircleSimulation Simulation { get; }

    public void Update(float deltaSeconds)
    {
        Simulation.Step(deltaSeconds);

        float decay = deltaSeconds / HighlightFadeSeconds;

        for (int i = 0; i < nodes.Length; i++)
        {
            Vector2 position = Simulation.PositionOf(i);
            nodes[i].Transform.Position = new Vector3(position.X, position.Y, 0f);

            highlight[i] = Simulation.IsColliding(i)
                ? 1f
                : MathF.Max(highlight[i] - decay, 0f);

            materials[i].Color = Vector3.Lerp(IdleColor, HitColor, highlight[i]);
        }
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        circleMesh.Dispose();
        disposed = true;
    }
}
