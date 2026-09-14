using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTkProjects.Core.Geometry;
using OpenTkProjects.Core.SceneGraph;

namespace OpenTkProjects.Core.Tests.Geometry;

public class MeshDataTests
{
    [Fact]
    public void HasNormals_IsFalseUntilNormalsAreAdded()
    {
        var mesh = new MeshData();
        mesh.AddVertex(Vector3.Zero);

        Assert.False(mesh.HasNormals);

        mesh.AddNormal(Vector3.UnitY);

        Assert.True(mesh.HasNormals);
    }

    [Fact]
    public void Validate_ThrowsWhenNormalCountDoesNotMatchVertexCount()
    {
        var mesh = new MeshData();
        mesh.AddVertex(Vector3.Zero);
        mesh.AddVertex(Vector3.UnitX);
        mesh.AddNormal(Vector3.UnitY);

        Assert.Throws<InvalidOperationException>(mesh.Validate);
    }

    [Fact]
    public void Validate_AcceptsAnEntirelyEmptyOptionalAttribute()
    {
        var mesh = new MeshData();
        mesh.AddVertex(Vector3.Zero);
        mesh.AddNormal(Vector3.UnitY);
        mesh.AddTriangle(0, 0, 0);

        mesh.Validate();
    }

    [Fact]
    public void Validate_ThrowsOnAnIndexOutsideTheVertexRange()
    {
        var mesh = new MeshData();
        mesh.AddVertex(Vector3.Zero);
        mesh.AddTriangle(0, 1, 2);

        Assert.Throws<InvalidOperationException>(mesh.Validate);
    }

    [Fact]
    public void Validate_ThrowsWhenThereAreNoVertices()
    {
        Assert.Throws<InvalidOperationException>(new MeshData().Validate);
    }

    [Fact]
    public void ComputeBounds_CoversEveryVertex()
    {
        var mesh = new MeshData();
        mesh.AddVertex(new Vector3(-5f, 0f, 0f));
        mesh.AddVertex(new Vector3(5f, 3f, 0f));

        Aabb bounds = mesh.ComputeBounds();

        Assert.Equal(-5f, bounds.Min.X, 4);
        Assert.Equal(5f, bounds.Max.X, 4);
        Assert.Equal(3f, bounds.Max.Y, 4);
    }
}

public class GeometryBuilderTests
{
    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 80)]
    [InlineData(2, 320)]
    [InlineData(3, 1280)]
    public void IcoSphere_FaceCountIsTwentyTimesFourToTheSubdivision(int subdivisions, int expectedFaces)
    {
        MeshData mesh = IcoSphereBuilder.Build(1f, subdivisions);

        Assert.Equal(expectedFaces * 3, mesh.Indices.Count);
    }

    [Fact]
    public void IcoSphere_EveryVertexLiesOnTheSphere()
    {
        const float radius = 7.5f;
        MeshData mesh = IcoSphereBuilder.Build(radius, 3);

        Assert.All(mesh.Positions, p => Assert.Equal(radius, p.Length, 3));
    }

    [Fact]
    public void IcoSphere_NormalsPointOutwardAndAreUnitLength()
    {
        MeshData mesh = IcoSphereBuilder.Build(3f, 2);

        Assert.True(mesh.HasNormals);
        Assert.Equal(mesh.Positions.Count, mesh.Normals.Count);

        for (int i = 0; i < mesh.Positions.Count; i++)
        {
            Vector3 expected = Vector3.Normalize(mesh.Positions[i]);
            Assert.Equal(expected.X, mesh.Normals[i].X, 4);
            Assert.Equal(expected.Y, mesh.Normals[i].Y, 4);
            Assert.Equal(expected.Z, mesh.Normals[i].Z, 4);
        }
    }

    [Fact]
    public void IcoSphere_SharesMidpointVerticesBetweenAdjacentFaces()
    {
        // Without the midpoint cache each face would emit its own copies.
        // Euler characteristic for a subdivided icosahedron: V = 10 * 4^n + 2.
        MeshData mesh = IcoSphereBuilder.Build(1f, 2);

        Assert.Equal((10 * 16) + 2, mesh.Positions.Count);
    }

    [Fact]
    public void IcoSphere_BoundsAreTheSphereExtents()
    {
        MeshData mesh = IcoSphereBuilder.Build(10f, 3);
        Aabb bounds = mesh.ComputeBounds();

        Assert.Equal(10f, bounds.Max.X, 2);
        Assert.Equal(-10f, bounds.Min.X, 2);
    }

    [Fact]
    public void IcoSphere_RejectsNegativeSubdivisions()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => IcoSphereBuilder.Build(1f, -1));
    }

    [Fact]
    public void IcoSphere_RejectsNonPositiveRadius()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => IcoSphereBuilder.Build(0f, 1));
    }

    [Fact]
    public void Quad_HasFourVerticesTwoTrianglesAndAConsistentNormal()
    {
        MeshData mesh = QuadBuilder.Build(10f, 4f, Vector4.One);

        Assert.Equal(4, mesh.Positions.Count);
        Assert.Equal(6, mesh.Indices.Count);
        Assert.Equal(PrimitiveType.Triangles, mesh.Primitive);
        Assert.All(mesh.Normals, n => Assert.Equal(Vector3.UnitZ, n));
    }

    [Fact]
    public void Quad_SpansTheRequestedWidthAndHeight()
    {
        MeshData mesh = QuadBuilder.Build(10f, 4f, Vector4.One);
        Aabb bounds = mesh.ComputeBounds();

        Assert.Equal(10f, bounds.Size.X, 4);
        Assert.Equal(4f, bounds.Size.Y, 4);
    }

    [Fact]
    public void Segment_IsATwoVertexLinePrimitive()
    {
        MeshData mesh = SegmentBuilder.Build(Vector3.Zero, new Vector3(0f, 10f, 0f), Vector4.One);

        Assert.Equal(2, mesh.Positions.Count);
        Assert.Equal(PrimitiveType.Lines, mesh.Primitive);
        Assert.Equal(2, mesh.Indices.Count);
    }

    [Fact]
    public void Circle2D_HasACentreVertexPlusOnePerSegment()
    {
        MeshData mesh = Circle2DBuilder.Build(5f, 32, Vector4.One);

        Assert.Equal(33, mesh.Positions.Count);
        Assert.Equal(32 * 3, mesh.Indices.Count);
    }

    [Fact]
    public void Circle2D_RimVerticesSitOnTheRadius()
    {
        const float radius = 5f;
        MeshData mesh = Circle2DBuilder.Build(radius, 16, Vector4.One);

        for (int i = 1; i < mesh.Positions.Count; i++)
        {
            Assert.Equal(radius, mesh.Positions[i].Length, 4);
        }
    }

    [Fact]
    public void Circle2D_RejectsFewerThanThreeSegments()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Circle2DBuilder.Build(1f, 2, Vector4.One));
    }

    [Fact]
    public void Rect2D_HasFourVerticesAndTwoTriangles()
    {
        MeshData mesh = Rect2DBuilder.Build(4f, 6f, Vector4.One);

        Assert.Equal(4, mesh.Positions.Count);
        Assert.Equal(6, mesh.Indices.Count);
    }

    [Fact]
    public void EveryBuilder_ProducesAMeshThatValidates()
    {
        MeshData[] meshes =
        [
            IcoSphereBuilder.Build(2f, 2),
            QuadBuilder.Build(3f, 4f, Vector4.One),
            SegmentBuilder.Build(Vector3.Zero, Vector3.UnitX, Vector4.One),
            Circle2DBuilder.Build(1f, 8, Vector4.One),
            Rect2DBuilder.Build(1f, 1f, Vector4.One),
        ];

        Assert.All(meshes, mesh => mesh.Validate());
    }
}
