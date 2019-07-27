using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace MazeGen3D
{
    public class Program
    {
        private readonly int nrRooms = 20;
        private readonly int ncRooms = 20;
        private readonly float rmWidth = 800f;
        private readonly float rmHeight = 400f;
        private readonly float lightHeightOffset = 1000f;
        private readonly float gap = 5f;
        private readonly float zNear = 25f;
        private readonly float zFar = 1000000f;
        private readonly float fov = MathHelper.DegreesToRadians(90f);
        private GameWindow window;
        private ShaderProgram shaderProgram;
        private Player player;
        private Room[] rooms;
        private Quad2DObject plane;
        private Stack<int> stack;
        private int curInd;

        private static void Main()
        {
            new Program().Start();
        }

        public void Start()
        {
            window = new GameWindow(1600, 900, GraphicsMode.Default, "Maze Gen 3D")
            {
                CursorVisible = false,
                WindowState = WindowState.Fullscreen
            };
            window.Load += Window_Load;
            window.UpdateFrame += Window_UpdateFrame;
            window.RenderFrame += Window_RenderFrame;
            window.Unload += Window_Unload;
            window.Run();
        }

        private void Window_Unload(object sender, EventArgs e)
        {
            shaderProgram.CleanUp();
            foreach (var room in rooms)
                room.CleanUp();
        }

        private void Window_UpdateFrame(object sender, FrameEventArgs e)
        {
            var prevPos = player.GetPosition();

            player.Input(window, (float)e.Time);

            var curPos = player.GetPosition();

            ApplyCollision(plane, player, prevPos, curPos);

            foreach (var room in rooms)
            {
                var walls = room.GetWalls();
                for (var index = 0; index < walls.Length; ++index)
                {
                    if (!room.GetDoor(index))
                        ApplyCollision(walls[index], player, prevPos, curPos);
                }
            }

            var rowCurInd = curInd / ncRooms;
            var colCurInd = curInd % ncRooms;
            var neighborInd = PickUpNeighbor(rowCurInd, colCurInd);

            if (neighborInd > -1)
            {
                stack.Push(curInd);

                var rowNeighborInd = neighborInd / ncRooms;
                var colNeighborInd = neighborInd % ncRooms;

                if (rowCurInd < rowNeighborInd)
                {
                    rooms[curInd].OpenTopDoor();
                    rooms[neighborInd].OpenBottomDoor();
                }

                if (rowCurInd > rowNeighborInd)
                {
                    rooms[curInd].OpenBottomDoor();
                    rooms[neighborInd].OpenTopDoor();
                }

                if (colCurInd < colNeighborInd)
                {
                    rooms[curInd].OpenLeftDoor();
                    rooms[neighborInd].OpenRightDoor();
                }

                if (colCurInd > colNeighborInd)
                {
                    rooms[curInd].OpenRightDoor();
                    rooms[neighborInd].OpenLeftDoor();
                }

                curInd = neighborInd;
                rooms[curInd].Visit();
            }
            else
            {
                if (stack.Count <= 0)
                    return;
                curInd = stack.Pop();
            }
        }

        private void ApplyCollision(Quad2DObject obstacle, Player targetPlayer, Vector3 prevPos, Vector3 curPos)
        {
            var detector = CollisionDetector.Detect(obstacle, targetPlayer);

            if (detector >= 0.0f)
                return;

            var diffPos = curPos - prevPos;
            targetPlayer.SetPosition(diffPos.Normalized() * detector + curPos);
            var normal = obstacle.GetNormal();
            targetPlayer.SetPosition(diffPos - Vector3.Dot(normal, diffPos) * normal + prevPos);
        }

        private void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
            GL.Viewport(0, 0, window.Width, window.Height);

            shaderProgram.bind();

            shaderProgram.SetUniform("viewPos", player.GetPosition());
            shaderProgram.SetUniform("light.color", Vector3.One);
            shaderProgram.SetUniform("light.ambientStrength", 0.4f);
            shaderProgram.SetUniform("light.position", new Vector3(ncRooms / 2f * (rmWidth + gap), rmHeight + 2f * player.GetRadius() + lightHeightOffset, nrRooms / 2f * (rmWidth + gap)));
            shaderProgram.SetUniform("material.color", new Vector3(0.6f, 0.6f, 0.6f));
            shaderProgram.SetUniform("material.specularStrength", 0.6f);
            shaderProgram.SetUniform("material.shininess", 100f);
            shaderProgram.SetUniform("projectionMatrix", 
                Transformation.GetPerspectiveProjectionMatrix(fov, window.Width, window.Height, zNear, zFar));
            shaderProgram.SetUniform("viewMatrix", Transformation.GetViewMatrix(player.GetCamera()));

            foreach (var room in rooms)
            {
                var walls = room.GetWalls();
                for (var index = 0; index < walls.Length; ++index)
                {
                    shaderProgram.SetUniform("worldMatrix", walls[index].GetTransformation());
                    if (!room.GetDoor(index))
                        walls[index].Render();
                }
            }

            shaderProgram.SetUniform("worldMatrix", plane.GetTransformation());
            plane.Render();

            shaderProgram.unbind();

            GL.Flush();
            window.SwapBuffers();
        }

        private void Window_Load(object sender, EventArgs e)
        {
            GL.ClearColor(Color.FromArgb(0, 0, 0, 0));
            GL.Enable(EnableCap.DepthTest);

            player = new Player(100f);

            shaderProgram = new ShaderProgram();
            shaderProgram.createVertexShader(Utils.loadShaderCode("vertex.glsl"));
            shaderProgram.createFragmentShader(Utils.loadShaderCode("fragment.glsl"));
            shaderProgram.link();
            shaderProgram.createUniform("viewMatrix");
            shaderProgram.createUniform("worldMatrix");
            shaderProgram.createUniform("projectionMatrix");
            shaderProgram.createUniform("viewPos");
            shaderProgram.createUniform("light.color");
            shaderProgram.createUniform("light.ambientStrength");
            shaderProgram.createUniform("light.position");
            shaderProgram.createUniform("material.color");
            shaderProgram.createUniform("material.specularStrength");
            shaderProgram.createUniform("material.shininess");

            stack = new Stack<int>();
            rooms = new Room[nrRooms * ncRooms];

            for (var i = 0; i < nrRooms; ++i)
            {
                for (var j = 0; j < ncRooms; ++j)
                {
                    var offset = new Vector3(j * (rmWidth + gap), 0.0f, i * (rmWidth + gap));
                    rooms[GetFlatteredIndex(i, j)] = new Room(offset, rmWidth, rmHeight);
                }
            }

            plane = new Quad2DObject(Vector3.Zero, ncRooms * (rmWidth + gap),  nrRooms * (rmWidth + gap), Vector3.UnitZ);
            plane.UpdateRotation(MathHelper.DegreesToRadians(-90f), 0.0f, 0.0f);
            plane.UpdatePosition(Vector3.UnitZ * nrRooms * (rmWidth + gap));

            curInd = GetFlatteredIndex(0, 0);
            rooms[curInd].Visit();
        }

        private int GetFlatteredIndex(int i, int j)
        {
            return i * ncRooms + j;
        }

        private int PickUpNeighbor(int i, int j)
        {
            var intList = new List<int>();

            if (i > 0)
            {
                var flatteredIndex = GetFlatteredIndex(i - 1, j);
                if (!rooms[flatteredIndex].IsVisited())
                    intList.Add(flatteredIndex);
            }

            if (j > 0)
            {
                var flatteredIndex = GetFlatteredIndex(i, j - 1);
                if (!rooms[flatteredIndex].IsVisited())
                    intList.Add(flatteredIndex);
            }

            if (i < nrRooms - 1)
            {
                var flatteredIndex = GetFlatteredIndex(i + 1, j);
                if (!rooms[flatteredIndex].IsVisited())
                    intList.Add(flatteredIndex);
            }

            if (j < ncRooms - 1)
            {
                var flatteredIndex = GetFlatteredIndex(i, j + 1);
                if (!rooms[flatteredIndex].IsVisited())
                    intList.Add(flatteredIndex);
            }

            var num = -1;

            if (intList.Count > 0)
            {
                var index = new Random().Next(intList.Count);
                num = intList[index];
            }

            return num;
        }
    }
}
