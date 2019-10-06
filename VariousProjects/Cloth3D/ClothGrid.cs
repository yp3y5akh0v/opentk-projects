using System.Collections.Generic;
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

        public ClothGrid(List<ClothPoint> clothPoints, int rows, int cols, string filePath)
        {
            ClothPoints = clothPoints;
            Rows = rows;
            Cols = cols;

            var mesh = new Mesh();
            mesh.SetBeginMode(BeginMode.TriangleStrip);

            foreach (var clothPoint in ClothPoints)
            {
                mesh.AddVertex(clothPoint.GetPosition());
                mesh.AddNormal(Vector3.Zero);
            }

            for (var i = 0; i < Rows; i++)
            {
                for (var j = 0; j < Cols; j++)
                {
                    var texV = Utils.Map(i, 0f, Rows - 1, 0f, 1f);
                    var texU = Utils.Map(j, 0f, Cols - 1, 0f, 1f);
                    mesh.AddTexCoord(new Vector2(texU, texV));
                }
            }

            for (var i = 0; i < Rows - 1; i++)
            {
                var step = 1;
                var j = 0;

                if (i % 2 == 1)
                {
                    step = -1;
                    j = Cols - 1;
                }

                while (j > -1 && j < Cols)
                {
                    var indA = GetIndex(i, j);
                    var indB = GetIndex(i + 1, j);
                    mesh.AddIndex(indA);
                    mesh.AddIndex(indB);

                    j += step;
                }
            }

            mesh.SetTexture(new Texture(filePath, TextureTarget.Texture2D));

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

        public void BindTexture(TextureUnit texUnit)
        {
            mesh.BindTexture(texUnit);
        }

        public void UnBindTexture()
        {
            mesh.UnBindTexture();
        }

        public IEnumerable<ClothPoint> GetClothPoints()
        {
            return ClothPoints;
        }

        private int GetIndex(int i, int j)
        {
            return i * Cols + j;
        }
    }
}
