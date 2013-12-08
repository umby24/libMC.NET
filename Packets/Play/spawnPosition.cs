using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class spawnPosition : Packet {
        // -- Not providing public accessors to this due to it being stored already in the World class for this Minecraft instance.
        public spawnPosition(ref Minecraft mc) {
            int x = mc.nh.wSock.readInt();
            int y = mc.nh.wSock.readInt();
            int z = mc.nh.wSock.readInt();

            if (mc.minecraftWorld == null)
                mc.minecraftWorld = new Classes.World();

            mc.minecraftWorld.Spawn_X = x;
            mc.minecraftWorld.Spawn_Y = y;
            mc.minecraftWorld.Spawn_Z = z;

            mc.RaiseDebug(this, String.Format("World spawn Set/Changed\nX: {0}\nY: {1}\nZ: {2}", x, y, z));
        }
    }
}
