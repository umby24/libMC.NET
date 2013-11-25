using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class entityLookRelativeMove : Packet {
        public entityLookRelativeMove(ref Minecraft mc) {
            int Entity_ID = mc.nh.wSock.readInt();
            byte Diff_X = mc.nh.wSock.readByte();
            byte Diff_Y = mc.nh.wSock.readByte();
            byte Diff_Z = mc.nh.wSock.readByte();
            byte yaw = mc.nh.wSock.readByte();
            byte pitch = mc.nh.wSock.readByte();

            if (mc.minecraftWorld != null) {
                int eIndex = mc.minecraftWorld.getEntityById(Entity_ID);

                if (eIndex != -1) {
                    mc.minecraftWorld.Entities[eIndex].Location.x += (Diff_X * 32);
                    mc.minecraftWorld.Entities[eIndex].Location.y += (Diff_Y * 32);
                    mc.minecraftWorld.Entities[eIndex].Location.z += (Diff_Z * 32);
                    mc.minecraftWorld.Entities[eIndex].yaw = yaw;
                    mc.minecraftWorld.Entities[eIndex].pitch = pitch;
                }
            }

            mc.raiseEntityRelMove(Entity_ID, (Diff_X * 32), (Diff_Y * 32), (Diff_Z * 32));
            mc.raiseEntityLookChanged(Entity_ID, yaw, pitch);
        }
    }
}
