using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SharedLib
{
    public class Rect2D: GameObject
    {
        protected Vector2 center { get; set; }
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

        public override Matrix4 GetTransformation()
        {
            return Transformation.GetWorldMatrix(GetPosition(), GetRotation(), GetScale());
        }
    }
}
