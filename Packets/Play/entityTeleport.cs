using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class entityTeleport : Packet {
        public entityTeleport(ref Minecraft mc) {
            int Entity_ID = mc.nh.wSock.readInt();
            int X = mc.nh.wSock.readInt();
            int Y = mc.nh.wSock.readInt();
            int Z = mc.nh.wSock.readInt();
            byte yaw = mc.nh.wSock.readByte();
            byte pitch = mc.nh.wSock.readByte();

            if (mc.minecraftWorld != null) {
                int eIndex = mc.minecraftWorld.getEntityById(Entity_ID);

                if (eIndex != -1) {
                    mc.minecraftWorld.Entities[eIndex].Location.x = X;
                    mc.minecraftWorld.Entities[eIndex].Location.y = Y;
                    mc.minecraftWorld.Entities[eIndex].Location.z = Z;
                    mc.minecraftWorld.Entities[eIndex].yaw = yaw;
                    mc.minecraftWorld.Entities[eIndex].pitch = pitch;
                }
            }

            mc.raiseEntityTeleport(Entity_ID, X, Y, Z);
            mc.raiseEntityLookChanged(Entity_ID, yaw, pitch);
        }
    }
}
