using OpenTK.Mathematics;
using OpenTkProjects.Core.Spatial;

namespace OpenTkProjects.Core.Tests.Spatial;

public class QuadTreeTests
{
    private static Box2 World => new(0f, 0f, 100f, 100f);

    [Fact]
    public void Insert_RejectsPointsOutsideBounds()
    {
        var tree = new QuadTree(World, capacity: 4);

        Assert.False(tree.Insert(1, new Vector2(-5f, 50f)));
        Assert.False(tree.Insert(2, new Vector2(50f, 140f)));
    }

    [Fact]
    public void Query_ReturnsOnlyPointsInsideTheRegion()
    {
        var tree = new QuadTree(World, capacity: 2);
        tree.Insert(1, new Vector2(10f, 10f));
        tree.Insert(2, new Vector2(90f, 90f));
        tree.Insert(3, new Vector2(12f, 12f));

        List<int> hits = [];
        tree.Query(new Box2(0f, 0f, 20f, 20f), hits);

        Assert.Equal(2, hits.Count);
        Assert.Contains(1, hits);
        Assert.Contains(3, hits);
    }

    [Fact]
    public void Query_FindsEveryPointWhenTheRegionCoversTheWorld()
    {
        var tree = new QuadTree(World, capacity: 2);

        for (int i = 0; i < 200; i++)
        {
            Assert.True(tree.Insert(i, new Vector2(i % 100, (i * 7) % 100)));
        }

        List<int> hits = [];
        tree.Query(World, hits);

        Assert.Equal(200, hits.Count);
    }

    [Fact]
    public void Query_MatchesBruteForceOnARandomCloud()
    {
        var random = new Random(20260913);
        var tree = new QuadTree(World, capacity: 4);
        var points = new List<Vector2>();

        for (int i = 0; i < 500; i++)
        {
            var point = new Vector2(random.NextSingle() * 100f, random.NextSingle() * 100f);
            points.Add(point);
            tree.Insert(i, point);
        }

        var region = new Box2(23f, 41f, 67f, 88f);

        List<int> expected = [];
        for (int i = 0; i < points.Count; i++)
        {
            if (points[i].X >= region.Min.X && points[i].X <= region.Max.X &&
                points[i].Y >= region.Min.Y && points[i].Y <= region.Max.Y)
            {
                expected.Add(i);
            }
        }

        List<int> actual = [];
        tree.Query(region, actual);

        Assert.Equal(expected.OrderBy(x => x), actual.OrderBy(x => x));
    }

    [Fact]
    public void Clear_EmptiesTheTree()
    {
        var tree = new QuadTree(World, capacity: 2);
        tree.Insert(1, new Vector2(10f, 10f));
        tree.Insert(2, new Vector2(20f, 20f));
        tree.Insert(3, new Vector2(30f, 30f));

        tree.Clear();

        List<int> hits = [];
        tree.Query(World, hits);

        Assert.Empty(hits);
    }

    [Fact]
    public void CircleBounds_EnclosesTheCircle()
    {
        Box2 box = QuadTree.CircleBounds(new Vector2(50f, 50f), 5f);

        Assert.Equal(45f, box.Min.X, 4);
        Assert.Equal(55f, box.Max.Y, 4);
    }

    [Fact]
    public void Constructor_RejectsNonPositiveCapacity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new QuadTree(World, capacity: 0));
    }
}
