using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class entityLook : Packet {
        public entityLook(ref Minecraft mc) {
            int Entity_ID = mc.nh.wSock.readInt();
            byte yaw = mc.nh.wSock.readByte();
            byte pitch = mc.nh.wSock.readByte();

            if (mc.minecraftWorld != null) {
                int eIndex = mc.minecraftWorld.getEntityById(Entity_ID);

                if (eIndex != -1) {
                    mc.minecraftWorld.Entities[eIndex].pitch = pitch;
                    mc.minecraftWorld.Entities[eIndex].yaw = yaw;
                }

            }

            mc.raiseEntityLookChanged(Entity_ID, yaw, pitch);
        }
    }
}
