using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OpenTkProjects.Core.Geometry;

/// <summary>A flat quad on the XY plane with its normal along +Z, spanning (0,0) to (width, height).</summary>
public static class QuadBuilder
{
    public static MeshData Build(float width, float height, Vector4 color)
    {
        var mesh = new MeshData { Primitive = PrimitiveType.Triangles };

        mesh.AddVertex(Vector3.Zero);
        mesh.AddVertex(new Vector3(width, 0f, 0f));
        mesh.AddVertex(new Vector3(0f, height, 0f));
        mesh.AddVertex(new Vector3(width, height, 0f));

        for (int i = 0; i < 4; i++)
        {
            mesh.AddNormal(Vector3.UnitZ);
            mesh.AddColor(color);
        }

        mesh.AddTexCoord(new Vector2(0f, 0f));
        mesh.AddTexCoord(new Vector2(1f, 0f));
        mesh.AddTexCoord(new Vector2(0f, 1f));
        mesh.AddTexCoord(new Vector2(1f, 1f));

        mesh.AddTriangle(0, 1, 2);
        mesh.AddTriangle(2, 1, 3);

        mesh.Validate();
        return mesh;
    }
}

public static class SegmentBuilder
{
    public static MeshData Build(Vector3 start, Vector3 end, Vector4 color)
    {
        var mesh = new MeshData { Primitive = PrimitiveType.Lines };

        mesh.AddVertex(start);
        mesh.AddVertex(end);

        for (int i = 0; i < 2; i++)
        {
            mesh.AddNormal(Vector3.UnitY);
            mesh.AddColor(color);
        }

        mesh.AddLine(0, 1);

        mesh.Validate();
        return mesh;
    }
}

/// <summary>A triangle fan on the XY plane: one centre vertex plus one rim vertex per segment.</summary>
public static class Circle2DBuilder
{
    public static MeshData Build(float radius, int segments, Vector4 color)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(segments, 3);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(radius);

        var mesh = new MeshData { Primitive = PrimitiveType.Triangles };

        mesh.AddVertex(Vector3.Zero);
        mesh.AddNormal(Vector3.UnitZ);
        mesh.AddColor(color);

        for (int i = 0; i < segments; i++)
        {
            float angle = MathHelper.TwoPi * i / segments;
            mesh.AddVertex(new Vector3(radius * MathF.Cos(angle), radius * MathF.Sin(angle), 0f));
            mesh.AddNormal(Vector3.UnitZ);
            mesh.AddColor(color);
        }

        for (int i = 0; i < segments; i++)
        {
            int current = i + 1;
            int next = ((i + 1) % segments) + 1;
            mesh.AddTriangle(0, current, next);
        }

        mesh.Validate();
        return mesh;
    }
}

public static class Rect2DBuilder
{
    public static MeshData Build(float width, float height, Vector4 color) =>
        QuadBuilder.Build(width, height, color);
}
