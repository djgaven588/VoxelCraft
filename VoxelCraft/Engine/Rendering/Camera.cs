using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common.Input;

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
            Vector3d movement = new Vector3d(InputManager.GetAxis(Key.D, Key.A), InputManager.GetAxis(Key.Space, Key.ShiftLeft), InputManager.GetAxis(Key.W, Key.S));

            if (movement.LengthSquared > 0)
            {
                movement = movement.Normalized() * timeDelta * 16;
            }

            cameraAngle += new Vector2d(InputManager.GetAxis(Key.Z, Key.X), InputManager.GetAxis(Key.E, Key.Q)) * timeDelta * 2;

            Quaterniond rot = Quaterniond.FromEulerAngles(0, cameraAngle.Y, 0);
            cameraPos += rot * movement;

            cameraRot = Quaterniond.FromEulerAngles(0, 0, cameraAngle.X) * rot;
        }
    }
}
