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

            Debug.Log(direction);

            double distanceX = (direction.X < 0.001 && direction.X > -0.001) ? double.MaxValue : currentPosition.X >= 0 ? currentPosition.X % 1 * Math.Abs(direction.X) : (1 - currentPosition.X % 1) * Math.Abs(direction.X);
            double distanceY = (direction.Y < 0.001 && direction.Y > -0.001) ? double.MaxValue : currentPosition.Y >= 0 ? currentPosition.Y % 1 * Math.Abs(direction.Y) : (1 - currentPosition.Y % 1) * Math.Abs(direction.Y);
            double distanceZ = (direction.Z < 0.001 && direction.Z > -0.001) ? double.MaxValue : currentPosition.Z >= 0 ? currentPosition.Z % 1 * Math.Abs(direction.Z) : (1 - currentPosition.Z % 1) * Math.Abs(direction.Z);
            while (distance > 0)
            {
                Coordinate lastChunk = currentChunk;

                Graphics.QueueDraw(World.TestMaterial, PrimitiveMeshes.Cube, Mathmatics.CreateTransformationMatrix(currentPosition - new Vector3d(0, 1, 0), Quaterniond.Identity, Vector3d.One * 0.2));

                Debug.Log(currentPosition + " " + direction);

                //Debug.Log($"{distanceX}, {distanceY}, {distanceZ} {currentBlock} - {currentPosition}");

                if(distanceX <= distanceY && distanceX <= distanceZ)
                {
                    if (direction.X >= 0)
                    {
                        if (currentBlock.X == ChunkData.CHUNK_SIZE_MINUS_ONE)
                        {
                            currentBlock.X = 0;
                            currentChunk.X++;
                        }
                        else
                        {
                            currentBlock.X++;
                        }
                    }
                    else
                    {
                        if(currentBlock.X == 0)
                        {
                            currentBlock.X = ChunkData.CHUNK_SIZE_MINUS_ONE;
                            currentChunk.X--;
                        }
                        else
                        {
                            currentBlock.X++;
                        }
                    }

                    distance -= distanceX;
                    distanceX += Math.Abs(direction.X);
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
                    }

                    distance -= distanceY;
                    distanceY += Math.Abs(direction.Y);
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
                    }

                    distance -= distanceZ;
                    distanceZ += Math.Abs(direction.Z);
                }

                currentPosition = startPosition + direction * (startDistance - distance);

                if (currentChunk != lastChunk)
                {
                    World.LoadedChunks.TryGetValue(currentChunk, out searchingChunk);
                }

                if (searchingChunk == null)
                {
                    return new RaycastData();
                }

                BlockData dat = searchingChunk.BlockData[currentBlock.X + currentBlock.Y * ChunkData.CHUNK_SIZE + currentBlock.Z * ChunkData.CHUNK_SIZE_SQR];
                if(dat.BlockID != 0)
                {
                    return new RaycastData()
                    {
                        Block = currentBlock,
                        Chunk = currentChunk,
                        BlockData = dat,
                        HitBlock = true,
                        RayTrapped = false,
                        HitSide = hitSide,
                        DistanceRemaining = distance
                    };
                }
            }

            return new RaycastData();
        }
    }
}
