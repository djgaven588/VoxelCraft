using System.Collections.Generic;

namespace VoxelCraft
{
    public static class ChunkHandler
    {
        public const byte DISTANCE_KEPT_LOADED = 10;
        public const byte DISTANCE_LOAD_UPDATE = 8;

        public static void CheckToUnload(Coordinate chunkPosition)
        {
            Queue<Coordinate> chunksToRemove = new Queue<Coordinate>();
            foreach (ChunkData chunk in World.LoadedChunks.Values)
            {
                Coordinate coord = chunkPosition - chunk.ChunkPosition;
                if (coord.X > DISTANCE_KEPT_LOADED || coord.X < -DISTANCE_KEPT_LOADED || coord.Z > DISTANCE_KEPT_LOADED || coord.Z < -DISTANCE_KEPT_LOADED)
                {
                    chunksToRemove.Enqueue(chunk.ChunkPosition);
                }
                else if(chunk.Mesh != null && (coord.X > World.RenderDistance || coord.X < -World.RenderDistance || coord.Z > World.RenderDistance || coord.Z < -World.RenderDistance))
                {
                    chunk.Mesh.CleanUp();
                    chunk.Mesh = null;
                    chunk.RegenerateMesh = true;
                }
            }

            while (chunksToRemove.TryDequeue(out Coordinate toRemove))
            {
                World.DeleteChunk(toRemove);
            }
        }

        public static void CheckToLoadAndUpdate(Coordinate chunkPosition)
        {
            chunkPosition.Y = 0;
            for (int i = 0; i <= DISTANCE_LOAD_UPDATE; i++)
            {
                HandleLoadAndUpdate(i, chunkPosition);
            }
        }

        public static void CheckToUpdate(Coordinate chunkPosition)
        {
            chunkPosition.Y = 0;
            for (int i = 0; i <= DISTANCE_LOAD_UPDATE; i++)
            {
                HandleUpdate(i, chunkPosition);
            }
        }

        public static void CheckToRender(Coordinate chunkPosition)
        {
            chunkPosition.Y = 0;
            for (int i = 0; i < World.RenderDistance; i++)
            {
                HandleRender(i, chunkPosition);
            }
        }

        private static readonly (int, int)[] XZChangeSets = new (int, int)[]
        {
            (1,0), (0, -1), (-1, 0), (0, 1)
        };

        private static void HandleRender(int ring, Coordinate centerChunk)
        {
            if (ring == 0)
            {
                for (int i = 0; i < Region.REGION_SIZE; i++)
                {
                    centerChunk.Y = i;
                    HandleAttemptRender(centerChunk);
                }
                return;
            }

            Coordinate currentOffset = new Coordinate(centerChunk.X - ring, centerChunk.Y, centerChunk.Z + ring);
            int currentSet = 0;
            int walkedSinceLastChange = 0;
            for (int walkedTiles = 0; walkedTiles < ring * 8; walkedTiles++)
            {
                for (int i = 0; i < Region.REGION_SIZE; i++)
                {
                    currentOffset.Y = i;
                    HandleAttemptRender(currentOffset);
                }

                currentOffset.X += XZChangeSets[currentSet].Item1;
                currentOffset.Z += XZChangeSets[currentSet].Item2;

                walkedSinceLastChange++;

                if (walkedSinceLastChange == ring * 2)
                {
                    currentSet++;
                    walkedSinceLastChange = 0;
                }
            }
        }

        private static void HandleLoadAndUpdate(int ring, Coordinate centerChunk)
        {
            if (ring == 0)
            {
                for (int i = 0; i < Region.REGION_SIZE; i++)
                {
                    centerChunk.Y = i;
                    HandleAttemptLoadUpdate(centerChunk);
                }
                return;
            }

            Coordinate currentOffset = new Coordinate(centerChunk.X - ring, centerChunk.Y, centerChunk.Z + ring);
            int currentSet = 0;
            int walkedSinceLastChange = 0;
            for (int walkedTiles = 0; walkedTiles < ring * 8; walkedTiles++)
            {
                for (int i = 0; i < Region.REGION_SIZE; i++)
                {
                    currentOffset.Y = i;
                    HandleAttemptLoadUpdate(currentOffset);
                }

                currentOffset.X += XZChangeSets[currentSet].Item1;
                currentOffset.Z += XZChangeSets[currentSet].Item2;

                walkedSinceLastChange++;

                if (walkedSinceLastChange == ring * 2)
                {
                    currentSet++;
                    walkedSinceLastChange = 0;
                }
            }
        }

        private static void HandleUpdate(int ring, Coordinate centerChunk)
        {
            if (ring == 0)
            {
                for (int i = 0; i < Region.REGION_SIZE; i++)
                {
                    centerChunk.Y = i;
                    HandleAttemptUpdate(centerChunk);
                }
                return;
            }

            Coordinate currentOffset = new Coordinate(centerChunk.X - ring, centerChunk.Y, centerChunk.Z + ring);
            int currentSet = 0;
            int walkedSinceLastChange = 0;
            for (int walkedTiles = 0; walkedTiles < ring * 8; walkedTiles++)
            {
                for (int i = 0; i < Region.REGION_SIZE; i++)
                {
                    currentOffset.Y = i;
                    HandleAttemptUpdate(currentOffset);
                }

                currentOffset.X += XZChangeSets[currentSet].Item1;
                currentOffset.Z += XZChangeSets[currentSet].Item2;

                walkedSinceLastChange++;

                if (walkedSinceLastChange == ring * 2)
                {
                    currentSet++;
                    walkedSinceLastChange = 0;
                }
            }
        }

        private static void HandleAttemptLoadUpdate(Coordinate position)
        {
            if (World.LoadedChunks.TryGetValue(position, out ChunkData chunk) == false)
            {
                World.CreateChunk(position);
            }
            else
            {
                World.UpdateChunk(chunk);
            }
        }

        private static void HandleAttemptUpdate(Coordinate position)
        {
            if (World.LoadedChunks.TryGetValue(position, out ChunkData chunk))
            {
                World.UpdateChunk(chunk);
            }
        }

        private static void HandleAttemptRender(Coordinate position)
        {
            if (World.LoadedChunks.TryGetValue(position, out ChunkData chunk))
            {
                World.RenderChunk(chunk);
            }
        }
    }
}
