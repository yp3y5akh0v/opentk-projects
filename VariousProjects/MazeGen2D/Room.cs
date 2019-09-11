using OpenTK;
using SharedLib;

namespace MazeGen2D
{
    public class Room
    {
        private bool[] doors;
        private Segment[] segments;
        private Quad shield;
        private bool visited;

        public Room(float x, float y, float z, float w, float h)
        {
            doors = new bool[4];
            segments = new Segment[4];
            visited = false;

            float epsX = w * 0.5f;
            float epsY = h * 0.5f;

            segments[0] = new Segment(new Vector3(x, y, z), new Vector3(x + w + epsX, y, z));
            segments[1] = new Segment(new Vector3(x, y, z), new Vector3(x, y + h + epsY, z));
            segments[2] = new Segment(new Vector3(x, y + h, z), new Vector3(x + w + epsX, y + h, z));
            segments[3] = new Segment(new Vector3(x + w, y, z), new Vector3(x + w, y + h + epsY, z));

            shield = new Quad(new Vector3(x, y, z), w, h, 0.2f * Vector3.One);            
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

        public void Render()
        {
            for (var i = 0; i < segments.Length; i++)
            {
                if (!doors[i])
                {
                    segments[i].Render();
                }
            }

            if (visited)
            {
                shield.Render();
            }
        }

        public void CleanUp()
        {
            for (var i = 0; i < segments.Length; i++)
            {                
                segments[i].CleanUp();
            }

            shield.CleanUp();
        }

        public Vector3 GetPosition()
        {
            return segments[0].GetPosition();
        }

        public Vector3 GetRotation()
        {
            return segments[0].GetRotation();
        }

        public float GetScale()
        {
            return segments[0].GetScale();
        }

        public void Visit()
        {
            visited = true;
        }

        public bool IsVisited()
        {
            return visited;
        }
    }
}
