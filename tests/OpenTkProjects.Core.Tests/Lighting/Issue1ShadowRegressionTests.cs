using OpenTK.Mathematics;
using OpenTkProjects.Core.Lighting;
using OpenTkProjects.Core.SceneGraph;

namespace OpenTkProjects.Core.Tests.Lighting;

/// <summary>
/// Scene constants copied verbatim from the 2019 MazeGen3D/Program.cs that issue #1 was filed against,
/// so the defect is pinned to exact numbers rather than to a description.
/// </summary>
public class Issue1ShadowRegressionTests
{
    private const int Rooms = 25;
    private const float RoomWidth = 800f;
    private const float Gap = 5f;
    private const float RoomHeight = 400f;
    private const float LightHeightOffset = 5000f;
    private const float PlayerRadius = 100f;
    private const float LightSphereRadius = 200f;
    private const float OccluderDistanceFromLight = 1000f;

    private const float LegacyNear = 25f;
    private const float LegacyFar = 1_000_000f;

    private static float Span => Rooms * (RoomWidth + Gap);

    private static Vector3 LightPosition =>
        new(Span / 2f, RoomHeight + (2f * PlayerRadius) + LightHeightOffset, Span / 2f);

    private static Vector3 LightDirection => Vector3.Normalize(new Vector3(-1f, -1f, -1f));

    private static Vector3 OccluderPosition => LightPosition + (OccluderDistanceFromLight * LightDirection);

    private static Vector3 GroundUnderOccluder => new(OccluderPosition.X, 0f, OccluderPosition.Z);

    private static Aabb SceneBounds =>
        new(Vector3.Zero, new Vector3(Span, LightPosition.Y + LightSphereRadius, Span));

    private static float LegacyNormalizedDepth(float distanceFromLight) =>
        (distanceFromLight - LegacyNear) / (LegacyFar - LegacyNear);

    private static float LegacyDepthGap()
    {
        float receiverDistance = LightPosition.Y / MathF.Abs(LightDirection.Y);
        return LegacyNormalizedDepth(receiverDistance) - LegacyNormalizedDepth(OccluderDistanceFromLight);
    }

    [Fact]
    public void LegacyBias_ExceedsTheDepthGap_WhichIsWhyTheGroundWasNeverShadowed()
    {
        float gap = LegacyDepthGap();

        // 2019 fragment.glsl: max(0.05 * (1.0 - dot(norm, lightDir)), 0.005),
        // where lightDir was light.direction, the direction light travels, rather than
        // the direction toward the light. That sign error inflates the bias.
        float legacyBias = MathF.Max(0.05f * (1f - Vector3.Dot(Vector3.UnitY, LightDirection)), 0.005f);

        Assert.Equal(0.0087f, gap, 4);
        Assert.Equal(0.0789f, legacyBias, 4);
        Assert.True(legacyBias > gap, $"Expected the legacy bias {legacyBias} to swallow the gap {gap}.");
    }

    [Fact]
    public void LegacyMinimumBias_ConsumesMostOfTheAvailableDepthGap()
    {
        float gap = LegacyDepthGap();

        // The floor of the 2019 bias expression, 0.005 normalized, is 5,000 world units across a
        // 999,975 unit range, against an occluder-to-ground gap of only about 8,700 units. The floor
        // alone does not block the shadow, but it leaves almost no margin. That near-miss is the
        // signature of a mis-scaled light frustum: it is the slope-scaled term, inflated to 0.0789
        // by the inverted direction, that actually swallows the gap.
        Assert.True(0.005f < gap, $"The floor bias should sit just under the gap, gap was {gap}.");
        Assert.InRange(0.005f / gap, 0.5f, 0.7f);
    }

    [Fact]
    public void CorrectingOnlyTheDirectionSign_IsNotEnough()
    {
        float gap = LegacyDepthGap();
        Vector3 toLight = -LightDirection;
        float signFixedBias = MathF.Max(0.05f * (1f - Vector3.Dot(Vector3.UnitY, toLight)), 0.005f);

        Assert.Equal(0.0211f, signFixedBias, 4);
        Assert.True(signFixedBias > gap,
            "Fixing the direction sign alone must still fail, which is why the frustum is also refitted.");
    }

    [Fact]
    public void FittedFrustum_GivesTheOccluderToReceiverGapRealHeadroom()
    {
        var light = new DirectionalLight(LightDirection);
        LightFrustum frustum = light.ComputeShadowFrustum(SceneBounds);

        float occluderDepth = frustum.NormalizedDepthOf(OccluderPosition);
        float receiverDepth = frustum.NormalizedDepthOf(GroundUnderOccluder);
        float gap = receiverDepth - occluderDepth;

        Assert.True(gap > 0.05f, $"Fitted frustum must leave real bias headroom, got {gap}.");
    }

    [Fact]
    public void FittedFrustum_ImprovesTheDepthGapByMoreThanTenFold()
    {
        var light = new DirectionalLight(LightDirection);
        LightFrustum frustum = light.ComputeShadowFrustum(SceneBounds);

        float fittedGap = frustum.NormalizedDepthOf(GroundUnderOccluder)
            - frustum.NormalizedDepthOf(OccluderPosition);

        Assert.True(fittedGap > LegacyDepthGap() * 10f,
            $"Fitted gap {fittedGap} should dwarf the legacy gap {LegacyDepthGap()}.");
    }

    [Fact]
    public void FittedFrustum_PlacesBothTheOccluderAndItsGroundInsideTheLightFrustum()
    {
        var light = new DirectionalLight(LightDirection);
        LightFrustum frustum = light.ComputeShadowFrustum(SceneBounds);

        foreach (Vector3 point in new[] { OccluderPosition, GroundUnderOccluder })
        {
            Vector3 ndc = frustum.ProjectToNdc(point);
            Assert.InRange(ndc.X, -1f, 1f);
            Assert.InRange(ndc.Y, -1f, 1f);
            Assert.InRange(ndc.Z, -1f, 1f);
        }
    }
}
