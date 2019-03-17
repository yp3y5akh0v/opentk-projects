﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeGen2D
{
    public class Utils
    {
        public static string loadShaderCode(string filepath)
        {
            string result = "";
            using (var sr = new StreamReader(filepath))
            {
                string line;
                while ((line = sr.ReadLine()) != null) {
                    result += line + "\n";
                } 
            }

            return result;
        }
    }
}
