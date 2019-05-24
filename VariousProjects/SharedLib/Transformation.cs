using OpenTK;
using System;

namespace SharedLib
{
    public class Transformation
    {
        public static Matrix4 GetWorldMatrix(Vector3 offset, Vector3 rotation, float scale)
        {
            var result = Matrix4.Identity;
            result = Matrix4.Mult(Matrix4.CreateScale(scale), result);
            result = Matrix4.Mult(Matrix4.CreateRotationX(rotation.X), result);
            result = Matrix4.Mult(Matrix4.CreateRotationY(rotation.Y), result);
            result = Matrix4.Mult(Matrix4.CreateRotationZ(rotation.Z), result);
            result = Matrix4.Mult(Matrix4.CreateTranslation(offset), result);
            return result;
        }

        public static Matrix4 GetPerspectiveProjectionMatrix(float fov, float width, float height, float zNear, float zFar)
        {
            return Matrix4.CreatePerspectiveOffCenter(-width / 2.0f, width / 2.0f, height / 2.0f, -height / 2.0f, zNear, zFar);
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
            var lookat = Vector3.Zero;
            var camPos = camera.GetPosition();
            var camRot = camera.GetRotation();

            lookat.X = (float) (Math.Sin(camRot.X) * Math.Cos(camRot.Y));
            lookat.Y = (float) Math.Sin(camRot.Y);
            lookat.Z = (float) (Math.Cos(camRot.X) * Math.Cos(camRot.Y));

            return Matrix4.LookAt(camPos, camPos + lookat, Vector3.UnitY);
        }
    }
}
