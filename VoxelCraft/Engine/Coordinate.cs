using OpenToolkit.Mathematics;
using System.Runtime.CompilerServices;

namespace VoxelCraft
{
    public struct Coordinate
    {
        public int X;
        public int Y;
        public int Z;

        public Coordinate(int value)
        {
            X = value;
            Y = value;
            Z = value;
        }

        public Coordinate(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d ToVector(Coordinate coord)
        {
            return new Vector3d(coord.X, coord.Y, coord.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Coordinate WorldToChunk(Vector3d position)
        {
            return new Coordinate((int)position.X >> ChunkData.CHUNK_LOG_SIZE, (int)position.Y >> ChunkData.CHUNK_LOG_SIZE, (int)position.Z >> ChunkData.CHUNK_LOG_SIZE);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Coordinate WorldToChunk()
        {
            return new Coordinate(X >> ChunkData.CHUNK_LOG_SIZE, Y >> ChunkData.CHUNK_LOG_SIZE, Z >> ChunkData.CHUNK_LOG_SIZE);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Coordinate ChunkToWorld()
        {
            return this * ChunkData.CHUNK_SIZE;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Coordinate WorldToRegion()
        {
            return new Coordinate(X >> (ChunkData.CHUNK_LOG_SIZE + Region.REGION_LOG_SIZE), Y >> (ChunkData.CHUNK_LOG_SIZE + Region.REGION_LOG_SIZE), Z >> (ChunkData.CHUNK_LOG_SIZE + Region.REGION_LOG_SIZE));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Coordinate RegionToWorld()
        {
            return this * ChunkData.CHUNK_SIZE * Region.REGION_SIZE;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3d ToVector()
        {
            return new Vector3d(X, Y, Z);
        }

        public override bool Equals(object obj)
        {
            Coordinate? coord = obj as Coordinate?;

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

        public static Coordinate operator *(Coordinate a, int b)
        {
            return new Coordinate(a.X * b, a.Y * b, a.Z * b);
        }

        public static Coordinate operator +(Coordinate a, Coordinate b)
        {
            return new Coordinate(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Coordinate operator -(Coordinate a, Coordinate b)
        {
            return new Coordinate(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Coordinate operator -(Coordinate a)
        {
            return new Coordinate(-a.X, -a.Y, -a.Z);
        }

        public static bool operator ==(Coordinate a, Coordinate b)
        {
            return !(a.X != b.X || a.Y != b.Y || a.Z != b.Z);
        }

        public static bool operator !=(Coordinate a, Coordinate b)
        {
            return !(a == b);
        }
    }
}
