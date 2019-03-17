﻿using OpenTK.Graphics.OpenGL;

namespace MazeGen2D
{
    public class SegmentObject: GameObject
    {
        public SegmentObject(float[] positions, float[] colors): base(new Mesh(positions, colors, new int[] { 0, 1 }, BeginMode.Lines))
        {
        }
    }
}
