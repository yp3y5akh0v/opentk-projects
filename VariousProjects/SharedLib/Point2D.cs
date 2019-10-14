using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SharedLib
{
    public class Point2D: GameObject
    {
        protected Vector2 position0 { get; set; }
        protected Vector4 color { get; set; }

        public Point2D() : this(Vector2.Zero, Vector4.One)
        {
        }

        public Point2D(float x, float y) : this(new Vector2(x, y), Vector4.One)
        {
        }

        public Point2D(Vector2 position, Vector4 color)
        {
            position0 = new Vector2(position.X, position.Y);

            this.position = new Vector3(position0.X, position0.Y, 0f);
            this.color = color;

            var mesh = new Mesh();
            mesh.SetBeginMode(BeginMode.Points);
            mesh.AddVertex(new Vector3(position0.X, position0.Y, 0f));
            mesh.AddNormal(Vector3.UnitZ);
            mesh.AddColor(color);
            mesh.AddTexCoord(Vector2.Zero);
            mesh.AddIndex(0);
            mesh.Init();
            SetMesh(mesh);
        }

        public override Matrix4 GetTransposeTransformation()
        {
            var tr = GetTransformation();
            tr.Transpose();
            return tr;
        }

        public override Matrix4 GetTransformation()
        {
            var pivotTr = Transformation.GetWorldMatrix(new Vector3(position0.X, position0.Y, 0f), Vector3.Zero, 1f);
            var rotTr = Transformation.GetWorldMatrix(Vector3.Zero, GetRotation(), 1f);
            var translationScaleTr = Transformation.GetWorldMatrix(
                GetPosition() - new Vector3(position0.X, position0.Y, 0f),
                Vector3.Zero, GetScale());
            return pivotTr.Inverted() * rotTr * pivotTr * translationScaleTr;
        }
    }
}
