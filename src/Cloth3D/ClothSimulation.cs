using OpenTK.Mathematics;

namespace Cloth3D;

/// <summary>
/// Verlet-integrated cloth: particles carry no explicit velocity, and structural springs are
/// enforced by relaxing positions rather than by applying spring forces.
/// </summary>
/// <remarks>
/// Velocity is implied by the gap between current and previous position, so damping and constraint
/// projection act on positions directly. Stable at step sizes where force-based springs explode.
/// </remarks>
public sealed class ClothSimulation
{
    /// <summary>
    /// Longest a weave spring may end a step at, as a multiple of rest length. Relaxation alone is
    /// super-elastic: the worst spring reaches 22 percent over rest at six sweeps and 8 percent at
    /// forty, so iterating harder does not fix it.
    /// </summary>
    private const float MaxStretch = 1.04f;

    private const int ClampPasses = 8;

    private readonly Vector3[] positions;
    private readonly Vector3[] previous;
    private readonly bool[] pinned;
    private readonly Vector3[] normals;
    private readonly Spring[] springs;
    private readonly int rows;
    private readonly int columns;

    public ClothSimulation(int rows, int columns, float spacing)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(rows, 2);
        ArgumentOutOfRangeException.ThrowIfLessThan(columns, 2);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(spacing);

        this.rows = rows;
        this.columns = columns;

        int count = rows * columns;
        positions = new Vector3[count];
        previous = new Vector3[count];
        pinned = new bool[count];
        normals = new Vector3[count];

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                int index = Index(row, column);
                positions[index] = new Vector3(column * spacing, -row * spacing, 0f);
                previous[index] = positions[index];
            }
        }

        springs = BuildSprings();
    }

    public IReadOnlyList<Vector3> Positions => positions;

    public IReadOnlyList<Vector3> Normals => normals;

    public int Rows => rows;

    public int Columns => columns;

    /// <summary>Relaxation sweeps per step. More sweeps make the cloth stiffer.</summary>
    public int ConstraintIterations { get; set; } = 6;

    public float Damping { get; set; } = 0.012f;

    public void Pin(int row, int column) => pinned[Index(row, column)] = true;

    public bool IsPinned(int row, int column) => pinned[Index(row, column)];

    public Vector3 PositionAt(int row, int column) => positions[Index(row, column)];

    public void Step(float deltaSeconds, Vector3 acceleration)
    {
        Integrate(deltaSeconds, acceleration);

        for (int iteration = 0; iteration < ConstraintIterations; iteration++)
        {
            RelaxSprings();
            ClampStretch();
        }

        RecomputeNormals();
    }

    private int Index(int row, int column) => (row * columns) + column;

    private Spring[] BuildSprings()
    {
        List<Spring> built = [];

        void Connect(int rowA, int columnA, int rowB, int columnB, float stiffness, bool weave)
        {
            if (rowB >= rows || columnB >= columns || columnB < 0)
            {
                return;
            }

            int a = Index(rowA, columnA);
            int b = Index(rowB, columnB);
            built.Add(new Spring(a, b, (positions[b] - positions[a]).Length, stiffness, weave));
        }

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                // Structural springs hold the weave together.
                Connect(row, column, row, column + 1, 1f, weave: true);
                Connect(row, column, row + 1, column, 1f, weave: true);

                // Shear springs stop the grid collapsing into a parallelogram.
                Connect(row, column, row + 1, column + 1, 1f, weave: false);
                Connect(row, column, row + 1, column - 1, 1f, weave: false);

                // Flexion springs span two cells, pricing the one-cell zigzag the sheet buckles
                // into wherever the weave goes slack while leaving broad folds free. Soft on
                // purpose: at full stiffness the flag turns into a board.
                Connect(row, column, row, column + 2, 0.3f, weave: false);
                Connect(row, column, row + 2, column, 0.3f, weave: false);
            }
        }

        return [.. built];
    }

    private void Integrate(float deltaSeconds, Vector3 acceleration)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            if (pinned[i])
            {
                previous[i] = positions[i];
                continue;
            }

            Vector3 current = positions[i];
            Vector3 velocity = (current - previous[i]) * (1f - Damping);

            positions[i] = current + velocity + (acceleration * deltaSeconds * deltaSeconds);
            previous[i] = current;
        }
    }

    private void RelaxSprings()
    {
        foreach (Spring spring in springs)
        {
            Vector3 delta = positions[spring.B] - positions[spring.A];
            float length = delta.Length;

            if (length <= float.Epsilon)
            {
                continue;
            }

            Move(spring.A, spring.B, delta * (0.5f * spring.Stiffness * (1f - (spring.RestLength / length))));
        }
    }

    /// <summary>
    /// Pulls back any weave spring over <see cref="MaxStretch"/>. Shear and flexion springs are
    /// left alone; they are meant to yield.
    /// </summary>
    /// <remarks>
    /// Swept repeatedly because pulling one spring in re-stretches the neighbour sharing its
    /// particle. Converges quickly since springs already inside the limit sit the pass out.
    /// </remarks>
    private void ClampStretch()
    {
        for (int pass = 0; pass < ClampPasses; pass++)
        {
            bool corrected = false;

            foreach (Spring spring in springs)
            {
                if (!spring.Weave)
                {
                    continue;
                }

                Vector3 delta = positions[spring.B] - positions[spring.A];
                float length = delta.Length;
                float limit = spring.RestLength * MaxStretch;

                if (length <= limit)
                {
                    continue;
                }

                Move(spring.A, spring.B, delta * (0.5f * (1f - (limit / length))));
                corrected = true;
            }

            if (!corrected)
            {
                return;
            }
        }
    }

    /// <summary>
    /// Half a correction to each end, or all of it to the free end when the other is pinned, so a
    /// spring anchored to the pole still reaches its target length.
    /// </summary>
    private void Move(int a, int b, Vector3 correction)
    {
        if (!pinned[a])
        {
            positions[a] += pinned[b] ? correction * 2f : correction;
        }

        if (!pinned[b])
        {
            positions[b] -= pinned[a] ? correction * 2f : correction;
        }
    }

    /// <param name="Stiffness">Share of the length error corrected per sweep, from 0 to 1.</param>
    /// <param name="Weave">True for the structural springs, the only ones the stretch clamp touches.</param>
    private readonly record struct Spring(int A, int B, float RestLength, float Stiffness, bool Weave);

    /// <summary>
    /// Area-weighted vertex normals, accumulated from the cross product of each quad's diagonals.
    /// Without this the cloth would light as a flat sheet no matter how it folds.
    /// </summary>
    private void RecomputeNormals()
    {
        Array.Clear(normals);

        for (int row = 0; row < rows - 1; row++)
        {
            for (int column = 0; column < columns - 1; column++)
            {
                int topLeft = Index(row, column);
                int topRight = Index(row, column + 1);
                int bottomLeft = Index(row + 1, column);
                int bottomRight = Index(row + 1, column + 1);

                // Operand order matters: triangles wind counter-clockwise from +Z, so the normal
                // must agree. Swapping these yields -Z, and every front face loses its sunlight.
                Vector3 face = Vector3.Cross(
                    positions[bottomLeft] - positions[topRight],
                    positions[bottomRight] - positions[topLeft]);

                normals[topLeft] += face;
                normals[topRight] += face;
                normals[bottomLeft] += face;
                normals[bottomRight] += face;
            }
        }

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = normals[i].LengthSquared > float.Epsilon
                ? Vector3.Normalize(normals[i])
                : Vector3.UnitZ;
        }
    }
}
