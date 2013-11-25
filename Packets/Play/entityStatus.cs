using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class entityStatus : Packet { // -- This is more an event than anything..
        public entityStatus(ref Minecraft mc) {
            int Entity_ID = mc.nh.wSock.readInt(); // -- Surprized these arn't varints..
            byte status = mc.nh.wSock.readByte();

            if (mc.minecraftWorld != null) {
                int eIndex = mc.minecraftWorld.getEntityById(Entity_ID);

                if (eIndex != -1) {
                    mc.minecraftWorld.Entities[eIndex].status = status;
                }
            }

            mc.raiseEntityStatus(Entity_ID);
        }
    }
}
