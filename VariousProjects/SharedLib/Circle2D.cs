using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SharedLib
{
    public class Circle2D: GameObject
    {
        private Vector2 center { get; set; }
        private float radius { get; set; }
        private int resolution { get; set; }
        private Vector4 color { get; set; }

        public Circle2D(Vector2 center, float radius): this(center, radius, Constants.DEFAULT_CIRCLE2D_RESOLUTION, Vector4.One)
        {
        }

        public Circle2D(float radius): this(Vector2.Zero, radius, Constants.DEFAULT_CIRCLE2D_RESOLUTION, Vector4.One)
        {
        }

        public Circle2D(Vector2 center, float radius, int resolution, Vector4 color)
        {
            this.center = center;
            this.radius = radius;
            this.resolution = resolution;
            this.color = color;

            var mesh = new Mesh();
            mesh.SetBeginMode(BeginMode.LineLoop);

            for (var i = 0; i < resolution; i++)
            {
                var p = 2 * (float) Math.PI * i / resolution;
                var x = center.X + radius * (float) Math.Cos(p);
                var y = center.Y + radius * (float) Math.Sin(p);

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
    }
}
