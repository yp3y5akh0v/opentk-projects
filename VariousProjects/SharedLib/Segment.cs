using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SharedLib
{
    public class Segment : GameObject
    {
        protected Vector3 _beginPoint { get; set; }
        protected Vector3 _endPoint { get; set; }
        protected Vector3 _normal { get; set; }

        public Segment(Vector3 beginPoint, Vector3 endPoint, Vector3 normal)
        {
            _beginPoint = beginPoint;
            _endPoint = endPoint;
            _normal = normal;

            var mesh = new Mesh();
            mesh.SetBeginMode(BeginMode.Lines);

            mesh.AddVertex(beginPoint);
            mesh.AddVertex(endPoint);

            mesh.AddNormal(normal);
            mesh.AddNormal(normal);

            mesh.AddIndex(0);
            mesh.AddIndex(1);

            mesh.Init();

            SetMesh(mesh);
        }

        public Vector3 GetNormal()
        {
            return _normal;
        }

        public Vector3 GetBeginPoint()
        {
            return _beginPoint;
        }

        public Vector3 GetEndPoint()
        {
            return _endPoint;
        }
    }
}
