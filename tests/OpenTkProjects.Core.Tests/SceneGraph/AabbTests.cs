using OpenTK.Mathematics;
using OpenTkProjects.Core.SceneGraph;

namespace OpenTkProjects.Core.Tests.SceneGraph;

public class AabbTests
{
    [Fact]
    public void GetCorners_WritesEightDistinctCornersAllInsideTheBox()
    {
        var bounds = new Aabb(new Vector3(-1f, -2f, -3f), new Vector3(1f, 2f, 3f));

        Span<Vector3> corners = stackalloc Vector3[8];
        bounds.GetCorners(corners);

        var distinct = new HashSet<Vector3>();
        foreach (Vector3 corner in corners)
        {
            Assert.True(bounds.Contains(corner));
            distinct.Add(corner);
        }

        Assert.Equal(8, distinct.Count);
    }

    [Fact]
    public void GetCorners_RejectsAnUndersizedDestination()
    {
        var bounds = new Aabb(Vector3.Zero, Vector3.One);

        Assert.Throws<ArgumentException>(() =>
        {
            Span<Vector3> tooSmall = new Vector3[7];
            bounds.GetCorners(tooSmall);
        });
    }

    [Fact]
    public void Constructor_NormalizesSwappedMinAndMax()
    {
        var bounds = new Aabb(new Vector3(5f, 5f, 5f), new Vector3(-5f, -5f, -5f));

        Assert.Equal(new Vector3(-5f), bounds.Min);
        Assert.Equal(new Vector3(5f), bounds.Max);
    }

    [Fact]
    public void FromPoints_BoundsEveryInputPoint()
    {
        Vector3[] points =
        [
            new(0f, 0f, 0f),
            new(10f, -4f, 7f),
            new(-3f, 12f, 2f),
        ];

        Aabb bounds = Aabb.FromPoints(points);

        Assert.Equal(new Vector3(-3f, -4f, 0f), bounds.Min);
        Assert.Equal(new Vector3(10f, 12f, 7f), bounds.Max);
        Assert.All(points, p => Assert.True(bounds.Contains(p)));
    }

    [Fact]
    public void FromPoints_RejectsAnEmptySet()
    {
        Assert.Throws<ArgumentException>(() => Aabb.FromPoints([]));
    }

    [Fact]
    public void Union_CoversBothOperands()
    {
        var left = new Aabb(new Vector3(0f), new Vector3(1f));
        var right = new Aabb(new Vector3(5f), new Vector3(6f));

        Aabb union = left.Union(right);

        Assert.Equal(new Vector3(0f), union.Min);
        Assert.Equal(new Vector3(6f), union.Max);
    }

    [Fact]
    public void Encapsulate_GrowsToIncludeAnOutsidePoint()
    {
        var bounds = new Aabb(Vector3.Zero, Vector3.One);

        Aabb grown = bounds.Encapsulate(new Vector3(0f, 10f, 0f));

        Assert.Equal(10f, grown.Max.Y, 4);
        Assert.True(grown.Contains(new Vector3(0f, 10f, 0f)));
    }

    [Fact]
    public void Transformed_TranslationMovesBoundsByTheSameOffset()
    {
        var bounds = new Aabb(new Vector3(-1f), new Vector3(1f));
        Matrix4 translate = Matrix4.CreateTranslation(new Vector3(10f, 20f, 30f));

        Aabb moved = bounds.Transformed(translate);

        Assert.Equal(new Vector3(9f, 19f, 29f), moved.Min);
        Assert.Equal(new Vector3(11f, 21f, 31f), moved.Max);
    }

    [Fact]
    public void Transformed_RotationRefitsAroundEveryRotatedCorner()
    {
        var bounds = new Aabb(new Vector3(-1f), new Vector3(1f));
        Matrix4 rotate = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(45f));

        Aabb rotated = bounds.Transformed(rotate);

        Span<Vector3> corners = stackalloc Vector3[8];
        bounds.GetCorners(corners);
        foreach (Vector3 corner in corners)
        {
            Vector3 moved = Vector3.TransformPosition(corner, rotate);
            Assert.True(rotated.Contains(moved), $"Rotated corner {moved} fell outside {rotated}.");
        }

        // A 45 degree rotation about Y widens the XZ footprint from 2 to 2*sqrt(2).
        Assert.Equal(2f * MathF.Sqrt(2f), rotated.Size.X, 3);
    }

    [Fact]
    public void Center_AndSize_DescribeTheBox()
    {
        var bounds = new Aabb(new Vector3(0f, 0f, 0f), new Vector3(4f, 8f, 12f));

        Assert.Equal(new Vector3(2f, 4f, 6f), bounds.Center);
        Assert.Equal(new Vector3(4f, 8f, 12f), bounds.Size);
    }

    [Fact]
    public void Radius_IsHalfTheDiagonalLength()
    {
        var bounds = new Aabb(new Vector3(0f), new Vector3(2f, 0f, 0f));

        Assert.Equal(1f, bounds.Radius, 5);
    }

    [Fact]
    public void Contains_RejectsAPointOutsideOnASingleAxis()
    {
        var bounds = new Aabb(Vector3.Zero, Vector3.One);

        Assert.False(bounds.Contains(new Vector3(0.5f, 0.5f, 1.5f)));
    }
}
