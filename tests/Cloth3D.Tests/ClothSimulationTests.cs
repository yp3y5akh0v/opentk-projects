using OpenTK.Mathematics;

namespace Cloth3D.Tests;

/// <summary>Direction a measurement walks the particle grid.</summary>
public enum Axis
{
    Row,
    Column,
}

public class ClothSimulationTests
{
    private const int Rows = 34;
    private const int Columns = 52;
    private const float Spacing = 6f;
    private const float Step = 1f / 120f;

    private static readonly Vector3 Gravity = new(0f, -175f, 0f);

    [Theory]
    [InlineData(0, 5)]
    [InlineData(5, 0)]
    [InlineData(2, 1)]
    public void Constructor_RejectsAGridThatCannotFormAQuad(int rows, int columns)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ClothSimulation(rows, columns, Spacing));
    }

    [Fact]
    public void Constructor_RejectsNonPositiveSpacing()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ClothSimulation(4, 4, 0f));
    }

    [Fact]
    public void Step_LeavesPinnedParticlesExactlyWhereTheyStarted()
    {
        ClothSimulation cloth = BuildFlag();
        Vector3 hoistTop = cloth.PositionAt(0, 0);

        Advance(cloth, seconds: 4f);

        Assert.Equal(hoistTop, cloth.PositionAt(0, 0));
        Assert.True(cloth.IsPinned(Rows - 1, 0));
    }

    /// <summary>
    /// Without bending resistance the cheapest way for the sheet to shed compression is to fold at
    /// the grid's shortest wavelength, one cell, which reads on screen as a serrated comb.
    /// </summary>
    /// <remarks>
    /// Both directions are measured: the buckle ran down the columns at 1.95 while the rows never
    /// passed 0.31, so a row-only check stays green on a visibly serrated sheet.
    /// </remarks>
    [Theory]
    [InlineData(Axis.Row)]
    [InlineData(Axis.Column)]
    public void Step_DoesNotBuckleAtTheOneCellWavelength(Axis axis)
    {
        ClothSimulation cloth = BuildFlag();

        Advance(cloth, seconds: 4f);

        Assert.InRange(PeakCurvature(cloth, axis), 0f, 0.7f);
    }

    [Fact]
    public void Step_HoldsSpringsNearTheirRestLength()
    {
        ClothSimulation cloth = BuildFlag();

        Advance(cloth, seconds: 4f);

        float longest = 0f;
        for (int row = 0; row < Rows; row++)
        {
            for (int column = 0; column + 1 < Columns; column++)
            {
                float length = (cloth.PositionAt(row, column + 1) - cloth.PositionAt(row, column)).Length;
                longest = MathF.Max(longest, length);
            }
        }

        // Inside the 10 percent Provot allows mass-spring cloth; relaxation alone leaves 22 percent.
        Assert.InRange(longest, Spacing * 0.9f, Spacing * 1.07f);
    }

    [Fact]
    public void Step_SettlesToRestWhenTheAirIsStill()
    {
        ClothSimulation cloth = BuildFlag();

        Advance(cloth, seconds: 30f, wind: false);

        Vector3 freeCorner = cloth.PositionAt(0, Columns - 1);
        Advance(cloth, seconds: 1f, wind: false);

        Assert.True(
            (cloth.PositionAt(0, Columns - 1) - freeCorner).Length < 1f,
            "A hanging flag in still air should come to rest.");
    }

    /// <summary>
    /// Largest second difference along one grid direction, in units of the grid spacing. A smooth
    /// drape keeps this well below one; a one-cell zigzag of amplitude a reports 4a/spacing.
    /// </summary>
    private static float PeakCurvature(ClothSimulation cloth, Axis axis)
    {
        float peak = 0f;

        for (int row = 0; row < cloth.Rows; row++)
        {
            for (int column = 0; column < cloth.Columns; column++)
            {
                int stepRow = axis == Axis.Column ? 1 : 0;
                int stepColumn = axis == Axis.Row ? 1 : 0;

                if (row - stepRow < 0 || row + stepRow >= cloth.Rows ||
                    column - stepColumn < 0 || column + stepColumn >= cloth.Columns)
                {
                    continue;
                }

                Vector3 bend = cloth.PositionAt(row - stepRow, column - stepColumn)
                    - (2f * cloth.PositionAt(row, column))
                    + cloth.PositionAt(row + stepRow, column + stepColumn);

                peak = MathF.Max(peak, bend.Length / Spacing);
            }
        }

        return peak;
    }

    private static ClothSimulation BuildFlag()
    {
        var cloth = new ClothSimulation(Rows, Columns, Spacing);

        for (int row = 0; row < Rows; row++)
        {
            cloth.Pin(row, 0);
        }

        return cloth;
    }

    private static void Advance(ClothSimulation cloth, float seconds, bool wind = true)
    {
        int steps = (int)(seconds / Step);

        for (int i = 0; i < steps; i++)
        {
            float time = i * Step;
            cloth.Step(Step, wind ? Gravity + Wind(time) : Gravity);
        }
    }

    /// <summary>Mirrors the demo's breeze, so the test exercises the shape that ships.</summary>
    private static Vector3 Wind(float time) =>
        new(
            300f + (110f * MathF.Sin(time * 0.9f)),
            22f * MathF.Sin(time * 2.3f),
            70f * MathF.Sin(time * 1.7f));
}
