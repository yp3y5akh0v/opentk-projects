using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace SharedLib
{
    public class Quad2DObject : GameObject
    {
        private Vector3 normal;
        private Vector3 offset;
        private float width;
        private float height;

        public Quad2DObject(Vector3 offset, float width, float height, Vector3 normal) 
            : base(
                  new Mesh(
                            GenVertexBuffer(width, height), 
                            GenNormalBuffer(normal), 
                            new int[] { 0, 1, 2, 2, 1, 3 }, 
                            BeginMode.Triangles
                          )
                  )
        {
            this.normal = normal;
            this.offset = offset;
            this.width = width;
            this.height = height;
        }

        private static float[] GenVertexBuffer(float width, float height)
        {
            var positions = new float[3 * 4] {
                0f, 0f, 0f,
                width, 0f, 0f,
                0f, height, 0f,
                width, height, 0f
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

        private Vector3 ReCalculateNormal()
        {
            var _n = (GetTransformation() * new Vector4(normal, 1.0f)).Xyz.Normalized();
            if (Math.Abs(_n.X) < Constants.EPS)
            {
                _n.X = 0.0f;
            }
            if (Math.Abs(_n.Y) < Constants.EPS)
            {
                _n.Y = 0.0f;
            }
            if (Math.Abs(_n.Z) < Constants.EPS)
            {
                _n.Z = 0.0f;
            }
            return _n;
        }

        public Matrix4 GetTransformation()
        {
            return Transformation.GetWorldMatrix(GetPosition(), base.GetRotation(), base.GetScale());
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
            return ReCalculateNormal();
        }

        public Vector3 GetOrigPosition()
        {
            return base.GetPosition();
        }

        public override Vector3 GetPosition()
        {
            return offset + base.GetPosition();
        }

        public Vector3 GetOffset()
        {
            return offset;
        }
    }
}
