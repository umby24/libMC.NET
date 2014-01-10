using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.World;

namespace libMC.NET.Packets.Play {
    class BlockChange : Packet {
        public int x, z, type;
        public byte y, data;

        public BlockChange(ref Minecraft mc) {
            x = mc.nh.wSock.readInt();
            y = mc.nh.wSock.readByte();
            z = mc.nh.wSock.readInt();
            type = mc.nh.wSock.readVarInt();
            data = mc.nh.wSock.readByte();

            decimal ChunkX = decimal.Divide(x, 16);
            decimal ChunkZ = decimal.Divide(z, 16);

            ChunkX = Math.Floor(ChunkX);
            ChunkZ = Math.Floor(ChunkZ);

            Chunk thisChunk = mc.MinecraftWorld.worldChunks[mc.MinecraftWorld.GetChunk(int.Parse(ChunkX.ToString()), int.Parse(ChunkZ.ToString()))];
            thisChunk.UpdateBlock(x, y, z, type);
            thisChunk.SetBlockData(x, y, z, data);

            mc.RaiseBlockChangedEvent(x, y, z, type, data);
        }
    }
}
