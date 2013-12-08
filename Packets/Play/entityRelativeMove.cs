using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class entityRelativeMove : Packet {
        public entityRelativeMove(ref Minecraft mc) {
            int Entity_ID = mc.nh.wSock.readInt();

            byte Diff_X = mc.nh.wSock.readByte(); // -- As fixed-point
            byte Diff_Y = mc.nh.wSock.readByte();
            byte Diff_Z = mc.nh.wSock.readByte();

            if (mc.MinecraftWorld != null) {
                int eIndex = mc.MinecraftWorld.getEntityById(Entity_ID);

                if (eIndex != -1) {
                    mc.MinecraftWorld.Entities[eIndex].Location.x += (Diff_X * 32);
                    mc.MinecraftWorld.Entities[eIndex].Location.y += (Diff_Y * 32);
                    mc.MinecraftWorld.Entities[eIndex].Location.z += (Diff_Z * 32);
                }

            }

            mc.raiseEntityRelMove(Entity_ID, (Diff_X * 32), (Diff_Y * 32), (Diff_Z * 32));
        }
    }
}
