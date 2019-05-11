using System.IO;

namespace SharedLib
{
    public class Utils
    {
        public static string loadShaderCode(string filepath)
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
    }
}
