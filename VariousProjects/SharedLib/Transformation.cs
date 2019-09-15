using OpenTK;

namespace SharedLib
{
    public class Transformation
    {
        public static Matrix4 GetWorldMatrix(Vector3 offset, Vector3 rotation, float scale)
        {
            return Matrix4.CreateScale(scale) *
                   Matrix4.CreateRotationX(rotation.X) *
                   Matrix4.CreateRotationY(rotation.Y) *
                   Matrix4.CreateRotationZ(rotation.Z) *
                   Matrix4.CreateTranslation(offset);
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

        public static Matrix4 GetViewMatrix(Vector3 position, Vector3 lookAt)
        {
            return Matrix4.LookAt(position, position + lookAt, Vector3.UnitY);
        }
    }
}
