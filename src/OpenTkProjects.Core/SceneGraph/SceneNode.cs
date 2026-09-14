using OpenTK.Mathematics;

namespace OpenTkProjects.Core.SceneGraph;

public class SceneNode
{
    private readonly List<SceneNode> children = [];

    public Transform Transform { get; } = new();

    public SceneNode? Parent { get; private set; }

    public bool Visible { get; set; } = true;

    /// <summary>
    /// Whether this node is drawn into the shadow depth pass. The light's own geometry sets this
    /// false, which is what stops it stamping itself across the centre of the shadow map.
    /// </summary>
    public bool CastsShadow { get; set; } = true;

    public Aabb? LocalBounds { get; set; }

    public IReadOnlyList<SceneNode> Children => children;

    public Matrix4 WorldMatrix =>
        Parent is null ? Transform.ToMatrix() : Transform.ToMatrix() * Parent.WorldMatrix;

    public void Add(SceneNode child)
    {
        ArgumentNullException.ThrowIfNull(child);

        child.Parent = this;
        children.Add(child);
    }

    /// <summary>
    /// Detaches every child. Used by demos that rebuild geometry in place, so their draw order
    /// stays fixed instead of shifting as nodes are replaced.
    /// </summary>
    public void Clear()
    {
        foreach (SceneNode child in children)
        {
            child.Parent = null;
        }

        children.Clear();
    }

    public IEnumerable<SceneNode> Traverse()
    {
        yield return this;

        foreach (SceneNode child in children)
        {
            foreach (SceneNode descendant in child.Traverse())
            {
                yield return descendant;
            }
        }
    }
}
