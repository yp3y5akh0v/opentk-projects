using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SharedLib
{
    public class Mesh
    {
        private int _vaoId { get; set; }
        private int _vertVboId { get; set; }
        private int _normVboId { get; set; }
        private int _indVboId { get; set; }
        private BeginMode _beginMode { get; set; }
        private List<Vector3> _vertices { get; set; }
        private List<int> _indices { get; set; }
        private List<Vector3> _normals { get; set; }

        public Mesh()
        {
            _vertices = new List<Vector3>();
            _indices = new List<int>();
            _normals = new List<Vector3>();
        }

        public int AddVertex(Vector3 v)
        {
            _vertices.Add(v);
            return _vertices.Count - 1;
        }

        public void AddNormal(Vector3 n)
        {
            _normals.Add(n);
        }

        public void AddIndex(int ind)
        {
            _indices.Add(ind);
        }

        public void AddTripleIndices(int a, int b, int c)
        {
            _indices.Add(a);
            _indices.Add(b);
            _indices.Add(c);
        }

        public void AddTripleNormals(Vector3 a, Vector3 b, Vector3 c)
        {
            _normals.Add(a);
            _normals.Add(b);
            _normals.Add(c);
        }

        public void SetBeginMode(BeginMode beginMode)
        {
            _beginMode = beginMode;
        }

        public BeginMode GetBeginMode()
        {
            return _beginMode;
        }

        public Vector3 GetVertexAt(int index)
        {
            return _vertices.ElementAt(index);
        }

        public void Init()
        {
            _vaoId = GL.GenVertexArray();
            GL.BindVertexArray(_vaoId);

            _vertVboId = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertVboId);
            GL.BufferData(BufferTarget.ArrayBuffer, 3 * _vertices.Count * sizeof(float), Utils.FlattenVectors(_vertices), BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            _normVboId = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _normVboId);
            GL.BufferData(BufferTarget.ArrayBuffer, 3 * _normals.Count * sizeof(float), Utils.FlattenVectors(_normals), BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            _indVboId = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indVboId);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Count * sizeof(int), _indices.ToArray(), BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public void Render()
        {
            GL.BindVertexArray(_vaoId);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.DrawElements(_beginMode, _indices.Count, DrawElementsType.UnsignedInt, 0);
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.BindVertexArray(0);
        }

        public void CleanUp()
        {
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);            
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(_vertVboId);
            GL.DeleteBuffer(_normVboId);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.DeleteBuffer(_indVboId);
            GL.BindVertexArray(0);
            GL.DeleteVertexArray(_vaoId);
        }
    }
}
