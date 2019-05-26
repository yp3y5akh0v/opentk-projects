using OpenTK.Graphics.OpenGL;

namespace SharedLib
{
    public class Segment2DObject : GameObject
    {
        public Segment2DObject(float[] positions, float[] normals, int[] indices) 
            : base(new Mesh(positions, normals, indices, BeginMode.Lines))
        {
        }
    }
}
