using OpenTK;
using SharedLib;

namespace MazeGen2D
{
    public class Room
    {
        private bool[] doors;
        private Segment2DObject[] segments;
        private Quad2DObject shield;
        private bool visited;

        public Room(float x, float y, float z, float w, float h)
        {
            doors = new bool[4];
            segments = new Segment2DObject[4];
            visited = false;

            float epsX = w * 0.5f;
            float epsY = h * 0.5f;

            segments[0] = new Segment2DObject(new float[] { x, y, z, x + w + epsX, y, z }, new float[] { 1.0f, 1.0f, 1.0f }, new int[] { 0, 1 });
            segments[1] = new Segment2DObject(new float[] { x, y, z, x, y + h + epsY, z }, new float[] { 1.0f, 1.0f, 1.0f }, new int[] { 0, 1 });
            segments[2] = new Segment2DObject(new float[] { x, y + h, z, x + w + epsX, y + h, z }, new float[] { 1.0f, 1.0f, 1.0f }, new int[] { 0, 1 });
            segments[3] = new Segment2DObject(new float[] { x + w, y, z, x + w, y + h + epsY, z }, new float[] { 1.0f, 1.0f, 1.0f }, new int[] { 0, 1 });

            var shieldPositions = new float[]
            {
                x, y, z,
                x, y + h, z,
                x + w, y + h, z,
                x + w, y, z
            };

            shield = new Quad2DObject(shieldPositions, new float[] { 0.372f, 0.207f, 0.592f }, new int[] { 0, 1, 2, 0, 2, 3 });            
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
