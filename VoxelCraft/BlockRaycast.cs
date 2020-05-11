using OpenToolkit.Mathematics;
using System;
using VoxelCraft.Rendering;

namespace VoxelCraft
{
    public static class BlockRaycast
    {
        public struct RaycastData
        {
            public Coordinate Chunk;
            public Coordinate Block;

            public BlockData BlockData;

            public byte HitSide;

            public bool HitBlock;
            public bool RayTrapped;

            public double DistanceRemaining;

            public RaycastData(bool failed = true)
            {
                Chunk = new Coordinate();
                Block = new Coordinate();
                BlockData = new BlockData();
                HitSide = 0;
                HitBlock = false;
                RayTrapped = false;
                DistanceRemaining = 0;
            }
        }

        public static RaycastData FindBlock(Vector3d startPosition, Vector3d direction, double distance)
        {
            Coordinate currentChunk = Coordinate.WorldToChunk(startPosition);
            Coordinate currentBlock = Coordinate.WorldToBlock(startPosition);
            // CHECK THIS WORLD_TO_BLOCK METHOD

            World.LoadedChunks.TryGetValue(currentChunk, out ChunkData searchingChunk);
            byte hitSide = 0;

            Vector3d currentPosition = startPosition;
            double startDistance = distance;

            if(searchingChunk == null)
            {
                return new RaycastData(failed: true);
            }
            else if(searchingChunk.BlockData[currentBlock.X + currentBlock.Y * ChunkData.CHUNK_SIZE + currentBlock.Z * ChunkData.CHUNK_SIZE_SQR].BlockID != 0)
            {
                return new RaycastData()
                {
                    Block = currentBlock,
                    Chunk = currentChunk,
                    HitBlock = true,
                    RayTrapped = true,
                    BlockData = new BlockData(),
                    HitSide = 0
                };
            }

            while(distance > 0)
            {
                double distanceX = GetDistance(currentPosition.X, direction.X, startDistance);// ((direction.X > 0.01 && direction.X < -0.01) || direction.X > 20 || direction.X < -20) ? double.MaxValue : ;//(Math.Ceiling(currentPosition.X + 0.00000001) - currentPosition.X) / direction.X;
                double distanceY = GetDistance(currentPosition.Y, direction.Y, startDistance);//((direction.Y > 0.01 && direction.Y < -0.01) || direction.Y > 20 || direction.Y < -20) ? double.MaxValue : (Math.Ceiling(currentPosition.Y + 0.00000001) - currentPosition.Y) / direction.Y;
                double distanceZ = GetDistance(currentPosition.Z, direction.Z, startDistance);//((direction.Z > 0.01 && direction.Z < -0.01) || direction.Z > 20 || direction.Z < -20) ? double.MaxValue : (Math.Ceiling(currentPosition.Z + 0.00000001) - currentPosition.Z) / direction.Z;

                /*
                if (distanceX < -startDistance)
                {
                    distanceX = double.MaxValue;
                }
                else if(distanceY < -startDistance)
                {
                    distanceY = double.MaxValue;
                }
                else if(distanceZ < -startDistance)
                {
                    distanceZ = double.MaxValue;
                }*/

                Debug.Log($"Distance: {distanceX}, {distanceY}, {distanceZ}");
                Debug.Log($"Current Position: {currentPosition}");

                Coordinate lastChunk = currentChunk;

                if (distanceX <= distanceY && distanceX <= distanceZ)
                {
                    if(direction.X >= 0)
                    {
                        if(currentBlock.X == ChunkData.CHUNK_SIZE_MINUS_ONE)
                        {
                            currentBlock.X = 0;
                            currentChunk.X++;
                        }
                        else
                        {
                            currentBlock.X++;
                        }

                        hitSide = 3; // Left side, west
                    }
                    else
                    {
                        if (currentBlock.X == 0)
                        {
                            currentBlock.X = ChunkData.CHUNK_SIZE_MINUS_ONE;
                            currentChunk.X--;
                        }
                        else
                        {
                            currentBlock.X--;
                        }

                        hitSide = 2; // Right side, east
                    }

                    currentPosition += direction * distanceX;
                    distance -= distanceX;
                }
                else if (distanceY <= distanceX && distanceY <= distanceZ)
                {
                    if (direction.Y >= 0)
                    {
                        if (currentBlock.Y == ChunkData.CHUNK_SIZE_MINUS_ONE)
                        {
                            currentBlock.Y = 0;
                            currentChunk.Y++;
                        }
                        else
                        {
                            currentBlock.Y++;
                        }

                        hitSide = 5; // Bottom side
                    }
                    else
                    {
                        if (currentBlock.Y == 0)
                        {
                            currentBlock.Y = ChunkData.CHUNK_SIZE_MINUS_ONE;
                            currentChunk.Y--;
                        }
                        else
                        {
                            currentBlock.Y--;
                        }

                        hitSide = 4; // Top side
                    }

                    currentPosition += direction * distanceY;
                    distance -= distanceY;
                }
                else if (distanceZ <= distanceX && distanceZ <= distanceY)
                {
                    if (direction.Z >= 0)
                    {
                        if (currentBlock.Z == ChunkData.CHUNK_SIZE_MINUS_ONE)
                        {
                            currentBlock.Z = 0;
                            currentChunk.Z++;
                        }
                        else
                        {
                            currentBlock.Z++;
                        }

                        hitSide = 1; // Back side, south
                    }
                    else
                    {
                        if (currentBlock.Z == 0)
                        {
                            currentBlock.Z = ChunkData.CHUNK_SIZE_MINUS_ONE;
                            currentChunk.Z--;
                        }
                        else
                        {
                            currentBlock.Z--;
                        }

                        hitSide = 0; // Front side, north
                    }

                    currentPosition += direction * distanceZ;
                    distance -= distanceZ;
                }

                Debug.Log($"After Position: {currentPosition}");
                Graphics.QueueDraw(World.TestMaterial, PrimitiveMeshes.Cube, Mathmatics.CreateTransformationMatrix(currentPosition + new Vector3d(0, -1, 0), Quaterniond.Identity, Vector3d.One));

                if (lastChunk != currentChunk)
                {
                    World.LoadedChunks.TryGetValue(currentChunk, out searchingChunk);
                }

                if(searchingChunk == null)
                {
                    return new RaycastData(failed: true);
                }

                BlockData dat = searchingChunk.BlockData[currentBlock.X + currentBlock.Y * ChunkData.CHUNK_SIZE + currentBlock.Z * ChunkData.CHUNK_SIZE_SQR];
                if(dat.BlockID != 0)
                {
                    return new RaycastData()
                    {
                        Block = currentBlock,
                        BlockData = dat,
                        Chunk = currentChunk,
                        DistanceRemaining = distance,
                        HitBlock = true,
                        HitSide = hitSide,
                        RayTrapped = false
                    };
                }
            }

            return new RaycastData();
        }

        private static double GetDistance(double location, double direction, double maxDistance)
        {
            if((direction > 0.001 && direction < -0.001) || direction > 20 || direction < -20)
            {
                return double.MaxValue;
            }

            double value;
            if(direction >= 0)
            {
                value = Math.Ceiling(location + 0.00000001) - location;
            }
            else
            {
                value = location - Math.Floor(location - 0.00000001);
            }

            value = Math.Abs(value) / Math.Abs(direction);

            if(value > maxDistance || value < -maxDistance)
            {
                return double.MaxValue;
            }

            return value;
        }
    }
}
