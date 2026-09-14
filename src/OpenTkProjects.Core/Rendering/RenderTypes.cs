using OpenTK.Mathematics;
using OpenTkProjects.Core.Graphics;
using OpenTkProjects.Core.Lighting;
using OpenTkProjects.Core.SceneGraph;

namespace OpenTkProjects.Core.Rendering;

public sealed class Material
{
    /// <summary>Base colour in sRGB. Multiplied over <see cref="AlbedoMap"/> when one is set.</summary>
    public Vector3 Color { get; set; } = new(0.6f, 0.6f, 0.6f);

    public float Shininess { get; set; } = 32f;

    public float SpecularStrength { get; set; } = 0.3f;

    /// <summary>Optional albedo texture. Requires the mesh to carry texture coordinates.</summary>
    public Texture2D? AlbedoMap { get; set; }
}

/// <summary>A scene node that draws a mesh. Its local bounds come from the mesh itself.</summary>
public sealed class MeshNode : SceneNode
{
    public MeshNode(Mesh mesh, Material material)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(material);

        Mesh = mesh;
        Material = material;
        LocalBounds = mesh.Bounds;
    }

    public Mesh Mesh { get; }

    public Material Material { get; }
}

/// <summary>
/// One flattened draw. Building these once per frame means the scene graph is walked a single
/// time and each world matrix is computed once, rather than once per pass.
/// </summary>
public readonly record struct RenderItem(Mesh Mesh, Matrix4 World, Material Material, bool CastsShadow);

public readonly record struct RenderContext(int ViewportWidth, int ViewportHeight, LightFrustum LightFrustum)
{
    public float AspectRatio => ViewportHeight == 0 ? 1f : ViewportWidth / (float)ViewportHeight;
}

public interface IRenderPass
{
    void Execute(IReadOnlyList<RenderItem> items, Scene scene, RenderContext context);
}

/// <summary>Flattens a scene graph into a draw list. Shared by the 3D and 2D renderers.</summary>
internal static class SceneFlattener
{
    public static void Collect(Scene scene, List<RenderItem> destination)
    {
        destination.Clear();

        foreach (SceneNode node in scene.Root.Traverse())
        {
            if (node is not MeshNode meshNode || !meshNode.Visible)
            {
                continue;
            }

            destination.Add(new RenderItem(
                meshNode.Mesh,
                meshNode.WorldMatrix,
                meshNode.Material,
                meshNode.CastsShadow));
        }
    }
}
