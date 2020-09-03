using System;
using System.Numerics;

// The below code is heavily based off of
// https://gist.github.com/dogfuntom/cc881c8fc86ad43d55d8
// It has been modified in order to work how I want it to
// but it is still based off of the initial implementation.

namespace VoxelCraft
{
    public static class BlockRaycast
    {
        public static (bool, Coordinate, Coordinate) Raycast(Vector3 origin, Vector3 direction, float radius)
        {
            // From "A Fast Voxel Traversal Algorithm for Ray Tracing"
            // by John Amanatides and Andrew Woo, 1987
            // <http://www.cse.yorku.ca/~amana/research/grid.pdf>
            // <http://citeseer.ist.psu.edu/viewdoc/summary?doi=10.1.1.42.3443>
            // Extensions to the described algorithm:
            //   • Imposed a distance limit.
            //   • The face passed through to reach the current cube is provided to
            //     the callback.

            // The foundation of this algorithm is a parameterized representation of
            // the provided ray,
            //                    origin + t * direction,
            // except that t is not actually stored; rather, at any given point in the
            // traversal, we keep track of the *greater* t values which we would have
            // if we took a step sufficient to cross a cube boundary along that axis
            // (i.e. change the integer part of the coordinate) in the variables
            // tMaxX, tMaxY, and tMaxZ.

            Coordinate currentCube = new Coordinate((int)MathF.Floor(origin.X), (int)MathF.Floor(origin.Y), (int)MathF.Floor(origin.Z));

            // Break out direction vector.
            var dx = direction.X;
            var dy = direction.Y;
            var dz = direction.Z;

            // Direction to increment x,y,z when stepping.
            var stepX = DirectionToIncrement(dx);
            var stepY = DirectionToIncrement(dy);
            var stepZ = DirectionToIncrement(dz);

            // See description above. The initial values depend on the fractional
            // part of the origin.
            var tMaxX = DistanceToGridIntersect(origin.X, dx);
            var tMaxY = DistanceToGridIntersect(origin.Y, dy);
            var tMaxZ = DistanceToGridIntersect(origin.Z, dz);

            // The change in t when taking a step (always positive).
            var tDeltaX = stepX / dx;
            var tDeltaY = stepY / dy;
            var tDeltaZ = stepZ / dz;

            // Buffer for reporting faces to the callback.
            var face = new Coordinate();

            // Avoids an infinite loop.
            if (dx == 0 && dy == 0 && dz == 0)
                throw new Exception("Ray-cast in zero direction!");

            // Rescale from units of 1 cube-edge to units of 'direction' so we can
            // compare with 't'.
            radius /= MathF.Sqrt(dx * dx + dy * dy + dz * dz);

            while (true)
            {
                // Check if current position is a hit block
                if (IsBlockHit(currentCube))
                {
                    return (true, currentCube, face);
                }

                // tMaxX stores the t-value at which we cross a cube boundary along the
                // X axis, and similarly for Y and Z. Therefore, choosing the least tMax
                // chooses the closest cube boundary. Only the first case of the four
                // has been commented in detail.
                if (tMaxX < tMaxY)
                {
                    if (tMaxX < tMaxZ)
                    {
                        if (tMaxX > radius)
                            return (false, new Coordinate(), new Coordinate());
                        // Update which cube we are now in.
                        currentCube.X += stepX;
                        // Adjust tMaxX to the next X-oriented boundary crossing.
                        tMaxX += tDeltaX;
                        // Record the normal vector of the cube face we entered.
                        face.X = -stepX;
                        face.Y = 0;
                        face.Z = 0;
                    }
                    else
                    {
                        if (tMaxZ > radius)
                            return (false, new Coordinate(), new Coordinate());
                        currentCube.Z += stepZ;
                        tMaxZ += tDeltaZ;
                        face.X = 0;
                        face.Y = 0;
                        face.Z = -stepZ;
                    }
                }
                else
                {
                    if (tMaxY < tMaxZ)
                    {
                        if (tMaxY > radius)
                            return (false, new Coordinate(), new Coordinate());
                        currentCube.Y += stepY;
                        tMaxY += tDeltaY;
                        face.X = 0;
                        face.Y = -stepY;
                        face.Z = 0;
                    }
                    else
                    {
                        // Identical to the second case, repeated for simplicity in
                        // the conditionals.
                        if (tMaxZ > radius)
                            return (false, new Coordinate(), new Coordinate());
                        currentCube.Z += stepZ;
                        tMaxZ += tDeltaZ;
                        face.X = 0;
                        face.Y = 0;
                        face.Z = -stepZ;
                    }
                }
            }
        }

        private static bool IsBlockHit(Coordinate pos)
        {
            return (!World.LoadedChunks.TryGetValue(pos.WorldToChunk(), out ChunkData data) || data.BlockData[Coordinate.BlockToIndex(pos.WorldToBlock())].BlockID != 0);
        }

        private static int DirectionToIncrement(float x)
        {
            return x > 0 ? 1 : x < 0 ? -1 : 0;
        }

        private static float DistanceToGridIntersect(float s, float ds)
        {
            // Some kind of edge case, see:
            // http://gamedev.stackexchange.com/questions/47362/cast-ray-to-select-block-in-voxel-game#comment160436_49423
            var sIsInteger = Math.Round(s) == s;
            if (ds < 0 && sIsInteger)
                return 0;

            return (float)(ds > 0 ? CustomCeil(s) - s : s - Math.Floor(s)) / Math.Abs(ds);
        }

        private static float CustomCeil(float s) 
        {
            if (s == 0f) 
                return 1f; 
            else 
                return (float)Math.Ceiling(s); 
        }
    }
}
