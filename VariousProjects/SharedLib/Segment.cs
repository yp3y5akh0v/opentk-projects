﻿using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SharedLib
{
    public class Segment : GameObject
    {
        protected Vector3 _beginPoint { get; set; }
        protected Vector3 _endPoint { get; set; }
        protected Vector3 _normal { get; set; }
        protected Vector4 _color { get; set; }

        public Segment(Vector3 beginPoint, Vector3 endPoint, Vector3 normal, Vector4 color)
        {
            _beginPoint = beginPoint;
            _endPoint = endPoint;
            _normal = normal;
            _color = color;

            var mesh = new Mesh();
            mesh.SetBeginMode(BeginMode.Lines);

            mesh.AddVertex(beginPoint);
            mesh.AddVertex(endPoint);

            mesh.AddNormal(normal);
            mesh.AddNormal(normal);

            mesh.AddColor(color);
            mesh.AddColor(color);

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
