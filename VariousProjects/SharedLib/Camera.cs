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
            rotation.Y = Math.Max(Math.Min(rotation.Y + dy, (float) Math.PI / 2), (float)- Math.PI / 2);
        }

        public Vector3 GetLookAt()
        {
            var lookat = Vector3.Zero;

            lookat.X = (float)(Math.Sin(rotation.X) * Math.Cos(rotation.Y));
            lookat.Y = (float)Math.Sin(rotation.Y);
            lookat.Z = (float)(Math.Cos(rotation.X) * Math.Cos(rotation.Y));

            return lookat.Normalized();
        }

        public void UpdatePosition(float dx, float dy, float dz)
        {
            Vector3 offset = Vector3.Zero, lookat = GetLookAt(), right = Vector3.Zero;

            right = Vector3.Cross(Vector3.UnitY, lookat);
            right.NormalizeFast();

            offset += Vector3.Multiply(lookat, dz);
            offset += Vector3.Multiply(right, dx);
            offset += Vector3.Multiply(Vector3.UnitY, dy);
            
            position += offset;
        }
    }
}
