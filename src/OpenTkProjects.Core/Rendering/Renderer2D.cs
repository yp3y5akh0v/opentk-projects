using OpenTK.Mathematics;
using OpenTkProjects.Core.Graphics;
using OpenTkProjects.Core.SceneGraph;

namespace OpenTkProjects.Core.Rendering;

/// <summary>
/// Flat unlit renderer for the 2D demos. No shadow pass, no lighting, one shader.
/// </summary>
public sealed class Renderer2D : IDisposable
{
    private readonly ShaderLibrary shaders = new();
    private readonly Shader shader;
    private readonly List<RenderItem> drawList = [];
    private bool disposed;

    public Renderer2D()
    {
        shader = shaders.Load("unlit.vert.glsl", "unlit.frag.glsl");
    }

    public Color4 ClearColor { get; set; } = new(0.07f, 0.08f, 0.11f, 1f);

    public void Render(Scene scene, Camera2D camera, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(scene);
        ArgumentNullException.ThrowIfNull(camera);

        SceneFlattener.Collect(scene, drawList);

        GraphicsDevice.SetViewport(width, height);
        GraphicsDevice.Clear(ClearColor);

        // Flat scenes have no meaningful depth, so layering follows scene order instead.
        GraphicsDevice.SetDepthTest(false);

        float aspectRatio = height == 0 ? 1f : width / (float)height;

        shader.Use();
        shader.SetUniform("viewProjection", camera.GetViewProjection(aspectRatio));

        for (int i = 0; i < drawList.Count; i++)
        {
            RenderItem item = drawList[i];

            shader.SetUniform("worldMatrix", item.World);
            shader.SetUniform("tint", item.Material.Color);

            item.Mesh.Draw();
        }
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        shaders.Dispose();
        disposed = true;
    }
}
