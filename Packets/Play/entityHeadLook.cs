using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class EntityHeadLook : Packet {
        public EntityHeadLook(ref Minecraft mc) {
            int Entity_ID = mc.nh.wSock.readInt();
            byte head_Yaw = mc.nh.wSock.readByte();

            if (mc.MinecraftWorld != null) {
                int eIndex = mc.MinecraftWorld.GetEntityById(Entity_ID);

                if (eIndex != -1) {
                    mc.MinecraftWorld.Entities[eIndex].headPitch = head_Yaw;
                }
            }

            mc.RaiseEntityHeadLookChanged(Entity_ID, head_Yaw);
        }
    }
}
