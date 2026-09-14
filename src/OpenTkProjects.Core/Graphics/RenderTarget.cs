using OpenTK.Graphics.OpenGL4;

namespace OpenTkProjects.Core.Graphics;

/// <summary>
/// Offscreen colour and depth pair the scene renders into before post-processing.
/// </summary>
/// <remarks>
/// Colour is 16-bit float so radiance above 1.0 survives to the composite pass, where tone mapping
/// happens once. Depth is a texture, not a renderbuffer, because the volumetric pass reads it.
/// </remarks>
public sealed class RenderTarget : IDisposable
{
    private int framebuffer;
    private int colorTexture;
    private int depthTexture;
    private bool disposed;

    public RenderTarget(int width, int height) => Resize(width, height);

    public int Width { get; private set; }

    public int Height { get; private set; }

    /// <summary>Reallocates only when the size actually changed, so it is safe to call per frame.</summary>
    public void Resize(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        if (width == Width && height == Height)
        {
            return;
        }

        Release();

        Width = width;
        Height = height;

        colorTexture = CreateAttachment(PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float);
        depthTexture = CreateAttachment(PixelInternalFormat.DepthComponent24, PixelFormat.DepthComponent, PixelType.Float);

        framebuffer = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
        GL.FramebufferTexture2D(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D,
            colorTexture,
            0);
        GL.FramebufferTexture2D(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.DepthAttachment,
            TextureTarget.Texture2D,
            depthTexture,
            0);

        FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        if (status != FramebufferErrorCode.FramebufferComplete)
        {
            throw new InvalidOperationException($"Render target framebuffer is incomplete: {status}");
        }
    }

    public void BindForWriting()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
        GL.Viewport(0, 0, Width, Height);
    }

    public void BindColor(TextureUnit unit) => Bind(unit, colorTexture);

    public void BindDepth(TextureUnit unit) => Bind(unit, depthTexture);

    public static void Unbind() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        Release();
        disposed = true;
    }

    private static void Bind(TextureUnit unit, int handle)
    {
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2D, handle);
    }

    private int CreateAttachment(PixelInternalFormat internalFormat, PixelFormat format, PixelType type)
    {
        int handle = GL.GenTexture();

        GL.BindTexture(TextureTarget.Texture2D, handle);
        GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, Width, Height, 0, format, type, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.BindTexture(TextureTarget.Texture2D, 0);

        return handle;
    }

    private void Release()
    {
        if (framebuffer != 0)
        {
            GL.DeleteFramebuffer(framebuffer);
            framebuffer = 0;
        }

        if (colorTexture != 0)
        {
            GL.DeleteTexture(colorTexture);
            colorTexture = 0;
        }

        if (depthTexture != 0)
        {
            GL.DeleteTexture(depthTexture);
            depthTexture = 0;
        }
    }
}
