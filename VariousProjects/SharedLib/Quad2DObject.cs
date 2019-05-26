using OpenTK.Graphics.OpenGL;

namespace SharedLib
{
    public class Quad2DObject : GameObject
    {
        public Quad2DObject(float[] positions, float[] normals, int[] indices) 
            : base(new Mesh(positions, normals, indices, BeginMode.Triangles))
        {
        }
    }
}
