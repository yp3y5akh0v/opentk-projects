using OpenTkProjects.Core.Graphics;
using OpenTkProjects.Core.SceneGraph;

namespace OpenTkProjects.Core.Rendering;

/// <summary>Renders scene depth from the light's point of view into the shadow map.</summary>
public sealed class ShadowDepthPass(Shader shader, ShadowMap shadowMap) : IRenderPass
{
    /// <summary>Slope-scaled component of the depth offset, in depth-buffer units.</summary>
    public float PolygonOffsetFactor { get; set; } = 1.2f;

    /// <summary>
    /// Constant component of the depth offset, in units of the smallest resolvable depth
    /// difference. Kept small because the fragment shader's normal offset does most of the work;
    /// a large value here is what causes shadows to visibly detach from their casters.
    /// </summary>
    public float PolygonOffsetUnits { get; set; } = 2f;

    public void Execute(IReadOnlyList<RenderItem> items, Scene scene, RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(items);

        shadowMap.BindForWriting();
        GraphicsDevice.ClearDepthOnly();
        GraphicsDevice.SetPolygonOffset(PolygonOffsetFactor, PolygonOffsetUnits);

        shader.Use();
        shader.SetUniform("lightViewProjection", context.LightFrustum.ViewProjection);

        for (int i = 0; i < items.Count; i++)
        {
            RenderItem item = items[i];

            if (!item.CastsShadow)
            {
                continue;
            }

            shader.SetUniform("worldMatrix", item.World);
            item.Mesh.Draw();
        }

        GraphicsDevice.ClearPolygonOffset();
        ShadowMap.Unbind();
    }
}
