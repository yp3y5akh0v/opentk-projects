using OpenTK.Graphics.OpenGL;

namespace MazeGen2D
{
    public class QuadObject: GameObject
    {
        public QuadObject(float[] positions, float[] colors) : base(new Mesh(positions, colors, new int[] { 0, 1, 2, 0, 2, 3 }, BeginMode.Triangles))
        {

        }
    }
}
