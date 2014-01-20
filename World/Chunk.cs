using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Entities;

namespace libMC.NET.World {
    public class Chunk {
        public int x, z, numBlocks, aBlocks;
        public short pbitmap, abitmap;
        public byte[] blocks;
        public byte[] Metadata;
        public byte[] Blocklight;
        public byte[] Skylight;
        public byte[] AddArray;
        public byte[] BiomeArray;

        public bool lighting, groundup = false;
        public Section[] sections;

        public Chunk(int X, int Z, short PBitmap, short ABitmap, bool Lighting, bool Groundup) {
            x = X;
            z = Z;
            pbitmap = PBitmap;
            abitmap = ABitmap;
            lighting = Lighting;
            groundup = Groundup;

            sections = new Section[16];

            numBlocks = 0;
            aBlocks = 0;

            CreateSections();
        }

        /// <summary>
        /// Creates the chunk sections for this column based on the primary and add bitmasks.
        /// </summary>
        void CreateSections() {
            for (int i = 0; i < 16; i++) {
                if ((pbitmap & (1 << i)) != 0) {
                    numBlocks++;
                    sections[i] = new Section((byte)i);
                }
            }

            for (int i = 0; i < 16; i++) {
                if ((abitmap & (1 << i)) != 0) {
                    aBlocks++;
                }
            }

            // -- Number of sections * Blocks per section = Blocks in this "Chunk"
            numBlocks = numBlocks * 4096;
        }

        /// <summary>
        /// Populates the chunk sections contained in this chunk column with their information.
        /// </summary>
        void Populate() {
            int offset = 0, current = 0, metaOff = 0, Lightoff = 0, Skylightoff = 0;

            for (int i = 0; i < 16; i++) {
                if ((pbitmap & (1 << i)) != 0) {

                    byte[] temp = new byte[4096];
                    byte[] temp2 = new byte[2048];
                    byte[] temp3 = new byte[2048];
                    byte[] temp4 = new byte[2048];

                    Buffer.BlockCopy(blocks, offset, temp, 0, 4096); // -- Block IDs
                    Buffer.BlockCopy(Metadata, metaOff, temp2, 0, 2048); // -- Metadata.
                    Buffer.BlockCopy(Blocklight, Lightoff, temp3, 0, 2048); // -- Block lighting.
                    Buffer.BlockCopy(Skylight, Skylightoff, temp4, 0, 2048);

                    Section mySection = sections[current];

                    mySection.Blocks = temp;
                    mySection.Metadata = CreateMetadataBytes(temp2);
                    mySection.BlockLight = CreateMetadataBytes(temp3);
                    mySection.Skylight = CreateMetadataBytes(temp4);

                    offset += 4096;
                    metaOff += 2048;
                    Lightoff += 2048;
                    Skylightoff += 2048;

                    current += 1;
                }
            }

            // -- Free the memory, everything is now stored in sections.
            blocks = null;
            Metadata = null;
        }

        /// <summary>
        /// Expand the compressed Metadata (half-byte per block) into single-byte per block for easier reading.
        /// </summary>
        /// <param name="oldMeta">Old (2048-byte) Metadata</param>
        /// <returns>4096 uncompressed Metadata</returns>
        public byte[] CreateMetadataBytes(byte[] oldMeta) {
            byte[] newMeta = new byte[4096];

            for (int i = 0; i < oldMeta.Length; i++) {
                byte block2 = (byte)((oldMeta[i] >> 4) & 15);
                byte block1 = (byte)(oldMeta[i] & 15);

                newMeta[(i * 2)] = block1;
                newMeta[(i * 2) + 1] = block2;
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

            byte[] temp;
            int offset = 0;

            blocks = new byte[numBlocks];
            Metadata = new byte[numBlocks / 2]; // -- Contains block Metadata.
            Blocklight = new byte[numBlocks / 2];

            if (lighting)
                Skylight = new byte[numBlocks / 2];

            AddArray = new byte[numBlocks / 2];

            if (groundup)
                BiomeArray = new byte[256];

            Buffer.BlockCopy(deCompressed, 0, blocks, 0, numBlocks);
            offset += numBlocks;

            Buffer.BlockCopy(deCompressed, offset, Metadata, 0, numBlocks / 2); // -- Copy in Metadata
            offset += numBlocks / 2;

            Buffer.BlockCopy(deCompressed, offset, Blocklight, 0, numBlocks / 2);
            offset += numBlocks / 2;

            if (lighting) {
                Buffer.BlockCopy(deCompressed, offset, Skylight, 0, numBlocks / 2);
                offset += numBlocks / 2;
            }

            Buffer.BlockCopy(deCompressed, offset, AddArray, 0, aBlocks / 2);
            offset += aBlocks / 2;

            if (groundup) {
                Buffer.BlockCopy(deCompressed, offset, BiomeArray, 0, 256);
                offset += 256;
            }

            temp = new byte[deCompressed.Length - offset];
            Buffer.BlockCopy(deCompressed, offset, temp, 0, temp.Length);

            Populate(); // -- Populate all of our sections with the bytes we just aquired.

            return temp;
        }

        public void UpdateBlock(int Bx, int By, int Bz, int id) {
            // -- Updates the block in this chunk.

            decimal ChunkX = decimal.Divide(Bx, 16);
            decimal ChunkZ = decimal.Divide(By, 16);

            ChunkX = Math.Floor(ChunkX);
            ChunkZ = Math.Floor(ChunkZ);

            if (ChunkX != x || ChunkZ != z)
                return; // -- Block is not in this chunk, user-error somewhere.

            Section thisSection = GetSectionByNumber(By);
            thisSection.SetBlock(GetXinSection(Bx), GetPositionInSection(By), GetZinSection(Bz), id);
            
        }

        public int GetBlockId(int Bx, int By, int Bz) {
            Section thisSection = GetSectionByNumber(By);
            return thisSection.GetBlock(GetXinSection(Bx), GetPositionInSection(By), GetZinSection(Bz)).ID;
        }

        public Block GetBlock(int Bx, int By, int Bz) {
            Section thisSection = GetSectionByNumber(By);
            return thisSection.GetBlock(GetXinSection(Bx), GetPositionInSection(By), GetZinSection(Bz));
        }

        public int GetBlockMetadata(int Bx, int By, int Bz) {
            Section thisSection = GetSectionByNumber(By);
            return thisSection.GetBlockMetadata(GetXinSection(Bx), GetPositionInSection(By), GetZinSection(Bz));
        }

        public void SetBlockData(int Bx, int By, int Bz, byte data) {
            // -- Update the Skylight and Metadata on this block.
            Section thisSection = GetSectionByNumber(By);
            thisSection.SetBlockMetadata(GetXinSection(Bx), GetPositionInSection(By), GetZinSection(Bz), data);
        }

        public byte GetBlockLight(int x, int y, int z) {
            var thisSection = GetSectionByNumber(y);
            return thisSection.GetBlockLighting(GetXinSection(x), GetPositionInSection(y), GetZinSection(z));
        }

        public void SetBlockLight(int x, int y, int z, byte light) {
            var thisSection = GetSectionByNumber(y);
            thisSection.SetBlockLighting(GetXinSection(x), GetPositionInSection(y), GetZinSection(z), light);
        }

        public byte GetBlockSkylight(int x, int y, int z) {
            var thisSection = GetSectionByNumber(y);
            return thisSection.GetBlockSkylight(GetXinSection(x), GetPositionInSection(y), GetZinSection(z));
        }

        public void SetBlockSkylight(int x, int y, int z, byte light) {
            var thisSection = GetSectionByNumber(y);
            thisSection.SetBlockSkylight(GetXinSection(x), GetPositionInSection(y), GetZinSection(z), light);
        }

        public byte GetBlockBiome(int x, int z) {
            return BiomeArray[(z * 16) + x];
        }

        public void SetBlockBiome(int x, int z, byte biome) {
            BiomeArray[(z * 16) + x] = biome;
        }

        #region Helping Methods
        private Section GetSectionByNumber(int blockY) {
            return sections[(byte)(blockY / 16)];
        }
        private int GetXinSection(int BlockX) {
            return Math.Abs(BlockX - (x * 16));
        }
        private int GetPositionInSection(int blockY) {
            return blockY % 16; // Credits: SirCmpwn Craft.net
        }
        private int GetZinSection(int BlockZ) {
            if (z == 0)
                return BlockZ;

            return BlockZ % z;
        }
        #endregion
    }
}
