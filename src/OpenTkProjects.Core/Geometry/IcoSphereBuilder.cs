using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OpenTkProjects.Core.Geometry;

public static class IcoSphereBuilder
{
    public static MeshData Build(float radius, int subdivisions)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(subdivisions);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(radius);

        float phi = (1f + MathF.Sqrt(5f)) / 2f;

        List<Vector3> vertices =
        [
            OnSphere(new Vector3(-1f, phi, 0f), radius), OnSphere(new Vector3(1f, phi, 0f), radius),
            OnSphere(new Vector3(-1f, -phi, 0f), radius), OnSphere(new Vector3(1f, -phi, 0f), radius),
            OnSphere(new Vector3(0f, -1f, phi), radius), OnSphere(new Vector3(0f, 1f, phi), radius),
            OnSphere(new Vector3(0f, -1f, -phi), radius), OnSphere(new Vector3(0f, 1f, -phi), radius),
            OnSphere(new Vector3(phi, 0f, -1f), radius), OnSphere(new Vector3(phi, 0f, 1f), radius),
            OnSphere(new Vector3(-phi, 0f, -1f), radius), OnSphere(new Vector3(-phi, 0f, 1f), radius),
        ];

        List<(int A, int B, int C)> faces =
        [
            (0, 11, 5), (0, 5, 1), (0, 1, 7), (0, 7, 10), (0, 10, 11),
            (1, 5, 9), (5, 11, 4), (11, 10, 2), (10, 7, 6), (7, 1, 8),
            (3, 9, 4), (3, 4, 2), (3, 2, 6), (3, 6, 8), (3, 8, 9),
            (4, 9, 5), (2, 4, 11), (6, 2, 10), (8, 6, 7), (9, 8, 1),
        ];

        Dictionary<long, int> midpointCache = [];

        for (int level = 0; level < subdivisions; level++)
        {
            List<(int A, int B, int C)> next = new(faces.Count * 4);

            foreach ((int a, int b, int c) in faces)
            {
                int ab = Midpoint(vertices, midpointCache, a, b, radius);
                int bc = Midpoint(vertices, midpointCache, b, c, radius);
                int ca = Midpoint(vertices, midpointCache, c, a, radius);

                next.Add((a, ab, ca));
                next.Add((b, bc, ab));
                next.Add((c, ca, bc));
                next.Add((ab, bc, ca));
            }

            faces = next;
        }

        var mesh = new MeshData { Primitive = PrimitiveType.Triangles };

        foreach (Vector3 vertex in vertices)
        {
            mesh.AddVertex(vertex);

            // Exact for a sphere centred on the origin: the radial direction is the normal.
            mesh.AddNormal(Vector3.Normalize(vertex));
        }

        foreach ((int a, int b, int c) in faces)
        {
            mesh.AddTriangle(a, b, c);
        }

        mesh.Validate();
        return mesh;
    }

    private static Vector3 OnSphere(Vector3 direction, float radius) => Vector3.Normalize(direction) * radius;

    /// <summary>
    /// Returns a shared index for the midpoint of an edge so adjacent faces reuse one vertex.
    /// </summary>
    private static int Midpoint(List<Vector3> vertices, Dictionary<long, int> cache, int a, int b, float radius)
    {
        long lower = Math.Min(a, b);
        long upper = Math.Max(a, b);
        long key = (lower << 32) + upper;

        if (cache.TryGetValue(key, out int existing))
        {
            return existing;
        }

        vertices.Add(OnSphere((vertices[a] + vertices[b]) * 0.5f, radius));

        int index = vertices.Count - 1;
        cache[key] = index;
        return index;
    }
}
