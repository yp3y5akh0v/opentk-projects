using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTkProjects.Core.Graphics;
using OpenTkProjects.Core.Lighting;
using OpenTkProjects.Core.SceneGraph;

namespace OpenTkProjects.Core.Rendering;

/// <summary>
/// Draws lit geometry into the HDR target. Output is linear radiance; tone mapping happens once,
/// in the composite pass.
/// </summary>
public sealed class ForwardLitPass(Shader shader, ShadowMap shadowMap, AmbientOcclusionPass occlusion) : IRenderPass
{
    /// <summary>
    /// Upper bound on the soft shadow filter, in shadow texels. Penumbra width is derived from
    /// the sun's angular size, but a very distant blocker would otherwise ask for a kernel far
    /// too wide to sample at a sensible cost.
    /// </summary>
    public float MaxFilterTexels { get; set; } = 18f;

    public void Execute(IReadOnlyList<RenderItem> items, Scene scene, RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(scene);

        shader.Use();
        SetFrameUniforms(scene, context);
        shadowMap.BindForReading(TextureUnit.Texture0);
        occlusion.BindResult(TextureUnit.Texture2);

        for (int i = 0; i < items.Count; i++)
        {
            RenderItem item = items[i];

            shader.SetUniform("worldMatrix", item.World);

            // Non-uniform scale would otherwise skew normals away from the surface.
            shader.SetUniform("normalMatrix", Matrix4.Transpose(Matrix4.Invert(item.World)));

            shader.SetUniform("material.color", item.Material.Color);
            shader.SetUniform("material.shininess", item.Material.Shininess);
            shader.SetUniform("material.specularStrength", item.Material.SpecularStrength);

            if (item.Material.AlbedoMap is { } albedo)
            {
                albedo.Bind(TextureUnit.Texture1);
                shader.SetUniform("useAlbedoMap", 1f);
            }
            else
            {
                shader.SetUniform("useAlbedoMap", 0f);
            }

            item.Mesh.Draw();
        }
    }

    private void SetFrameUniforms(Scene scene, RenderContext context)
    {
        AtmosphereUniforms.Apply(shader, scene);

        shader.SetUniform("viewMatrix", scene.Camera.GetViewMatrix());
        shader.SetUniform("projectionMatrix", scene.Camera.GetProjectionMatrix(context.AspectRatio));
        shader.SetUniform("lightViewProjection", context.LightFrustum.ViewProjection);
        shader.SetUniform("cameraPos", scene.Camera.Position);
        shader.SetUniform("ambientStrength", scene.Atmosphere.AmbientStrength);

        shader.SetUniform("shadowMap", 0);
        shader.SetUniform("albedoMap", 1);
        shader.SetUniform("occlusionMap", 2);
        shader.SetUniform("screenSize", new Vector2(context.ViewportWidth, context.ViewportHeight));
        shader.SetUniform("shadowTexelSize", 1f / shadowMap.Width);
        shader.SetUniform("shadowTexelWorldSize", context.LightFrustum.TexelWorldSize(shadowMap.Width));
        shader.SetUniform("shadowDepthRange", context.LightFrustum.FarPlane - context.LightFrustum.NearPlane);
        shader.SetUniform("shadowMaxFilterTexels", MaxFilterTexels);

        // Penumbra width for a directional source follows from its angular radius alone:
        // width = 2 * blockerToReceiverDistance * tan(angularRadius).
        shader.SetUniform("sunTanAngularRadius", MathF.Tan(scene.Atmosphere.SunAngularRadius));
    }
}
