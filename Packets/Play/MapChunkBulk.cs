using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Common;
using libMC.NET.World;

namespace libMC.NET.Packets.Play {
    class MapChunkBulk : Packet {
        public MapChunkBulk(ref Minecraft mc) {
            short chunkColumnCount = mc.nh.wSock.readShort();
            int dataLength = mc.nh.wSock.readInt();
            bool skylight = mc.nh.wSock.readBool();

            byte[] CompressedData = mc.nh.wSock.readByteArray(dataLength);
            byte[] trim = new byte[dataLength - 2];
            byte[] DecompressedData;

            Chunk[] chunks = new Chunk[chunkColumnCount];

            Array.Copy(CompressedData, 2, trim, 0, trim.Length);

            DecompressedData = Decompressor.Decompress(trim);

            for (int i = 0; chunkColumnCount > i; i++) {
                int x = mc.nh.wSock.readInt();
                int z = mc.nh.wSock.readInt();
                short pbitmap = mc.nh.wSock.readShort();
                short abitmap = mc.nh.wSock.readShort();

                chunks[i] = new Chunk(x, z, pbitmap, abitmap, skylight, true); // -- Assume true for Ground Up Continuous

                DecompressedData = chunks[i].GetData(DecompressedData); // -- Calls the chunk class to take all of the bytes it needs, and return whats left.

                if (mc.MinecraftWorld == null)
                    mc.MinecraftWorld = new WorldClass();

                mc.MinecraftWorld.worldChunks.Add(chunks[i]);
            }

        }
    }
}
