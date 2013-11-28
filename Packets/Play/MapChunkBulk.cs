using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Classes;

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

            DecompressedData = Decompressor.decompress(trim);

            for (int i = 0; chunkColumnCount > i; i++) {
                int x = mc.nh.wSock.readInt();
                int z = mc.nh.wSock.readInt();
                short pbitmap = mc.nh.wSock.readShort();
                short abitmap = mc.nh.wSock.readShort();

                chunks[i] = new Chunk(x, z, pbitmap, abitmap, skylight, true); // -- Assume true for Ground Up Continuous

                DecompressedData = chunks[i].getData(DecompressedData); // -- Calls the chunk class to take all of the bytes it needs, and return whats left.

                if (mc.minecraftWorld == null)
                    mc.minecraftWorld = new World();

                mc.minecraftWorld.worldChunks.Add(chunks[i]);
            }

            ServerBound.Player p = new ServerBound.Player(ref mc);
        }
    }
}
