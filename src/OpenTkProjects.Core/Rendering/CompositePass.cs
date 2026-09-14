using OpenTK.Graphics.OpenGL4;
using OpenTkProjects.Core.Graphics;
using OpenTkProjects.Core.SceneGraph;

namespace OpenTkProjects.Core.Rendering;

/// <summary>
/// Resolves the HDR scene target to the screen: adds in-scattering, then exposure, tone mapping
/// and sRGB encoding.
/// </summary>
public sealed class CompositePass(Shader shader, FullscreenTriangle triangle)
{
    public void Execute(Scene scene, RenderTarget source, VolumetricPass scattering, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(scene);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(scattering);

        RenderTarget.Unbind();
        GraphicsDevice.SetViewport(width, height);
        GraphicsDevice.SetDepthTest(false);

        shader.Use();
        shader.SetUniform("exposure", scene.Atmosphere.Exposure);

        shader.SetUniform("sceneColor", 0);
        source.BindColor(TextureUnit.Texture0);

        shader.SetUniform("scatteringColor", 1);
        shader.SetUniform("scatteringEnabled", scattering.Enabled ? 1f : 0f);

        if (scattering.Enabled)
        {
            scattering.BindResult(TextureUnit.Texture1);
        }
        else
        {
            // Sampling an unbound unit is undefined, so point it at something real.
            source.BindColor(TextureUnit.Texture1);
        }

        triangle.Draw();

        GraphicsDevice.SetDepthTest(true);
    }
}
