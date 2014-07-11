using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Common;
using libMC.NET.World;

namespace libMC.NET.Packets.Play {
    public class MultiBlockChange : Packet {
        public MultiBlockChange(ref Minecraft mc) {
            int X = mc.nh.wSock.readInt();
            int Z = mc.nh.wSock.readInt();
            short recordCount = mc.nh.wSock.readShort();
            int dataSize = mc.nh.wSock.readInt();

            byte[] data = mc.nh.wSock.readByteArray(dataSize);
            int chunkID = mc.MinecraftWorld.GetChunk(X, Z);

            if (chunkID == -1) {
                mc.RaiseError(this, "Attempted to access uninitialized chunk");
                return;
            }

            Chunk thisChunk = mc.MinecraftWorld.worldChunks[chunkID];

            for (int i = 0; i < recordCount - 1; i++) {
                byte[] blockData = new byte[4];
                Buffer.BlockCopy(data, (i * 4), blockData, 0, 4);

                int z = (blockData[0] & 0x0f);
                int x = (blockData[0] >> 4) & 0x0f;
                int y = (blockData[1]);
                int block_id = (blockData[2] << 4) | ((blockData[3] & 0xF0) >> 4);
                int metadata = blockData[3] & 0xF;

                x = (X * 16) + x;
                Z = (Z * 16) + z;

                thisChunk.UpdateBlock(x, y, z, block_id);
                thisChunk.SetBlockData(x, y, z, (byte)metadata);
            }

            mc.RaiseMultiBlockChange(X, Z);
        }
    }
}
