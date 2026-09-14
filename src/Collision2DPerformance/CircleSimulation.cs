using OpenTK.Mathematics;
using OpenTkProjects.Core.Spatial;

namespace Collision2DPerformance;

/// <summary>
/// Circles moving in a box, with quadtree broad-phase collision detection and elastic response.
/// Holds no GL resources, so it can be tested without a window or a GPU.
/// </summary>
public sealed class CircleSimulation
{
    /// <summary>Broad-phase candidates per node before it subdivides.</summary>
    private const int QuadTreeCapacity = 8;

    /// <summary>Fraction of an overlap corrected per step, to stop jitter from over-correction.</summary>
    private const float SeparationFactor = 0.8f;

    private readonly Vector2[] positions;
    private readonly Vector2[] velocities;
    private readonly float[] radii;
    private readonly float[] inverseMasses;
    private readonly bool[] colliding;

    private readonly QuadTree quadTree;
    private readonly List<int> candidates = [];
    private readonly Box2 world;
    private readonly float maxRadius;

    /// <summary>
    /// Total fraction of the world covered. Held constant so changing the count changes circle
    /// size instead of density, keeping the field readable at any count.
    /// </summary>
    private const float TargetCoverage = 0.09f;

    public CircleSimulation(int count, Vector2 worldSize, int seed)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        world = new Box2(0f, 0f, worldSize.X, worldSize.Y);
        quadTree = new QuadTree(world, QuadTreeCapacity);

        // Area per circle at the target coverage, converted back to a radius.
        float averageRadius = MathF.Sqrt(worldSize.X * worldSize.Y * TargetCoverage / (count * MathF.PI));
        float minRadius = averageRadius * 0.6f;
        float radiusSpread = averageRadius * 0.8f;

        positions = new Vector2[count];
        velocities = new Vector2[count];
        radii = new float[count];
        inverseMasses = new float[count];
        colliding = new bool[count];

        var random = new Random(seed);

        for (int i = 0; i < count; i++)
        {
            radii[i] = minRadius + (random.NextSingle() * radiusSpread);
            maxRadius = MathF.Max(maxRadius, radii[i]);

            // Mass follows area, so a large circle shrugs off a small one.
            inverseMasses[i] = 1f / (radii[i] * radii[i]);

            positions[i] = new Vector2(
                radii[i] + (random.NextSingle() * (worldSize.X - (2f * radii[i]))),
                radii[i] + (random.NextSingle() * (worldSize.Y - (2f * radii[i]))));

            float angle = random.NextSingle() * MathHelper.TwoPi;
            float speed = 30f + (random.NextSingle() * 70f);
            velocities[i] = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
        }
    }

    public int Count => positions.Length;

    /// <summary>Pairs found touching during the last step.</summary>
    public int CollidingPairs { get; private set; }

    /// <summary>
    /// Circles touching at least one other. Lower than twice <see cref="CollidingPairs"/> when a
    /// circle is caught between two others.
    /// </summary>
    public int CollidingCircles { get; private set; }

    /// <summary>Candidate pairs the narrow phase had to examine. Broad-phase quality shows here.</summary>
    public int CandidatePairs { get; private set; }

    /// <summary>Whether circles bounce off each other, or merely report contact.</summary>
    public bool ResolveCollisions { get; set; } = true;

    public Vector2 PositionOf(int index) => positions[index];

    public Vector2 VelocityOf(int index) => velocities[index];

    public float RadiusOf(int index) => radii[index];

    public bool IsColliding(int index) => colliding[index];

    public void Step(float deltaSeconds)
    {
        Integrate(deltaSeconds);
        RebuildQuadTree();
        DetectAndResolve();
    }

    /// <summary>Total kinetic energy. Elastic response should conserve this closely.</summary>
    public float TotalKineticEnergy()
    {
        float total = 0f;

        for (int i = 0; i < positions.Length; i++)
        {
            total += velocities[i].LengthSquared / inverseMasses[i];
        }

        return total * 0.5f;
    }

    private void Integrate(float deltaSeconds)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] += velocities[i] * deltaSeconds;

            // Reflect off the walls, clamping back inside so a circle cannot stick to an edge.
            if (positions[i].X - radii[i] < world.Min.X)
            {
                positions[i].X = world.Min.X + radii[i];
                velocities[i].X = MathF.Abs(velocities[i].X);
            }
            else if (positions[i].X + radii[i] > world.Max.X)
            {
                positions[i].X = world.Max.X - radii[i];
                velocities[i].X = -MathF.Abs(velocities[i].X);
            }

            if (positions[i].Y - radii[i] < world.Min.Y)
            {
                positions[i].Y = world.Min.Y + radii[i];
                velocities[i].Y = MathF.Abs(velocities[i].Y);
            }
            else if (positions[i].Y + radii[i] > world.Max.Y)
            {
                positions[i].Y = world.Max.Y - radii[i];
                velocities[i].Y = -MathF.Abs(velocities[i].Y);
            }
        }
    }

    private void RebuildQuadTree()
    {
        // Rebuilt wholesale: with everything moving each frame, an incremental update preserves
        // nothing and costs more.
        quadTree.Clear();

        for (int i = 0; i < positions.Length; i++)
        {
            quadTree.Insert(i, positions[i]);
        }
    }

    private void DetectAndResolve()
    {
        CollidingPairs = 0;
        CandidatePairs = 0;
        Array.Clear(colliding);

        for (int i = 0; i < positions.Length; i++)
        {
            // Using the global maximum radius makes this box a guaranteed superset, so the broad
            // phase can offer too many candidates but never miss a pair.
            float reach = radii[i] + maxRadius;

            candidates.Clear();
            quadTree.Query(QuadTree.CircleBounds(positions[i], reach), candidates);

            foreach (int other in candidates)
            {
                // Each unordered pair is examined exactly once.
                if (other <= i)
                {
                    continue;
                }

                CandidatePairs++;

                Vector2 delta = positions[other] - positions[i];
                float touching = radii[i] + radii[other];

                if (delta.LengthSquared > touching * touching)
                {
                    continue;
                }

                colliding[i] = true;
                colliding[other] = true;
                CollidingPairs++;

                if (ResolveCollisions)
                {
                    Resolve(i, other, delta, touching);
                }
            }
        }

        CollidingCircles = 0;

        foreach (bool flagged in colliding)
        {
            if (flagged)
            {
                CollidingCircles++;
            }
        }
    }

    /// <summary>
    /// Equal and opposite impulse along the contact normal, plus positional separation. Without
    /// the separation step, pairs still overlapping next frame take a second impulse and vibrate.
    /// </summary>
    private void Resolve(int a, int b, Vector2 delta, float touching)
    {
        float distance = delta.Length;

        // Exactly coincident centres have no defined normal; nudge them apart along X instead.
        Vector2 normal = distance > float.Epsilon ? delta / distance : Vector2.UnitX;

        float separating = Vector2.Dot(velocities[b] - velocities[a], normal);

        // Already moving apart. Applying an impulse here would suck them back together.
        if (separating < 0f)
        {
            float inverseMassSum = inverseMasses[a] + inverseMasses[b];
            float impulse = -2f * separating / inverseMassSum;

            velocities[a] -= normal * (impulse * inverseMasses[a]);
            velocities[b] += normal * (impulse * inverseMasses[b]);
        }

        float overlap = touching - distance;

        if (overlap > 0f)
        {
            Vector2 correction = normal * (overlap * SeparationFactor / (inverseMasses[a] + inverseMasses[b]));

            positions[a] -= correction * inverseMasses[a];
            positions[b] += correction * inverseMasses[b];
        }
    }
}
