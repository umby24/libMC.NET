using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class entityMetadata : Packet {
        public entityMetadata(ref Minecraft mc) {
            int Entity_ID = mc.nh.wSock.readInt();

            if (mc.minecraftWorld != null) {
                int eIndex = mc.minecraftWorld.getEntityById(Entity_ID);

                if (eIndex == -1) {
                    Classes.Entity asdf = new Classes.Entity(ref mc, "");
                    asdf.readEntityMetadata(ref mc);
                    // -- The problem is that we don't know what kind of entity this is.
                    // -- That can cause redundencies and multiple entries for one entity of an indeterminant type.
                } else {
                    mc.minecraftWorld.Entities[eIndex].readEntityMetadata(ref mc);
                }

            }
            ServerBound.Player player = new ServerBound.Player(ref mc);
        }
    }
}
