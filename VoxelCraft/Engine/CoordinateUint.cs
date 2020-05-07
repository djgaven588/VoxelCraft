using OpenToolkit.Mathematics;
using System.Runtime.CompilerServices;

namespace VoxelCraft
{
    public struct CoordinateUint
    {
        public uint X;
        public uint Y;
        public uint Z;

        public CoordinateUint(uint value)
        {
            X = value;
            Y = value;
            Z = value;
        }

        public CoordinateUint(uint x, uint y, uint z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3d ToVector()
        {
            return new Vector3d(X, Y, Z);
        }

        public override bool Equals(object obj)
        {
            CoordinateUint? coord = obj as CoordinateUint?;

            return !(coord == null || coord.Value.X != X || coord.Value.Y != Y || coord.Value.Z != Z);
        }

        public override string ToString()
        {
            return $"X: {X}, Y: {Y}, Z: {Z}";
        }

        public override int GetHashCode()
        {
            return (X, Y, Z).GetHashCode();
        }

        public static CoordinateUint operator *(CoordinateUint a, uint b)
        {
            return new CoordinateUint(a.X * b, a.Y * b, a.Z * b);
        }

        public static CoordinateUint operator +(CoordinateUint a, CoordinateUint b)
        {
            return new CoordinateUint(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static CoordinateUint operator -(CoordinateUint a, CoordinateUint b)
        {
            return new CoordinateUint(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static bool operator ==(CoordinateUint a, CoordinateUint b)
        {
            return !(a.X != b.X || a.Y != b.Y || a.Z != b.Z);
        }

        public static bool operator !=(CoordinateUint a, CoordinateUint b)
        {
            return !(a == b);
        }
    }
}
