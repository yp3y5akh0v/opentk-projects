using OpenTK;
using OpenTK.Input;
using System.Drawing;

namespace SharedLib
{
    public class MouseInput : IGameInput
    {

        private Vector3 prevPos;
        private Vector3 curPos;
        private Vector3 posDisp;

        private Camera camera;

        private bool isMouseInWindow;

        public MouseInput(Camera camera)
        {
            this.camera = camera;

            prevPos = Vector3.Zero;
            curPos = Vector3.Zero;
            posDisp = Vector3.Zero;
        }

        public void UpdateMouseEnter(bool val)
        {
            isMouseInWindow = val;
        }

        public void Input(GameWindow window)
        {
            var state = Mouse.GetCursorState();   
            var p = window.PointToClient(new Point(state.X, state.Y));

            posDisp = Vector3.Zero;
            curPos = new Vector3(p.X, p.Y, 0f);

            if (prevPos.X > 0.0f && prevPos.Y > 0.0f && isMouseInWindow && state.IsButtonDown(MouseButton.Left))
            {                
                posDisp = Vector3.Subtract(curPos, prevPos);
            }            

            prevPos = new Vector3(curPos.X, curPos.Y, curPos.Z);
        }

        public void Update(float interval)
        {
            var ds = Vector3.Multiply(posDisp, Constants.SENSITIVITY * interval);
            camera.UpdateRotation(ds.X, ds.Y);
        }
    }
}
