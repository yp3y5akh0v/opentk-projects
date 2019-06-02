using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using SharedLib;
using System.Collections.Generic;

namespace MazeGen3D
{
    public class Program
    {
        private GameWindow window;
        private ShaderProgram shaderProgram;
        private List<Quad2DObject> quads;

        private Player player;

        private readonly float zNear = 0.1f;
        private readonly float zFar = 1e+6f;
        private readonly float fov = MathHelper.DegreesToRadians(70f);

        static void Main(string[] args)
        {
            new Program().Start();
        }

        public void Start()
        {
            window = new GameWindow(1600, 900, GraphicsMode.Default, "Maze Gen 3D");
            window.CursorVisible = false;
            //window.WindowState = WindowState.Fullscreen;

            window.Load += Window_Load;
            window.UpdateFrame += Window_UpdateFrame;
            window.RenderFrame += Window_RenderFrame;
            window.Unload += Window_Unload;

            window.Run();
        }

        private void Window_Unload(object sender, EventArgs e)
        {
            shaderProgram.CleanUp();
        }

        private void Window_UpdateFrame(object sender, FrameEventArgs e)
        {
            player.Input(window, (float)e.Time);

            foreach (var quad in quads)
            {
                var undoDist = CollisionDetector.Detect(quad, player);
                
                if (undoDist < 0f)
                {
                    //TODO
                    Console.WriteLine(undoDist);
                }
            }
        }
        
        private void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, window.Width, window.Height);

            shaderProgram.bind();

            var projectionMatrix = Transformation.GetPerspectiveProjectionMatrix(fov, window.Width, window.Height, zNear, zFar);
            shaderProgram.SetUniform("projectionMatrix", projectionMatrix);

            var viewMatrix = Transformation.GetViewMatrix(player.GetCamera());
            shaderProgram.SetUniform("viewMatrix", viewMatrix);           

            foreach (var quad in quads)
            {
                var worldMatrix = Transformation.GetWorldMatrix(quad.GetPosition(), quad.GetRotation(), quad.GetScale());
                var normal = worldMatrix * new Vector4(quad.GetNormal().X, quad.GetNormal().Y, quad.GetNormal().Z, 1.0f);

                quad.SetNormal(normal.X, normal.Y, normal.Z);

                shaderProgram.SetUniform("worldMatrix", worldMatrix);
            }

            shaderProgram.SetUniform("viewPos", player.GetPosition());
            shaderProgram.SetUniform("light.color", Vector3.One);
            shaderProgram.SetUniform("light.ambientStrength", 0.4f);
            shaderProgram.SetUniform("light.position", 300 * Vector3.UnitX);
            shaderProgram.SetUniform("material.color", new Vector3(0.6f, 0.6f, 0.6f));
            shaderProgram.SetUniform("material.specularStrength", 0.6f);
            shaderProgram.SetUniform("material.shininess", 100f);

            foreach (var quad in quads)
            {
                quad.Render();
            }            

            shaderProgram.unbind();

            GL.Flush();
            window.SwapBuffers();
        }

        private void Window_Load(object sender, EventArgs e)
        {
            GL.ClearColor(Color.FromArgb(0, 0, 0, 0));
            GL.Enable(EnableCap.DepthTest);

            player = new Player(50f);

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

            quads = new List<Quad2DObject>();

            float w = window.Width;
            float h = window.Height;
            
            var quad = new Quad2DObject(Vector3.Zero, w / 2.0f, h / 2.0f, Vector3.UnitZ);
           
            quad.SetPosition(-w / 4, -h / 4, -450f);

            quads.Add(quad);
        }
    }
}
