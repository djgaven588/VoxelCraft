using OpenToolkit.Mathematics;

namespace VoxelCraft
{
    public static class Mathmatics
    {
        public const double PI = 3.1415926535897931;
        public const double E = 2.7182818284590451;

        public static Matrix4 CreateTransformationMatrix(Vector3d translation, Quaterniond rotation, Vector3d scale)
        {
            Matrix4 matrix = Matrix4.Identity;

            translation.Z *= -1;

            matrix *= Matrix4.CreateFromQuaternion(new Quaternion((float)rotation.X, (float)rotation.Y, (float)rotation.Z, (float)rotation.W));
            matrix *= Matrix4.CreateScale((Vector3)scale);
            matrix *= Matrix4.CreateTranslation((Vector3)translation);
            return matrix;
        }

        public static Matrix4 CreateViewMatrix(Vector3d position, Quaterniond rotation)
        {
            Matrix4 matrix = Matrix4.Identity;
            Vector3 negativeCameraPos = (Vector3)(-position);
            matrix *= Matrix4.CreateTranslation(negativeCameraPos);
            matrix *= Matrix4.CreateFromQuaternion(new Quaternion((float)rotation.X, (float)rotation.Y, (float)rotation.Z, (float)rotation.W));
            return matrix;
        }

        /// <summary>
        /// Converts degrees into radians
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static double ConvertToRadians(double degrees)
        {
            return (PI / 180) * degrees;
        }

        /// <summary>
        /// Converts radians into degrees
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static double ConvertToDegrees(double radians)
        {
            return radians * (180 / PI);
        }
    }
}
