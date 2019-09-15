using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SharedLib
{
    public class GameObject
    {
        private Mesh _mesh;
        private Vector3 position;
        private float scale;
        private Vector3 rotation;

        public GameObject()
        {
            position = Vector3.Zero;
            scale = 1.0f;
            rotation = Vector3.Zero;
        }

        public virtual Vector3 GetPosition()
        {
            return position;
        }

        public virtual void SetScale(float scale)
        {
            this.scale = scale;
        }

        public virtual float GetScale()
        {
            return scale;
        }

        public virtual Vector3 GetRotation()
        {
            return rotation;
        }

        public void SetMesh(Mesh mesh)
        {
            _mesh = mesh;
        }

        public Mesh GetMesh()
        {
            return _mesh;
        }

        public virtual void SetWidth(float w)
        {
            GL.LineWidth(w);
        }

        public virtual void CleanUp()
        {
            _mesh?.CleanUp();
        }

        public virtual void Render()
        {
            _mesh?.Render();
        }

        public virtual void SetPosition(float x, float y, float z)
        {
            position.X = x;
            position.Y = y;
            position.Z = z;
        }

        public virtual void SetPosition(Vector3 ds)
        {
            SetPosition(ds.X, ds.Y, ds.Z);
        }

        public virtual void SetRotation(float x, float y, float z)
        {
            rotation.X = x;
            rotation.Y = y;
            rotation.Z = z;
        }

        public virtual void SetRotation(Vector3 ds)
        {
            SetRotation(ds.X, ds.Y, ds.Z);
        }

        public virtual void UpdatePosition(float dx, float dy, float dz)
        {
            position.X += dx;
            position.Y += dy;
            position.Z += dz;
        }

        public virtual void UpdatePosition(Vector3 ds)
        {
            UpdatePosition(ds.X, ds.Y, ds.Z);
        }

        public virtual void UpdateRotation(float dx, float dy, float dz)
        {
            rotation.X += dx;
            rotation.Y += dy;
            rotation.Z += dz;
        }

        public virtual void UpdateRotation(Vector3 ds)
        {
            UpdateRotation(ds.X, ds.Y, ds.Z);
        }

        public virtual Matrix4 GetTransformation()
        {
            return Transformation.GetWorldMatrix(GetPosition(), GetRotation(), GetScale());
        }
    }
}
