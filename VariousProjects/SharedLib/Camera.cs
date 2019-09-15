using OpenTK;
using System;

namespace SharedLib
{
    public class Camera
    {
        private Vector3 position;
        private Vector3 rotation;

        public Camera(Vector3 position, Vector3 rotation)
        {            
            this.position = position;            
            this.rotation = rotation;
        }

        public Vector3 GetPosition()
        {
            return position;
        }

        public Vector3 GetRotation()
        {
            return rotation;
        }
        
        public void SetPosition(float x, float y, float z)
        {
            position.X = x;
            position.Y = y;
            position.Z = z;
        }

        public void SetPosition(Vector3 v)
        {
            SetPosition(v.X, v.Y, v.Z);
        }

        public void SetRotation(float x, float y, float z)
        {
            rotation.X = x;
            rotation.Y = y;
            rotation.Z = z;
        }

        public void UpdateRotation(float dx, float dy)
        {
            rotation.X = (rotation.X + dx) % ((float) Math.PI * 2);
            rotation.Y = Math.Max(Math.Min(rotation.Y + dy, (float) Math.PI / 2 - 0.001f), (float)- Math.PI / 2 + 0.001f);
        }

        public void UpdatePosition(float dx, float dy, float dz)
        {
            var offset = Vector3.Zero;
            var lookAt = Utils.GetLookAt(rotation);
            var right = Vector3.Cross(Vector3.UnitY, lookAt);

            right.NormalizeFast();

            offset += Vector3.Multiply(lookAt, dz);
            offset += Vector3.Multiply(right, dx);
            offset += Vector3.Multiply(Vector3.UnitY, dy);
            
            position += offset;
        }
    }
}
