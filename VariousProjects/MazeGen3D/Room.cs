using OpenTK;
using SharedLib;

namespace MazeGen3D
{
    public class Room
    {
        private readonly bool[] doors;
        private readonly Quad2DObject[] walls;
        private bool visited;

        public Room(Vector3 offset, float w, float h)
        {
            doors = new bool[4];
            walls = new Quad2DObject[4];
            visited = false;

            for (var i = 0; i < walls.Length; i++)
            {
                walls[i] = new Quad2DObject(offset, w, h, Vector3.UnitZ);
            }

            walls[0].SetRotation(0f, MathHelper.DegreesToRadians(-180f), 0f);
            walls[0].UpdatePosition(w * Vector3.UnitZ);
            walls[0].UpdatePosition(w * Vector3.UnitX);

            walls[1].SetRotation(0f, MathHelper.DegreesToRadians(-90f), 0f);
            walls[1].UpdatePosition(w * Vector3.UnitX);

            walls[3].SetRotation(0f, MathHelper.DegreesToRadians(90f), 0f);
            walls[3].UpdatePosition(w * Vector3.UnitZ);
        }

        public void OpenTopDoor()
        {
            doors[0] = true;
        }

        public void OpenLeftDoor()
        {
            doors[1] = true;
        }

        public void OpenBottomDoor()
        {
            doors[2] = true;
        }

        public void OpenRightDoor()
        {
            doors[3] = true;
        }

        public void CleanUp()
        {
            foreach (var wall in walls)
            {
                wall.CleanUp();
            }
        }

        public void Visit()
        {
            visited = true;
        }

        public bool IsVisited()
        {
            return visited;
        }

        public Quad2DObject[] GetWalls()
        {
            return walls;
        }

        public bool GetDoor(int index)
        {
            return doors[index];
        }
    }
}
