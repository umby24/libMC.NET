using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class destroyEntities : Packet {
        public destroyEntities(ref Minecraft mc) {
            byte count = mc.nh.wSock.readByte();

            for (int x = 0; x < (int)count; x++) {
                int Entity_ID = mc.nh.wSock.readInt();
                if (mc.minecraftWorld != null) {
                    int eIndex = mc.minecraftWorld.getEntityById(Entity_ID);

                    if (eIndex != -1)
                        mc.minecraftWorld.Entities.RemoveAt(eIndex);
                }
                mc.raiseEntityDestruction(Entity_ID);
            }

            mc.RaiseDebug(this, "Entities Deleted.");
        }
    }
}
