using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SharedLib
{
    public class GameObject
    {
        private readonly Mesh mesh;
        private Vector3 position;
        private float scale;
        private Vector3 rotation;

        public GameObject(Mesh mesh)
        {
            this.mesh = mesh;

            position = Vector3.Zero;
            scale = 1;
            rotation = Vector3.Zero;
        }

        public Vector3 GetPosition()
        {
            return position;
        }

        public void SetPosition(float x, float y, float z)
        {
            position.X = x;
            position.Y = y;
            position.Z = z;
        }

        public void SetScale(float scale)
        {
            this.scale = scale;
        }

        public float GetScale()
        {
            return scale;
        }

        public void SetRotation(float x, float y, float z)
        {
            rotation.X = x;
            rotation.Y = y;
            rotation.Z = z;
        }

        public Vector3 GetRotation()
        {
            return rotation;
        }

        public Mesh GetMesh()
        {
            return mesh;
        }

        public void SetWidth(float w)
        {
            GL.LineWidth(w);
        }

        public void CleanUp()
        {
            mesh.CleanUp();
        }

        public void Render()
        {
            mesh.Render();
        }

    }
}
