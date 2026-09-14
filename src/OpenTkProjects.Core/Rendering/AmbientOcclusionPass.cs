using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTkProjects.Core.Graphics;
using OpenTkProjects.Core.SceneGraph;

namespace OpenTkProjects.Core.Rendering;

/// <summary>
/// Renders view-space normals, derives screen-space ambient occlusion from them and the depth
/// buffer, and blurs the result.
/// </summary>
/// <remarks>
/// Runs before the lit pass: occlusion modulates ambient, which is baked into radiance as soon as
/// a surface is shaded. Applied afterwards it could only dim sunlight along with skylight.
/// </remarks>
public sealed class AmbientOcclusionPass : IDisposable
{
    private readonly Shader normalShader;
    private readonly Shader occlusionShader;
    private readonly Shader blurShader;
    private readonly FullscreenTriangle triangle;

    private readonly RenderTarget normalTarget;
    private readonly RenderTarget occlusionTarget;
    private readonly RenderTarget blurredTarget;

    private bool disposed;

    public AmbientOcclusionPass(
        Shader normalShader,
        Shader occlusionShader,
        Shader blurShader,
        FullscreenTriangle triangle)
    {
        this.normalShader = normalShader;
        this.occlusionShader = occlusionShader;
        this.blurShader = blurShader;
        this.triangle = triangle;

        normalTarget = new RenderTarget(1, 1);
        occlusionTarget = new RenderTarget(1, 1);
        blurredTarget = new RenderTarget(1, 1);
    }

    /// <summary>World-space radius of the sampling hemisphere. Scene scale decides this.</summary>
    public float Radius { get; set; } = 90f;

    /// <summary>Depth tolerance before a sample counts as occluding, which prevents self-occlusion.</summary>
    public float Bias { get; set; } = 2.5f;

    public float Intensity { get; set; } = 1f;

    /// <summary>Resolution divisor for the normal, occlusion and blur targets.</summary>
    public int Downscale { get; set; } = 2;

    public void Execute(IReadOnlyList<RenderItem> items, Scene scene, RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(scene);

        // Occlusion is smooth and low frequency, and it is blurred afterwards regardless, so full
        // resolution buys nothing but fragment cost.
        int width = Math.Max(context.ViewportWidth / Downscale, 1);
        int height = Math.Max(context.ViewportHeight / Downscale, 1);

        normalTarget.Resize(width, height);
        occlusionTarget.Resize(width, height);
        blurredTarget.Resize(width, height);

        Matrix4 view = scene.Camera.GetViewMatrix();
        Matrix4 projection = scene.Camera.GetProjectionMatrix(context.AspectRatio);

        RenderNormals(items, view, projection);
        RenderOcclusion(projection);
        RenderBlur(width, height);
    }

    public void BindResult(TextureUnit unit) => blurredTarget.BindColor(unit);

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        normalTarget.Dispose();
        occlusionTarget.Dispose();
        blurredTarget.Dispose();
        disposed = true;
    }

    private void RenderNormals(IReadOnlyList<RenderItem> items, Matrix4 view, Matrix4 projection)
    {
        normalTarget.BindForWriting();
        GraphicsDevice.Clear(new Color4(0f, 0f, 1f, 1f));
        GraphicsDevice.SetDepthTest(true);
        GraphicsDevice.SetDepthWrite(true);

        normalShader.Use();
        normalShader.SetUniform("viewMatrix", view);
        normalShader.SetUniform("projectionMatrix", projection);

        for (int i = 0; i < items.Count; i++)
        {
            RenderItem item = items[i];

            normalShader.SetUniform("worldMatrix", item.World);
            normalShader.SetUniform("normalMatrix", Matrix4.Transpose(Matrix4.Invert(item.World)));

            item.Mesh.Draw();
        }
    }

    private void RenderOcclusion(Matrix4 projection)
    {
        occlusionTarget.BindForWriting();
        GraphicsDevice.SetDepthTest(false);

        occlusionShader.Use();
        occlusionShader.SetUniform("projectionMatrix", projection);
        occlusionShader.SetUniform("inverseProjectionMatrix", Matrix4.Invert(projection));
        occlusionShader.SetUniform("radius", Radius);
        occlusionShader.SetUniform("bias", Bias);
        occlusionShader.SetUniform("intensity", Intensity);

        occlusionShader.SetUniform("normalTexture", 0);
        occlusionShader.SetUniform("depthTexture", 1);
        normalTarget.BindColor(TextureUnit.Texture0);
        normalTarget.BindDepth(TextureUnit.Texture1);

        triangle.Draw();
    }

    private void RenderBlur(int width, int height)
    {
        blurredTarget.BindForWriting();

        blurShader.Use();
        blurShader.SetUniform("texelSize", new Vector2(1f / width, 1f / height));
        blurShader.SetUniform("source", 0);
        occlusionTarget.BindColor(TextureUnit.Texture0);

        triangle.Draw();

        GraphicsDevice.SetDepthTest(true);
    }
}
