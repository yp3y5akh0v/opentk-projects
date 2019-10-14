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
        private Circle2D[] circles;
        private int numCircles = 1000;
        private float elapsedTime;
        private Random rand;
        private Vector2 minMaxRadius = new Vector2(1f, 6f);
        private QuadTree quadtree;

        static void Main(string[] args)
        {
            new Program().Start();
        }

        public void Start()
        {
            window = new GameWindow(1024, 800, GraphicsMode.Default, "Collision 2D Performance");
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

            var pi2 = 2 * Math.PI;
            elapsedTime += (float) e.Time;

            if (elapsedTime > 1)
            {
                elapsedTime--;
            }

            foreach (var circle in circles)
            {
                var rAngleX = (elapsedTime + 3 * rand.NextDouble());
                var rAngleY = (elapsedTime + 3 * rand.NextDouble());

                var rX = 5 * (float)rand.NextDouble();
                var rY = 5 * (float)rand.NextDouble();

                var ds = new Vector2(rX * (float)Math.Cos(pi2 * rAngleX), rY * (float)Math.Sin(pi2 * rAngleY));
                circle.UpdateCenter(ds);
            }

            quadtree.Update();

            for (var i = 0; i < circles.Length; i++)
            {
                circles[i].SetColor(Vector4.One);
                var others = quadtree.QueryCircle(circles[i].GetCenter(), 2 * minMaxRadius.Y);

                foreach (var other in others)
                {
                    if (other != circles[i] && circles[i].Intersect(other))
                    {
                        circles[i].SetColor(new Vector4(1f, 0f, 0f, 1f));
                    }
                }
            }
        }

        private void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Viewport(0, 0, window.Width, window.Height);

            shaderProgram.Bind();

            var projectionMatrix = Transformation.GetOrthoProjectionMatrix(window.Width, window.Height, zNear, zFar, false);
            shaderProgram.SetUniform("projectionMatrix", projectionMatrix);

            foreach (var circle in circles)
            {
                shaderProgram.SetUniform("worldMatrix", circle.GetTransformation());
                circle.Render();
            }

            quadtree.Render(shaderProgram);

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

            rand = new Random();
            circles = new Circle2D[numCircles];
            for (var i = 0; i < numCircles; i++)
            {
                var radius = Utils.Map((float) rand.NextDouble(), 0, 1, 2, minMaxRadius.Y);
                var rX = Utils.Map((float) rand.NextDouble(), 0, 1, 0, window.Width);
                var rY = Utils.Map((float) rand.NextDouble(), 0, 1, 0, window.Height);

                circles[i] = new Circle2D(new Vector2(rX, rY), radius);
            }

            quadtree = new QuadTree(new Rect2D(new Vector2(window.Width / 2f, window.Height / 2f),
                new Vector2(window.Width / 2f, window.Height / 2f)), 10);
            foreach (var circle in circles)
            {
                quadtree.Insert(circle);
            }
        }

        private void Window_Unload(object sender, EventArgs e)
        {
            shaderProgram.CleanUp();
            foreach (var circle in circles)
            {
                circle.CleanUp();
            }
        }
    }
}
