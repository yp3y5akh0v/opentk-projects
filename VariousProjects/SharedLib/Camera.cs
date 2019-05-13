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

        public void UpdateRotation(float dx, float dy, float dz)
        {
            rotation.X += dx;
            rotation.Y += dy;
            rotation.Z += dz;
        }

        public void UpdatePosition(float dx, float dy, float dz)
        {        
            if (dx != 0)
            {
                position.X += -dx * (float)Math.Sin(MathHelper.DegreesToRadians(rotation.Y - 90));
                position.Z += dx * (float)Math.Cos(MathHelper.DegreesToRadians(rotation.Y - 90));
            }

            position.Y += dy;

            if (dz != 0)
            {
                position.X += -dz * (float)Math.Sin(MathHelper.DegreesToRadians(rotation.Y));
                position.Z += dz * (float)Math.Cos(MathHelper.DegreesToRadians(rotation.Y));
            }
        }
    }
}
