namespace OpenTkProjects.Maze;

/// <summary>Wall index within a room. Values are used directly as indices into Room.Doors.</summary>
public enum Side
{
    /// <summary>The wall at increasing row.</summary>
    Top = 0,

    /// <summary>The wall at decreasing column.</summary>
    Left = 1,

    /// <summary>The wall at decreasing row.</summary>
    Bottom = 2,

    /// <summary>The wall at increasing column.</summary>
    Right = 3,
}

public sealed class Room
{
    /// <summary>True where a wall has been removed to join this room to its neighbour.</summary>
    public bool[] Doors { get; } = new bool[4];

    public bool Visited { get; set; }
}
