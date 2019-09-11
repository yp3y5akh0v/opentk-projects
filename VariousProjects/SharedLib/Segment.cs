﻿using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SharedLib
{
    public class Segment : GameObject
    {
        private Vector3 _beginPoint { get; set; }
        private Vector3 _endPoint { get; set; }

        public Segment(Vector3 beginPoint, Vector3 endPoint)
        {
            _beginPoint = beginPoint;
            _endPoint = endPoint;
            
            var mesh = new Mesh();
            mesh.SetBeginMode(BeginMode.Lines);
            mesh.AddVertex(beginPoint);
            mesh.AddVertex(endPoint);

            mesh.AddNormal(Vector3.One);

            mesh.AddIndex(0);
            mesh.AddIndex(1);

            mesh.Init();

            SetMesh(mesh);
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
