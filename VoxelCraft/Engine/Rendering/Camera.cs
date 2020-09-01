using OpenTK.Windowing.Common.Input;
using System;
using System.Numerics;

namespace VoxelCraft.Rendering
{
    public class Camera
    {
        public Vector3 Rotation;
        public Vector3 Position = new Vector3(0, 0, -5);

        public Camera(float x, float y, float z)
        {
            Position = new Vector3(x, y, z);
        }

        public void Update(float timeDelta)
        {
            if (InputManager.IsKeyNowDown(Key.Escape))
                InputManager.ToggleMouseState();

            Vector3 movement = new Vector3(InputManager.GetAxis(Key.D, Key.A), InputManager.GetAxis(Key.Space, Key.ShiftLeft), InputManager.GetAxis(Key.W, Key.S));

            if (movement.Length() > 0)
            {
                movement = (movement / movement.Length()) * timeDelta * 16;
            }

            var ang = InputManager.MouseDelta().Yx * timeDelta * 5;
            Rotation += new Vector3(ang.X, ang.Y, 0);

            Rotation.X = Math.Clamp(Rotation.X, -90, 90);

            Position += Vector3.Transform(movement, Quaternion.CreateFromYawPitchRoll(Mathmatics.ConvertToRadians(Rotation.Y), 0, 0));
        }
    }
}
