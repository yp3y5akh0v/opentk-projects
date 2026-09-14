using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTkProjects.Core.Graphics;
using OpenTkProjects.Core.Lighting;
using OpenTkProjects.Core.SceneGraph;

namespace OpenTkProjects.Core.Rendering;

/// <summary>
/// Ray marched single scattering: sunlight visibly travelling through the air, shadowed by the
/// same geometry that shadows the ground.
/// </summary>
/// <remarks>
/// Rendered at reduced resolution and upscaled bilinearly. Each pixel costs 64 shadow lookups, and
/// scattering is low frequency enough that the detail full resolution would buy is lost anyway.
/// </remarks>
public sealed class VolumetricPass : IDisposable
{
    private readonly Shader shader;
    private readonly FullscreenTriangle triangle;
    private readonly ShadowMap shadowMap;
    private readonly RenderTarget target;

    private bool disposed;

    public VolumetricPass(Shader shader, FullscreenTriangle triangle, ShadowMap shadowMap)
    {
        this.shader = shader;
        this.triangle = triangle;
        this.shadowMap = shadowMap;

        target = new RenderTarget(1, 1);
    }

    /// <summary>Resolution divisor. Two means quarter the pixels, four means a sixteenth.</summary>
    public int Downscale { get; set; } = 2;

    public bool Enabled { get; private set; }

    public void Execute(Scene scene, RenderTarget sceneTarget, RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(scene);
        ArgumentNullException.ThrowIfNull(sceneTarget);

        Atmosphere atmosphere = scene.Atmosphere;
        Enabled = atmosphere.ScatteringStrength > 0f;

        if (!Enabled)
        {
            return;
        }

        int width = Math.Max(context.ViewportWidth / Downscale, 1);
        int height = Math.Max(context.ViewportHeight / Downscale, 1);

        target.Resize(width, height);
        target.BindForWriting();

        GraphicsDevice.SetDepthTest(false);
        GraphicsDevice.SetDepthWrite(false);

        shader.Use();
        AtmosphereUniforms.Apply(shader, scene);

        Matrix4 viewProjection =
            scene.Camera.GetViewMatrix() * scene.Camera.GetProjectionMatrix(context.AspectRatio);

        shader.SetUniform("inverseViewProjection", Matrix4.Invert(viewProjection));
        shader.SetUniform("lightViewProjection", context.LightFrustum.ViewProjection);
        shader.SetUniform("cameraPos", scene.Camera.Position);

        shader.SetUniform("scatteringAnisotropy", atmosphere.ScatteringAnisotropy);
        shader.SetUniform("scatteringStrength", atmosphere.ScatteringStrength);
        shader.SetUniform("marchDistance", atmosphere.VolumetricDistance);

        shader.SetUniform("sceneDepth", 0);
        shader.SetUniform("shadowMap", 1);
        sceneTarget.BindDepth(TextureUnit.Texture0);
        shadowMap.BindForReading(TextureUnit.Texture1);

        triangle.Draw();

        GraphicsDevice.SetDepthWrite(true);
        GraphicsDevice.SetDepthTest(true);
    }

    public void BindResult(TextureUnit unit) => target.BindColor(unit);

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        target.Dispose();
        disposed = true;
    }
}
