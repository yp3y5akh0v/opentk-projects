using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SharedLib
{
    public class IcoSphere: GameObject
    {
        private int _levels { get; set; }
        private float _radius { get; set; }
        private Dictionary<long, int> _middlePointIndexCache { get; set; }

        public IcoSphere(float radius, int levels)
        {
            _radius = radius;
            _levels = levels;
            _middlePointIndexCache = new Dictionary<long, int>();

            var phi = (1.0f + (float)Math.Sqrt(5.0f)) / 2.0f;
            var mesh = new Mesh();

            mesh.AddVertex(Utils.AdjustVectorLength(new Vector3(-1, phi, 0), radius));
            mesh.AddVertex(Utils.AdjustVectorLength(new Vector3(1, phi, 0), radius));
            mesh.AddVertex(Utils.AdjustVectorLength(new Vector3(-1, -phi, 0), radius));
            mesh.AddVertex(Utils.AdjustVectorLength(new Vector3(1, -phi, 0), radius));

            mesh.AddVertex(Utils.AdjustVectorLength(new Vector3(0, -1, phi), radius));
            mesh.AddVertex(Utils.AdjustVectorLength(new Vector3(0, 1, phi), radius));
            mesh.AddVertex(Utils.AdjustVectorLength(new Vector3(0, -1, -phi), radius));
            mesh.AddVertex(Utils.AdjustVectorLength(new Vector3(0, 1, -phi), radius));

            mesh.AddVertex(Utils.AdjustVectorLength(new Vector3(phi, 0, -1), radius));
            mesh.AddVertex(Utils.AdjustVectorLength(new Vector3(phi, 0, 1), radius));
            mesh.AddVertex(Utils.AdjustVectorLength(new Vector3(-phi, 0, -1), radius));
            mesh.AddVertex(Utils.AdjustVectorLength(new Vector3(-phi, 0, 1), radius));

            var faces = new List<Tuple<int, int, int>>();

            faces.Add(Tuple.Create(0, 11, 5));
            faces.Add(Tuple.Create(0, 5, 1));
            faces.Add(Tuple.Create(0, 1, 7));
            faces.Add(Tuple.Create(0, 7, 10));
            faces.Add(Tuple.Create(0, 10, 11));

            faces.Add(Tuple.Create(1, 5, 9));
            faces.Add(Tuple.Create(5, 11, 4));
            faces.Add(Tuple.Create(11, 10, 2));
            faces.Add(Tuple.Create(10, 7, 6));
            faces.Add(Tuple.Create(7, 1, 8));

            faces.Add(Tuple.Create(3, 9, 4));
            faces.Add(Tuple.Create(3, 4, 2));
            faces.Add(Tuple.Create(3, 2, 6));
            faces.Add(Tuple.Create(3, 6, 8));
            faces.Add(Tuple.Create(3, 8, 9));

            faces.Add(Tuple.Create(4, 9, 5));
            faces.Add(Tuple.Create(2, 4, 11));
            faces.Add(Tuple.Create(6, 2, 10));
            faces.Add(Tuple.Create(8, 6, 7));
            faces.Add(Tuple.Create(9, 8, 1));

            for (var i = 0; i < levels; i++)
            {
                var faces2 = new List<Tuple<int, int, int>>();
                foreach (var face in faces)
                {
                    int a = GetMiddlePoint(mesh, face.Item1, face.Item2);
                    int b = GetMiddlePoint(mesh, face.Item2, face.Item3);
                    int c = GetMiddlePoint(mesh, face.Item3, face.Item1);

                    faces2.Add(Tuple.Create(face.Item1, a, c));
                    faces2.Add(Tuple.Create(face.Item2, b, a));
                    faces2.Add(Tuple.Create(face.Item3, c, b));
                    faces2.Add(Tuple.Create(a, b, c));
                }

                faces = faces2;
            }

            foreach (var face in faces)
            {
                var a = face.Item1;
                var b = face.Item2;
                var c = face.Item3;
                var n = Utils.SafeNormalized(GetFaceNormal(mesh, face.Item1, face.Item2, face.Item3));

                mesh.AddTripleIndices(a, b, c);
                mesh.AddTripleNormals(n, n, n);
            }

            mesh.SetBeginMode(BeginMode.Triangles);

            mesh.Init();
            SetMesh(mesh);
        }

        private int GetMiddlePoint(Mesh mesh, int p1, int p2)
        {
            long l = Math.Min(p1, p2);
            long r = Math.Max(p1, p2);
            long key = (l << 32) + r;

            int ret;
            if (_middlePointIndexCache.TryGetValue(key, out ret))
            {
                return ret;
            }

            var point1 = mesh.GetVertexAt(p1);
            var point2 = mesh.GetVertexAt(p2);
            var middle = new Vector3((point1.X + point2.X) / 2 , (point1.Y + point2.Y) / 2, (point1.Z + point2.Z) / 2);

            int index = mesh.AddVertex(Utils.AdjustVectorLength(middle, _radius));
            _middlePointIndexCache.Add(key, index);

            return index;
        }

        private Vector3 GetFaceNormal(Mesh mesh, int p1, int p2, int p3)
        {
            var point1 = mesh.GetVertexAt(p1);
            var point2 = mesh.GetVertexAt(p2);
            var point3 = mesh.GetVertexAt(p3);

            var e1 = point2 - point1;
            var e2 = point3 - point1;
            var c = Vector3.Cross(e1, e2);

            if (c.Length > 0)
            {
                return c / c.Length;
            }

            return Vector3.Zero;
        }

        public float GetRadius()
        {
            return _radius;
        }

        public int GetLevels()
        {
            return _levels;
        }
    }
}
