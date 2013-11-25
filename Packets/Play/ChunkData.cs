using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Classes;

namespace libMC.NET.Packets.Play {
    class ChunkData : Packet {
        public ChunkData(ref Minecraft mc) {
            // -- Yay, a fun one.
            int X = mc.nh.wSock.readInt();
            int Z = mc.nh.wSock.readInt();

            bool groundUp = mc.nh.wSock.readBool();
            short pbitmap = mc.nh.wSock.readShort();
            short abitmap = mc.nh.wSock.readShort();
            int size = mc.nh.wSock.readInt();

            byte[] compressedData = mc.nh.wSock.readByteArray(size);
            byte[] trim = new byte[size - 2];
            byte[] decompressedData;

            if (pbitmap == 0) {
                // -- Unload chunk.
                int cIndex = -1;

                if (mc.minecraftWorld != null)
                    cIndex = mc.minecraftWorld.getChunk(X, Z);

                if (cIndex != -1)
                    mc.minecraftWorld.worldChunks.RemoveAt(cIndex);

                mc.raiseChunkUnload(X, Z);
                return;
            }

            // -- Remove GZip Header
            Array.Copy(compressedData, 2, trim, 0, trim.Length);

            // -- Decompress the data
            decompressedData = Decompressor.decompress(trim);

            // -- Create new chunk
            Chunk newChunk = new Chunk(X, Z, pbitmap, abitmap, true, groundUp); // -- Skylight assumed true
            newChunk.getData(decompressedData);

            if (mc.minecraftWorld == null)
                mc.minecraftWorld = new World();

            // -- Add the chunk to the world
            mc.minecraftWorld.worldChunks.Add(newChunk);

            mc.raiseChunkLoad(X, Z);
        }
    }
}
