using OpenTK.Mathematics;
using OpenTkProjects.Core.Lighting;
using OpenTkProjects.Core.SceneGraph;

namespace OpenTkProjects.Core.Tests.Lighting;

public class DirectionalLightTests
{
    private static Aabb Scene => new(Vector3.Zero, new Vector3(20125f, 5800f, 20125f));

    [Fact]
    public void Constructor_NormalizesDirection()
    {
        var light = new DirectionalLight(new Vector3(0f, -10f, 0f));

        Assert.Equal(new Vector3(0f, -1f, 0f), light.Direction);
    }

    [Fact]
    public void Constructor_RejectsZeroDirection()
    {
        Assert.Throws<ArgumentException>(() => new DirectionalLight(Vector3.Zero));
    }

    [Fact]
    public void ToLight_IsTheNegatedTravelDirection()
    {
        var light = new DirectionalLight(new Vector3(-1f, -1f, -1f));

        Assert.Equal(-light.Direction, light.ToLight);
    }

    [Theory]
    [InlineData(-1f, -1f, -1f)]
    [InlineData(0f, -1f, 0f)]
    [InlineData(1f, -2f, 0.5f)]
    [InlineData(-0.3f, -1f, 0.8f)]
    public void ComputeShadowFrustum_ContainsEverySceneCorner(float x, float y, float z)
    {
        var light = new DirectionalLight(new Vector3(x, y, z));
        LightFrustum frustum = light.ComputeShadowFrustum(Scene);

        Span<Vector3> corners = stackalloc Vector3[8];
        Scene.GetCorners(corners);

        foreach (Vector3 corner in corners)
        {
            Vector3 ndc = frustum.ProjectToNdc(corner);
            Assert.InRange(ndc.X, -1.0001f, 1.0001f);
            Assert.InRange(ndc.Y, -1.0001f, 1.0001f);
            Assert.InRange(ndc.Z, -1.0001f, 1.0001f);
        }
    }

    [Fact]
    public void ComputeShadowFrustum_ProjectsSceneCentreNearNdcOrigin()
    {
        var light = new DirectionalLight(new Vector3(-1f, -1f, -1f));
        LightFrustum frustum = light.ComputeShadowFrustum(Scene);

        Vector3 ndc = frustum.ProjectToNdc(Scene.Center);

        Assert.Equal(0f, ndc.X, 3);
        Assert.Equal(0f, ndc.Y, 3);
    }

    [Fact]
    public void ComputeShadowFrustum_DepthRangeTracksSceneSizeNotAMagicConstant()
    {
        var light = new DirectionalLight(new Vector3(-1f, -1f, -1f));
        LightFrustum frustum = light.ComputeShadowFrustum(Scene);

        float depthRange = frustum.FarPlane - frustum.NearPlane;

        Assert.InRange(depthRange, Scene.Size.Length * 0.5f, Scene.Size.Length * 2f);
    }

    [Fact]
    public void ComputeShadowFrustum_KeepsTheNearPlanePositive()
    {
        var light = new DirectionalLight(new Vector3(-1f, -1f, -1f));
        LightFrustum frustum = light.ComputeShadowFrustum(Scene);

        Assert.True(frustum.NearPlane > 0f);
        Assert.True(frustum.FarPlane > frustum.NearPlane);
    }

    [Fact]
    public void ComputeShadowFrustum_ScalesWithTheScene_NotWithAFixedNumber()
    {
        var light = new DirectionalLight(new Vector3(-1f, -1f, -1f));

        var small = new Aabb(Vector3.Zero, new Vector3(10f));
        var large = new Aabb(Vector3.Zero, new Vector3(10_000f));

        float smallRange = Range(light.ComputeShadowFrustum(small));
        float largeRange = Range(light.ComputeShadowFrustum(large));

        Assert.True(largeRange > smallRange * 100f,
            $"A 1000x larger scene must widen the depth range, got {smallRange} and {largeRange}.");

        static float Range(LightFrustum frustum) => frustum.FarPlane - frustum.NearPlane;
    }

    [Fact]
    public void ComputeShadowFrustum_IsStableForADegenerateFlatScene()
    {
        var light = new DirectionalLight(new Vector3(0f, -1f, 0f));
        var flat = new Aabb(Vector3.Zero, new Vector3(100f, 0f, 100f));

        LightFrustum frustum = light.ComputeShadowFrustum(flat);

        Span<Vector3> corners = stackalloc Vector3[8];
        flat.GetCorners(corners);

        foreach (Vector3 corner in corners)
        {
            Vector3 ndc = frustum.ProjectToNdc(corner);
            Assert.InRange(ndc.Z, -1.0001f, 1.0001f);
        }
    }

    /// <summary>
    /// A small scene viewed down a long camera range. No shadow can land outside the casters' XY
    /// footprint, so fitting to the view would spend most texels on ground that is provably lit.
    /// </summary>
    [Fact]
    public void ComputeShadowFrustum_FitsTheCastersWhenTheViewReachesWellPastThem()
    {
        var light = new DirectionalLight(new Vector3(-0.55f, -0.62f, -0.55f));
        var casters = new Aabb(new Vector3(-250f, -285f, -250f), new Vector3(306f, 277f, 4f));

        Span<Vector3> distantView =
        [
            new(-4000f, -4000f, -4000f),
            new(4000f, 4000f, 4000f),
        ];

        LightFrustum frustum = light.ComputeShadowFrustum(distantView, casters, shadowMapResolution: 4096);

        Assert.InRange(frustum.WorldWidth, 0f, casters.Radius * 3f);
        Assert.InRange(frustum.TexelWorldSize(4096), 0f, 0.5f);
    }

    [Fact]
    public void ComputeShadowFrustum_FitsTheViewWhenTheCastersReachWellPastIt()
    {
        var light = new DirectionalLight(new Vector3(-1f, -1f, -1f));
        Aabb casters = Scene;

        Span<Vector3> nearView =
        [
            new(9000f, 0f, 9000f),
            new(11000f, 500f, 11000f),
        ];

        LightFrustum frustum = light.ComputeShadowFrustum(nearView, casters, shadowMapResolution: 4096);

        // The camera sees a corner of a very large maze, so the view is what limits the fit.
        Assert.InRange(frustum.WorldWidth, 0f, casters.Radius);
    }

    [Fact]
    public void ComputeShadowFrustum_StillCoversEveryCasterShadowAfterFittingToTheCasters()
    {
        var light = new DirectionalLight(new Vector3(-0.55f, -0.62f, -0.55f));
        var casters = new Aabb(new Vector3(-250f, -285f, -250f), new Vector3(306f, 277f, 4f));

        Span<Vector3> distantView =
        [
            new(-4000f, -4000f, -4000f),
            new(4000f, 4000f, 4000f),
        ];

        LightFrustum frustum = light.ComputeShadowFrustum(distantView, casters, shadowMapResolution: 4096);

        Span<Vector3> corners = stackalloc Vector3[8];
        casters.GetCorners(corners);

        foreach (Vector3 corner in corners)
        {
            Vector3 ndc = frustum.ProjectToNdc(corner);

            Assert.InRange(ndc.X, -1.0001f, 1.0001f);
            Assert.InRange(ndc.Y, -1.0001f, 1.0001f);
            Assert.InRange(ndc.Z, -1.0001f, 1.0001f);
        }
    }

    [Fact]
    public void NormalizedDepthOf_IsZeroAtTheNearPlaneAndOneAtTheFarPlane()
    {
        var light = new DirectionalLight(new Vector3(0f, -1f, 0f));
        var scene = new Aabb(Vector3.Zero, new Vector3(100f, 100f, 100f));
        LightFrustum frustum = light.ComputeShadowFrustum(scene);

        // Straight down, so the topmost point is nearest the light and the lowest is farthest.
        float top = frustum.NormalizedDepthOf(new Vector3(50f, 100f, 50f));
        float bottom = frustum.NormalizedDepthOf(new Vector3(50f, 0f, 50f));

        Assert.InRange(top, 0f, 1f);
        Assert.InRange(bottom, 0f, 1f);
        Assert.True(bottom > top, "A point farther from the light must have greater depth.");
    }
}
