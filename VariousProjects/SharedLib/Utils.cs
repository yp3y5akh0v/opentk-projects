using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTK;

namespace SharedLib
{
    public class Utils
    {
        public static string LoadShaderCode(string filepath)
        {
            string result = "";
            using (var sr = new StreamReader(filepath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    result += line + "\n";
                }
            }

            return result;
        }

        public static Vector3 GetLookAt(Vector3 rotation)
        {
            var lookAt = Vector3.Zero;

            lookAt.X = (float)(Math.Sin(rotation.X) * Math.Cos(rotation.Y));
            lookAt.Y = (float)Math.Sin(rotation.Y);
            lookAt.Z = (float)(Math.Cos(rotation.X) * Math.Cos(rotation.Y));

            return lookAt.Normalized();
        }

        public static float[] FlattenVectors(List<Vector2> list)
        {
            var result = new float[2 * list.Count];
            for (var i = 0; i < list.Count; i++)
            {
                var e = list.ElementAt(i);
                result[2 * i] = e.X;
                result[2 * i + 1] = e.Y;
            }

            return result;
        }

        public static float[] FlattenVectors(List<Vector3> list)
        {
            var result = new float[3 * list.Count];
            for (var i = 0; i < list.Count; i++)
            {
                var e = list.ElementAt(i);
                result[3 * i] = e.X;
                result[3 * i + 1] = e.Y;
                result[3 * i + 2] = e.Z;
            }

            return result;
        }

        public static Vector3 AdjustVectorLength(Vector3 v, float l)
        {
            return l * SafeNormalized(v);
        }

        public static Vector3 SafeNormalized(Vector3 v)
        {
            return v.Length > 0 ? v.Normalized() : v;
        }

        public static float Map(float value, float fromSource, float toSource, float fromTarget, float toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }
    }
}
