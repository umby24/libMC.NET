using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Classes;

namespace libMC.NET.Packets.Play {
    class blockChange : Packet {
        public int x, z, type;
        public byte y, data;

        public blockChange(ref Minecraft mc) {
            x = mc.nh.wSock.readInt();
            y = mc.nh.wSock.readByte();
            z = mc.nh.wSock.readInt();
            type = mc.nh.wSock.readVarInt();
            data = mc.nh.wSock.readByte();

            decimal ChunkX = decimal.Divide(x, 16);
            decimal ChunkZ = decimal.Divide(z, 16);

            ChunkX = Math.Floor(ChunkX);
            ChunkZ = Math.Floor(ChunkZ);

            Chunk thisChunk = mc.minecraftWorld.worldChunks[mc.minecraftWorld.getChunk(int.Parse(ChunkX.ToString()), int.Parse(ChunkZ.ToString()))];
            thisChunk.updateBlock(x, y, z, type);
            thisChunk.setBlockData(x, y, z, data);

            mc.raiseBlockChangedEvent(x, y, z, type, data);
        }
    }
}
