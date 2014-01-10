using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libMC.NET.Packets.Play {
    class EntityMetadata : Packet {
        public EntityMetadata(ref Minecraft mc) {
            int Entity_ID = mc.nh.wSock.readInt();

            if (mc.MinecraftWorld != null) {
                int eIndex = mc.MinecraftWorld.GetEntityById(Entity_ID);

                if (eIndex == -1) {
                    Entities.Entity asdf = new Entities.Entity(ref mc, "");
                    asdf.ReadEntityMetadata(ref mc);
                    // -- The problem is that we don't know what kind of entity this is.
                    // -- That can cause redundencies and multiple entries for one entity of an indeterminant type.
                } else {
                    mc.MinecraftWorld.Entities[eIndex].ReadEntityMetadata(ref mc);
                }

            }
            //ServerBound.Player player = new ServerBound.Player(ref mc);
        }
    }
}
