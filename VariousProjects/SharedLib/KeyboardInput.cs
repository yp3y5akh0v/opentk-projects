using OpenTK;
using OpenTK.Input;
using System;

namespace SharedLib
{
    public class KeyboardInput : IGameInput
    {

        private Vector3 posDisp;
        private Camera camera;

        public KeyboardInput(Camera camera)
        {
            this.camera = camera;            
            posDisp = Vector3.Zero;
        }

        public void Input(GameWindow window)
        {
            var state = Keyboard.GetState();

            posDisp = Vector3.Zero;

            if (state.IsKeyDown(Key.W) || state.IsKeyDown(Key.Up))
            {
                posDisp.Y = 1;
            }
            else if (state.IsKeyDown(Key.S) || state.IsKeyDown(Key.Down))
            {
                posDisp.Y = -1;
            }

            if (state.IsKeyDown(Key.A) || state.IsKeyDown(Key.Left))
            {
                posDisp.X = -1;
            }
            else if (state.IsKeyDown(Key.D) || state.IsKeyDown(Key.Right))
            {
                posDisp.X = 1;
            }

            if (state.IsKeyDown(Key.Q))
            {
                posDisp.Z = 1;
            }
            else if (state.IsKeyDown(Key.E))
            {
                posDisp.Z = -1;
            }          
        }

        public void Update(float interval)
        {
            var ds = Vector3.Multiply(posDisp, Constants.SPEED * interval);

            camera.UpdatePosition(ds.X, ds.Y, ds.Z);
        }
    }
}
