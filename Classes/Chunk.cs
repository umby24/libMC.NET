using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Classes {
    public class Chunk {
        public int x, z, numBlocks, aBlocks;
        public short pbitmap, abitmap;
        public byte[] blocks;
        public byte[] Metadata;
        public bool lighting, groundup = false;
        public List<Section> sections;

        public Chunk(int X, int Z, short PBitmap, short ABitmap, bool Lighting, bool Groundup) {
            x = X;
            z = Z;
            pbitmap = PBitmap;
            abitmap = ABitmap;
            lighting = Lighting;
            groundup = Groundup;

            sections = new List<Section>();

            numBlocks = 0;
            aBlocks = 0;

            createSections();
        }

        void createSections() {
            for (int i = 0; i < 16; i++) {
                if ((pbitmap & (1 << i)) != 0) {
                    numBlocks++;
                    sections.Add(new Section((byte)i));
                }
            }

            for (int i = 0; i < 16; i++) {
                if ((abitmap & (1 << i)) != 0) {
                    aBlocks++;
                }
            }

            // -- Number of sections * blocks per section = blocks in this "Chunk"
            numBlocks = numBlocks * 4096;
        }

        void populate() {
            int offset = 0, current = 0, metaOff = 0;

            for (int i = 0; i < 16; i++) {
                if ((pbitmap & (1 << i)) != 0) {

                    byte[] temp = new byte[4096];
                    byte[] temp2 = new byte[2048];

                    Array.Copy(blocks, offset, temp, 0, 4096); // -- Block IDs
                    Array.Copy(Metadata, metaOff, temp2, 0, 2048); // -- Metadata and lighting.

                    Section mySection = sections[current];

                    mySection.blocks = temp;
                    mySection.metadata = createMetadataBytes(temp2);

                    offset += 4096;
                    metaOff += 2048;

                    current += 1;
                }
            }

            // -- Free the memory, everything is now stored in sections.
            blocks = null;
            Metadata = null;
        }

        /// <summary>
        /// Expand the compressed metadata (half-byte per block) into single-byte per block for easier reading.
        /// </summary>
        /// <param name="oldMeta">Old (2048-byte) Metadata</param>
        /// <returns>4096 uncompressed metadata</returns>
        public byte[] createMetadataBytes(byte[] oldMeta) {
            byte[] newMeta = new byte[4096];

            for (int i = 0; i < oldMeta.Length; i++) {
                byte block2 = (byte)((oldMeta[i] >> 4) & 15);
                byte block1 = (byte)(oldMeta[i] & 15);

                newMeta[(i * 2)] = block1;
                newMeta[(i * 2) + 1] = block2;
            }

            return newMeta;
        }

        public byte[] getData(byte[] deCompressed) {
            // -- Loading chunks, network handler hands off the decompressed bytes
            // -- This function takes its portion, and returns what's left.

            blocks = new byte[numBlocks];
            Metadata = new byte[numBlocks / 2]; // -- Contains block light and block metadata.

            byte[] temp;
            int removeable = numBlocks;

            if (lighting == true)
                removeable += (numBlocks / 2);

            if (groundup)
                removeable += 256;

            Array.Copy(deCompressed, 0, blocks, 0, numBlocks);
            Array.Copy(deCompressed, numBlocks, Metadata, 0, numBlocks / 2); // -- Copy in Metadata

            temp = new byte[deCompressed.Length - (numBlocks + removeable)];

            Array.Copy(deCompressed, (numBlocks + removeable), temp, 0, temp.Length);

            populate(); // -- Populate all of our sections with the bytes we just aquired.

            return temp;
        }

        public void updateBlock(int Bx, int By, int Bz, int id) {
            // -- Updates the block in this chunk.

            decimal ChunkX = decimal.Divide(Bx, 16);
            decimal ChunkZ = decimal.Divide(By, 16);

            ChunkX = Math.Floor(ChunkX);
            ChunkZ = Math.Floor(ChunkZ);

            if (ChunkX != x || ChunkZ != z)
                return; // -- Block is not in this chunk, user-error somewhere.

            Section thisSection = GetSectionByNumber(By);
            thisSection.setBlock(getXinSection(Bx), GetPositionInSection(By), getZinSection(Bz), id);
            
        }

        public int getBlockId(int Bx, int By, int Bz) {
            Section thisSection = GetSectionByNumber(By);
            return thisSection.getBlock(getXinSection(Bx), GetPositionInSection(By), getZinSection(Bz)).ID;
        }

        public Block getBlock(int Bx, int By, int Bz) {
            Section thisSection = GetSectionByNumber(By);
            return thisSection.getBlock(getXinSection(Bx), GetPositionInSection(By), getZinSection(Bz));
        }

        public int getBlockMetadata(int Bx, int By, int Bz) {
            Section thisSection = GetSectionByNumber(By);
            return thisSection.getBlockMetadata(getXinSection(Bx), GetPositionInSection(By), getZinSection(Bz));
        }

        public void setBlockData(int Bx, int By, int Bz, byte data) {
            // -- Update the skylight and metadata on this block.
            Section thisSection = GetSectionByNumber(By);
            thisSection.setBlockData(getXinSection(Bx), GetPositionInSection(By), getZinSection(Bz), data);
        }
        #region Helping Methods
        private Section GetSectionByNumber(int blockY) {
            Section thisSection = null;

            foreach (Section y in sections) {
                if (y.y == blockY / 16) {
                    thisSection = y;
                    break;
                }
            }

            if (thisSection == null) { // Add a new section, if it doesn't exist yet.
                thisSection = new Section((byte)(blockY / 16));
                sections.Add(thisSection);
            }

            return thisSection;
        }
        private int getXinSection(int BlockX) {
            return Math.Abs(BlockX - (x * 16));
        }
        private int GetPositionInSection(int blockY) {
            return blockY % 16; // Credits: SirCmpwn Craft.net
        }
        private int getZinSection(int BlockZ) {
            if (z == 0)
                return BlockZ;

            return BlockZ % z;
        }
        #endregion
    }
}
