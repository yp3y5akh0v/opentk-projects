using OpenTK.Mathematics;

namespace OpenTkProjects.Core.Spatial;

/// <summary>
/// Point quadtree over integer ids, used to cut broad-phase collision from O(n squared) to
/// something closer to O(n log n).
/// </summary>
/// <remarks>
/// Points stay in the node they were inserted into rather than being redistributed on subdivide,
/// so <see cref="Query"/> checks a node's own points and then recurses. Avoids a redistribution
/// pass that can drop points sitting on a boundary.
/// </remarks>
public sealed class QuadTree
{
    private readonly Box2 bounds;
    private readonly int capacity;
    private readonly List<int> ids = [];
    private readonly List<Vector2> positions = [];

    private QuadTree[]? children;

    public QuadTree(Box2 bounds, int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);

        this.bounds = bounds;
        this.capacity = capacity;
    }

    public bool Insert(int id, Vector2 position)
    {
        if (!Contains(bounds, position))
        {
            return false;
        }

        if (children is null)
        {
            if (ids.Count < capacity)
            {
                ids.Add(id);
                positions.Add(position);
                return true;
            }

            Subdivide();
        }

        foreach (QuadTree child in children!)
        {
            if (child.Insert(id, position))
            {
                return true;
            }
        }

        // Floating point can leave a point on a seam that no child claims. Keeping it here is
        // correct because Query always inspects a node's own points as well as its children.
        ids.Add(id);
        positions.Add(position);
        return true;
    }

    public void Query(Box2 region, List<int> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        if (!Intersects(bounds, region))
        {
            return;
        }

        for (int i = 0; i < positions.Count; i++)
        {
            if (Contains(region, positions[i]))
            {
                results.Add(ids[i]);
            }
        }

        if (children is null)
        {
            return;
        }

        foreach (QuadTree child in children)
        {
            child.Query(region, results);
        }
    }

    public void Clear()
    {
        ids.Clear();
        positions.Clear();
        children = null;
    }

    public static Box2 CircleBounds(Vector2 center, float radius) =>
        new(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius);

    private static bool Contains(Box2 box, Vector2 point) =>
        point.X >= box.Min.X && point.X <= box.Max.X &&
        point.Y >= box.Min.Y && point.Y <= box.Max.Y;

    private static bool Intersects(Box2 a, Box2 b) =>
        a.Min.X <= b.Max.X && a.Max.X >= b.Min.X &&
        a.Min.Y <= b.Max.Y && a.Max.Y >= b.Min.Y;

    private void Subdivide()
    {
        Vector2 min = bounds.Min;
        Vector2 max = bounds.Max;
        Vector2 mid = (min + max) * 0.5f;

        children =
        [
            new QuadTree(new Box2(min.X, min.Y, mid.X, mid.Y), capacity),
            new QuadTree(new Box2(mid.X, min.Y, max.X, mid.Y), capacity),
            new QuadTree(new Box2(min.X, mid.Y, mid.X, max.Y), capacity),
            new QuadTree(new Box2(mid.X, mid.Y, max.X, max.Y), capacity),
        ];
    }
}
