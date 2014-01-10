using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class EntityLookRelativeMove : Packet {
        public EntityLookRelativeMove(ref Minecraft mc) {
            int Entity_ID = mc.nh.wSock.readInt();
            byte Diff_X = mc.nh.wSock.readByte();
            byte Diff_Y = mc.nh.wSock.readByte();
            byte Diff_Z = mc.nh.wSock.readByte();
            byte yaw = mc.nh.wSock.readByte();
            byte pitch = mc.nh.wSock.readByte();

            if (mc.MinecraftWorld != null) {
                int eIndex = mc.MinecraftWorld.GetEntityById(Entity_ID);

                if (eIndex != -1) {
                    mc.MinecraftWorld.Entities[eIndex].Location.x += (Diff_X * 32);
                    mc.MinecraftWorld.Entities[eIndex].Location.y += (Diff_Y * 32);
                    mc.MinecraftWorld.Entities[eIndex].Location.z += (Diff_Z * 32);
                    mc.MinecraftWorld.Entities[eIndex].yaw = yaw;
                    mc.MinecraftWorld.Entities[eIndex].pitch = pitch;
                }
            }

            mc.RaiseEntityRelMove(Entity_ID, (Diff_X * 32), (Diff_Y * 32), (Diff_Z * 32));
            mc.RaiseEntityLookChanged(Entity_ID, yaw, pitch);
        }
    }
}
