using OpenTK;
using SharedLib;

namespace MazeGen3D
{
    public class Player
    {
        private readonly float radius;
        private readonly Camera camera;
        private readonly KeyboardInput keyboard;
        private readonly MouseInput mouse;

        public Player(float radius)
        {
            this.radius = radius;

            camera = new Camera(1000f * Vector3.One, new Vector3(MathHelper.DegreesToRadians(90f), 0f, 0f));
            keyboard = new KeyboardInput(camera);
            mouse = new MouseInput(camera);
        }

        public void Input(GameWindow window, float interval)
        {
            keyboard.Input(window, interval);
            mouse.Input(window, interval);
        }

        public Camera GetCamera()
        {
            return camera;
        }

        public float GetRadius()
        {
            return radius;
        }
        
        public Vector3 GetPosition()
        {
            return camera.GetPosition();
        }

        public void SetPosition(Vector3 v)
        {
            camera.SetPosition(v);
        }
    }
}
