using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class EntityStatus : Packet { // -- This is more an event than anything..
        public EntityStatus(ref Minecraft mc) {
            int Entity_ID = mc.nh.wSock.readInt(); // -- Surprized these arn't varints..
            byte status = mc.nh.wSock.readByte();

            if (mc.MinecraftWorld != null) {
                int eIndex = mc.MinecraftWorld.GetEntityById(Entity_ID);

                if (eIndex != -1) {
                    mc.MinecraftWorld.Entities[eIndex].status = status;
                }
            }

            mc.RaiseEntityStatus(Entity_ID);
        }
    }
}
