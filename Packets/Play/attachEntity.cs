using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class attachEntity : Packet {
        public attachEntity(ref Minecraft mc) {
            int Entity_ID = mc.nh.wSock.readInt();
            int Vechile_ID = mc.nh.wSock.readInt();
            bool leash = mc.nh.wSock.readBool();

            if (mc.minecraftWorld != null) {
                int eIndex = mc.minecraftWorld.getEntityById(Entity_ID);

                if (eIndex != -1) {
                    mc.minecraftWorld.Entities[eIndex].attached = true;
                    mc.minecraftWorld.Entities[eIndex].Vehicle_ID = Vechile_ID;
                    mc.minecraftWorld.Entities[eIndex].leashed = leash;
                }
            }

            mc.raiseEntityAttached(Entity_ID, Vechile_ID, leash);
        }
    }
}
