using OpenTK;
using System;

namespace SharedLib
{
    public class Transformation
    {
        public static Matrix4 GetWorldMatrix(Vector3 offset, Vector3 rotation, float scale)
        {
            var result = Matrix4.Identity;
            result = Matrix4.Mult(Matrix4.CreateTranslation(offset), result);
            result = Matrix4.Mult(Matrix4.CreateRotationX(rotation.X), result);
            result = Matrix4.Mult(Matrix4.CreateRotationY(rotation.Y), result);                  
            result = Matrix4.Mult(Matrix4.CreateRotationZ(rotation.Z), result);
            result = Matrix4.Mult(Matrix4.CreateScale(scale), result);
            return result;
        }

        public static Matrix4 GetPerspectiveProjectionMatrix(float fov, float width, float height, float zNear, float zFar)
        {
            return Matrix4.CreatePerspectiveFieldOfView(fov, width / height, zNear, zFar);
        }

        public static Matrix4 GetOrthoProjectionMatrix(float width, float height, float zNear, float zFar)
        {
            var result = Matrix4.Identity;
            result[1, 1] = -1;
            result = Matrix4.Mult(Matrix4.CreateOrthographicOffCenter(0, width, 0, height, zNear, zFar), result);
            return result;
        }

        public static Matrix4 GetViewMatrix(Camera camera)
        {
            var lookat = camera.GetLookAt();
            var camPos = camera.GetPosition();

            return Matrix4.LookAt(camPos, camPos + lookat, Vector3.UnitY);
        }
    }
}
