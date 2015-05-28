using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Engine
{
    public abstract class CameraController : IDisposable
    {
        public Camera Camera { get; set; }

        public abstract void UpdateInput(FrameEventArgs e);
        public abstract void LookAt(Vector3 target);
        public abstract void Dispose();
    }

    public class FirstPersonCameraController : CameraController
    {
        public const float DefaultCameraSpeed = 100.0f;
        public const float DefaultCameraRotationSpeed = 0.1f;
        public const float TwoPi = (float)(Math.PI * 2.0);
        public const float PhiError = 0.1f;

        public float Phi { get; set; }
        public float Theta { get; set; }
        public float CameraSpeed { get; set; }
        public float CameraRotationSpeed { get; set; }

        protected KeyboardDevice keyboard;
        protected MouseDevice mouse;
        protected GameWindow gameWindow;
        protected bool bMouseDown = false;

        public Vector3 LookDirection
        {
            get
            {
                return new Vector3((float)(Math.Cos(Theta) * Math.Sin(Phi)), (float)Math.Cos(Phi), (float)(Math.Sin(Theta) * Math.Sin(Phi)));
            }
        }

        public FirstPersonCameraController(Camera camera, GameWindow gameWindow)
        {
            Camera = camera;

            this.keyboard = gameWindow.Keyboard;
            this.mouse = gameWindow.Mouse;
            this.gameWindow = gameWindow;

            mouse.ButtonDown += OnMouseDown;
            mouse.ButtonUp += OnMouseUp;

            CameraSpeed = DefaultCameraSpeed;
            CameraRotationSpeed = DefaultCameraRotationSpeed;

            Phi = (float)Math.PI / 2f;

            OrientCamera();
        }

        public override void UpdateInput(FrameEventArgs e)
        {
            UpdateInputKeyboard(keyboard, e);

            if (bMouseDown)
                UpdateInputMouse(mouse, gameWindow.ClientSize, e);

            OrientCamera();
        }

        protected void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Right)
            {
                bMouseDown = true;
                gameWindow.CursorVisible = false;
                CenterMouse();
            }
        }

        protected void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Right)
            {
                bMouseDown = false;
                gameWindow.CursorVisible = true;
            }
        }

        protected void UpdateInputKeyboard(KeyboardDevice keyboard, FrameEventArgs e)
        {
            Vector3 lookNormalized = LookDirection;
            Vector3 sideNormalized = Vector3.Cross(lookNormalized, Camera.Up);

            lookNormalized.Normalize();
            sideNormalized.Normalize();

            if (keyboard[Key.W])
                Camera.Position += lookNormalized * CameraSpeed * (float)e.Time;
            if (keyboard[Key.S])
                Camera.Position -= lookNormalized * CameraSpeed * (float)e.Time;
            if (keyboard[Key.D])
                Camera.Position += sideNormalized * CameraSpeed * (float)e.Time;
            if (keyboard[Key.A])
                Camera.Position -= sideNormalized * CameraSpeed * (float)e.Time;
        }

        protected void UpdateInputMouse(MouseDevice mouse, Size ClientSize, FrameEventArgs e)
        {
            int mouseX = mouse.X - ClientSize.Width / 2;
            int mouseY = mouse.Y - ClientSize.Height / 2;

            Theta += (float)mouseX * CameraRotationSpeed * (float)e.Time;
            Phi += (float)mouseY * CameraRotationSpeed * (float)e.Time;

            ClampAngles();
            CenterMouse();
        }

        public override void LookAt(Vector3 target)
        {
            var cameraToTarget = target - Camera.Position;

            Theta = (float)Math.Atan2(cameraToTarget.Z, cameraToTarget.X);
            Phi = (float)Math.Acos(cameraToTarget.Y / cameraToTarget.Length);

            ClampAngles();
            OrientCamera();
        }

        protected void ClampAngles()
        {
            Phi = Clamp(Phi, PhiError, (float)Math.PI - PhiError);
            Theta = Theta % TwoPi;
        }

        protected float Clamp(float value, float min, float max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        protected void CenterMouse()
        {
            var screenPosition = gameWindow.PointToScreen(new Point(gameWindow.ClientSize.Width / 2, gameWindow.ClientSize.Height / 2));
            Mouse.SetPosition((double)screenPosition.X, (double)screenPosition.Y);
        }

        protected void OrientCamera()
        {
            Camera.Target = Camera.Position + LookDirection;
        }

        public override void Dispose()
        {
            mouse.ButtonDown -= OnMouseDown;
            mouse.ButtonUp -= OnMouseUp;
        }
    }
}
