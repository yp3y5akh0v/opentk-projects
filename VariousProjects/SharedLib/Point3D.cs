using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SharedLib
{
    public class Point3D: GameObject
    {
        protected Vector3 position0 { get; set; }
        protected Vector4 color { get; set; }

        public Point3D(): this(Vector3.Zero, Vector4.One)
        {
        }

        public Point3D(float x, float y, float z): this(new Vector3(x, y, z), Vector4.One)
        {
        }

        public Point3D(Vector3 position, Vector4 color)
        {
            position0 = new Vector3(position);

            this.position = new Vector3(position0.X, position0.Y, 0f);
            this.color = color;

            var mesh = new Mesh();
            mesh.SetBeginMode(BeginMode.Points);
            mesh.AddVertex(position0);
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
            var pivotTr = Transformation.GetWorldMatrix(position0, Vector3.Zero, 1f);
            var rotTr = Transformation.GetWorldMatrix(Vector3.Zero, GetRotation(), 1f);
            var translationScaleTr = Transformation.GetWorldMatrix(GetPosition() - position0,
                Vector3.Zero, GetScale());
            return pivotTr.Inverted() * rotTr * pivotTr * translationScaleTr;
        }
    }
}
