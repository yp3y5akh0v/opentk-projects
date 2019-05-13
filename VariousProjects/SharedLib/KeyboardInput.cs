using OpenTK;
using OpenTK.Input;
using System;

namespace SharedLib
{
    public class KeyboardInput : IGameInput
    {
        private readonly float SPEED = 5f;
        private readonly float ACCELERATION = 2f;

        private Vector3 posDisp;
        private Camera camera;
        private bool nitro;
        private float zNear;
        private float zFar;

        public KeyboardInput(Camera camera, float zNear, float zFar)
        {
            this.camera = camera;
            this.zNear = zNear;
            this.zFar = zFar;

            posDisp = Vector3.Zero;
        }

        public void Input(GameWindow window)
        {
            var state = Keyboard.GetState();

            posDisp = Vector3.Zero;

            if (state.IsKeyDown(Key.W) || state.IsKeyDown(Key.Up))
            {
                posDisp.Z = -1;
            }
            else if (state.IsKeyDown(Key.S) || state.IsKeyDown(Key.Down))
            {
                posDisp.Z = 1;
            }

            if (state.IsKeyDown(Key.A) || state.IsKeyDown(Key.Left))
            {
                posDisp.X = -1;
            }
            else if (state.IsKeyDown(Key.D) || state.IsKeyDown(Key.Right))
            {
                posDisp.X = 1;
            }

            if (state.IsKeyDown(Key.Z))
            {
                posDisp.Y = -1;
            }
            else if (state.IsKeyDown(Key.X))
            {
                posDisp.Y = 1;
            }

            if (state.IsKeyDown(Key.ShiftLeft))
            {
                nitro = true;
            }
            else
            {
                nitro = false;
            }            
        }

        public void Update(float interval)
        {
            var ds = Vector3.Multiply(posDisp, SPEED * interval);

            if (nitro)
            {
                ds = Vector3.Add(ds, Vector3.Multiply(posDisp, ACCELERATION * interval * interval / 2));
            }

            camera.UpdatePosition(ds.X, ds.Y, ds.Z);
        }
    }
}
