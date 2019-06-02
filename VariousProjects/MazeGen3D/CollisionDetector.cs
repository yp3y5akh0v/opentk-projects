using OpenTK;
using SharedLib;
using System;

namespace MazeGen3D
{
    public class CollisionDetector
    {
        public static float Detect(Quad2DObject quad, Player player)
        {
            var q = quad.GetPosition();
            var p = player.GetPosition();
            var qp = p - q;
            var n = quad.GetNormal().Normalized();
            var sgnd = Vector3.Dot(qp, n);
            var r = -sgnd * n + p;
            var w = new Vector3(q.X + quad.GetWidth(), q.Y + quad.GetHeight(), q.Z);
            var cmpR = Vector3.Clamp(r, q, w);
            var cmpRP = p - cmpR;
            var diff = cmpRP.Length - player.GetRadius();

            return diff;
        }
    }
}
