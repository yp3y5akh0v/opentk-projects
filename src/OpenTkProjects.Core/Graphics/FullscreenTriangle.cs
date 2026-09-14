using OpenTK.Graphics.OpenGL4;

namespace OpenTkProjects.Core.Graphics;

/// <summary>
/// Draws a screen-covering triangle with no vertex data at all; the vertex shader derives the
/// corners from gl_VertexID. Core profile still requires a bound vertex array object even when it
/// holds nothing, which is the only reason this type exists.
/// </summary>
public sealed class FullscreenTriangle : IDisposable
{
    private readonly int vertexArray = GL.GenVertexArray();
    private bool disposed;

    public void Draw()
    {
        GL.BindVertexArray(vertexArray);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        GL.BindVertexArray(0);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        GL.DeleteVertexArray(vertexArray);
        disposed = true;
    }
}
