using OpenTK.Mathematics;
using OpenTkProjects.Core.Lighting;

namespace OpenTkProjects.Core.SceneGraph;

public sealed class Scene
{
    public SceneNode Root { get; } = new();

    public Camera Camera { get; } = new();

    public DirectionalLight Light { get; set; } = new(new Vector3(-1f, -1f, -1f));

    public Atmosphere Atmosphere { get; set; } = new();

    public Aabb ComputeWorldBounds() => Accumulate(castersOnly: false) ?? new Aabb(Vector3.Zero, Vector3.Zero);

    /// <summary>
    /// Every caster plus the volume its shadow sweeps down to the lowest surface. Fitting to the
    /// whole scene instead would stretch the shadow map across empty ground, since a plane large
    /// enough to reach the fog horizon dwarfs the objects standing on it.
    /// </summary>
    public Aabb ComputeShadowBounds()
    {
        Aabb everything = ComputeWorldBounds();

        if (Accumulate(castersOnly: true) is not { } bounds)
        {
            return everything;
        }

        float drop = bounds.Max.Y - everything.Min.Y;

        if (drop <= 0f || MathF.Abs(Light.Direction.Y) < 1e-4f)
        {
            return bounds;
        }

        Vector3 sweep = Light.Direction * (drop / MathF.Abs(Light.Direction.Y));

        return bounds.Union(new Aabb(bounds.Min + sweep, bounds.Max + sweep));
    }

    private Aabb? Accumulate(bool castersOnly)
    {
        Aabb? total = null;

        foreach (SceneNode node in Root.Traverse())
        {
            if (node.LocalBounds is not { } local || (castersOnly && !node.CastsShadow))
            {
                continue;
            }

            Aabb world = local.Transformed(node.WorldMatrix);
            total = total is { } current ? current.Union(world) : world;
        }

        return total;
    }
}
