using System.Reflection;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTkProjects.Core.Geometry;
using OpenTkProjects.Core.Graphics;
using OpenTkProjects.Core.Lighting;
using OpenTkProjects.Core.Rendering;
using OpenTkProjects.Core.SceneGraph;

namespace Cloth3D;

/// <summary>Builds the flag scene and keeps its mesh in step with the simulation.</summary>
public sealed class ClothScene : IDisposable
{
    private const int Rows = 34;
    private const int Columns = 52;
    private const float Spacing = 6f;
    private const float PoleHeight = 260f;

    private static readonly Vector3 Gravity = new(0f, -175f, 0f);

    private readonly ClothSimulation simulation = new(Rows, Columns, Spacing);
    private readonly List<IDisposable> owned = [];

    private Mesh? clothMesh;
    private float elapsed;
    private bool disposed;

    public Scene Scene { get; } = new();

    public void Build()
    {
        Scene.Light = new DirectionalLight(new Vector3(-0.55f, -0.62f, -0.55f))
        {
            Color = new Vector3(1.75f, 1.56f, 1.26f),
        };

        Scene.Atmosphere = new Atmosphere
        {
            SkyZenithColor = new Vector3(0.14f, 0.29f, 0.62f),
            SkyHorizonColor = new Vector3(0.70f, 0.78f, 0.88f),
            GroundColor = new Vector3(0.28f, 0.26f, 0.20f),
            AmbientStrength = 0.55f,

            // A much smaller scene than the maze, so fog has to be denser per unit to read at all.
            FogDensity = 5e-4f,
            FogHeightFalloff = 0.006f,

            SunDiscScale = 3.5f,
        };

        PinHoistEdge();
        AddCloth();
        AddPole();
        AddGround();
        FrameCamera();
    }

    public void Update(float deltaSeconds)
    {
        elapsed += deltaSeconds;

        simulation.Step(deltaSeconds, Gravity + Wind(elapsed));

        clothMesh?.UpdatePositions(simulation.Positions);
        clothMesh?.UpdateNormals(simulation.Normals);
    }

    /// <summary>
    /// Gusting breeze. The dominant component runs along +X, away from the pole, because that is
    /// what holds the flag out; the smaller terms make it ripple instead of sitting rigid.
    /// </summary>
    private static Vector3 Wind(float time) =>
        new(
            300f + (110f * MathF.Sin(time * 0.9f)),
            22f * MathF.Sin(time * 2.3f),
            70f * MathF.Sin(time * 1.7f));

    /// <summary>Pins the hoist edge, the side attached to the pole. Everything else hangs free.</summary>
    private void PinHoistEdge()
    {
        for (int row = 0; row < Rows; row++)
        {
            simulation.Pin(row, 0);
        }
    }

    private void AddCloth()
    {
        var data = new MeshData { Primitive = PrimitiveType.Triangles };

        foreach (Vector3 position in simulation.Positions)
        {
            data.AddVertex(position);
            data.AddNormal(Vector3.UnitZ);
        }

        for (int row = 0; row < Rows; row++)
        {
            for (int column = 0; column < Columns; column++)
            {
                data.AddTexCoord(new Vector2(column / (float)(Columns - 1), 1f - (row / (float)(Rows - 1))));
            }
        }

        for (int row = 0; row < Rows - 1; row++)
        {
            for (int column = 0; column < Columns - 1; column++)
            {
                int topLeft = (row * Columns) + column;
                int topRight = topLeft + 1;
                int bottomLeft = topLeft + Columns;
                int bottomRight = bottomLeft + 1;

                data.AddTriangle(topLeft, bottomLeft, topRight);
                data.AddTriangle(topRight, bottomLeft, bottomRight);
            }
        }

        data.Validate();

        clothMesh = new Mesh(data, MeshUsage.Dynamic);
        Texture2D flag = LoadFlagTexture();
        owned.Add(clothMesh);
        owned.Add(flag);

        var node = new MeshNode(clothMesh, new Material
        {
            Color = Vector3.One,
            AlbedoMap = flag,
            SpecularStrength = 0.08f,
            Shininess = 16f,
        });

        node.Transform.Position = new Vector3(0f, PoleHeight, 0f);
        Scene.Root.Add(node);
    }

    private static Texture2D LoadFlagTexture()
    {
        Assembly assembly = typeof(ClothScene).Assembly;
        const string name = "Cloth3D.Assets.USA.png";

        using Stream stream = assembly.GetManifestResourceStream(name)
            ?? throw new FileNotFoundException(
                $"Embedded texture '{name}' not found. Available: {string.Join(", ", assembly.GetManifestResourceNames())}");

        return Texture2D.FromStream(stream);
    }

    private void AddPole()
    {
        Mesh mesh = Track(IcoSphereBuilder.Build(4f, 2));
        var material = new Material { Color = new Vector3(0.42f, 0.36f, 0.30f), SpecularStrength = 0.2f };

        // A column of spheres reads as a pole without needing a cylinder primitive.
        for (float y = 0f; y <= PoleHeight + 18f; y += 7f)
        {
            var segment = new MeshNode(mesh, material);
            segment.Transform.Position = new Vector3(0f, y, 0f);
            Scene.Root.Add(segment);
        }
    }

    private void AddGround()
    {
        // Large enough that fog swallows the edges before they reach the camera.
        const float extent = 4000f;

        Mesh mesh = Track(QuadBuilder.Build(extent, extent, Vector4.One));
        var node = new MeshNode(mesh, new Material { Color = new Vector3(0.33f, 0.35f, 0.29f), SpecularStrength = 0.03f })
        {
            CastsShadow = false,
        };

        node.Transform.Rotation = new Vector3(MathHelper.DegreesToRadians(-90f), 0f, 0f);
        node.Transform.Position = new Vector3(-extent * 0.5f, 0f, extent * 0.5f);

        Scene.Root.Add(node);
    }

    private void FrameCamera()
    {
        var focus = new Vector3(Columns * Spacing * 0.5f, PoleHeight - (Rows * Spacing * 0.45f), 0f);

        Scene.Camera.Position = focus + new Vector3(-95f, 25f, 330f);
        Scene.Camera.LookAt(focus);
        Scene.Camera.NearPlane = 1f;
        Scene.Camera.FarPlane = 12_000f;
    }

    private Mesh Track(MeshData data)
    {
        var mesh = new Mesh(data);
        owned.Add(mesh);
        return mesh;
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        foreach (IDisposable resource in owned)
        {
            resource.Dispose();
        }

        disposed = true;
    }
}
