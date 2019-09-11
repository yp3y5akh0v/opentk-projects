using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace SharedLib
{
    public class Quad : GameObject
    {
        private Vector3 normal;
        private Vector3 offset;
        private float width;
        private float height;

        public Quad(Vector3 offset, float width, float height, Vector3 normal)
        {
            this.normal = normal;
            this.offset = offset;
            this.width = width;
            this.height = height;

            var mesh = new Mesh();
            mesh.SetBeginMode(BeginMode.Triangles);

            mesh.AddVertex(Vector3.Zero);
            mesh.AddVertex(new Vector3(width, 0f, 0f));
            mesh.AddVertex(new Vector3(0f, height, 0f));
            mesh.AddVertex(new Vector3(width, height, 0f));

            for (var i = 0; i < 4; i++)
            {
                mesh.AddNormal(new Vector3(normal.X, normal.Y, normal.Z));
            }

            mesh.AddTripleIndices(0, 1, 2);
            mesh.AddTripleIndices(2, 1, 3);

            mesh.Init();

            SetMesh(mesh);
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

        public override Matrix4 GetTransformation()
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
