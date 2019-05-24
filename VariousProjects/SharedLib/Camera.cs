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

        public void SetRotation(float x, float y, float z)
        {
            rotation.X = x;
            rotation.Y = y;
            rotation.Z = z;
        }

        public void UpdateRotation(float dx, float dy)
        {
            rotation.X = (rotation.X + dx) % ((float) Math.PI * 2);
            rotation.Y = Math.Max(Math.Min(rotation.Y + dy, (float) Math.PI / 2 - 1.0f), (float)- Math.PI / 2 + 1.0f);
        }

        public void UpdatePosition(float dx, float dy, float dz)
        {
            var offset = Vector3.Zero;
            var forward = new Vector3((float)Math.Sin(rotation.X), 0f, (float)Math.Cos(rotation.X));
            var right = new Vector3(-forward.Z, 0f, forward.X);

            offset += dx * right;
            offset += dy * forward;
            offset.Y += dz;

            offset.NormalizeFast();
            offset = Vector3.Multiply(offset, Constants.SPEED);

            position += offset;
        }
    }
}
