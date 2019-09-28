using OpenTK;

namespace SharedLib
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

            camera = new Camera(Vector3.Zero, new Vector3(MathHelper.DegreesToRadians(90f), 0f, 0f));
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

        public void SetRotation(Vector3 v)
        {
            camera.SetRotation(v);
        }

        public void SetRotationX(float x)
        {
            camera.SetRotationX(x);
        }

        public void SetRotationY(float y)
        {
            camera.SetRotationY(y);
        }

        public void SetRotationZ(float z)
        {
            camera.SetRotationZ(z);
        }

        public void UpdatePosition(Vector3 offset)
        {
            camera.UpdatePosition(offset);
        }
    }
}
