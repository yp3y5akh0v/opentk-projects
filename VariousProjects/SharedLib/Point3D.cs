using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SharedLib
{
    public class Point3D: GameObject
    {
        public Point3D()
        {
            ConfigureMesh();
        }

        public Point3D(float x, float y, float z)
        {
            base.SetPosition(x, y, z);
            ConfigureMesh();
        }

        public Point3D(Vector3 position)
        {
            base.SetPosition(position);
            ConfigureMesh();
        }

        private void ConfigureMesh()
        {
            var mesh = new Mesh();
            mesh.SetBeginMode(BeginMode.Points);
            mesh.AddVertex(GetPosition());
            mesh.AddNormal(Vector3.UnitY);
            mesh.AddIndex(0);
            mesh.Init();
            SetMesh(mesh);
        }
    }
}
