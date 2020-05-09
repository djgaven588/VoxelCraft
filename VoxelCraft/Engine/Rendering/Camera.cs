using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common.Input;
using System;

namespace VoxelCraft.Rendering
{
    public class Camera
    {
        private Vector2d cameraAngle;
        public Quaterniond cameraRot;
        public Vector3d cameraPos = new Vector3d(0, 0, -5);

        public Camera(double x, double y, double z)
        {
            cameraPos = new Vector3d(x, y, z);
        }

        public void Update(double timeDelta)
        {
            if (InputManager.IsKeyNowDown(Key.Escape))
                InputManager.ToggleMouseState();

            Vector3d movement = new Vector3d(InputManager.GetAxis(Key.D, Key.A), InputManager.GetAxis(Key.Space, Key.ShiftLeft), InputManager.GetAxis(Key.W, Key.S));

            if (movement.LengthSquared > 0)
            {
                movement = movement.Normalized() * timeDelta * 16;
            }

            cameraAngle += (Vector2d)InputManager.MouseDelta().Yx * timeDelta * 5;//new Vector2d(InputManager.GetAxis(Key.Z, Key.X), InputManager.GetAxis(Key.E, Key.Q)) * timeDelta * 2;

            cameraAngle.X = Math.Clamp(cameraAngle.X, -90, 90);

            cameraPos += Quaterniond.FromEulerAngles(0, Mathmatics.ConvertToRadians(cameraAngle.Y), 0) * movement;

            cameraRot = Quaterniond.FromEulerAngles(Mathmatics.ConvertToRadians(cameraAngle.X), Mathmatics.ConvertToRadians(cameraAngle.Y), 0);
        }
    }
}
