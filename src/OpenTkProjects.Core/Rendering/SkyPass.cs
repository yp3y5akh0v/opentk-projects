using OpenTK.Mathematics;
using OpenTkProjects.Core.Graphics;
using OpenTkProjects.Core.SceneGraph;

namespace OpenTkProjects.Core.Rendering;

/// <summary>
/// Fills the frame with procedural sky and the solar disc before geometry is drawn.
/// </summary>
/// <remarks>
/// A fullscreen triangle with the depth test off, so no skybox geometry is needed. Run first so it
/// doubles as the clear and leaves sky at far-plane depth, which the volumetric pass keys off.
/// </remarks>
public sealed class SkyPass(Shader shader, FullscreenTriangle triangle)
{
    public void Execute(Scene scene, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(scene);

        GraphicsDevice.SetDepthTest(false);
        GraphicsDevice.SetDepthWrite(false);

        shader.Use();
        AtmosphereUniforms.Apply(shader, scene);

        float aspectRatio = height == 0 ? 1f : width / (float)height;
        Matrix4 viewProjection = scene.Camera.GetViewMatrix() * scene.Camera.GetProjectionMatrix(aspectRatio);

        shader.SetUniform("inverseViewProjection", Matrix4.Invert(viewProjection));
        shader.SetUniform("cameraPos", scene.Camera.Position);

        triangle.Draw();

        GraphicsDevice.SetDepthWrite(true);
        GraphicsDevice.SetDepthTest(true);
    }
}
