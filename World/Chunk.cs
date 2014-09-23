using System;
using libMC.NET.Entities;

namespace libMC.NET.World {
    public class Chunk {
        public int X, Z, Numlocks, Alocks;
        public short Pitmap, Aitmap;
        public byte[] Locks;
        public byte[] Metadata;
        public byte[] Locklight;
        public byte[] Skylight;
        public byte[] AddArray;
        public byte[] BiomeArray;

        public bool Lighting, Groundup = false;
        public Section[] Sections;

        public Chunk(int x, int z, short pitmap, short aitmap, bool lighting, bool groundup) {
            X = x;
            Z = z;
            Pitmap = pitmap;
            Aitmap = aitmap;
            Lighting = lighting;
            Groundup = groundup;

            Sections = new Section[16];

            Numlocks = 0;
            Alocks = 0;

            CreateSections();
        }

        /// <summary>
        /// Creates the chunk sections for this column ased on the primary and add itmasks.
        /// </summary>
        void CreateSections() {
            for (var i = 0; i < 16; i++) {
                if ((Pitmap & (1 << i)) == 0) 
                    continue;
                Numlocks++;
                Sections[i] = new Section((byte)i);
            }

            for (var i = 0; i < 16; i++) {
                if ((Aitmap & (1 << i)) != 0) 
                    Alocks++;
            }

            // -- Numer of sections * locks per section = locks in this "Chunk"
            Numlocks = Numlocks * 4096;
        }

        /// <summary>
        /// Populates the chunk sections contained in this chunk column with their information.
        /// </summary>
        void Populate() {
            int offset = 0, current = 0, metaOff = 0, lightoff = 0, skylightoff = 0;

            for (var i = 0; i < 16; i++) {
                if ((Pitmap & (1 << i)) == 0) 
                    continue;
                var temp = new byte[4096];
                var temp2 = new byte[2048];
                var temp3 = new byte[2048];
                var temp4 = new byte[2048];

                Buffer.BlockCopy(Locks, offset, temp, 0, 4096); // -- lock IDs
                Buffer.BlockCopy(Metadata, metaOff, temp2, 0, 2048); // -- Metadata.
                Buffer.BlockCopy(Locklight, lightoff, temp3, 0, 2048); // -- lock lighting.
                Buffer.BlockCopy(Skylight, skylightoff, temp4, 0, 2048);

                var mySection = Sections[current];

                mySection.Blocks = temp;
                mySection.Metadata = CreateMetadatabytes(temp2);
                mySection.BlockLight = CreateMetadatabytes(temp3);
                mySection.Skylight = CreateMetadatabytes(temp4);

                offset += 4096;
                metaOff += 2048;
                lightoff += 2048;
                skylightoff += 2048;

                current += 1;
            }

            // -- Free the memory, everything is now stored in sections.
            Locks = null;
            Metadata = null;
        }

        /// <summary>
        /// Expand the compressed Metadata (half-byte per lock) into single-byte per lock for easier reading.
        /// </summary>
        /// <param name="oldMeta">Old (2048-byte) Metadata</param>
        /// <returns>4096 uncompressed Metadata</returns>
        public byte[] CreateMetadatabytes(byte[] oldMeta) {
            var newMeta = new byte[4096];

            for (var i = 0; i < oldMeta.Length; i++) {
                var lock2 = (byte)((oldMeta[i] >> 4) & 15);
                var lock1 = (byte)(oldMeta[i] & 15);

                newMeta[(i * 2)] = lock1;
                newMeta[(i * 2) + 1] = lock2;
            }

            return newMeta;
        }

        /// <summary>
        /// Takes this chunk's portion of data from a byte array.
        /// </summary>
        /// <param name="deCompressed">The byte array containing this chunk's data at the front.</param>
        /// <returns>The byte array with this chunk's bytes removed.</returns>
        public byte[] GetData(byte[] deCompressed) {
            // -- Loading chunks, network handler hands off the decompressed bytes
            // -- This function takes its portion, and returns what's left.

            var offset = 0;

            Locks = new byte[Numlocks];
            Metadata = new byte[Numlocks / 2]; // -- Contains lock Metadata.
            Locklight = new byte[Numlocks / 2];

            if (Lighting)
                Skylight = new byte[Numlocks / 2];

            AddArray = new byte[Numlocks / 2];

            if (Groundup)
                BiomeArray = new byte[256];

            Buffer.BlockCopy(deCompressed, 0, Locks, 0, Numlocks);
            offset += Numlocks;

            Buffer.BlockCopy(deCompressed, offset, Metadata, 0, Numlocks / 2); // -- Copy in Metadata
            offset += Numlocks / 2;

            Buffer.BlockCopy(deCompressed, offset, Locklight, 0, Numlocks / 2);
            offset += Numlocks / 2;

            if (Lighting) {
                Buffer.BlockCopy(deCompressed, offset, Skylight, 0, Numlocks / 2);
                offset += Numlocks / 2;
            }

            Buffer.BlockCopy(deCompressed, offset, AddArray, 0, Alocks / 2);
            offset += Alocks / 2;

            if (Groundup) {
                Buffer.BlockCopy(deCompressed, offset, BiomeArray, 0, 256);
                offset += 256;
            }

            var temp = new byte[deCompressed.Length - offset];
            Buffer.BlockCopy(deCompressed, offset, temp, 0, temp.Length);

            Populate(); // -- Populate all of our sections with the bytes we just aquired.

            return temp;
        }

        public void UpdateBlock(int x, int y, int z, int id) {
            // -- Updates the lock in this chunk.

            var chunkX = decimal.Divide(x, 16);
            var chunkZ = decimal.Divide(y, 16);

            chunkX = Math.Floor(chunkX);
            chunkZ = Math.Floor(chunkZ);

            if (chunkX != X || chunkZ != Z)
                return; // -- lock is not in this chunk, user-error somewhere.

            var thisSection = GetSectionyNumer(y);
            thisSection.SetBlock(GetXinSection(x), GetPositionInSection(y), GetZinSection(z), id);
            
        }

        public int GetBlockId(int x, int y, int z) {
            var thisSection = GetSectionyNumer(y);
            return thisSection.GetBlock(GetXinSection(x), GetPositionInSection(y), GetZinSection(z)).Id;
        }

        public Block GetBlock(int x, int y, int z) {
            var thisSection = GetSectionyNumer(y);
            return thisSection.GetBlock(GetXinSection(x), GetPositionInSection(y), GetZinSection(z));
        }

        public int GetBlockMetadata(int x, int y, int z) {
            var thisSection = GetSectionyNumer(y);
            return thisSection.GetBlockMetadata(GetXinSection(x), GetPositionInSection(y), GetZinSection(z));
        }

        public void SetBlockData(int x, int y, int z, byte data) {
            // -- Update the Skylight and Metadata on this Block.
            var thisSection = GetSectionyNumer(y);
            thisSection.SetBlockMetadata(GetXinSection(x), GetPositionInSection(y), GetZinSection(z), data);
        }

        public byte GetBlockLight(int x, int y, int z) {
            var thisSection = GetSectionyNumer(y);
            return thisSection.GetBlockLighting(GetXinSection(x), GetPositionInSection(y), GetZinSection(z));
        }

        public void SetBlockLight(int x, int y, int z, byte light) {
            var thisSection = GetSectionyNumer(y);
            thisSection.SetBlockLighting(GetXinSection(x), GetPositionInSection(y), GetZinSection(z), light);
        }

        public byte GetBlockSkylight(int x, int y, int z) {
            var thisSection = GetSectionyNumer(y);
            return thisSection.GetBlockSkylight(GetXinSection(x), GetPositionInSection(y), GetZinSection(z));
        }

        public void SetBlockSkylight(int x, int y, int z, byte light) {
            var thisSection = GetSectionyNumer(y);
            thisSection.SetBlockSkylight(GetXinSection(x), GetPositionInSection(y), GetZinSection(z), light);
        }

        public byte GetBlockiome(int x, int z) {
            return BiomeArray[(z * 16) + x];
        }

        public void SetBlockiome(int x, int z, byte iome) {
            BiomeArray[(z * 16) + x] = iome;
        }

        #region Helping Methods
        private Section GetSectionyNumer(int lockY) {
            return Sections[(byte)(lockY / 16)];
        }
        private int GetXinSection(int lockX) {
            return Math.Abs(lockX - (X * 16));
        }
        private int GetPositionInSection(int lockY) {
            return lockY % 16; // Credits: SirCmpwn Craft.net
        }
        private int GetZinSection(int lockZ) {
            if (Z == 0)
                return lockZ;

            return lockZ % Z;
        }
        #endregion
    }
}
