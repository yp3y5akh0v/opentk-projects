using OpenTK.Graphics.OpenGL;

namespace SharedLib
{
    public class Quad2DObject : GameObject
    {
        public Quad2DObject(float[] positions, float[] colors, int[] indices) : base(new Mesh(positions, colors, indices, BeginMode.Triangles))
        {

        }
    }
}
