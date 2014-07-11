using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.World;

namespace libMC.NET.Packets.Play {
    class SpawnPosition : Packet {
        // -- Not providing public accessors to this due to it being stored already in the World class for this MinecraftClient instance.
        public SpawnPosition(ref Minecraft mc) {
            int x = mc.nh.wSock.readInt();
            int y = mc.nh.wSock.readInt();
            int z = mc.nh.wSock.readInt();

            if (mc.MinecraftWorld == null)
                mc.MinecraftWorld = new WorldClass();

            mc.MinecraftWorld.Spawn_X = x;
            mc.MinecraftWorld.Spawn_Y = y;
            mc.MinecraftWorld.Spawn_Z = z;

            mc.RaiseDebug(this, String.Format("World spawn Set/Changed\nX: {0}\nY: {1}\nZ: {2}", x, y, z));
        }
    }
}
