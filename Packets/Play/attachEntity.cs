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

            if (mc.MinecraftWorld != null) {
                int eIndex = mc.MinecraftWorld.getEntityById(Entity_ID);

                if (eIndex != -1) {
                    mc.MinecraftWorld.Entities[eIndex].attached = true;
                    mc.MinecraftWorld.Entities[eIndex].Vehicle_ID = Vechile_ID;
                    mc.MinecraftWorld.Entities[eIndex].leashed = leash;
                }
            }

            mc.raiseEntityAttached(Entity_ID, Vechile_ID, leash);
        }
    }
}
