using OpenTK;
using SharedLib;
using System;

namespace MazeGen3D
{
    public class CollisionDetector
    {
        public static float Detect(Quad obstacle, Player player)
        {
            var tr = FixedTransformation(obstacle);            
            var q = (tr * Vector4.UnitW).Xyz;
            var p = player.GetPosition();
            var n = obstacle.GetNormal();
            var qp = p - q;
            var sgnd = Vector3.Dot(qp, n);
            var r = -sgnd * n + p;            
            var w = (tr * new Vector4(obstacle.GetWidth(), obstacle.GetHeight(), 0f, 1.0f)).Xyz;
            var cmpR = new Vector3(FixedClamp(r.X, q.X, w.X), FixedClamp(r.Y, q.Y, w.Y), FixedClamp(r.Z, q.Z, w.Z));            
            var cmpRP = p - cmpR;
            var diff = cmpRP.Length - player.GetRadius();
            
            return diff;
        }

        private static float FixedClamp(float c, float a, float b)
        {
            return MathHelper.Clamp(c, Math.Min(a, b), Math.Max(a, b));
        }

        private static Matrix4 FixedTransformation(Quad obstacle)
        {
            var result = Transformation.GetWorldMatrix(obstacle.GetPosition(), obstacle.GetRotation(), obstacle.GetScale());
            result.Transpose();
            return result;
        }
    }
}
