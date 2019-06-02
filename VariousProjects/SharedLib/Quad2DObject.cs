using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SharedLib
{
    public class Quad2DObject : GameObject
    {

        private Vector3 normal;
        private Vector3 offset;
        private float width;
        private float height;

        public Quad2DObject(Vector3 offset, float width, float height, Vector3 normal) 
            : base(new Mesh(GenVertexBuffer(offset, width, height), GenNormalBuffer(normal), new int[] { 0, 1, 2, 2, 1, 3 }, BeginMode.Triangles))
        {
            this.normal = normal;
            this.offset = offset;
            this.width = width;
            this.height = height;
        }

        private static float[] GenVertexBuffer(Vector3 offset, float width, float height)
        {
            var positions = new float[3 * 4] {
                offset.X, offset.Y, offset.Z,
                offset.X + width, offset.Y, offset.Z,
                offset.X, offset.Y + height, offset.Z,
                offset.X + width, offset.Y + height, offset.Z
            };
            return positions;
        }

        private static float[] GenNormalBuffer(Vector3 normal)
        {
            float[] normals = new float[3 * 4];

            for (var i = 0; i < normals.Length / 3; i++)
            {
                normals[3 * i] = normal.X;
                normals[3 * i + 1] = normal.Y;
                normals[3 * i + 2] = normal.Z;
            }

            return normals;
        }

        public float GetWidth()
        {
            return width;
        }

        public float GetHeight()
        {
            return height;
        }

        public Vector3 GetNormal()
        {
            return normal;
        }

        public void SetNormal(float nx, float ny, float nz)
        {
            normal.X = nx;
            normal.Y = ny;
            normal.Z = nz;
        }
    }
}
