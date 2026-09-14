namespace OpenTkProjects.Maze.Tests;

public class MazeGeneratorTests
{
    [Fact]
    public void Generate_VisitsEveryRoom()
    {
        Room[] rooms = new MazeGenerator(10, 10, seed: 1234).Generate();

        Assert.Equal(100, rooms.Length);
        Assert.All(rooms, room => Assert.True(room.Visited));
    }

    [Fact]
    public void Generate_IsDeterministicForAGivenSeed()
    {
        Room[] first = new MazeGenerator(8, 8, seed: 99).Generate();
        Room[] second = new MazeGenerator(8, 8, seed: 99).Generate();

        for (int i = 0; i < first.Length; i++)
        {
            Assert.Equal(first[i].Doors, second[i].Doors);
        }
    }

    [Fact]
    public void Generate_DiffersBetweenSeeds()
    {
        Room[] first = new MazeGenerator(8, 8, seed: 1).Generate();
        Room[] second = new MazeGenerator(8, 8, seed: 2).Generate();

        bool anyDifference = false;
        for (int i = 0; i < first.Length && !anyDifference; i++)
        {
            for (int side = 0; side < 4; side++)
            {
                if (first[i].Doors[side] != second[i].Doors[side])
                {
                    anyDifference = true;
                    break;
                }
            }
        }

        Assert.True(anyDifference, "Different seeds should produce different mazes.");
    }

    [Fact]
    public void Generate_OpensDoorsInMatchingPairs()
    {
        const int rows = 6;
        const int columns = 6;
        Room[] rooms = new MazeGenerator(rows, columns, seed: 7).Generate();

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                Room room = rooms[(row * columns) + column];

                if (row + 1 < rows)
                {
                    Room ahead = rooms[((row + 1) * columns) + column];
                    Assert.Equal(room.Doors[(int)Side.Top], ahead.Doors[(int)Side.Bottom]);
                }

                if (column + 1 < columns)
                {
                    Room right = rooms[(row * columns) + column + 1];
                    Assert.Equal(room.Doors[(int)Side.Right], right.Doors[(int)Side.Left]);
                }
            }
        }
    }

    [Fact]
    public void Generate_NeverOpensADoorOnTheOuterBoundary()
    {
        const int rows = 5;
        const int columns = 5;
        Room[] rooms = new MazeGenerator(rows, columns, seed: 42).Generate();

        for (int column = 0; column < columns; column++)
        {
            Assert.False(rooms[column].Doors[(int)Side.Bottom]);
            Assert.False(rooms[((rows - 1) * columns) + column].Doors[(int)Side.Top]);
        }

        for (int row = 0; row < rows; row++)
        {
            Assert.False(rooms[row * columns].Doors[(int)Side.Left]);
            Assert.False(rooms[(row * columns) + columns - 1].Doors[(int)Side.Right]);
        }
    }

    [Fact]
    public void Generate_ProducesASpanningTreeWithExactlyOnePathBetweenRooms()
    {
        const int rows = 7;
        const int columns = 7;
        Room[] rooms = new MazeGenerator(rows, columns, seed: 5).Generate();

        int openDoorPairs = 0;
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                Room room = rooms[(row * columns) + column];

                if (row + 1 < rows && room.Doors[(int)Side.Top])
                {
                    openDoorPairs++;
                }

                if (column + 1 < columns && room.Doors[(int)Side.Right])
                {
                    openDoorPairs++;
                }
            }
        }

        // A spanning tree over N rooms has exactly N-1 edges, so the maze is fully
        // connected and contains no loops.
        Assert.Equal((rows * columns) - 1, openDoorPairs);
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(5, 0)]
    [InlineData(-1, 5)]
    public void Constructor_RejectsNonPositiveDimensions(int rows, int columns)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MazeGenerator(rows, columns, seed: 1));
    }
}
