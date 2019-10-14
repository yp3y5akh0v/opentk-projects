using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SharedLib
{
    public class Rect2D: GameObject
    {
        protected Vector2 center { get; set; }
        protected Vector2 center0 { get; set; }
        protected Vector2 halfsize { get; set; }
        protected Vector4 color { get; set; }

        public Rect2D(Vector2 halfsize): this(Vector2.Zero, halfsize, Vector4.One)
        {
        }

        public Rect2D(Vector2 center, Vector2 halfsize) : this(center, halfsize, Vector4.One)
        {
        }

        public Rect2D(float halfwidth, float halfheight): this(new Vector2(halfwidth, halfheight))
        {
        }

        public Rect2D(float halfsize): this(halfsize * Vector2.One)
        {
        }

        public Rect2D(Vector2 center, Vector2 halfsize, Vector4 color)
        {
            this.center = center;
            this.halfsize = halfsize;
            this.color = color;

            center0 = new Vector2(center.X, center.Y);
            position = new Vector3(center0.X, center0.Y, 0f);

            var mesh = new Mesh();
            mesh.SetBeginMode(BeginMode.LineLoop);

            mesh.AddVertex(new Vector3(center.X - halfsize.X, center.Y - halfsize.Y, 0f));
            mesh.AddVertex(new Vector3(center.X - halfsize.X, center.Y + halfsize.Y, 0f));
            mesh.AddVertex(new Vector3(center.X + halfsize.X, center.Y + halfsize.Y, 0f));
            mesh.AddVertex(new Vector3(center.X + halfsize.X, center.Y - halfsize.Y, 0f));

            for (var i = 0; i < 4; i++)
            {
                mesh.AddNormal(Vector3.UnitZ);
                mesh.AddIndex(i);
                mesh.AddColor(color);
            }

            mesh.Init();
            SetMesh(mesh);
        }

        public Vector2 GetCenter()
        {
            return center;
        }

        public float GetWidth()
        {
            return 2 * halfsize.X;
        }

        public float GetHeight()
        {
            return 2 * halfsize.Y;
        }

        public Vector2 GetHalfSize()
        {
            return halfsize;
        }

        public void SetCenter(Vector2 center)
        {
            var dir = center - this.center;
            UpdateCenter(dir);
        }

        public void SetHalfSize(Vector2 halfsize)
        {
            this.halfsize = halfsize;

            mesh.SetVertexBuffer(0, new Vector3(center.X - halfsize.X, center.Y - halfsize.Y, 0f));
            mesh.SetVertexBuffer(1, new Vector3(center.X - halfsize.X, center.Y + halfsize.Y, 0f));
            mesh.SetVertexBuffer(2, new Vector3(center.X + halfsize.X, center.Y + halfsize.Y, 0f));
            mesh.SetVertexBuffer(3, new Vector3(center.X + halfsize.X, center.Y - halfsize.Y, 0f));
        }

        public void UpdateHalfSize(Vector2 ds)
        {
            SetHalfSize(halfsize + ds);
        }

        public void UpdateCenter(Vector2 ds)
        {
            center += ds;
            UpdatePosition(new Vector3(ds.X, ds.Y, 0f));
        }

        public void SetColor(Vector4 color)
        {
            this.color = color;

            for (var i = 0; i < 4; i++)
            {
                mesh.SetColorBuffer(i, color);
            }
        }

        public bool Contains(Vector2 point)
        {
            var vertices = new List<Vector2>();
            vertices.Add(new Vector2(center0.X - halfsize.X, center0.Y - halfsize.Y));
            vertices.Add(new Vector2(center0.X - halfsize.X, center0.Y + halfsize.Y));
            vertices.Add(new Vector2(center0.X + halfsize.X, center0.Y + halfsize.Y));
            vertices.Add(new Vector2(center0.X + halfsize.X, center0.Y - halfsize.Y));

            for (var i = 0; i < 4; i++)
            {
                var vert = vertices.ElementAt(i);
                vert = (GetTransposeTransformation() * new Vector4(vert.X, vert.Y, 0f, 1f)).Xy;
                vertices[i] = vert;
            }

            var vAB = vertices[1] - vertices[0];
            var vAD = vertices[3] - vertices[0];
            var vAP = new Vector2(point.X, point.Y) - vertices[0];

            var dotAB = Vector2.Dot(vAB, vAB);
            var dotAD = Vector2.Dot(vAD, vAD);
            var dotAPB = Vector2.Dot(vAP, vAB);
            var dotAPD = Vector2.Dot(vAP, vAD);

            return 0 < dotAPB && dotAPB < dotAB &&
                   0 < dotAPD && dotAPD < dotAD;
        }

        public bool IntersectCircle(Vector2 c, float r)
        {
            var xMin = float.MaxValue;
            var xMax = float.MinValue;
            var yMin = float.MaxValue;
            var yMax = float.MinValue;

            var vertices = new List<Vector2>
            {
                new Vector2(center.X - halfsize.X, center.Y - halfsize.Y),
                new Vector2(center.X - halfsize.X, center.Y + halfsize.Y),
                new Vector2(center.X + halfsize.X, center.Y + halfsize.Y),
                new Vector2(center.X + halfsize.X, center.Y - halfsize.Y)
            };

            var rectTr = GetTransposeTransformation();

            for (var i = 0; i < 4; i++)
            {
                var vert = vertices.ElementAt(i);

                vert = (rectTr * new Vector4(vert.X, vert.Y, 0f, 1f)).Xy;

                if (xMin > vert.X)
                {
                    xMin = vert.X;
                }

                if (xMax < vert.X)
                {
                    xMax = vert.X;
                }

                if (yMin > vert.Y)
                {
                    yMin = vert.Y;
                }

                if (yMax < vert.Y)
                {
                    yMax = vert.Y;
                }
            }

            var closestToCircle = new Vector2(MathHelper.Clamp(c.X, xMin, xMax), MathHelper.Clamp(c.Y, yMin, yMax));
            return (closestToCircle - c).LengthSquared <= r * r;
        }

        public bool IntersectCircle(Circle2D circle)
        {
            return IntersectCircle(circle.GetCenter(), circle.GetRadius());
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
            var translationScaleTr = Transformation.GetWorldMatrix(GetPosition() - new Vector3(center0.X, center0.Y, 0f),
                Vector3.Zero, GetScale());

            return pivotTr.Inverted() * rotTr * pivotTr * translationScaleTr;
        }
    }
}
