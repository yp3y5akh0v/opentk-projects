using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SharedLib;

namespace Cloth3D
{
    public class ClothGrid: GameObject
    {
        private List<ClothPoint> ClothPoints { get; set; }
        private int Rows { get; set; }
        private int Cols { get; set; }

        public ClothGrid(List<ClothPoint> clothPoints, int rows, int cols)
        {
            ClothPoints = clothPoints;
            Rows = rows;
            Cols = cols;

            var mesh = new Mesh();
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            mesh.SetBeginMode(BeginMode.TriangleStrip);

            foreach (var clothPoint in clothPoints)
            {
                mesh.AddVertex(clothPoint.GetPosition());
                mesh.AddNormal(Vector3.Zero);
            }

            for (var i = 0; i < rows - 1; i++)
            {
                var step = 1;
                var j = 0;

                if (i % 2 == 1)
                {
                    step = -1;
                    j = cols - 1;
                }

                while (j > -1 && j < cols)
                {
                    var indA = GetIndex(i, j);
                    var indB = GetIndex(i + 1, j);
                    mesh.AddIndex(indA);
                    mesh.AddIndex(indB);

                    j += step;
                }
            }

            mesh.Init();
            SetMesh(mesh);
        }

        public void Update()
        {
            foreach (var clothPoint in ClothPoints)
            {
                mesh.SetVertexBuffer(clothPoint.GetId(), clothPoint.GetPosition());
            }
        }

        private int GetIndex(int i, int j)
        {
            return i * Cols + j;
        }
    }
}
