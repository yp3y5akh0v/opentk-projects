using OpenTK.Graphics.OpenGL;

namespace MazeGen2D
{
    public class Mesh
    {
        private readonly int vaoId;
        private readonly int posVboId;
        private readonly int colorVboId;
        private readonly int idxVboId;
        private readonly int vertexCount;
        private readonly BeginMode beginMode;

        public Mesh(float[] positions, float[] colors, int[] indices, BeginMode beginMode)
        {
            vertexCount = indices.Length;
            this.beginMode = beginMode;

            vaoId = GL.GenVertexArray();
            GL.BindVertexArray(vaoId);

            posVboId = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, posVboId);
            GL.BufferData(BufferTarget.ArrayBuffer, positions.Length * sizeof(float), positions, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            colorVboId = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, colorVboId);
            GL.BufferData(BufferTarget.ArrayBuffer, colors.Length * sizeof(float), colors, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            idxVboId = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, idxVboId);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(float), indices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public void Render()
        {
            GL.BindVertexArray(vaoId);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.DrawElements(beginMode, vertexCount, DrawElementsType.UnsignedInt, 0);
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.BindVertexArray(0);
        }

        public void CleanUp()
        {
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(posVboId);
            GL.DeleteBuffer(colorVboId);
            GL.BindVertexArray(0);
            GL.DeleteVertexArray(vaoId);
        }
    }
}
