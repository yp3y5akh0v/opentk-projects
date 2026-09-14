using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTkProjects.Core.Geometry;
using OpenTkProjects.Core.Graphics;
using OpenTkProjects.Maze;
using OpenTkProjects.Core.Rendering;
using OpenTkProjects.Core.SceneGraph;

namespace MazeGen2D;

/// <summary>
/// Draws the maze as it is carved. The whole grid is batched into one mesh rather than one node
/// per wall, so it costs a single draw call however large the maze gets.
/// </summary>
public sealed class MazeGrid : IDisposable
{
    private const float CellSize = 40f;
    private const float WallThickness = 4f;

    private static readonly Vector4 WallColor = new(0.86f, 0.89f, 0.96f, 1f);
    private static readonly Vector4 VisitedColor = new(0.17f, 0.21f, 0.31f, 1f);

    private readonly MazeGenerator generator;
    private readonly int rows;
    private readonly int columns;

    // Two fixed slots keep draw order stable while the wall geometry is replaced underneath.
    private readonly SceneNode wallHolder = new();
    private readonly MeshNode cursorNode;
    private readonly Mesh cursorMesh;

    private Mesh? wallMesh;
    private bool disposed;

    public MazeGrid(int rows, int columns, int seed)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rows);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(columns);

        this.rows = rows;
        this.columns = columns;

        generator = new MazeGenerator(rows, columns, seed);

        cursorMesh = new Mesh(Rect2DBuilder.Build(CellSize, CellSize, Vector4.One));
        cursorNode = new MeshNode(cursorMesh, new Material { Color = new Vector3(0.96f, 0.56f, 0.22f) });

        Scene.Root.Add(wallHolder);
        Scene.Root.Add(cursorNode);

        Refresh();
    }

    public Scene Scene { get; } = new();

    public bool IsComplete => generator.IsComplete;

    public Vector2 WorldMin => Vector2.Zero;

    public Vector2 WorldMax => new(columns * CellSize, rows * CellSize);

    /// <summary>Carves one room and refreshes the geometry.</summary>
    public void Step()
    {
        if (generator.Step())
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        int row = generator.CurrentIndex / columns;
        int column = generator.CurrentIndex % columns;

        cursorNode.Transform.Position = new Vector3(column * CellSize, row * CellSize, 0f);
        cursorNode.Visible = !generator.IsComplete;

        Mesh rebuilt = new(BuildWallMesh());

        wallHolder.Clear();
        wallHolder.Add(new MeshNode(rebuilt, new Material { Color = Vector3.One }));

        wallMesh?.Dispose();
        wallMesh = rebuilt;
    }

    private MeshData BuildWallMesh()
    {
        var data = new MeshData { Primitive = PrimitiveType.Triangles };

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                Room room = generator.Rooms[(row * columns) + column];
                Vector2 origin = new(column * CellSize, row * CellSize);

                if (room.Visited)
                {
                    AddRect(data, origin, new Vector2(CellSize, CellSize), VisitedColor);
                }

                if (!room.Doors[(int)Side.Bottom])
                {
                    AddRect(data, origin, new Vector2(CellSize, WallThickness), WallColor);
                }

                if (!room.Doors[(int)Side.Left])
                {
                    AddRect(data, origin, new Vector2(WallThickness, CellSize), WallColor);
                }

                if (!room.Doors[(int)Side.Top])
                {
                    AddRect(
                        data,
                        origin + new Vector2(0f, CellSize - WallThickness),
                        new Vector2(CellSize, WallThickness),
                        WallColor);
                }

                if (!room.Doors[(int)Side.Right])
                {
                    AddRect(
                        data,
                        origin + new Vector2(CellSize - WallThickness, 0f),
                        new Vector2(WallThickness, CellSize),
                        WallColor);
                }
            }
        }

        data.Validate();
        return data;
    }

    private static void AddRect(MeshData data, Vector2 origin, Vector2 size, Vector4 color)
    {
        int baseIndex = data.AddVertex(new Vector3(origin.X, origin.Y, 0f));
        data.AddVertex(new Vector3(origin.X + size.X, origin.Y, 0f));
        data.AddVertex(new Vector3(origin.X, origin.Y + size.Y, 0f));
        data.AddVertex(new Vector3(origin.X + size.X, origin.Y + size.Y, 0f));

        for (int i = 0; i < 4; i++)
        {
            data.AddColor(color);
        }

        data.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
        data.AddTriangle(baseIndex + 2, baseIndex + 1, baseIndex + 3);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        wallMesh?.Dispose();
        cursorMesh.Dispose();
        disposed = true;
    }
}
