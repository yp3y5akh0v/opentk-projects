namespace OpenTkProjects.Maze;

/// <summary>
/// Randomised depth-first maze carving, producing a spanning tree over the room grid.
/// Carving is exposed both as a single call and one room at a time, since MazeGen3D wants a
/// finished maze while MazeGen2D animates it. Seeded once, so a given seed is reproducible.
/// </summary>
public sealed class MazeGenerator
{
    private readonly int rows;
    private readonly int columns;
    private readonly Random random;
    private readonly Room[] rooms;
    private readonly Stack<int> trail = new();

    private int visitedCount;

    public MazeGenerator(int rows, int columns, int seed)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rows);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(columns);

        this.rows = rows;
        this.columns = columns;
        random = new Random(seed);

        rooms = new Room[rows * columns];
        for (int i = 0; i < rooms.Length; i++)
        {
            rooms[i] = new Room();
        }

        CurrentIndex = 0;
        rooms[CurrentIndex].Visited = true;
        visitedCount = 1;
    }

    public IReadOnlyList<Room> Rooms => rooms;

    /// <summary>Index of the room the carve is standing on. Useful for highlighting the frontier.</summary>
    public int CurrentIndex { get; private set; }

    public bool IsComplete => visitedCount >= rooms.Length;

    /// <summary>
    /// Advances the carve by one room. Returns false once every room has been visited.
    /// </summary>
    public bool Step()
    {
        if (IsComplete)
        {
            return false;
        }

        int next = PickUnvisitedNeighbour(CurrentIndex);

        if (next >= 0)
        {
            OpenWall(CurrentIndex, next);
            trail.Push(CurrentIndex);

            CurrentIndex = next;
            rooms[CurrentIndex].Visited = true;
            visitedCount++;

            return true;
        }

        if (trail.Count > 0)
        {
            CurrentIndex = trail.Pop();
            return true;
        }

        // Unreachable for a rectangular grid, but guards against spinning forever.
        visitedCount = rooms.Length;
        return false;
    }

    /// <summary>Runs the carve to completion and returns the finished grid.</summary>
    public Room[] Generate()
    {
        while (Step())
        {
        }

        return rooms;
    }

    private int Index(int row, int column) => (row * columns) + column;

    private int PickUnvisitedNeighbour(int current)
    {
        int row = current / columns;
        int column = current % columns;

        List<int> candidates = [];

        if (row > 0 && !rooms[Index(row - 1, column)].Visited)
        {
            candidates.Add(Index(row - 1, column));
        }

        if (row + 1 < rows && !rooms[Index(row + 1, column)].Visited)
        {
            candidates.Add(Index(row + 1, column));
        }

        if (column > 0 && !rooms[Index(row, column - 1)].Visited)
        {
            candidates.Add(Index(row, column - 1));
        }

        if (column + 1 < columns && !rooms[Index(row, column + 1)].Visited)
        {
            candidates.Add(Index(row, column + 1));
        }

        return candidates.Count == 0 ? -1 : candidates[random.Next(candidates.Count)];
    }

    private void OpenWall(int from, int to)
    {
        int fromRow = from / columns;
        int fromColumn = from % columns;
        int toRow = to / columns;
        int toColumn = to % columns;

        if (fromRow < toRow)
        {
            rooms[from].Doors[(int)Side.Top] = true;
            rooms[to].Doors[(int)Side.Bottom] = true;
        }
        else if (fromRow > toRow)
        {
            rooms[from].Doors[(int)Side.Bottom] = true;
            rooms[to].Doors[(int)Side.Top] = true;
        }
        else if (fromColumn < toColumn)
        {
            rooms[from].Doors[(int)Side.Right] = true;
            rooms[to].Doors[(int)Side.Left] = true;
        }
        else
        {
            rooms[from].Doors[(int)Side.Left] = true;
            rooms[to].Doors[(int)Side.Right] = true;
        }
    }
}
