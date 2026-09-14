using OpenTK.Mathematics;
using OpenTkProjects.Core.Lighting;
using OpenTkProjects.Core.SceneGraph;

namespace OpenTkProjects.Core.Tests.SceneGraph;

public class TransformTests
{
    [Fact]
    public void ToMatrix_IsIdentityWhenDefaulted()
    {
        Assert.Equal(Matrix4.Identity, new Transform().ToMatrix());
    }

    [Fact]
    public void ToMatrix_AppliesScaleBeforeTranslation()
    {
        var transform = new Transform
        {
            Position = new Vector3(10f, 0f, 0f),
            Scale = 2f,
        };

        Vector3 result = Vector3.TransformPosition(new Vector3(1f, 0f, 0f), transform.ToMatrix());

        Assert.Equal(12f, result.X, 4);
    }

    [Fact]
    public void ToMatrix_RotatesAboutYBeforeTranslating()
    {
        var transform = new Transform
        {
            Rotation = new Vector3(0f, MathHelper.DegreesToRadians(90f), 0f),
            Position = new Vector3(0f, 0f, 5f),
        };

        Vector3 result = Vector3.TransformPosition(new Vector3(1f, 0f, 0f), transform.ToMatrix());

        Assert.Equal(0f, result.X, 4);
        Assert.Equal(4f, result.Z, 4);
    }
}

public class CameraTests
{
    [Fact]
    public void Forward_PointsAlongPositiveXAtYawNinetyDegrees()
    {
        var camera = new Camera { Yaw = MathHelper.DegreesToRadians(90f), Pitch = 0f };

        Assert.Equal(1f, camera.Forward.X, 4);
        Assert.Equal(0f, camera.Forward.Y, 4);
        Assert.Equal(0f, camera.Forward.Z, 4);
    }

    [Fact]
    public void Right_IsPerpendicularToForwardAndWorldUp()
    {
        var camera = new Camera { Yaw = MathHelper.DegreesToRadians(35f), Pitch = MathHelper.DegreesToRadians(20f) };

        Assert.Equal(0f, Vector3.Dot(camera.Right, Vector3.UnitY), 4);
        Assert.Equal(1f, camera.Right.Length, 4);
    }

    [Fact]
    public void Pitch_IsClampedBelowVertical()
    {
        var camera = new Camera { Pitch = MathHelper.DegreesToRadians(180f) };

        Assert.True(camera.Pitch < MathHelper.PiOver2);
        Assert.True(camera.Pitch > -MathHelper.PiOver2);
    }

    [Fact]
    public void GetProjectionMatrix_ProducesAFiniteMatrix()
    {
        var camera = new Camera();

        Matrix4 projection = camera.GetProjectionMatrix(16f / 9f);

        Assert.False(float.IsNaN(projection.M11));
        Assert.NotEqual(0f, projection.M11);
    }

    [Theory]
    [InlineData(100f, 0f, 0f)]
    [InlineData(0f, 0f, 100f)]
    [InlineData(-70f, 40f, 25f)]
    [InlineData(4462f, 0f, 4462f)]
    public void LookAt_PointsForwardAtTheTarget(float x, float y, float z)
    {
        var camera = new Camera { Position = new Vector3(10f, 50f, 10f) };
        var target = new Vector3(x, y, z);

        camera.LookAt(target);

        Vector3 expected = Vector3.Normalize(target - camera.Position);

        Assert.Equal(expected.X, camera.Forward.X, 3);
        Assert.Equal(expected.Y, camera.Forward.Y, 3);
        Assert.Equal(expected.Z, camera.Forward.Z, 3);
    }

    [Fact]
    public void LookAt_RejectsATargetAtTheCameraPosition()
    {
        var camera = new Camera { Position = new Vector3(5f, 5f, 5f) };

        Assert.Throws<ArgumentException>(() => camera.LookAt(new Vector3(5f, 5f, 5f)));
    }
}

public class SceneNodeTests
{
    [Fact]
    public void CastsShadow_DefaultsToTrue()
    {
        Assert.True(new SceneNode().CastsShadow);
    }

    [Fact]
    public void Visible_DefaultsToTrue()
    {
        Assert.True(new SceneNode().Visible);
    }

    [Fact]
    public void Traverse_VisitsSelfAndEveryDescendant()
    {
        var root = new SceneNode();
        var child = new SceneNode();
        var grandchild = new SceneNode();

        child.Add(grandchild);
        root.Add(child);

        Assert.Equal(3, root.Traverse().Count());
    }

    [Fact]
    public void WorldMatrix_ComposesParentAndChildTranslations()
    {
        var parent = new SceneNode();
        parent.Transform.Position = new Vector3(10f, 0f, 0f);

        var child = new SceneNode();
        child.Transform.Position = new Vector3(5f, 0f, 0f);
        parent.Add(child);

        Vector3 worldOrigin = Vector3.TransformPosition(Vector3.Zero, child.WorldMatrix);

        Assert.Equal(15f, worldOrigin.X, 4);
    }

    [Fact]
    public void Add_RejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new SceneNode().Add(null!));
    }
}

public class SceneTests
{
    [Fact]
    public void ComputeWorldBounds_UnionsChildBoundsInWorldSpace()
    {
        var scene = new Scene();

        var left = new SceneNode { LocalBounds = new Aabb(new Vector3(-1f), new Vector3(1f)) };
        left.Transform.Position = new Vector3(-10f, 0f, 0f);

        var right = new SceneNode { LocalBounds = new Aabb(new Vector3(-1f), new Vector3(1f)) };
        right.Transform.Position = new Vector3(10f, 0f, 0f);

        scene.Root.Add(left);
        scene.Root.Add(right);

        Aabb bounds = scene.ComputeWorldBounds();

        Assert.Equal(-11f, bounds.Min.X, 4);
        Assert.Equal(11f, bounds.Max.X, 4);
    }

    [Fact]
    public void ComputeWorldBounds_IgnoresNodesWithoutBounds()
    {
        var scene = new Scene();

        scene.Root.Add(new SceneNode());
        var sized = new SceneNode { LocalBounds = new Aabb(Vector3.Zero, new Vector3(4f)) };
        scene.Root.Add(sized);

        Aabb bounds = scene.ComputeWorldBounds();

        Assert.Equal(new Vector3(4f), bounds.Max);
    }

    [Fact]
    public void ComputeWorldBounds_ReturnsAnEmptyBoxForAnEmptyScene()
    {
        Aabb bounds = new Scene().ComputeWorldBounds();

        Assert.Equal(Vector3.Zero, bounds.Min);
        Assert.Equal(Vector3.Zero, bounds.Max);
    }

    [Fact]
    public void ComputeWorldBounds_FeedsAFrustumThatContainsTheWholeScene()
    {
        var scene = new Scene { Light = new DirectionalLight(new Vector3(-1f, -1f, -1f)) };

        var ground = new SceneNode { LocalBounds = new Aabb(Vector3.Zero, new Vector3(5000f, 0f, 5000f)) };
        scene.Root.Add(ground);

        var sphere = new SceneNode { LocalBounds = new Aabb(new Vector3(-50f), new Vector3(50f)) };
        sphere.Transform.Position = new Vector3(2500f, 900f, 2500f);
        scene.Root.Add(sphere);

        Aabb bounds = scene.ComputeWorldBounds();
        LightFrustum frustum = scene.Light.ComputeShadowFrustum(bounds);

        Span<Vector3> corners = stackalloc Vector3[8];
        bounds.GetCorners(corners);

        foreach (Vector3 corner in corners)
        {
            Vector3 ndc = frustum.ProjectToNdc(corner);
            Assert.InRange(ndc.X, -1.0001f, 1.0001f);
            Assert.InRange(ndc.Y, -1.0001f, 1.0001f);
            Assert.InRange(ndc.Z, -1.0001f, 1.0001f);
        }
    }
}
