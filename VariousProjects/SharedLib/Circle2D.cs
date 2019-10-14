using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SharedLib
{
    public class Circle2D: GameObject
    {
        protected Vector2 center { get; set; }
        protected Vector2 center0 { get; set; }
        protected float radius { get; set; }
        protected int resolution { get; set; }
        protected Vector4 color { get; set; }

        public Circle2D(Vector2 center, float radius): this(center, radius, Constants.DEFAULT_CIRCLE2D_RESOLUTION, Vector4.One)
        {
        }

        public Circle2D(float radius): this(Vector2.Zero, radius, Constants.DEFAULT_CIRCLE2D_RESOLUTION, Vector4.One)
        {
        }

        public Circle2D(Vector2 center, float radius, int resolution, Vector4 color)
        {
            center0 = new Vector2(center.X, center.Y);
            position = new Vector3(center0.X, center0.Y, 0f);

            this.center = new Vector2(center0.X, center0.Y);
            this.radius = radius;
            this.resolution = resolution;
            this.color = color;

            var mesh = new Mesh();
            mesh.SetBeginMode(BeginMode.LineLoop);

            for (var i = 0; i < resolution; i++)
            {
                var p = 2 * (float) Math.PI * i / resolution;
                var x = center0.X + radius * (float) Math.Cos(p);
                var y = center0.Y + radius * (float) Math.Sin(p);

                mesh.AddVertex(new Vector3(x, y, 0f));
                mesh.AddNormal(Vector3.UnitZ);
                mesh.AddColor(color);
                mesh.AddIndex(i);
            }

            mesh.Init();
            SetMesh(mesh);
        }

        public void SetRadius(float radius)
        {
            for (var i = 0; i < resolution; i++)
            {
                var vert = mesh.GetVertexAt(i);

                vert -= new Vector3(center.X, center.Y, 0f);
                vert /= this.radius;
                vert *= radius;

                vert += new Vector3(center.X, center.Y, 0f);

                mesh.SetVertexBuffer(i, vert);
            }

            this.radius = radius;
        }

        public void UpdateRadius(float offset)
        {
            SetRadius(radius + offset);
        }

        public void SetCenter(Vector2 center)
        {
            var dir = center - this.center;
            UpdateCenter(dir);
        }

        public void SetColor(Vector4 color)
        {
            this.color = color;

            for (var i = 0; i < resolution; i++)
            {
                mesh.SetColorBuffer(i, color);
            }
        }

        public void UpdateCenter(Vector2 ds)
        {
            center += ds;
            UpdatePosition(new Vector3(ds.X, ds.Y, 0f));
        }

        public Vector2 GetCenter()
        {
            return center;
        }

        public float GetRadius()
        {
            return radius;
        }

        public bool Contains(Vector2 point)
        {
            var diff = point - center;
            return diff.LengthSquared <= radius * radius;
        }

        public bool Contains(Circle2D other)
        {
            var diff = other.GetCenter() - center;
            return diff.Length + other.radius < radius;
        }

        public bool Intersect(Circle2D other)
        {
            var diff = other.GetCenter() - center;
            return diff.Length < radius + other.GetRadius();
        }

        public override Matrix4 GetTransposeTransformation()
        {
            var tr = GetTransformation();
            tr.Transpose();
            return tr;
        }

        public override Matrix4 GetTransformation()
        {
            var pivotTr = Transformation.GetWorldMatrix(new Vector3(center0.X, center0.Y, 0f), Vector3.Zero, 1f);
            var rotTr = Transformation.GetWorldMatrix(Vector3.Zero, GetRotation(), 1f);
            var translationScaleTr = Transformation.GetWorldMatrix(
                GetPosition() - new Vector3(center0.X, center0.Y, 0f),
                Vector3.Zero, GetScale());

            return pivotTr.Inverted() * rotTr * pivotTr * translationScaleTr;
        }
    }
}
