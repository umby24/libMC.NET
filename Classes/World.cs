using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Classes {
    public class World {
        public long currentTime;
        public long worldAge;
        public sbyte dimension;
        public byte difficulty;
        public byte maxPlayers;
        public string levelType;

        //---------------------
        public int Spawn_X;
        public int Spawn_Y;
        public int Spawn_Z;
        //---------------------

        public List<Entity> Entities;
        public List<Classes.Object> worldObjects;
        public List<Classes.Chunk> worldChunks;
        //--------------------

        public World() {
            worldObjects = new List<Object>();
            worldChunks = new List<Chunk>();
            Entities = new List<Entity>();
        }


        /// <summary>
        /// Returns the index of which the entity resides in for the Entities list
        /// </summary>
        /// <param name="Entity_ID"></param>
        /// <returns></returns>
        public int getEntityById(int Entity_ID) {
            int thisEntity = -1;

            try {
                foreach (Entity b in Entities) {
                    if (b.Entity_ID == Entity_ID) {
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
        public int getChunk(int x, int z) {
            int chunkIndex = -1;

            try {
                foreach (Classes.Chunk c in worldChunks) {
                    if (c.x == x && c.z == z) {
                        chunkIndex = worldChunks.IndexOf(c);
                    }
                }
            } catch {
                return chunkIndex;
            }

            return chunkIndex;
        }

    }
}
