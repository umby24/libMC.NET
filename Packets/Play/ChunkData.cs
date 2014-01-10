using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Common;
using libMC.NET.World;

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

                if (mc.MinecraftWorld != null)
                    cIndex = mc.MinecraftWorld.GetChunk(X, Z);

                if (cIndex != -1)
                    mc.MinecraftWorld.worldChunks.RemoveAt(cIndex);

                mc.RaiseChunkUnload(X, Z);
                return;
            }

            // -- Remove GZip Header
            Array.Copy(compressedData, 2, trim, 0, trim.Length);

            // -- Decompress the data
            decompressedData = Decompressor.Decompress(trim);

            // -- Create new chunk
            Chunk newChunk = new Chunk(X, Z, pbitmap, abitmap, true, groundUp); // -- Skylight assumed true
            newChunk.GetData(decompressedData);

            if (mc.MinecraftWorld == null)
                mc.MinecraftWorld = new WorldClass();

            // -- Add the chunk to the world
            mc.MinecraftWorld.worldChunks.Add(newChunk);

            mc.RaiseChunkLoad(X, Z);
        }
    }
}
