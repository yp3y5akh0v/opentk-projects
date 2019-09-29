using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SharedLib;

namespace Cloth3D
{
    public class Program
    {
        private GameWindow window;
        private Player player;
        private ShaderProgram sceneShaderProgram;
        private readonly float zNear = 25f;
        private readonly float zFar = 1000000f;
        private readonly float fov = MathHelper.DegreesToRadians(90f);
        private int vertRows = 60;
        private int vertColumns = 100;
        private float gap = 5f;
        private List<ClothPoint> cPoints;
        private List<ClothSpring> cSprings;
        private Vector3 gravity = new Vector3(0f, -30f, 0f);
        private Vector3 directionalForce = new Vector3(1f, 0f, 1f);
        private int SmoothnessConstraint = 5;
        private ClothGrid clothGrid;

        static void Main(string[] args)
        {
            new Program().Start(args);
        }

        public void Start(string[] args)
        {
            window = new GameWindow(1600, 900, GraphicsMode.Default, "Cloth 3D")
            {
                CursorVisible = false,
                WindowState = WindowState.Normal
            };
            window.Load += Window_Load;
            window.UpdateFrame += Window_UpdateFrame;
            window.RenderFrame += Window_RenderFrame;
            window.Unload += Window_Unload;
            window.Run();
        }

        private void ConfigureSceneShader()
        {
            sceneShaderProgram.SetUniform("projectionMatrix",
                Transformation.GetPerspectiveProjectionMatrix(fov, window.Width, window.Height, zNear, zFar));
            var camera = player.GetCamera();
            sceneShaderProgram.SetUniform("viewMatrix",
                Transformation.GetViewMatrix(camera.GetPosition(), Utils.GetLookAt(camera.GetRotation())));
        }

        private void Window_Unload(object sender, EventArgs e)
        {
            sceneShaderProgram.CleanUp();

            foreach (var cPoint in cPoints)
            {
                cPoint.CleanUp();
            }

            clothGrid.CleanUp();
        }

        private void RenderGameObjects(ShaderProgram sp, string targetWorldMatrix)
        {
            //foreach (var cPoint in cPoints)
            //{
            //    sp.SetUniform(targetWorldMatrix, cPoint.GetTransformation());
            //    cPoint.Render();
            //}

            sp.SetUniform(targetWorldMatrix, clothGrid.GetTransformation());
            clothGrid.Render();
        }

        private void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            GL.Viewport(0, 0, window.Width, window.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            sceneShaderProgram.Bind();
            ConfigureSceneShader();
            RenderGameObjects(sceneShaderProgram, "worldMatrix");
            sceneShaderProgram.Unbind();

            GL.Flush();
            window.SwapBuffers();
        }

        private void Window_UpdateFrame(object sender, FrameEventArgs e)
        {
            player.Input(window, (float) e.Time);

            foreach (var cPoint in cPoints)
            {
                cPoint.ApplyForce(gravity);
            }

            for (var i = 0; i < vertRows - 1; i++)
            {
                for (var j = 0; j < vertColumns - 1; j++)
                {
                    var pA = cPoints.ElementAt(GetIndex(i, j));
                    var pB = cPoints.ElementAt(GetIndex(i, j + 1));
                    var pC = cPoints.ElementAt(GetIndex(i + 1, j));
                    var pD = cPoints.ElementAt(GetIndex(i + 1, j + 1));

                    var rnd = new Random().Next(0, 20);
                    ApplyTriangleForce(pA, pB, pC, rnd * directionalForce);
                    ApplyTriangleForce(pC, pB, pD, rnd * directionalForce);
                }
            }
            foreach (var cPoint in cPoints)
            {
                cPoint.ApplyPositionStep((float)e.Time);
            }

            for (var i = 0; i < SmoothnessConstraint; i++)
            {
                foreach (var cSpring in cSprings)
                {
                    cSpring.ApplyConstraint();
                }
            }

            clothGrid.Update();
        }

        private int GetIndex(int i, int j)
        {
            return i * vertColumns + j;
        }

        private void ApplyTriangleForce(ClothPoint cpA, ClothPoint cpB, ClothPoint cpC, Vector3 force)
        {
            var pA = cpA.GetPosition();
            var pB = cpB.GetPosition();
            var pC = cpC.GetPosition();
            var vAB = pB - pA;
            var vAC = pC - pA;
            var nABC = Vector3.Cross(vAB, vAC).Normalized();
            var actualForce = Vector3.Dot(force, nABC) * nABC;

            cpA.ApplyForce(actualForce);
            cpB.ApplyForce(actualForce);
            cpC.ApplyForce(actualForce);
        }

        private void Window_Load(object sender, EventArgs e)
        {
            GL.ClearColor(Color.FromArgb(0, 0, 0, 0));
            GL.Enable(EnableCap.DepthTest);

            sceneShaderProgram = new ShaderProgram();
            sceneShaderProgram.CreateVertexShader(Utils.LoadShaderCode("vertex.glsl"));
            sceneShaderProgram.CreateFragmentShader(Utils.LoadShaderCode("fragment.glsl"));
            sceneShaderProgram.Link();
            sceneShaderProgram.CreateUniform("viewMatrix");
            sceneShaderProgram.CreateUniform("worldMatrix");
            sceneShaderProgram.CreateUniform("projectionMatrix");

            player = new Player(100f);
            player.SetRotationX(MathHelper.DegreesToRadians(180f));
            player.SetPosition( new Vector3(vertRows * gap / 2f, vertColumns * gap / 2, 150f));

            cPoints = new List<ClothPoint>();
            cSprings = new List<ClothSpring>();

            for (var i = 0; i < vertRows; i++)
            {
                for (var j = 0; j < vertColumns; j++)
                {
                    var cPoint = new ClothPoint(GetIndex(i, j), j * gap, (vertRows - i - 1) * gap, 0f);
                    cPoint.SetMass(0.1f);
                    cPoints.Add(cPoint);
                }
            }

            for (var i = 0; i < vertRows - 1; i++)
            {
                for (var j = 0; j < vertColumns - 1; j++)
                {
                    var pA = cPoints.ElementAt(GetIndex(i, j));
                    var pB = cPoints.ElementAt(GetIndex(i, j + 1));
                    var pC = cPoints.ElementAt(GetIndex(i + 1, j));
                    var pD = cPoints.ElementAt(GetIndex(i + 1, j + 1));

                    var sAB = new ClothSpring(pA, pB);
                    var sAC = new ClothSpring(pA, pC);
                    var sAD = new ClothSpring(pA, pD);
                    var sBC = new ClothSpring(pB, pC);

                    cSprings.Add(sAB);
                    cSprings.Add(sAC);
                    cSprings.Add(sAD);
                    cSprings.Add(sBC);
                }
            }

            for (var i = 0; i < vertRows - 1; i++)
            {
                var pLastA = cPoints.ElementAt(GetIndex(i, vertColumns - 1));
                var pLastB = cPoints.ElementAt(GetIndex(i + 1, vertColumns - 1));

                var sLastAB = new ClothSpring(pLastA, pLastB);

                cSprings.Add(sLastAB);
            }

            for (var j = 0; j < vertColumns - 1; j++)
            {
                var pLastA = cPoints.ElementAt(GetIndex(vertRows - 1, j));
                var pLastB = cPoints.ElementAt(GetIndex(vertRows - 1, j + 1));

                var sLastAB = new ClothSpring(pLastA, pLastB);

                cSprings.Add(sLastAB);
            }

            for (var i = 0; i < vertRows; i++)
            {
                //cPoints.ElementAt(GetIndex(i, 0)).Lock();
                //cPoints.ElementAt(GetIndex(i, vertColumns - 1)).Lock();
            }

            for (var j = 0; j < vertColumns; j++)
            {
                cPoints.ElementAt(GetIndex(0, j)).Lock();
                //cPoints.ElementAt(GetIndex(vertRows - 1, j)).Lock();
            }

            clothGrid = new ClothGrid(cPoints, vertRows, vertColumns);
        }
    }
}
