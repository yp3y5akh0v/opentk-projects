using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Collections.Generic;
using SharedLib;

namespace MazeGen2D
{
    public class Program
    {
        private GameWindow window;
        private ShaderProgram shaderProgram;
        private Room[] rooms;
        private Stack<int> stack;
        private readonly int nrRooms = 20;
        private readonly int ncRooms = 20;
        private int curInd;

        private readonly float zNear = 0.01f;
        private readonly float zFar = 1000f;
        private readonly float fov = MathHelper.DegreesToRadians(60f);
        private double ellapsedTime = 0f;

        static void Main(string[] args)
        {
            new Program().Start();
        }

        public void Start()
        {
            window = new GameWindow(800, 800, GraphicsMode.Default, "Maze Gen 2D");

            window.Load += Window_Load;
            window.UpdateFrame += Window_UpdateFrame;
            window.RenderFrame += Window_RenderFrame;
            window.Unload += Window_Unload;
            window.Run();
        }

        private void Window_Unload(object sender, EventArgs e)
        {
            shaderProgram.CleanUp();

            for (var i = 0; i < rooms.Length; i++)
            {
                rooms[i].CleanUp();
            }
        }

        private void Window_UpdateFrame(object sender, FrameEventArgs e)
        {
            ellapsedTime += e.Time;
            if (ellapsedTime > 0.05)
            {
                int rowCurInd = curInd / ncRooms;
                int colCurInd = curInd % ncRooms;

                int neighborInd = PickUpNeighbor(rowCurInd, colCurInd);
                if (neighborInd > -1)
                {
                    stack.Push(curInd);

                    int rowNeighborInd = neighborInd / ncRooms;
                    int colNeighborInd = neighborInd % ncRooms;

                    if (rowCurInd < rowNeighborInd)
                    {
                        rooms[curInd].OpenBottomDoor();
                        rooms[neighborInd].OpenTopDoor();
                    }

                    if (rowCurInd > rowNeighborInd)
                    {
                        rooms[curInd].OpenTopDoor();
                        rooms[neighborInd].OpenBottomDoor();
                    }

                    if (colCurInd < colNeighborInd)
                    {
                        rooms[curInd].OpenRightDoor();
                        rooms[neighborInd].OpenLeftDoor();
                    }

                    if (colCurInd > colNeighborInd)
                    {
                        rooms[curInd].OpenLeftDoor();
                        rooms[neighborInd].OpenRightDoor();
                    }

                    curInd = neighborInd;
                    rooms[curInd].Visit();
                }
                else if (stack.Count > 0)
                {
                    curInd = stack.Pop();
                }
                ellapsedTime = 0f;
            }
        }

        private void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Viewport(0, 0, window.Width, window.Height);

            shaderProgram.bind();

            var projectionMatrix = Transformation.GetOrthoProjectionMatrix(window.Width, window.Height, zNear, zFar);
            shaderProgram.SetUniform("projectionMatrix", projectionMatrix);

            for (var i = 0; i < rooms.Length; i++)
            {
                var worldMatrix = Transformation.GetWorldMatrix(rooms[i].GetPosition(), rooms[i].GetRotation(), rooms[i].GetScale());
                shaderProgram.SetUniform("worldMatrix", worldMatrix);

                var maskColor = new Vector3(1.0f, 1.0f, 1.0f);
                if (i == curInd)
                {
                    maskColor.X = 1f / 0.372f;
                    maskColor.Y = 0;
                    maskColor.Z = 0;
                }
                shaderProgram.SetUniform("maskColor", maskColor);

                rooms[i].Render();
            }

            shaderProgram.unbind();

            GL.Flush();
            window.SwapBuffers();
        }

        private void Window_Load(object sender, EventArgs e)
        {
            GL.ClearColor(Color.FromArgb(0, 0, 0, 0));

            shaderProgram = new ShaderProgram();
            shaderProgram.createVertexShader(Utils.loadShaderCode("vertex.glsl"));
            shaderProgram.createFragmentShader(Utils.loadShaderCode("fragment.glsl"));
            shaderProgram.link();

            shaderProgram.createUniform("worldMatrix");
            shaderProgram.createUniform("projectionMatrix");
            shaderProgram.createUniform("maskColor");

            stack = new Stack<int>();
            rooms = new Room[nrRooms * ncRooms];

            float rmWidth = 1f * window.Width / ncRooms;
            float rmHeight = 1f * window.Height / nrRooms;

            for (var i = 0; i < nrRooms; i++)
            {
                for (var j = 0; j < ncRooms; j++)
                {
                    rooms[GetFlatteredIndex(i, j)] = new Room(j * rmWidth, i * rmHeight, -0.5f, rmWidth, rmHeight);
                }
            }

            curInd = GetFlatteredIndex(0, 0);
            rooms[curInd].Visit();
        }

        private int GetFlatteredIndex(int i, int j)
        {
            return i * ncRooms + j;
        }

        private int PickUpNeighbor(int i, int j)
        {
            var unvisitedNeighbors = new List<int>();

            if (i > 0)
            {
                var topInd = GetFlatteredIndex(i - 1, j);
                if (!rooms[topInd].IsVisited())
                {
                    unvisitedNeighbors.Add(topInd);
                }
            }

            if (j > 0)
            {
                var leftInd = GetFlatteredIndex(i, j - 1);
                if (!rooms[leftInd].IsVisited())
                {
                    unvisitedNeighbors.Add(leftInd);
                }
            }

            if (i < nrRooms - 1)
            {
                var bottomInd = GetFlatteredIndex(i + 1, j);
                if (!rooms[bottomInd].IsVisited())
                {
                    unvisitedNeighbors.Add(bottomInd);
                }
            }

            if (j < ncRooms - 1)
            {
                var rightInd = GetFlatteredIndex(i, j + 1);
                if (!rooms[rightInd].IsVisited())
                {
                    unvisitedNeighbors.Add(rightInd);
                }
            }

            int result = -1;
            if (unvisitedNeighbors.Count > 0)
            {
                int neighbotInd = new Random().Next(unvisitedNeighbors.Count);
                result = unvisitedNeighbors[neighbotInd];
            }
            return result;
        }
    }
}
