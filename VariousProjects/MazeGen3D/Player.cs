﻿using OpenTK;
using SharedLib;
using System;

namespace MazeGen3D
{
    public class Player
    {
        private float radius;

        private Camera camera;
        private KeyboardInput keyboard;
        private MouseInput mouse;

        public Player(float radius)
        {
            this.radius = radius;

            camera = new Camera(Vector3.Zero, new Vector3((float)Math.PI, 0f, 0f));
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
        
        public Vector3 GetPosition()
        {
            return camera.GetPosition();
        }
    }
}
