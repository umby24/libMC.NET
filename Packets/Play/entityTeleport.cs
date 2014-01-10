using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class EntityTeleport : Packet {
        public EntityTeleport(ref Minecraft mc) {
            int Entity_ID = mc.nh.wSock.readInt();
            int X = mc.nh.wSock.readInt();
            int Y = mc.nh.wSock.readInt();
            int Z = mc.nh.wSock.readInt();
            byte yaw = mc.nh.wSock.readByte();
            byte pitch = mc.nh.wSock.readByte();

            if (mc.MinecraftWorld != null) {
                int eIndex = mc.MinecraftWorld.GetEntityById(Entity_ID);

                if (eIndex != -1) {
                    mc.MinecraftWorld.Entities[eIndex].Location.x = X;
                    mc.MinecraftWorld.Entities[eIndex].Location.y = Y;
                    mc.MinecraftWorld.Entities[eIndex].Location.z = Z;
                    mc.MinecraftWorld.Entities[eIndex].yaw = yaw;
                    mc.MinecraftWorld.Entities[eIndex].pitch = pitch;
                }
            }

            mc.RaiseEntityTeleport(Entity_ID, X, Y, Z);
            mc.RaiseEntityLookChanged(Entity_ID, yaw, pitch);
        }
    }
}
