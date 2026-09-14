using OpenTK.Mathematics;
using OpenTkProjects.Core.Geometry;
using OpenTkProjects.Core.Graphics;
using OpenTkProjects.Core.Lighting;
using OpenTkProjects.Maze;
using OpenTkProjects.Core.Rendering;
using OpenTkProjects.Core.SceneGraph;

namespace MazeGen3D;

/// <summary>Builds the maze demo scene: ground, walls, and a floating shadow-casting sphere.</summary>
public sealed class MazeScene : IDisposable
{
    private const int Rows = 25;
    private const int Columns = 25;
    private const float RoomWidth = 800f;
    private const float RoomHeight = 400f;
    private const float Gap = 5f;
    private const float LightHeightOffset = 5000f;
    private const float PlayerRadius = 100f;
    private const float OccluderRadius = 100f;

    /// <summary>Occluder altitude. Penumbra grows with it, so higher means a softer shadow.</summary>
    private const float OccluderHeight = 2_000f;

    private readonly List<Mesh> ownedMeshes = [];
    private bool disposed;

    public Scene Scene { get; } = new();

    /// <summary>Which framing <see cref="Build"/> leaves the camera in.</summary>
    public CameraPose Pose { get; set; } = CameraPose.Showcase;

    /// <summary>Where the occluder's shadow lands on the ground. Not directly below it.</summary>
    public Vector3 ShadowCentre { get; private set; }

    public Vector3 OccluderPosition { get; private set; }

    public static float Span => Columns * (RoomWidth + Gap);

    public void Build(int seed)
    {
        Vector3 lightPosition = new(
            Span / 2f,
            RoomHeight + (2f * PlayerRadius) + LightHeightOffset,
            Span / 2f);

        // Radiance above 1 gives the tone mapper highlights to roll off instead of clipping.
        Scene.Light = new DirectionalLight(new Vector3(-1f, -1f, -1f))
        {
            Color = new Vector3(1.7f, 1.52f, 1.24f),
        };

        Scene.Atmosphere = new Atmosphere
        {
            SkyZenithColor = new Vector3(0.13f, 0.27f, 0.60f),
            SkyHorizonColor = new Vector3(0.66f, 0.75f, 0.87f),
            GroundColor = new Vector3(0.26f, 0.22f, 0.17f),

            // Too little crushes shadowed walls to black; too much flattens the scene into haze.
            AmbientStrength = 0.72f,

            // Low base density keeps surfaces crisp; ScatteringStrength drives the shafts instead.
            // A single value for both cannot do it: shafts only show at a density that hazes over.
            FogDensity = 1.2e-4f,
            FogHeightFalloff = 0.0038f,

            ScatteringStrength = 9f,

            // Only the drawn disc is enlarged; physical radius stays real so the sphere's shadow survives.
            SunDiscScale = 3.5f,
        };

        ShadowCentre = ChooseClearShadowGround();
        OccluderPosition = LiftAlongLight(ShadowCentre, OccluderHeight, Scene.Light.Direction);

        AddGround();
        AddWalls(seed);
        AddOccluder();
        FrameDemoView();
    }

    /// <summary>
    /// Ground where the shadow lands, placed so no wall shadow can reach it. Light travels along
    /// (-1,-1,-1), so a 400 unit wall casts 400 units toward -X and -Z; only the quadrant nearest
    /// a room's origin corner is clear of the walls at higher X and Z.
    /// <para>
    /// Choosing a seed instead is impossible, not merely unlucky: clearing this point needs four
    /// doors open across rooms (5,5), (5,6), (6,5) and (6,6), and those edges form a cycle. A
    /// perfect maze is a spanning tree, so no seed can satisfy all four.
    /// </para>
    /// </summary>
    private static Vector3 ChooseClearShadowGround()
    {
        const int room = 5;
        const float quadrant = RoomWidth * 0.25f;

        float origin = room * (RoomWidth + Gap);

        return new Vector3(origin + quadrant, 0f, origin + quadrant);
    }

    /// <summary>Walks back up the light ray from a ground point to the height that casts onto it.</summary>
    private static Vector3 LiftAlongLight(Vector3 groundPoint, float height, Vector3 lightDirection)
    {
        float travel = height / MathF.Abs(lightDirection.Y);
        return groundPoint - (travel * lightDirection);
    }

    private Mesh Track(MeshData data)
    {
        var mesh = new Mesh(data);
        ownedMeshes.Add(mesh);
        return mesh;
    }

    /// <summary>
    /// Ground extending well past the maze so the terrain reaches the fog horizon instead of
    /// stopping on a visible edge. Excluded from shadow casting, which also keeps it out of the
    /// shadow frustum fit, so its size costs no shadow resolution.
    /// </summary>
    private void AddGround()
    {
        const float overhang = 3f;
        float extent = Span * (1f + (2f * overhang));

        Mesh mesh = Track(QuadBuilder.Build(extent, extent, Vector4.One));
        var node = new MeshNode(
            mesh,
            new Material { Color = new Vector3(0.50f, 0.45f, 0.37f), SpecularStrength = 0.04f, Shininess = 12f })
        {
            CastsShadow = false,
        };

        // Lay the XY quad flat onto XZ, then slide it so the maze sits in the middle.
        node.Transform.Rotation = new Vector3(MathHelper.DegreesToRadians(-90f), 0f, 0f);
        node.Transform.Position = new Vector3(-Span * overhang, 0f, Span * (1f + overhang));

        Scene.Root.Add(node);
    }

    private void AddWalls(int seed)
    {
        Room[] rooms = new MazeGenerator(Rows, Columns, seed).Generate();

        Mesh wallMesh = Track(QuadBuilder.Build(RoomWidth, RoomHeight, Vector4.One));
        var wallMaterial = new Material { Color = new Vector3(0.60f, 0.58f, 0.55f), SpecularStrength = 0.08f, Shininess = 18f };

        for (int row = 0; row < Rows; row++)
        {
            for (int column = 0; column < Columns; column++)
            {
                Room room = rooms[(row * Columns) + column];
                Vector3 origin = new(column * (RoomWidth + Gap), 0f, row * (RoomWidth + Gap));

                for (int side = 0; side < 4; side++)
                {
                    if (room.Doors[side])
                    {
                        continue;
                    }

                    var node = new MeshNode(wallMesh, wallMaterial);
                    ConfigureWall(node, (Side)side, origin);
                    Scene.Root.Add(node);
                }
            }
        }
    }

    /// <summary>
    /// Places one wall of a room. Rotations are about Y, so each quad's local +X maps onto the
    /// world axis that wall runs along, and its +Z normal ends up pointing into the room.
    /// </summary>
    private static void ConfigureWall(MeshNode node, Side side, Vector3 origin)
    {
        switch (side)
        {
            case Side.Bottom:
                node.Transform.Position = origin;
                break;

            case Side.Top:
                node.Transform.Rotation = new Vector3(0f, MathHelper.DegreesToRadians(180f), 0f);
                node.Transform.Position = origin + new Vector3(RoomWidth, 0f, RoomWidth);
                break;

            case Side.Right:
                node.Transform.Rotation = new Vector3(0f, MathHelper.DegreesToRadians(-90f), 0f);
                node.Transform.Position = origin + new Vector3(RoomWidth, 0f, 0f);
                break;

            case Side.Left:
                node.Transform.Rotation = new Vector3(0f, MathHelper.DegreesToRadians(90f), 0f);
                node.Transform.Position = origin + new Vector3(0f, 0f, RoomWidth);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(side), side, "Unknown wall side.");
        }
    }

    private void AddOccluder()
    {
        Mesh mesh = Track(IcoSphereBuilder.Build(OccluderRadius, 3));
        var node = new MeshNode(mesh, new Material { Color = new Vector3(0.9f, 0.25f, 0.25f), Shininess = 64f });

        node.Transform.Position = OccluderPosition;
        Scene.Root.Add(node);
    }

    private void FrameDemoView()
    {
        Scene.Camera.NearPlane = 10f;
        Scene.Camera.FarPlane = 120_000f;

        if (Pose == CameraPose.ShadowDetail)
        {
            // Sideways onto the occluder-to-shadow axis so both ends fit the frame. Ground
            // distance stays under twice the eye height; past that a 400 unit wall rises into
            // the sight line and hides the shadow.
            Scene.Camera.Position = ShadowCentre + new Vector3(-625f, 1300f, 2375f);
            Scene.Camera.Yaw = MathHelper.DegreesToRadians(132f);
            Scene.Camera.Pitch = MathHelper.DegreesToRadians(-6f);
            return;
        }

        // Above wall height, so the framing does not depend on which doors a seed left open. Walls
        // are single sided with lit faces along +X and +Z, so holding the sun on screen would show
        // only unlit backs; 110 degrees off its bearing trades the sun disc for lit faces.
        Scene.Camera.Position = ShadowCentre + new Vector3(-1500f, 1400f, 2400f);
        Scene.Camera.Yaw = MathHelper.DegreesToRadians(150f);
        Scene.Camera.Pitch = MathHelper.DegreesToRadians(-22f);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        foreach (Mesh mesh in ownedMeshes)
        {
            mesh.Dispose();
        }

        disposed = true;
    }
}
