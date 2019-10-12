using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using SharedLib;

namespace Collision2DPerformance
{
    public class Program
    {
        private GameWindow window;
        private ShaderProgram shaderProgram;
        private readonly float zNear = 0f;
        private readonly float zFar = 1f;

        static void Main(string[] args)
        {
            new Program().Start();
        }

        public void Start()
        {
            window = new GameWindow(800, 800, GraphicsMode.Default, "Collision 2D Performance");

            window.Load += Window_Load;
            window.UpdateFrame += Window_UpdateFrame;
            window.RenderFrame += Window_RenderFrame;
            window.Unload += Window_Unload;
            window.Run();
        }

        private void Window_UpdateFrame(object sender, FrameEventArgs e)
        {
            var state = Keyboard.GetState();
            if (state.IsKeyDown(Key.Escape))
            {
                window.Exit();
            }
        }

        private void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Viewport(0, 0, window.Width, window.Height);

            shaderProgram.Bind();

            var projectionMatrix = Transformation.GetOrthoProjectionMatrix(window.Width, window.Height, zNear, zFar, false);
            shaderProgram.SetUniform("projectionMatrix", projectionMatrix);

            var worldMatrix = Matrix4.Identity;
            shaderProgram.SetUniform("worldMatrix", worldMatrix);

            shaderProgram.Unbind();

            GL.Flush();
            window.SwapBuffers();
        }

        private void Window_Load(object sender, EventArgs e)
        {
            GL.ClearColor(Color.FromArgb(0, 0, 0, 0));

            shaderProgram = new ShaderProgram();
            shaderProgram.CreateVertexShader(Utils.LoadShaderCode("vertex.glsl"));
            shaderProgram.CreateFragmentShader(Utils.LoadShaderCode("fragment.glsl"));
            shaderProgram.Link();

            shaderProgram.CreateUniform("worldMatrix");
            shaderProgram.CreateUniform("projectionMatrix");
        }

        private void Window_Unload(object sender, EventArgs e)
        {
            shaderProgram.CleanUp();
        }
    }
}
