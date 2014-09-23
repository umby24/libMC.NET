using System.Collections.Generic;
using libMC.NET.Entities;

namespace libMC.NET.World {
    public class WorldClass {
        public long CurrentTime;
        public long WorldAge;
        public sbyte Dimension;
        public byte Difficulty;
        public byte MaxPlayers;
        public string LevelType;

        //---------------------
        public int SpawnX;
        public int SpawnY;
        public int SpawnZ;
        //---------------------

        public List<Entity> Entities;
        public List<ObjectEntity> WorldObjects;
        public List<Chunk> WorldChunks;
        //--------------------

        public WorldClass() {
            WorldObjects = new List<ObjectEntity>();
            WorldChunks = new List<Chunk>();
            Entities = new List<Entity>();
        }


        /// <summary>
        /// Returns the index of which the entity resides in for the Entities list
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public int GetEntityById(int entityId) {
            var thisEntity = -1;

            try {
                foreach (var b in Entities) {
                    if (b.Entity_ID == entityId) {
                        thisEntity = Entities.IndexOf(b);
                        break;
                    }
                }
            } catch {
                return thisEntity;
            }

            return thisEntity;
        }

        /// <summary>
        /// Returns the index where the chunk resides in worldChunks.
        /// </summary>
        /// <param name="x">X location of chunk</param>
        /// <param name="z">Z location of chunk</param>
        /// <returns>Index of Chunk in worldChunks</returns>
        public int GetChunk(int x, int z) {
            var chunkIndex = -1;

            try {
                foreach (var c in WorldChunks) {
                    if (c.X == x && c.Z == z) {
                        chunkIndex = WorldChunks.IndexOf(c);
                    }
                }
            } catch {
                return chunkIndex;
            }

            return chunkIndex;
        }

    }
}
