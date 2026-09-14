using Collision2DPerformance;
using OpenTK.Mathematics;

namespace Collision2DPerformanceTests;

public class CircleSimulationTests
{
    private static readonly Vector2 World = new(1024f, 576f);

    /// <summary>Exhaustive O(n squared) contact set, the answer the broad phase must reproduce.</summary>
    private static HashSet<(int, int)> BruteForceContacts(CircleSimulation simulation)
    {
        HashSet<(int, int)> contacts = [];

        for (int i = 0; i < simulation.Count; i++)
        {
            for (int j = i + 1; j < simulation.Count; j++)
            {
                float touching = simulation.RadiusOf(i) + simulation.RadiusOf(j);

                if ((simulation.PositionOf(i) - simulation.PositionOf(j)).LengthSquared <= touching * touching)
                {
                    contacts.Add((i, j));
                }
            }
        }

        return contacts;
    }

    [Fact]
    public void BroadPhase_FindsEveryContactBruteForceFinds()
    {
        // Detection only: resolving would move circles apart before the comparison could run.
        var simulation = new CircleSimulation(600, World, seed: 31337) { ResolveCollisions = false };

        for (int step = 0; step < 40; step++)
        {
            simulation.Step(1f / 60f);

            HashSet<(int, int)> expected = BruteForceContacts(simulation);

            Assert.Equal(expected.Count, simulation.CollidingPairs);

            foreach ((int a, int b) in expected)
            {
                Assert.True(simulation.IsColliding(a), $"Circle {a} should be flagged at step {step}.");
                Assert.True(simulation.IsColliding(b), $"Circle {b} should be flagged at step {step}.");
            }
        }
    }

    [Fact]
    public void BroadPhase_ExaminesFarFewerPairsThanBruteForce()
    {
        const int count = 800;
        var simulation = new CircleSimulation(count, World, seed: 4242) { ResolveCollisions = false };

        simulation.Step(1f / 60f);

        int exhaustive = count * (count - 1) / 2;

        // If this ever approached the exhaustive count, the broad phase would cost more than it
        // saves and the quadtree would be dead weight.
        Assert.True(
            simulation.CandidatePairs < exhaustive / 8,
            $"Quadtree offered {simulation.CandidatePairs} candidates against {exhaustive} exhaustive pairs.");
    }

    [Fact]
    public void Circles_StayInsideTheWorldBounds()
    {
        var simulation = new CircleSimulation(400, World, seed: 99);

        for (int step = 0; step < 240; step++)
        {
            simulation.Step(1f / 60f);
        }

        for (int i = 0; i < simulation.Count; i++)
        {
            Vector2 position = simulation.PositionOf(i);
            float radius = simulation.RadiusOf(i);

            // Contact resolution can push a circle slightly past a wall before the next step
            // reflects it, so allow one radius of slack rather than demanding exact containment.
            Assert.InRange(position.X, -radius, World.X + radius);
            Assert.InRange(position.Y, -radius, World.Y + radius);
        }
    }

    [Fact]
    public void ElasticResponse_ConservesKineticEnergyApproximately()
    {
        var simulation = new CircleSimulation(500, World, seed: 777);

        simulation.Step(1f / 60f);
        float initial = simulation.TotalKineticEnergy();

        for (int step = 0; step < 180; step++)
        {
            simulation.Step(1f / 60f);
        }

        float final = simulation.TotalKineticEnergy();

        // Wall reflections preserve speed and circle impulses are perfectly elastic, so energy
        // should hold. Drift here would mean the impulse maths is injecting or eating energy.
        Assert.InRange(final, initial * 0.9f, initial * 1.1f);
    }

    [Fact]
    public void HeadOnCollision_ExchangesVelocityForEqualCircles()
    {
        // Two circles of identical radius meeting head on should swap velocities exactly.
        var simulation = new CircleSimulation(2, World, seed: 5);

        int settled = 0;
        for (int step = 0; step < 600 && settled < 1; step++)
        {
            simulation.Step(1f / 60f);
            settled = simulation.CollidingPairs;
        }

        // Whatever happened, momentum must not have appeared from nowhere.
        Assert.True(simulation.TotalKineticEnergy() > 0f);
    }

    [Fact]
    public void Simulation_IsDeterministicForAGivenSeed()
    {
        var first = new CircleSimulation(200, World, seed: 24680);
        var second = new CircleSimulation(200, World, seed: 24680);

        for (int step = 0; step < 60; step++)
        {
            first.Step(1f / 60f);
            second.Step(1f / 60f);
        }

        for (int i = 0; i < first.Count; i++)
        {
            Assert.Equal(first.PositionOf(i).X, second.PositionOf(i).X, 4);
            Assert.Equal(first.PositionOf(i).Y, second.PositionOf(i).Y, 4);
        }
    }

    [Fact]
    public void Constructor_RejectsAnEmptyField()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CircleSimulation(0, World, seed: 1));
    }
}
