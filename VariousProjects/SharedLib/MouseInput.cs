using OpenTK;
using OpenTK.Input;
using System;
using System.Drawing;

namespace SharedLib
{
    public class MouseInput : IGameInput
    {
        private Camera camera;

        public MouseInput(Camera camera)
        {
            this.camera = camera;
        }

        public void Input(GameWindow window, float interval)
        {
            if (window.Focused)
            {
                var state = Mouse.GetCursorState();
                var middle = new Vector2(window.Width / 2.0f, window.Height / 2.0f);
                var p = window.PointToClient(new Point(state.X, state.Y));
                var cursor = new Vector2(p.X, p.Y);
                var delta = middle - cursor;

                delta.NormalizeFast();

                delta *= Constants.SENSITIVITY * interval;

                camera.UpdateRotation(delta.X, delta.Y);

                var invPos = window.PointToScreen(new Point((int) middle.X, (int) middle.Y));

                Mouse.SetPosition(invPos.X, invPos.Y);
            }
        }
    }
}
