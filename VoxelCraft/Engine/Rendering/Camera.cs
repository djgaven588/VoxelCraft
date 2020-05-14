using OpenToolkit.Windowing.Common.Input;
using System;
using System.Numerics;

namespace VoxelCraft.Rendering
{
    public class Camera
    {
        public Vector2 Angle;
        public Quaternion Rotation;
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

            var ang = InputManager.MouseDelta() * timeDelta * 5;
            Angle += new Vector2(ang.X, ang.Y);

            Angle.Y = Math.Clamp(Angle.Y, -90, 90);

            Position += Vector3.Transform(movement, Quaternion.CreateFromYawPitchRoll(0, Mathmatics.ConvertToRadians(Angle.Y), 0));//Mathmatics.TransformUnitZ(Quaternion.CreateFromYawPitchRoll(Mathmatics.ConvertToRadians(Angle.Y), 0, 0)) * movement;//new Quaterniond(temp.X, temp.Y, temp.Z, temp.W) * movement;

            Rotation = Quaternion.CreateFromYawPitchRoll(Mathmatics.ConvertToRadians(Angle.X), Mathmatics.ConvertToRadians(Angle.Y), 0);
        }
    }
}
