using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTkProjects.Core.SceneGraph;

namespace OpenTkProjects.Core.Geometry;

/// <summary>
/// CPU-side geometry. Holds no GL state, so builders and their tests need no context.
/// </summary>
public sealed class MeshData
{
    private readonly List<Vector3> positions = [];
    private readonly List<Vector3> normals = [];
    private readonly List<Vector2> texCoords = [];
    private readonly List<Vector4> colors = [];
    private readonly List<int> indices = [];

    public PrimitiveType Primitive { get; set; } = PrimitiveType.Triangles;

    public IReadOnlyList<Vector3> Positions => positions;

    public IReadOnlyList<Vector3> Normals => normals;

    public IReadOnlyList<Vector2> TexCoords => texCoords;

    public IReadOnlyList<Vector4> Colors => colors;

    public IReadOnlyList<int> Indices => indices;

    public bool HasNormals => normals.Count > 0;

    public bool HasTexCoords => texCoords.Count > 0;

    public bool HasColors => colors.Count > 0;

    public int AddVertex(Vector3 position)
    {
        positions.Add(position);
        return positions.Count - 1;
    }

    public void AddNormal(Vector3 normal) => normals.Add(normal);

    public void AddTexCoord(Vector2 texCoord) => texCoords.Add(texCoord);

    public void AddColor(Vector4 color) => colors.Add(color);

    public void AddTriangle(int a, int b, int c)
    {
        indices.Add(a);
        indices.Add(b);
        indices.Add(c);
    }

    public void AddLine(int a, int b)
    {
        indices.Add(a);
        indices.Add(b);
    }

    public Aabb ComputeBounds() => Aabb.FromPoints(positions);

    /// <summary>
    /// Catches an attribute buffer shorter than the position buffer, which GL would otherwise read
    /// past without complaint.
    /// </summary>
    public void Validate()
    {
        if (positions.Count == 0)
        {
            throw new InvalidOperationException("Mesh has no vertices.");
        }

        ValidateAttribute(normals.Count, nameof(Normals));
        ValidateAttribute(texCoords.Count, nameof(TexCoords));
        ValidateAttribute(colors.Count, nameof(Colors));

        foreach (int index in indices)
        {
            if (index < 0 || index >= positions.Count)
            {
                throw new InvalidOperationException(
                    $"Index {index} is outside the vertex range 0..{positions.Count - 1}.");
            }
        }
    }

    private void ValidateAttribute(int count, string name)
    {
        if (count != 0 && count != positions.Count)
        {
            throw new InvalidOperationException(
                $"{name} holds {count} entries but the mesh has {positions.Count} vertices.");
        }
    }
}
