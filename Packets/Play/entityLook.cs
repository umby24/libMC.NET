using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class EntityLook : Packet {
        public EntityLook(ref Minecraft mc) {
            int Entity_ID = mc.nh.wSock.readInt();
            byte yaw = mc.nh.wSock.readByte();
            byte pitch = mc.nh.wSock.readByte();

            if (mc.MinecraftWorld != null) {
                int eIndex = mc.MinecraftWorld.GetEntityById(Entity_ID);

                if (eIndex != -1) {
                    mc.MinecraftWorld.Entities[eIndex].pitch = pitch;
                    mc.MinecraftWorld.Entities[eIndex].yaw = yaw;
                }

            }

            mc.RaiseEntityLookChanged(Entity_ID, yaw, pitch);
        }
    }
}
