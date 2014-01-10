using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libMC.NET.Common;

namespace libMC.NET.Packets.Play {
    class SpawnGlobalEntity : Packet { // -- Thunderbolt
        Vector location;

        public SpawnGlobalEntity(ref Minecraft mc) {
            mc.nh.wSock.readVarInt(); // -- Entity ID
            mc.nh.wSock.readByte(); // -- Type (Currently always 1, thunderbolt)
            location = new Vector(mc.nh.wSock.readInt(), mc.nh.wSock.readInt(), mc.nh.wSock.readInt());

            mc.RaiseDebug(this, "Thunderbolt has stricken.");
        }
    }
}
