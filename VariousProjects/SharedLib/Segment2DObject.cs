using OpenTK.Graphics.OpenGL;

namespace SharedLib
{
    public class Segment2DObject : GameObject
    {
        public Segment2DObject(float[] positions, float[] colors, int[] indices) : base(new Mesh(positions, colors, indices, BeginMode.Lines))
        {
        }
    }
}
