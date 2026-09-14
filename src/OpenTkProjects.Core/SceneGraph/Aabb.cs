using OpenTK.Mathematics;

namespace OpenTkProjects.Core.SceneGraph;

/// <summary>
/// Axis-aligned bounding box. Pure math with no GL dependency, so it is testable without a context.
/// </summary>
public readonly record struct Aabb
{
    public Aabb(Vector3 min, Vector3 max)
    {
        Min = Vector3.ComponentMin(min, max);
        Max = Vector3.ComponentMax(min, max);
    }

    public Vector3 Min { get; }

    public Vector3 Max { get; }

    public Vector3 Center => (Min + Max) * 0.5f;

    public Vector3 Size => Max - Min;

    public float Radius => Size.Length * 0.5f;

    public static Aabb FromPoints(IReadOnlyList<Vector3> points)
    {
        ArgumentNullException.ThrowIfNull(points);

        if (points.Count == 0)
        {
            throw new ArgumentException("Cannot build bounds from an empty point set.", nameof(points));
        }

        Vector3 min = points[0];
        Vector3 max = points[0];

        for (int i = 1; i < points.Count; i++)
        {
            min = Vector3.ComponentMin(min, points[i]);
            max = Vector3.ComponentMax(max, points[i]);
        }

        return new Aabb(min, max);
    }

    public Aabb Union(Aabb other) =>
        new(Vector3.ComponentMin(Min, other.Min), Vector3.ComponentMax(Max, other.Max));

    public Aabb Encapsulate(Vector3 point) =>
        new(Vector3.ComponentMin(Min, point), Vector3.ComponentMax(Max, point));

    /// <summary>
    /// Transforms the eight corners and refits. Transforming <see cref="Min"/> and <see cref="Max"/>
    /// directly is wrong under rotation, because the extremes move to different corners.
    /// </summary>
    public Aabb Transformed(Matrix4 matrix)
    {
        Span<Vector3> corners = stackalloc Vector3[8];
        GetCorners(corners);

        Vector3 min = Vector3.TransformPosition(corners[0], matrix);
        Vector3 max = min;

        for (int i = 1; i < corners.Length; i++)
        {
            Vector3 transformed = Vector3.TransformPosition(corners[i], matrix);
            min = Vector3.ComponentMin(min, transformed);
            max = Vector3.ComponentMax(max, transformed);
        }

        return new Aabb(min, max);
    }

    public void GetCorners(Span<Vector3> destination)
    {
        if (destination.Length < 8)
        {
            throw new ArgumentException("Destination must hold at least 8 corners.", nameof(destination));
        }

        destination[0] = new Vector3(Min.X, Min.Y, Min.Z);
        destination[1] = new Vector3(Max.X, Min.Y, Min.Z);
        destination[2] = new Vector3(Min.X, Max.Y, Min.Z);
        destination[3] = new Vector3(Max.X, Max.Y, Min.Z);
        destination[4] = new Vector3(Min.X, Min.Y, Max.Z);
        destination[5] = new Vector3(Max.X, Min.Y, Max.Z);
        destination[6] = new Vector3(Min.X, Max.Y, Max.Z);
        destination[7] = new Vector3(Max.X, Max.Y, Max.Z);
    }

    public bool Contains(Vector3 point) =>
        point.X >= Min.X && point.X <= Max.X &&
        point.Y >= Min.Y && point.Y <= Max.Y &&
        point.Z >= Min.Z && point.Z <= Max.Z;
}
