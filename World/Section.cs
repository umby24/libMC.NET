using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Entities;

namespace libMC.NET.World {
    public class Section {
        public byte[] blocks;
        public byte[] metadata;
        public byte y;

        public Section(byte Y) {
            y = Y;
            blocks = new byte[4096];
            metadata = new byte[4096];
        }

        public void setBlock(int x, int y, int z, int id) {
            int index = x + (z * 16) + (y * 16 * 16);
            blocks[index] = (byte)id;
        }

        public Block getBlock(int x, int y, int z) {
            int index = x + (z * 16) + (y * 16 * 16);
            Block thisBlock = new Block((int)blocks[index], x, y, z, (int)Math.Floor(decimal.Divide(x, 16)), (int)Math.Floor(decimal.Divide(z, 16)));

            return thisBlock;
        }

        public int getBlockMetadata(int x, int y, int z) {
            int index = (x + (z * 16) + (y * 16 * 16));
            byte value = metadata[index];

            return value;
        }

        public void setBlockData(int x, int y, int z, byte data) {
            int index = (x + (z * 16) + (y * 16 * 16));
            metadata[index] = data;
        }
    }
}
