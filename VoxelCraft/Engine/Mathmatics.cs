using System.Numerics;

namespace VoxelCraft
{
    public static class Mathmatics
    {
        public const float PI = 3.1415926535897931f;
        public const float E = 2.7182818284590451f;

        public static Matrix4x4 CreateTransformationMatrix(Vector3 translation, Vector3 rotation, Vector3 scale)
        {
            Matrix4x4 matrix = Matrix4x4.Identity;

            translation.Z = -translation.Z;

            matrix *= Matrix4x4.CreateScale(scale);
            matrix *= Matrix4x4.CreateRotationY(ConvertToRadians(rotation.Y));
            matrix *= Matrix4x4.CreateRotationX(ConvertToRadians(rotation.X));
            matrix *= Matrix4x4.CreateRotationZ(ConvertToRadians(rotation.Z));
            matrix *= Matrix4x4.CreateTranslation(translation);
            return matrix;
        }

        public static Matrix4x4 CreateViewMatrix(Vector3 position, Vector3 rotation)
        {
            Matrix4x4 matrix = Matrix4x4.Identity;

            position.Z = -position.Z;

            Vector3 negativeCameraPos = -position;
            matrix *= Matrix4x4.CreateTranslation(negativeCameraPos);
            matrix *= Matrix4x4.CreateRotationY(ConvertToRadians(rotation.Y));
            matrix *= Matrix4x4.CreateRotationX(ConvertToRadians(rotation.X));
            matrix *= Matrix4x4.CreateRotationZ(ConvertToRadians(rotation.Z));
            return matrix;
        }

        /// <summary>
        /// Converts degrees into radians
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static float ConvertToRadians(float degrees)
        {
            return (PI / 180) * degrees;
        }

        /// <summary>
        /// Converts radians into degrees
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static float ConvertToDegrees(float radians)
        {
            return radians * (180 / PI);
        }

        public static Vector3 TransformUnitZ(in Quaternion rotation)
        {
            //This operation is an optimized-down version of v' = q * v * q^-1.
            //The expanded form would be to treat v as an 'axis only' quaternion
            //and perform standard quaternion multiplication.  Assuming q is normalized,
            //q^-1 can be replaced by a conjugation.
            float x2 = rotation.X + rotation.X;
            float y2 = rotation.Y + rotation.Y;
            float z2 = rotation.Z + rotation.Z;
            float xx2 = rotation.X * x2;
            float xz2 = rotation.X * z2;
            float yy2 = rotation.Y * y2;
            float yz2 = rotation.Y * z2;
            float wx2 = rotation.W * x2;
            float wy2 = rotation.W * y2;
            return new Vector3(xz2 + wy2, yz2 - wx2, xx2 - yy2);
        }
    }
}
