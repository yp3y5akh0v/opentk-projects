using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace OpenTkProjects.Core.Graphics;

/// <summary>
/// A 2D texture. Uses bind-then-modify entry points throughout rather than direct state access,
/// which would require GL 4.5.
/// </summary>
public sealed class Texture2D : IDisposable
{
    private bool disposed;

    private Texture2D(int handle, int width, int height)
    {
        Handle = handle;
        Width = width;
        Height = height;
    }

    public int Handle { get; }

    public int Width { get; }

    public int Height { get; }

    public static Texture2D FromStream(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        StbImage.stbi_set_flip_vertically_on_load(1);
        ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        int handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, handle);
        GL.TexImage2D(
            TextureTarget.Texture2D,
            0,
            PixelInternalFormat.Rgba8,
            image.Width,
            image.Height,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            image.Data);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.BindTexture(TextureTarget.Texture2D, 0);

        return new Texture2D(handle, image.Width, image.Height);
    }

    /// <summary>
    /// Depth target for shadow mapping, configured for hardware depth comparison.
    /// </summary>
    /// <remarks>
    /// Deliberately left as a plain depth texture with no comparison mode. Percentage-closer soft
    /// shadows need the actual blocker depth during the search phase, which a comparison sampler
    /// cannot return, so the depth test happens in the shader instead. Filtering is nearest for
    /// the same reason: linear filtering would average raw depths, which is meaningless.
    /// The white border colour means a fragment sampled outside the map reads the farthest depth
    /// and resolves as lit, backing up the explicit frustum guard in the fragment shader.
    /// </remarks>
    public static Texture2D CreateDepth(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        int handle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, handle);
        GL.TexImage2D(
            TextureTarget.Texture2D,
            0,
            PixelInternalFormat.DepthComponent24,
            width,
            height,
            0,
            PixelFormat.DepthComponent,
            PixelType.Float,
            IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, new[] { 1f, 1f, 1f, 1f });
        GL.BindTexture(TextureTarget.Texture2D, 0);

        return new Texture2D(handle, width, height);
    }

    public void Bind(TextureUnit unit)
    {
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2D, Handle);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        GL.DeleteTexture(Handle);
        disposed = true;
    }
}
