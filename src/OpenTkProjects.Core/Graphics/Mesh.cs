using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTkProjects.Core.Geometry;
using OpenTkProjects.Core.SceneGraph;

namespace OpenTkProjects.Core.Graphics;

public enum MeshUsage
{
    /// <summary>Uploaded once and never changed.</summary>
    Static,

    /// <summary>Vertex data is respecified each frame, as with a simulated cloth.</summary>
    Dynamic,
}

/// <summary>
/// A VAO plus buffers for only the attributes the source data carries, so an absent attribute is
/// never left enabled against a zero-byte buffer.
/// </summary>
public sealed class Mesh : IDisposable
{
    private const int PositionSlot = 0;
    private const int NormalSlot = 1;
    private const int TexCoordSlot = 2;
    private const int ColorSlot = 3;

    private readonly List<int> buffers = [];
    private readonly int vertexArray;
    private readonly int indexCount;
    private readonly int vertexCount;
    private readonly PrimitiveType primitive;
    private readonly BufferUsageHint usage;

    private int positionBuffer;
    private int normalBuffer;
    private bool disposed;

    public Mesh(MeshData data, MeshUsage meshUsage = MeshUsage.Static)
    {
        ArgumentNullException.ThrowIfNull(data);
        data.Validate();

        Bounds = data.ComputeBounds();
        primitive = data.Primitive;
        indexCount = data.Indices.Count;
        vertexCount = data.Positions.Count;
        usage = meshUsage == MeshUsage.Dynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw;

        vertexArray = GL.GenVertexArray();
        GL.BindVertexArray(vertexArray);

        positionBuffer = UploadVec3(PositionSlot, data.Positions);

        if (data.HasNormals)
        {
            normalBuffer = UploadVec3(NormalSlot, data.Normals);
        }

        if (data.HasTexCoords)
        {
            UploadVec2(TexCoordSlot, data.TexCoords);
        }

        if (data.HasColors)
        {
            UploadVec4(ColorSlot, data.Colors);
        }

        int indexBuffer = GL.GenBuffer();
        buffers.Add(indexBuffer);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
        GL.BufferData(
            BufferTarget.ElementArrayBuffer,
            indexCount * sizeof(int),
            data.Indices.ToArray(),
            BufferUsageHint.StaticDraw);

        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    public Aabb Bounds { get; }

    /// <summary>Attribute enable state lives inside the VAO, so binding it is enough.</summary>
    public void Draw()
    {
        GL.BindVertexArray(vertexArray);
        GL.DrawElements(primitive, indexCount, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        foreach (int buffer in buffers)
        {
            GL.DeleteBuffer(buffer);
        }

        GL.DeleteVertexArray(vertexArray);
        disposed = true;
    }

    /// <summary>
    /// Replaces the position buffer in place. Only valid on a mesh created with
    /// <see cref="MeshUsage.Dynamic"/>, which is what the cloth simulation uses each frame.
    /// </summary>
    public void UpdatePositions(IReadOnlyList<Vector3> values) =>
        Respecify(positionBuffer, values, nameof(values));

    /// <summary>Replaces the normal buffer in place. See <see cref="UpdatePositions"/>.</summary>
    public void UpdateNormals(IReadOnlyList<Vector3> values) =>
        Respecify(normalBuffer, values, nameof(values));

    private void Respecify(int buffer, IReadOnlyList<Vector3> values, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (usage != BufferUsageHint.DynamicDraw)
        {
            throw new InvalidOperationException("Mesh was not created with MeshUsage.Dynamic.");
        }

        if (buffer == 0)
        {
            throw new InvalidOperationException("Mesh has no buffer bound to that attribute.");
        }

        if (values.Count != vertexCount)
        {
            throw new ArgumentException(
                $"Expected {vertexCount} values but received {values.Count}.", parameterName);
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, values.Count * 3 * sizeof(float), Flatten(values));
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    private static float[] Flatten(IReadOnlyList<Vector3> values)
    {
        float[] flat = new float[values.Count * 3];

        for (int i = 0; i < values.Count; i++)
        {
            flat[(3 * i) + 0] = values[i].X;
            flat[(3 * i) + 1] = values[i].Y;
            flat[(3 * i) + 2] = values[i].Z;
        }

        return flat;
    }

    private int UploadVec2(int slot, IReadOnlyList<Vector2> values)
    {
        float[] flat = new float[values.Count * 2];

        for (int i = 0; i < values.Count; i++)
        {
            flat[(2 * i) + 0] = values[i].X;
            flat[(2 * i) + 1] = values[i].Y;
        }

        return Upload(slot, 2, flat);
    }

    private int UploadVec3(int slot, IReadOnlyList<Vector3> values) => Upload(slot, 3, Flatten(values));

    private int UploadVec4(int slot, IReadOnlyList<Vector4> values)
    {
        float[] flat = new float[values.Count * 4];

        for (int i = 0; i < values.Count; i++)
        {
            flat[(4 * i) + 0] = values[i].X;
            flat[(4 * i) + 1] = values[i].Y;
            flat[(4 * i) + 2] = values[i].Z;
            flat[(4 * i) + 3] = values[i].W;
        }

        return Upload(slot, 4, flat);
    }

    private int Upload(int slot, int componentCount, float[] data)
    {
        int buffer = GL.GenBuffer();
        buffers.Add(buffer);

        GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
        GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, usage);
        GL.VertexAttribPointer(
            slot,
            componentCount,
            VertexAttribPointerType.Float,
            false,
            componentCount * sizeof(float),
            0);
        GL.EnableVertexAttribArray(slot);

        return buffer;
    }
}
