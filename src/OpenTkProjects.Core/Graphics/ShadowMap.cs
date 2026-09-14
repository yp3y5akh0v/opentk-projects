using OpenTK.Graphics.OpenGL4;

namespace OpenTkProjects.Core.Graphics;

/// <summary>Depth-only framebuffer used as the shadow pass render target.</summary>
public sealed class ShadowMap : IDisposable
{
    private readonly int framebuffer;
    private readonly Texture2D depthTexture;
    private bool disposed;

    public ShadowMap(int width, int height)
    {
        Width = width;
        Height = height;

        depthTexture = Texture2D.CreateDepth(width, height);

        framebuffer = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
        GL.FramebufferTexture2D(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.DepthAttachment,
            TextureTarget.Texture2D,
            depthTexture.Handle,
            0);

        // Depth only: no colour attachment exists, so both colour buffers must be disabled.
        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);

        FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        if (status != FramebufferErrorCode.FramebufferComplete)
        {
            depthTexture.Dispose();
            GL.DeleteFramebuffer(framebuffer);
            throw new InvalidOperationException($"Shadow map framebuffer is incomplete: {status}");
        }
    }

    public int Width { get; }

    public int Height { get; }

    public void BindForWriting()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
        GL.Viewport(0, 0, Width, Height);
    }

    public void BindForReading(TextureUnit unit) => depthTexture.Bind(unit);

    public static void Unbind() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        GL.DeleteFramebuffer(framebuffer);
        depthTexture.Dispose();
        disposed = true;
    }
}
