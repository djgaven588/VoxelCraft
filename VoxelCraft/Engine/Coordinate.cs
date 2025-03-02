﻿using System;
using System.Numerics;
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

        public Coordinate(Vector3 pos)
        {
            X = (int)Math.Floor(pos.X);
            Y = (int)Math.Floor(pos.Y);
            Z = (int)Math.Floor(pos.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BlockToIndex(Coordinate coord)
        {
            return coord.X + coord.Y * ChunkData.CHUNK_SIZE + coord.Z * ChunkData.CHUNK_SIZE_SQR;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int BlockToIndex()
        {
            return X + Y * ChunkData.CHUNK_SIZE + Z * ChunkData.CHUNK_SIZE_SQR;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVector(Coordinate coord)
        {
            return new Vector3(coord.X, coord.Y, coord.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Coordinate WorldToChunk(Vector3 position)
        {
            Coordinate chunk = new Coordinate(position);
            chunk.X >>= ChunkData.CHUNK_LOG_SIZE;
            chunk.Y >>= ChunkData.CHUNK_LOG_SIZE;
            chunk.Z >>= ChunkData.CHUNK_LOG_SIZE;
            return chunk;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Coordinate WorldToBlock(Vector3 position)
        {
            Coordinate block = new Coordinate(position);
            block.X &= ChunkData.CHUNK_SIZE_MINUS_ONE;
            block.Y &= ChunkData.CHUNK_SIZE_MINUS_ONE;
            block.Z &= ChunkData.CHUNK_SIZE_MINUS_ONE;
            return block;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Coordinate WorldToBlock()
        {
            Coordinate block = this;
            block.X &= ChunkData.CHUNK_SIZE_MINUS_ONE;
            block.Y &= ChunkData.CHUNK_SIZE_MINUS_ONE;
            block.Z &= ChunkData.CHUNK_SIZE_MINUS_ONE;
            return block;
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
        public Vector3 ToVector()
        {
            return new Vector3(X, Y, Z);
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
            return HashCode.Combine(X, Y, Z);
        }

        /// <summary>
        /// Returns a deterministic hashcode.
        /// WARNING: GetHashCode should be used especially when interacting with networking, use with caution.
        /// </summary>
        /// <returns>A hashcode</returns>
        public int GetDeterministicHashcode()
        {
            unchecked
            {
                int hashCode = 107;
                hashCode = (hashCode * 397) ^ X;
                hashCode = (hashCode * 359) ^ Y;
                hashCode = (hashCode * 563) ^ Z;
                return hashCode;
            }
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

        public static Coordinate Left       = new Coordinate(-1, 0, 0);
        public static Coordinate Right      = new Coordinate(1, 0, 0);
        public static Coordinate Up         = new Coordinate(0, 1, 0);
        public static Coordinate Down       = new Coordinate(0, -1, 0);
        public static Coordinate Forward    = new Coordinate(0, 0, 1);
        public static Coordinate Backward   = new Coordinate(0, 0, -1);
    }
}
